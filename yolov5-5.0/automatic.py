import serial
import struct
import threading
import time
# import pygame
import keyword
# import keyboard
import pyzed.sl as sl
import torch
import cv2
import tkinter as tk
import queue
from pathlib import Path
from models.experimental import attempt_load
from utils.general import non_max_suppression, scale_coords, xyxy2xywh
from utils.torch_utils import select_device

# 初始化串口，分别用于 ESC 和 STM32
esc_ser = serial.Serial(
    port='/dev/ttyUSB2',  # ESC 的串口
    baudrate=115200,
    parity=serial.PARITY_NONE,
    stopbits=serial.STOPBITS_ONE,
    bytesize=serial.EIGHTBITS,
    timeout=1
)
servo_ser = serial.Serial(
    port='/dev/ttyUSB1',  # 舵机控制的串口
    baudrate=115200,
    parity=serial.PARITY_NONE,
    stopbits=serial.STOPBITS_ONE,
    bytesize=serial.EIGHTBITS,
    timeout=1
)

stm32_ser = serial.Serial(
    port='/dev/ttyUSB0',  # STM32 的串口
    baudrate=115200,
    parity=serial.PARITY_NONE,
    stopbits=serial.STOPBITS_ONE,
    bytesize=serial.EIGHTBITS,
    timeout=1
)

# 初始化ZED相机
zed = sl.Camera()
init_params = sl.InitParameters()
init_params.depth_mode = sl.DEPTH_MODE.PERFORMANCE  # 设置深度模式
init_params.coordinate_units = sl.UNIT.METER  # 设置单位为米
init_params.camera_resolution = sl.RESOLUTION.HD2K  # 设置相机分辨率为2K
zed.open(init_params)

# 设置设备
device = select_device('0')  # '0' 表示使用第一个 GPU，如果没有 GPU 则使用 CPU

# 加载本地权重文件
# weights_path = '/home/nvidia/下载/yolov5-5.0/yolov5-5.0/903bast.pt'  # 替换为本地权重文件的路径
weights_path = '/home/nvidia/下载/yolov5-5.0/yolov5-5.0/yolov5s.pt'  # 替换为本地权重文件的路径
model = attempt_load(weights_path)  # 加载 YOLOv5 模型
model = model.to(device)
names = model.module.names if hasattr(model, 'module') else model.names  # 获取类别名称

object_tracker = {}
time_window_data = {}
# 创建图像和深度测量对象
image = sl.Mat()
depth = sl.Mat()



# control_flag = False  # 设置标志位，确保舵机控制时，电机不查询。避免串口冲突
servo_id = 1  # 舵机ID
ctrl_flag = 0
motor_threshold = 5  # 电机电流阈值(A)
servo_threshold = 3  # 舵机电流阈值(A)
serial_lock = threading.Lock()

# 用于存储接收到的 STM32 数据的队列
stm32_data_queue = queue.Queue()

def parse_data_string(data_string):
    # 将字符串拆分并转换为字节列表
    data_parts = data_string.split()  # 根据空格拆分字符串
    byte_list = [int(part, 16) for part in data_parts]  # 将每个部分转换为十六进制整数
    # print(byte_list)
    return byte_list

# 上位机与32通信的完整数据帧,通过传入数据帧中的数据部分，返回完整数据帧
def communication_frames(data):
    # 帧头
    frame_header = [0xFF, 0xFF]

    # 数据部分
    data_without_checksum = data

    # 计算校验和
    data_checksum = calc_checksum(data_without_checksum)

    # 组成完整数据帧：帧头 + 数据 + 校验位
    full_frame = frame_header + data_without_checksum + [data_checksum]

    # 将所有字节转换为两位大写的十六进制字符串，并用空格连接
    full_frame_hex = ' '.join(format(byte, '02X') for byte in full_frame)

    return full_frame_hex

# 将X轴和Y轴的坐标转换成16进制字符串，并返回
def convert_coordinates_to_hex(x, y):
    # 定义 X 和 Y 的范围
    x_min, x_max = 0, 2200
    y_min, y_max = 0, 2900

    # 确保 X 和 Y 在指定范围内
    if not (x_min <= x <= x_max):
        raise ValueError(f"X must be in range {x_min} to {x_max}")
    if not (y_min <= y <= y_max):
        raise ValueError(f"Y must be in range {y_min} to {y_max}")

    # 将 X 和 Y 转换为十六进制字符串（前补零）
    x_hex = format(x, '04X')  # 四位十六进制字符串
    y_hex = format(y, '04X')  # 四位十六进制字符串

    # 每个字节之间用空格分隔
    hex_string = f"{x_hex[:2]} {x_hex[2:]} {y_hex[:2]} {y_hex[2:]}"

    return hex_string

# 得到坐标信息后正常控制流程函数，
def integral_control(x, y):
    # 传递坐标，到达目标位置
    ctrl_flag = 1
    data_coordinate = convert_coordinates_to_hex(x, y)
    data_string = f"02 {data_coordinate} 00"  # 数据部分（字符串形式）
    data_byte = parse_data_string(data_string)
    data_frames = communication_frames(data_byte)
    send_to_stm32_hex(data_frames)
    # 缓存区读取完就没有数据了，当有数据时，代表机械臂到达指定位置
    # 当队列为空时，函数会阻塞等待队列中的数据，知道队列中有数据时才继续进行下面操作
    receive32_coord_data = stm32_data_queue.get()  # 获取接收到的数据
    if receive32_coord_data[2] == 0x02:  # 机械臂到达指定位置成功
        current_servo_position = app.query_all_servo_data(servo_id)
        control_servo_move(current_servo_position, target_position=3300)  # 下放机械臂到物体
        time.sleep(1)
        # 夹子闭合
        data1_string = f"03 {data_coordinate} 01"
        data1_byte = parse_data_string(data1_string)
        data1_frames = communication_frames(data1_byte)
        send_to_stm32_hex(data1_frames)  # 发送抓手闭合指令
        time.sleep(1)
        receive32_servo_data1 = stm32_data_queue.get()
        if receive32_servo_data1[2] == 0x03:  # 抓取
            time.sleep(1)
            current_servo_position = app.query_all_servo_data(servo_id)
            control_servo_move(current_servo_position, target_position=2900)  # 反转手臂
            # 机械臂移动到投放位置
            data_put_coordinate = convert_coordinates_to_hex(200, 2260)
            data2_string = f'02 {data_put_coordinate} 00'
            data2_byte = parse_data_string(data2_string)
            data2_frames = communication_frames(data2_byte)
            send_to_stm32_hex(data2_frames)
            # 等待机械臂到达投放位置
            receive32_coord_data1 = stm32_data_queue.get()  # 获取接收到的数据
            if receive32_coord_data1[2] == 0x02:  # 机械臂到达投放的指定位置成功
                current_servo_position = app.query_all_servo_data(servo_id)
                control_servo_move(current_servo_position, target_position=502)  # 反转手臂
                time.sleep(1)
                # 抓手松开、放置抓取物
                data3_string = f'03 {data_put_coordinate} 02'
                data3_byte = parse_data_string(data3_string)
                data3_frames = communication_frames(data3_byte)
                send_to_stm32_hex(data3_frames)  # 发送松手指令，开始投放
                receive32_servo_data2 = stm32_data_queue.get()
                if receive32_servo_data2[2] == 0x03:
                    time.sleep(1)
                    current_servo_position = app.query_all_servo_data(servo_id)
                    control_servo_move(current_servo_position, target_position=2900)  # 反转手臂
    ctrl_flag = 0


#关于电机ESC有关函数
# 计算校验和
def calc_checksum(data):
    return sum(data) & 0xFF

# 发送查询ESC电机状态的指令
def send_query_status(esc_id):

    header = [0xAA, 0x55]  # 固定头部
    function_code = 0x82  # 查询ESC状态功能码
    data = []  # 无额外数据
    length = 2  # 数据部分长度为2（功能码和ESC ID）
    checksum_data = [function_code, esc_id] + data
    checksum = calc_checksum(checksum_data)
    # 组装帧
    frame = header + [length, function_code, esc_id] + data + [checksum]
    # 发送帧
    send_data(esc_ser, ''.join(format(x, '02x') for x in frame))

#解析从电机ESC接收到的数据
def parse_response(response, motor_id):
    try:           # '<'代表低字节在前高字节在后的解析方式，‘h’代表有符号的短整型（16位两个字节），‘H’表示无符号的短整型（16位两个字节）
        speed = struct.unpack('<H', response[5:7])[0]  # 转速 (RPM)
        current = struct.unpack('<h', response[7:9])[0] * 0.01  # 电流 (A)
        voltage = struct.unpack('<H', response[9:11])[0] * 0.1  # 电压 (V)（注意：无符号整数通常不需要调换字节序）
        vout = struct.unpack('<H', response[11:13])[0]  # 调制比 (0~666)
        ppm_recv = struct.unpack('<H', response[13:15])[0] * 0.1  # 接收到的 PWM 信号 (0.1us)
        temp_mos = struct.unpack('<h', response[15:17])[0] * 0.1  # MOS 温度 (°C)
        temp_cap = struct.unpack('<h', response[17:19])[0] * 0.1  # 电容温度 (°C)
        temp_mcu = struct.unpack('<h', response[19:21])[0]  # MCU 温度 (°C)
        error1 = struct.unpack('<H', response[21:23])[0]  # 自检错误代码
        error2 = struct.unpack('<H', response[23:25])[0]  # 运行错误代码
        time_count = struct.unpack('<H', response[25:27])[0] * 10  # 时间计数 (10ms)
        ibus = struct.unpack('<H', response[27:29])[0] * 0.01  # 母线电流 (A)

        return(motor_id, speed, current, voltage, vout, ppm_recv, temp_mos, temp_cap,temp_mcu, error1, error2,time_count, ibus)

    except struct.error:
        print("Failed to unpack ESC response data.")


#关于舵机有关函数
# 校验和计算
def calculate_checksum1(packet):
    return (~sum(packet) & 0xFF)


# 发送指令并获取应答
def send_command(command_packet):
    servo_ser.write(command_packet)
    time.sleep(0.1)  # 短暂等待舵机应答
    response = servo_ser.read(servo_ser.in_waiting)
    # 将接收到的字节流转换为字节数组
    response = bytearray(response)

    # 定义要去除的序列
    remove_sequence = bytearray([0xFF, 0xFF, 0x01, 0x02, 0x00, 0xFC])

    # 去掉所有的 FF FF 01 02 00 FC 序列
    while remove_sequence in response:
        start = response.find(remove_sequence)
        if start != -1:
            del response[start:start + len(remove_sequence)]
    return response


# 读取数据 (READ DATA 指令)
def read_servo_data(servo_id, address, length):
    # 构建读取指令
    command_packet = [0xFF, 0xFF, servo_id, 0x04, 0x02] + address
    checksum = calculate_checksum1(command_packet[2:])
    command_packet.append(checksum)

    # 发送指令并获取响应
    response = send_command(bytes(command_packet))

    # 解析应答包
    if response:
        response = list(response)
        if response[4] == 0x00:  # 检查是否有错误
            data = response[5:5 + length]
            return data
        else:
            print(f"错误: {response[4]}")
    else:
        print(f"舵机 {servo_id} 无应答。")
    return None


# 解析两字节数据 (低字节在前，高字节在后)
def parse_two_bytes(data):
    return data[0] + (data[1] << 8)


# 舵机位置写入指令函数
# 将一个十进制数转换成格式化的十六进制字符串，并调换循序（高字节 低字节 -> 低字节 高字节）然后返回
def decimal_to_hex_pairs(decimal_number):
    hex_string = format(decimal_number, 'X').upper()
    if len(hex_string) % 2 != 0:
        hex_string = '0' + hex_string
    hex_pairs = [hex_string[i:i + 2] for i in range(0, len(hex_string), 2)]
    hex_pairs.reverse()
    formatted_result = ' '.join(hex_pairs)
    return formatted_result

# 计算发送舵机位置指令帧的检验和并返回
def calculate_checksum(hex_string):
    hex_string = hex_string.replace(' ', '')
    byte_array = bytes.fromhex(hex_string)
    checksum = sum(byte_array)
    checksum = checksum & 0xFF
    checksum = (~checksum) & 0xFF
    return checksum
# order为帧的数据部分，传入数据部分，返回完整数据帧
def order_out(order):
    data_without_checksum = order
    checksum = calculate_checksum(data_without_checksum)
    checksum_hex = format(checksum, '02X')
    complete_command = f"FF FF {data_without_checksum} {checksum_hex}"
    return complete_command

# 将数据帧转换成字节流形式，并通过串口写入
def send_data(serial_port, data):
    with serial_lock:  # 使用锁保护
        hex_data = bytes.fromhex(data)
        serial_port.write(hex_data)

# 传入当前位置和目标位置，逐步到达
def control_servo_move(current_position, target_position):
    try:
        current_position = int(current_position)
    except ValueError:
        print("Invalid data received for current position, skipping move.")
        return
    # current_position = int(current_position)
    while abs(target_position - current_position) > 7:
        if current_position > target_position:
            current_position -= 7
            if current_position < 500:
                current_position = 500
        elif current_position < target_position:
            current_position += 7
            if current_position > 3300:
                current_position = 3300
        # 将当前位置转化成低字节在前，高字节在后的两个字节
        count = decimal_to_hex_pairs(current_position)
        order = f"01 09 03 2A {count} 00 00 E8 03"
        x = order_out(order)
        send_data(servo_ser, x)
        time.sleep(0.01)

# def listen_for_keys():
#     """监听按键 1、2、3、4 的按下事件"""
#     keyboard.add_hotkey('1', lambda: integral_control(350, 2400))
#     keyboard.add_hotkey('2', lambda: integral_control(1400, 2400))
#     keyboard.add_hotkey('3', lambda: integral_control(350, 1000))
#     keyboard.add_hotkey('4', lambda: integral_control(1400, 1000))
#
#     try:
#         # 保持程序持续运行，等待按键事件
#         print("Listening for keys 1, 2, 3, 4... Press Ctrl+C to stop.")
#         while True:
#             time.sleep(1)
#     except KeyboardInterrupt:
#         print("Program stopped.")
#         ser.close()  # 关闭串口连接

# 创建主窗口
class ESCStatusApp:
    def __init__(self, master):
        self.master = master
        master.title("ESC 状态实时显示")
        master.geometry("600x750")  # 设置窗口大小

        # 创建 ESC 状态框架
        self.esc_frame = tk.Frame(master, padx=10, pady=10, relief=tk.RIDGE, bd=2)
        self.esc_frame.grid(row=0, column=0, sticky="nsew")

        # 创建舵机状态框架
        self.servo_frame = tk.Frame(master, padx=10, pady=10, relief=tk.RIDGE, bd=2)
        self.servo_frame.grid(row=0, column=1, sticky="nsew")  # 舵机状态框在右上方

        # 在 ESC 框架中创建标签
        self.motor_frames = {}
        for motor_id in [1, 2]:
            frame = tk.Frame(self.esc_frame, padx=10, pady=10, relief=tk.RIDGE, bd=2)
            frame.pack(pady=10)

            label_title = tk.Label(frame, text=f"ESC ID {motor_id}", font=("Arial", 16, "bold"))
            label_title.pack()

            labels = {
                "speed": tk.Label(frame, text="ESC Speed: "),
                "current": tk.Label(frame, text="ESC Current: "),
                "voltage": tk.Label(frame, text="ESC Voltage: "),
                "vout": tk.Label(frame, text="ESC Modulation Ratio (Vout): "),
                "ppm_recv": tk.Label(frame, text="ESC Received PWM: "),
                "temp_mos": tk.Label(frame, text="ESC MOS Temperature: "),
                "temp_cap": tk.Label(frame, text="ESC Capacitor Temperature: "),
                "temp_mcu": tk.Label(frame, text="ESC MCU Temperature: "),
                "error1": tk.Label(frame, text="ESC Self-check Error Code: "),
                "error2": tk.Label(frame, text="ESC Running Error Code: "),
                "time_count": tk.Label(frame, text="ESC Time Count: "),
                "ibus": tk.Label(frame, text="ESC Bus Current (Ibus): ")
            }

            for label in labels.values():
                label.pack(anchor=tk.W)  # 左对齐

            self.motor_frames[motor_id] = labels

        # 在舵机框架中创建标签
        self.servo_frame_inner = tk.Frame(self.servo_frame, padx=15, pady=15, relief=tk.RIDGE, bd=2)
        self.servo_frame_inner.pack(pady=10)

        label_title = tk.Label(self.servo_frame_inner, text="舵机状态", font=("Arial", 16, "bold"))
        label_title.pack()

        self.servo_labels = {

            "position": tk.Label(self.servo_frame_inner, text="位置: "),
            "current": tk.Label(self.servo_frame_inner, text="电流: ", fg="red"),

        }

        for label in self.servo_labels.values():
            label.pack(anchor=tk.W)  # 左对齐

        # 添加初始化和结束按钮
        self.init_button = tk.Button(master, text="初始化", command=self.system_init)  # 绑定初始化函数
        self.init_button.grid(row=1, column=0, sticky="se")  # 初始化按钮在左下方

        self.end_button = tk.Button(master, text="结束", command=self.system_exit)  # 绑定结束函数
        self.end_button.grid(row=1, column=1, sticky="se")  # 结束按钮在右下方

        # 配置网格布局
        master.grid_rowconfigure(0, weight=1)
        master.grid_rowconfigure(1, weight=0)  # 让初始化按钮不拉伸
        master.grid_columnconfigure(0, weight=1)
        master.grid_columnconfigure(1, weight=1)

    def update_ESC_display(self, motor_id, speed, current, voltage, vout, ppm_recv, temp_mos, temp_cap, temp_mcu, error1,
                           error2, time_count, ibus):
        labels = self.motor_frames[motor_id]
        labels["speed"].config(text=f"ESC Speed: {speed} RPM")
        labels["current"].config(text=f"ESC Current: {current:.2f} A")
        labels["voltage"].config(text=f"ESC Voltage: {voltage:.2f} V")
        labels["vout"].config(text=f"ESC Modulation Ratio (Vout): {vout}")
        labels["ppm_recv"].config(text=f"ESC Received PWM: {ppm_recv:.1f} us")
        labels["temp_mos"].config(text=f"ESC MOS Temperature: {temp_mos:.1f} °C")
        labels["temp_cap"].config(text=f"ESC Capacitor Temperature: {temp_cap:.1f} °C")
        labels["temp_mcu"].config(text=f"ESC MCU Temperature: {temp_mcu} °C")
        labels["error1"].config(text=f"ESC Self-check Error Code: {error1}")
        labels["error2"].config(text=f"ESC Running Error Code: {error2}")
        labels["time_count"].config(text=f"ESC Time Count: {time_count} ms")
        labels["ibus"].config(text=f"ESC Bus Current (Ibus): {ibus:.2f} A")
        # 判断电机电流是否超过阈值
        # if ibus > motor_threshold:
            # data = [0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00]
            # data_frames = communication_frames(data)
            # send_to_stm32_hex(data_frames)
            # labels["ibus"].config(fg="red")  # 将电流标签设为红色


    def system_init(self):
        # global control_flag
        # 电机配ID
        data1 = [0x01, 0x00, 0x00, 0x00, 0x00, 0x00]
        data1_frames = communication_frames(data1)
        send_to_stm32_hex(data1_frames)
        receive32_id_data = stm32_data_queue.get()
        if receive32_id_data[2] == 0x01:
            time.sleep(0.5)
            data1_to_send = bytes([0xAA, 0x55, 0x02, 0x80, 0x01, 0x81])
            esc_ser.write(data1_to_send)
            time.sleep(0.1)
            response = esc_ser.read(esc_ser.in_waiting)
            hex_response = ' '.join([f'{byte:02X}' for byte in response])
            print(f"Received: {hex_response}")
            time.sleep(2)
            data2_to_send = bytes([0xAA, 0x55, 0x02, 0x80, 0x02, 0x82])
            esc_ser.write(data2_to_send)
            time.sleep(0.1)
            response = esc_ser.read(esc_ser.in_waiting)
            hex_response = ' '.join([f'{byte:02X}' for byte in response])
            print(f"Received: {hex_response}")
            time.sleep(3)
            # 到达初始位置
            data2 = [0x02, 0x00, 0x00, 0x08, 0xD4, 0x00]
            data2_frames = communication_frames(data2)
            send_to_stm32_hex(data2_frames)
            # 接收32返回数据包，确认是否到达目标位置
            receive32_coord_data = stm32_data_queue.get()
            if receive32_coord_data[2] == 0x02:  # 机械臂到达指定位置成功
                current_servo_position = app.query_all_servo_data(servo_id)
                control_servo_move(current_servo_position, target_position=3000)
                time.sleep(1)
                # 发送抓手松开指令
                data3 = [0x03, 0x00, 0x00, 0x00, 0x00, 0x02]
                data3_frames = communication_frames(data3)
                send_to_stm32_hex(data3_frames)
                receive32_servo_data = stm32_data_queue.get()
                if receive32_servo_data[2] == 0x03:  # 抓手闭合
                    print("初始化成功")



    def system_exit(self):
        # global control_flag
        # 到达初始化位置
        data1 = [0x02, 0x00, 0x00, 0x08, 0xD4, 0x00]
        data1_frames = communication_frames(data1)
        send_to_stm32_hex(data1_frames)
        receive32_coord_data1 = stm32_data_queue.get()
        if receive32_coord_data1[2] == 0x02:  # 机械臂到达指定位置成功
            current_servo_position = app.query_all_servo_data(servo_id)
            control_servo_move(current_servo_position, target_position=500)
            time.sleep(1)
            # 发送抓手闭合指令
            data2 = [0x03, 0x00, 0x00, 0x00, 0x00, 0x01]
            data2_frames = communication_frames(data2)
            send_to_stm32_hex(data2_frames)
            receive_servo_data = stm32_data_queue.get()
            if receive_servo_data[2] == 0x03:  # 抓手闭合
                # 回到初始位置
                data0 = [0x02, 0x00, 0x00, 0x00, 0x00, 0x00]
                data0_frames = communication_frames(data0)
                send_to_stm32_hex(data0_frames)
                receive32_coord_data0 = stm32_data_queue.get()
                if receive32_coord_data0[2] == 0x02:
                    print("结束")




    # 电机和舵机实时状态查询
    def query_esc_status_real_time(self):
      # with serial_lock:
        while True:
      # 查询 ESC ID 1
            send_query_status(0x01)
            time.sleep(0.1)  # 等待 100ms 读取反馈
            if esc_ser.in_waiting > 0:
                response = esc_ser.read(esc_ser.in_waiting)
                # 将接收到的字节流转换为字节数组
                response = bytearray(response)
                # 定义要去除的序列              #
                remove_sequence = bytearray([0xFF, 0xFF, 0x01, 0x02, 0x00, 0xFC])

                # 去掉所有的 FF FF 01 02 00 FC 序列>>>>>>>>>>>>>>
                while remove_sequence in response:
                    start = response.find(remove_sequence)
                    if start != -1:
                        del response[start:start + len(remove_sequence)]

                # 检查响应是否以 AA55 开头且长度为 30 字节
                if response.startswith(b'\xAA\x55') and len(response) == 30:
                    # 开始调用外部解析函数来解析接收到电机1数据
                    parse_data1 = parse_response(response, motor_id=1)
                    # 将解析的ID为1的电机信息更新至GUI界面
                    if parse_data1:
                        self.update_ESC_display(*parse_data1)
            time.sleep(0.5)
            # 查询 ESC ID 2
            send_query_status(0x02)
            time.sleep(0.1)  # 等待 100ms 读取反馈
            if esc_ser.in_waiting > 0:
                response = esc_ser.read(esc_ser.in_waiting)
                # 将接收到的字节流转换为字节数组
                response = bytearray(response)
                # 定义要去除的序列
                remove_sequence = bytearray([0xFF, 0xFF, 0x01, 0x02, 0x00, 0xFC])

                # 去掉所有的 FF FF 01 02 00 FC 序列
                while remove_sequence in response:
                    start = response.find(remove_sequence)
                    if start != -1:
                        del response[start:start + len(remove_sequence)]

                # 检查响应是否以 AA55 开头且长度为 30 字节
                if response.startswith(b'\xAA\x55') and len(response) == 30:
                    # 开始调用外部解析函数来解析接收到电机1数据
                    parse_data2 = parse_response(response, motor_id=2)
                    # 将解析的ID为2的电机信息更新至GUI界面
                    if parse_data2:
                        self.update_ESC_display(*parse_data2)
            time.sleep(0.5)
            # 舵机状态信息实时查询
            # self.query_all_servo_data(servo_id)
            # time.sleep(0.5)

    def update_servo_display(self, position, current):

        self.servo_labels["position"].config(text=f"位置: {position}")
        self.servo_labels["current"].config(text=f"电流: {current} mV")
        # if current >= servo_threshold:

        # servo_labels["current"].config(fg="red")  # 将舵机电流标签设为红色


    def query_all_servo_data(self, servo_id):
        position_address = [0x38, 0x39]   # 位置地址
        position_data = read_servo_data(servo_id, position_address, 2)
        position = parse_two_bytes(position_data) if position_data else "无数据"

        current_address = [0x45, 0x46]  # 电流地址
        current_data = read_servo_data(servo_id, current_address, 2)
        current = parse_two_bytes(current_data) * 6.45 if current_data else "无数据"

        self.update_servo_display(position, current)
        return position    # 返回当前位置值

# 向 STM32 逐个字节发送十六进制数据
def send_to_stm32_hex(hex_string):
    try:
        # 将十六进制字符串转换为字节数据
        data = bytes.fromhex(hex_string)
        # 逐个字节发送数据
        for byte in data:
            stm32_ser.write(bytes([byte]))  # 将单个字节发送
            print(f"Sent byte to STM32: {byte:02X}")
    except ValueError:
        print("Invalid hex string.")

# 接收 STM32 数据并返回接收的数据帧
def receive_from_stm32():
    while True:
        if stm32_ser.in_waiting > 0:   # 读取串口缓冲区中的所有字节
            stm32_data = stm32_ser.read(stm32_ser.in_waiting)  # 读取所有数据
            # 检查接收到的帧是否有效
            if len(stm32_data) == 3 and stm32_data[0] == 0xFC and stm32_data[1] == 0xFC:
                print(f"Valid frame received: {list(stm32_data)}")  # 打印原始字节
                stm32_data_queue.put(stm32_data)  # 将数据放入队列
            else:
                print(f"Invalid frame: {list(stm32_data)}")  # 打印原始字节
        time.sleep(0.1)  # 避免占用过多 CPU 资源


# 启动程序
# 在主函数中启动 GUI 和 ESC 实时查询线程
if __name__ == "__main__":
    root = tk.Tk()
    app = ESCStatusApp(root)

    # 启动线程查询电机 ESC 状态
    esc_thread = threading.Thread(target=app.query_esc_status_real_time)
    esc_thread.daemon = True  # 将线程设置为守护线程，确保程序退出时自动结束
    esc_thread.start()

    # 开启 STM32 数据接收线程
    stm32_thread = threading.Thread(target=receive_from_stm32)
    stm32_thread.daemon = True
    stm32_thread.start()

    # 启动 Tkinter 主循环
    # root.mainloop()

    while True:
        if zed.grab() == sl.ERROR_CODE.SUCCESS:
            zed.retrieve_image(image, sl.VIEW.LEFT)  # 获取左视图图像
            zed.retrieve_measure(depth, sl.MEASURE.DEPTH)  # 获取深度信息

            # 将图像转换为OpenCV格式
            frame = image.get_data()
            frame = cv2.cvtColor(frame, cv2.COLOR_RGBA2RGB)

            # 调整图像大小为32的倍数
            img = cv2.resize(frame, (640, 352))
            img = torch.from_numpy(img).to(device).float() / 255.0  # 转换为 Tensor 并归一化
            img = img.permute(2, 0, 1).unsqueeze(0)  # 转换为 NCHW 格式

            with torch.no_grad():  # 禁用梯度计算
                pred = model(img)[0]  # 预测
            pred = non_max_suppression(pred, 0.50, 0.45)  # 应用 NMS

            # 在图像上绘制检测框和深度信息，并通过串口发送数据
            for det in pred:
                if len(det):
                    det[:, :4] = scale_coords(img.shape[2:], det[:, :4], frame.shape).round()
                    current_time = time.time()  # 获取当前时间
                    for *xyxy, conf, cls in det:
                        x1, y1, x2, y2 = map(int, xyxy)
                        label = f'{names[int(cls)]} {conf:.2f}'

                        # 获取中心点的深度信息
                        cx, cy = (x1 + x2) // 2, (y1 + y2) // 2
                        # 判断物体位置
                        height, width, _ = frame.shape
                        if cy < height // 2:
                            if cx < width // 2:
                                position = 'Top Left'  # 左上
                            else:
                                position = 'Top Right'  # 右上
                        else:
                            if cx < width // 2:
                                position = 'Bottom Left'  # 左下
                            else:
                                position = 'Bottom Right'  # 右下

                        err, depth_value = depth.get_value(cx, cy)
                        if err == sl.ERROR_CODE.SUCCESS:
                            depth_label = f'Depth: {depth_value:.2f}m'
                            # 将中心坐标、物体类别名称和深度信息通过串口发送出去
                            object_name = names[int(cls)]  # 获取物体名称
                            # data = f'{position},{object_name},{depth_value:.2f}\n'

                            # # ser_com.write(data.encode())
                            # # ser_com.write(data.encode())

                            # 只处理置信度大于 0.7 的物体
                            if conf > 0.7:
                                object_key = f"{label}_{cx}_{cy}"  # 使用标签和坐标作为唯一标识
                                if object_key not in object_tracker:
                                    # 初始化物体跟踪器
                                    object_tracker[object_key] = {"time": current_time, "label": label,
                                                                  "depth": depth_value, "position": position}
                                else:
                                    # 更新存在时间
                                    object_tracker[object_key]["time"] = current_time

                            # 检查物体是否存在超过 1 秒
                            for key, value in list(object_tracker.items()):
                                if current_time - value["time"] > 2:  # 超过 2 秒
                                    object_info = (value["label"], value["depth"], value["position"], conf)
                                    timestamp = current_time
                                    if timestamp not in time_window_data:
                                        time_window_data[timestamp] = []
                                    time_window_data[timestamp].append(object_info)
                                    # 输出物品信息、深度信息和位置信息
                                    # print(
                                    #     f'Object: {value["label"]}, Depth: {value["depth"]:.2f}m, Position: {value["position"]}')
                                    # 从跟踪器中移除该物体
                                    del object_tracker[key]
                            # 清理时间窗内超过 5 秒的数据
                            current_time_window = current_time - 5
                            for timestamp in list(time_window_data.keys()):
                                if timestamp < current_time_window:
                                    del time_window_data[timestamp]

                            frequency_counter = {}
                            for infos in time_window_data.values():
                                for info in infos:
                                    key = (info[0], info[2])  # 物品名称和位置作为键
                                    depth_value = info[1]  # 深度值
                                    conf_value = info[3]  # 置信度
                                    if key not in frequency_counter:
                                        frequency_counter[key] = {"count": 0, "depths": [], "confidences": []}
                                    frequency_counter[key]["count"] += 1
                                    frequency_counter[key]["depths"].append(depth_value)
                                    frequency_counter[key]["confidences"].append(conf_value)

                            # 找到出现频率超过 10 次的信息
                            for (label, position), data in frequency_counter.items():
                                if data["count"] > 10:
                                    avg_depth = sum(data["depths"]) / len(data["depths"])  # 计算平均深度
                                    avg_conf = sum(data["confidences"]) / len(data["confidences"])  # 计算平均置信度
                                    if ctrl_flag == 0:
                                        if position == 'Top Left':
                                            integral_control(350, 2400)
                                            print("11111")
                                        elif position == 'Top Right':
                                            integral_control(1400, 2400)
                                            print("222222")
                                        elif position == 'Bottom Left':
                                            integral_control(350, 1000)
                                            print("333333")
                                        elif position == 'Bottom Right':
                                            integral_control(1400, 1000)
                                            print("444444")
                        else:
                            depth_label = 'Depth: N/A'

                        # 绘制检测框和标签
                        cv2.rectangle(frame, (x1, y1), (x2, y2), (0, 255, 0), 2)
                        cv2.putText(frame, label, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)
                        cv2.putText(frame, depth_label, (x1, y2 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)

            # 显示结果
            cv2.imshow('YOLOv5 ZED Real-time Detection', frame)

        # 按下 'q' 键退出
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break

    # 关闭串口通信
    # ser.close()
    # ser_com.close()

    # 释放ZED相机并关闭窗口
    zed.close()
    cv2.destroyAllWindows()
    # / dev / ttyUSB *