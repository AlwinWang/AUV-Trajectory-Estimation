import serial
import struct
import threading
import time
#sudo chmod 777 /dev/ttyUSB0 给串口权限的指令
#ls -l /dev/ttyUSB*   查询端口指令
# 初始化两个串口，分别用于 ESC 和 STM32
esc_ser = serial.Serial(
    port='/dev/ttyUSB0',  # ESC 的串口
    baudrate=115200,
    parity=serial.PARITY_NONE,
    stopbits=serial.STOPBITS_ONE,
    bytesize=serial.EIGHTBITS,
    timeout=1
)

stm32_ser = serial.Serial(
    port='/dev/ttyUSB2',  # STM32 的串口
    baudrate=115200,
    parity=serial.PARITY_NONE,
    stopbits=serial.STOPBITS_ONE,
    bytesize=serial.EIGHTBITS,
    timeout=1
)

# 计算校验和
def calc_checksum(data):
    return sum(data) & 0xFF

# 发送查询ESC状态的指令
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
    esc_ser.write(bytearray(frame))
    #esc_ser.write(bytearray([0xAA, 0x55, 0x02, 0x82, 0x06, 0x88]))
    hex_frame = ' '.join([f'{byte:02X}' for byte in frame])
    print(f"Sent to ESC: {hex_frame}")

# 解析并显示 ESC 反馈数据
def parse_and_display_response(response):
    if len(response) < 26:
        print("Invalid response length")
        return

    try:
        # 按顺序解析字段，分别提取响应中的值
        speed = struct.unpack('>h', response[5:7])[0]  # 转速 (RPM)
        current = struct.unpack('>h', response[7:9])[0] * 0.01  # 电流 (A)
        voltage = struct.unpack('>H', response[9:11])[0] * 0.1  # 电压 (V)
        vout = struct.unpack('>H', response[11:13])[0]  # 调制比 (0~666)
        ppm_recv = struct.unpack('>H', response[13:15])[0] * 0.1  # 接收到的 PWM 信号 (0.1us)
        temp_mos = struct.unpack('>h', response[15:17])[0] * 0.1  # MOS 温度 (°C)
        temp_cap = struct.unpack('>h', response[17:19])[0] * 0.1  # 电容温度 (°C)
        temp_mcu = struct.unpack('>h', response[19:21])[0]  # MCU 温度 (°C)
        error1 = struct.unpack('>H', response[21:23])[0]  # 自检错误代码
        error2 = struct.unpack('>H', response[23:25])[0]  # 运行错误代码
        time_count = struct.unpack('>H', response[25:27])[0] * 10  # 时间计数 (10ms)
        ibus = struct.unpack('>H', response[27:29])[0] * 0.01  # 母线电流 (A)

        # 打印解析后的数据
        print(f"ESC Speed: {speed} RPM")
        print(f"ESC Current: {current:.2f} A")
        print(f"ESC Voltage: {voltage:.2f} V")
        print(f"ESC Modulation Ratio (Vout): {vout}")
        print(f"ESC Received PWM: {ppm_recv:.1f} us")
        print(f"ESC MOS Temperature: {temp_mos:.1f} °C")
        print(f"ESC Capacitor Temperature: {temp_cap:.1f} °C")
        print(f"ESC MCU Temperature: {temp_mcu} °C")
        print(f"ESC Self-check Error Code: {error1}")
        print(f"ESC Running Error Code: {error2}")
        print(f"ESC Time Count: {time_count} ms")
        print(f"ESC Bus Current (Ibus): {ibus:.2f} A")

    except struct.error:
        print("Failed to unpack ESC response data.")

# 接收 STM32 数据
def receive_from_stm32():
    while True:
        if stm32_ser.in_waiting > 0:
            stm32_data = stm32_ser.readline().decode('utf-8').strip()
            print(f"Received from STM32: {stm32_data}")
        time.sleep(0.1)

# 发送数据到 STM32
def send_to_stm32_hex(hex_string):
    """
    将十六进制字符串转换为字节并发送到 STM32
    :param hex_string: 需要发送的十六进制字符串，如 'AABBCCDD'
    """
    try:
        # 将十六进制字符串转换为字节
        data = bytes.fromhex(hex_string)
        stm32_ser.write(data)  # 发送字节数据
        print(f"Sent hex data to STM32: {hex_string}")
    except ValueError:
        print("Invalid hex string. Please enter a valid hex string.")

# 实时查询 ESC 状态
def query_esc_status_real_time(esc_id=0xFF):
    try:
        while True:
            # 等待键盘输入 'c'，然后发送查询指令
            user_input = input("按 'c' 键发送查询指令到 ESC，按 's' 键发送数据到 STM32，按 'q' 键退出: ").lower()

            if user_input == 'c':
                # 发送查询指令到 ESC
                send_query_status(esc_id)

                # 等待并读取 ESC 的反馈数据
                time.sleep(0.1)  # 等待100ms后读取反馈
                if esc_ser.in_waiting > 0:
                    response = esc_ser.read(esc_ser.in_waiting)
                    # 将接收到的response转换为十六进制格式并打印
                    hex_response = ' '.join([f'{byte:02X}' for byte in response])
                    print(f"Received from ESC: {hex_response}")
                    parse_and_display_response(response)

            elif user_input == 's':
                # 向 STM32 发送数据
                data_to_send = input("输入要发送到 STM32 的数据: ")
                send_to_stm32_hex(data_to_send)

            elif user_input == 'q':
                print("程序退出")
                break

            else:
                print("无效输入，请按 'c' 发送查询到 ESC，'s' 发送数据到 STM32，或按 'q' 退出程序")

    except KeyboardInterrupt:
        print("程序中止")
    finally:
        esc_ser.close()
        stm32_ser.close()

# 开启 STM32 数据接收线程
stm32_thread = threading.Thread(target=receive_from_stm32)
stm32_thread.daemon = True
stm32_thread.start()

if __name__ == "__main__":
    query_esc_status_real_time(esc_id=0xFF)  # 实时查询ESC和与STM32通信
