import serial
import struct
import time

# 初始化串口
ser = serial.Serial(
    port='/dev/ttyUSB0',  # 根据你的系统修改端口
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
    ser.write(bytearray(frame))
    hex_frame = ' '.join([f'{byte:02X}' for byte in frame])
    print(f"Sent: {hex_frame}")


import struct


# 解析并显示反馈数据
def parse_and_display_response(response):
    if len(response) < 26:
        print("Invalid response length")
        return

    # 根据提供的数据格式逐个字段解析
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
        print(f"Speed: {speed} RPM")
        print(f"Current: {current:.2f} A")
        print(f"Voltage: {voltage:.2f} V")
        print(f"Modulation Ratio (Vout): {vout}")
        print(f"Received PWM: {ppm_recv:.1f} us")
        print(f"MOS Temperature: {temp_mos:.1f} °C")
        print(f"Capacitor Temperature: {temp_cap:.1f} °C")
        print(f"MCU Temperature: {temp_mcu} °C")
        print(f"Self-check Error Code: {error1}")
        print(f"Running Error Code: {error2}")
        print(f"Time Count: {time_count} ms")
        print(f"Bus Current (Ibus): {ibus:.2f} A")

    except struct.error:
        print("Failed to unpack response data.")


# 实时发送查询并接收数据
def query_motor_status_real_time(esc_id=0xFF):
    try:
        while True:
            # 等待键盘输入 'c'，然后发送查询指令
            user_input = input("按 'c' 键发送查询指令，按 'q' 键退出: ").lower()

            if user_input == 'c':
                # 发送查询指令
                send_query_status(esc_id)

                # 等待并读取反馈数据
                time.sleep(0.1)  # 等待100ms后读取反馈
                if ser.in_waiting > 0:
                    response = ser.read(ser.in_waiting)
                    # 将接收到的response转换为十六进制格式并打印
                    hex_response = ' '.join([f'{byte:02X}' for byte in response])
                    print(f"Received: {hex_response}")
                    parse_and_display_response(response)

            elif user_input == 'q':
                print("程序退出")
                break

            else:
                print("无效输入，请按 'c' 发送查询指令，或按 'q' 退出程序")

    except KeyboardInterrupt:
        print("程序中止")
    finally:
        ser.close()


if __name__ == "__main__":
    query_motor_status_real_time(esc_id=0xFF)  # 实时查询ID为1的电机状态