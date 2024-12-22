
import serial
import threading
import time
import queue

# 配置串口参数
PORT = '/dev/ttyUSB0'  # 替换为你的串口端口
BAUDRATE = 115200
TIMEOUT = 1     # 超时时间设置为1秒

# 用于存储输入的队列
input_queue = queue.Queue()

def read_input():
    while True:
        # 读取输入并将其放入队列中
        user_input = input()
        input_queue.put(user_input)

def decimal_to_hex_pairs(decimal_number):
    # 将十进制数转换为十六进制字符串，并去掉'0x'前缀
    hex_string = format(decimal_number, 'X').upper()

    # 如果十六进制字符串的长度不是偶数，前面填零
    if len(hex_string) % 2 != 0:
        hex_string = '0' + hex_string

    # 将十六进制字符串分割成两位两位的组
    hex_pairs = [hex_string[i:i + 2] for i in range(0, len(hex_string), 2)]

    # 反转组的顺序
    hex_pairs.reverse()

    # 拼接成最终结果
    formatted_result = ' '.join(hex_pairs)

    return formatted_result


def number_count(decimal_number):
    # 计算结果
    result = decimal_to_hex_pairs(decimal_number)

    #print(f"Decimal {decimal_number} in formatted hex: {result}")
    return result



def calculate_checksum(hex_string):
    # 去掉十六进制字符串中的空格
    hex_string = hex_string.replace(' ', '')

    # 将十六进制字符串转换为字节数组
    byte_array = bytes.fromhex(hex_string)

    # 计算字节和
    checksum = sum(byte_array)

    # 取最低一个字节
    checksum = checksum & 0xFF

    # 取反并返回
    checksum = (~checksum) & 0xFF

    return checksum


def order_out(order):
    # 示例十六进制数据（不包括校验码）
    data_without_checksum = order

    # 计算校验码
    checksum = calculate_checksum(data_without_checksum)

    # 将校验码格式化为两位十六进制字符串
    checksum_hex = format(checksum, '02X')

    # print(f"Calculated checksum: {checksum_hex}")

    # 组合完整的指令，包括校验码
    complete_command = f"{'FF FF'} {data_without_checksum} {checksum_hex}"
    # print(f"Complete command with checksum: {complete_command}")
    return complete_command

def setup_serial(port, baudrate, timeout):
    return serial.Serial(port, baudrate, timeout=timeout)

def send_data(serial_port, data):
    # 将十六进制字符串转换为字节流
    hex_data = bytes.fromhex(data)
    print(f"Sending: {hex_data.hex().upper()}")
    serial_port.write(hex_data)  # 发送数据

def receive_data(serial_port):
    data = serial_port.readline()  # 读取一行数据
    if data:
        print(f"Received: {data.hex().upper()}")  # 打印接收到的字节流

def main():
    # 启动一个线程来读取用户输入
    input_thread = threading.Thread(target=read_input, daemon=True)
    input_thread.start()

    a = 2048
    judge = 0

    # 设置串口
    ser = setup_serial(PORT, BAUDRATE, TIMEOUT)

    try:
        while True:
            ##################################################################
            # 每秒检查一次输入队列
            if not input_queue.empty():
                user_input = input_queue.get()
                # print(f"Input received: {user_input}")
                judge = user_input
                # print(judge)

            else:
                # print(judge)
                judge = judge

            # 等待1秒
            #time.sleep(1)

            if judge == str(1):
                a += 20
                if a > 3020:
                    a = 3020
                print(a)
                count = number_count(a)
                order = '01 09 03 2A ' + count + ' 00 00 E8 03'
                x = order_out(order)
                print(x)
                # 发送十六进制数据
                send_data(ser, x)

                # 等待一会儿
                #time.sleep(2)

                # 接收数据
                #receive_data(ser)
            if judge == str(2):
                a = a
                print(a)
                count = number_count(a)
                order = '01 09 03 2A ' + count + ' 00 00 E8 03'
                x = order_out(order)
                print(x)
                # 发送十六进制数据
                send_data(ser, x)

                # 等待一会儿
                #time.sleep(2)

                # 接收数据
                receive_data(ser)
            if judge == str(3):
                a -= 20
                if a <500:
                    a = 500
                print(a)
                count = number_count(a)
                order = '01 09 03 2A ' + count + ' 00 00 E8 03'
                x = order_out(order)
                print(x)
                # 发送十六进制数据
                send_data(ser, x)

                # 等待一会儿
                #time.sleep(2)

                # 接收数据
                #receive_data(ser)
            ##################################################################
            # # 发送十六进制数据
            # send_data(ser, x)
            #
            # # 等待一会儿
            # time.sleep(2)
            #
            # # 接收数据
            # receive_data(ser)

    except KeyboardInterrupt:
        print("Interrupted by user")

    finally:
        ser.close()  # 关闭串口

if __name__ == "__main__":
    main()