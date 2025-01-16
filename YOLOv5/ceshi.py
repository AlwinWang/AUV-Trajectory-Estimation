import pyzed.sl as sl
import torch
import cv2
import numpy as np
from pathlib import Path
from models.experimental import attempt_load
from utils.general import non_max_suppression, scale_coords, xyxy2xywh
from utils.torch_utils import select_device

# 初始化ZED相机
zed = sl.Camera()
init_params = sl.InitParameters()
init_params.depth_mode = sl.DEPTH_MODE.PERFORMANCE  # 设置深度模式
init_params.coordinate_units = sl.UNIT.METER  # 设置单位为米
init_params.camera_resolution = sl.RESOLUTION.HD1080  # 设置相机分辨率为2K
zed.open(init_params)

# 设置设备
device = select_device('0')  # '0' 表示使用第一个 GPU，如果没有 GPU 则使用 CPU

# 加载本地权重文件
weights_path = '/home/nvidia/下载/yolov5-5.0/yolov5-5.0/903bast.pt'  # 替换为本地权重文件的路径
model = attempt_load(weights_path)  # 加载 YOLOv5 模型
model = model.to(device)
names = model.module.names if hasattr(model, 'module') else model.names  # 获取类别名称

# 创建图像和深度测量对象
image = sl.Mat()
depth = sl.Mat()

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
        pred = non_max_suppression(pred, 0.25, 0.45)  # 应用 NMS

        # 在图像上绘制检测框和深度信息
        for det in pred:
            if len(det):
                det[:, :4] = scale_coords(img.shape[2:], det[:, :4], frame.shape).round()
                for *xyxy, conf, cls in det:
                    x1, y1, x2, y2 = map(int, xyxy)
                    label = f'{names[int(cls)]} {conf:.2f}'

                    # 获取中心点的深度信息
                    cx, cy = (x1 + x2) // 2, (y1 + y2) // 2
                    err, depth_value = depth.get_value(cx, cy)
                    if err == sl.ERROR_CODE.SUCCESS:
                        depth_label = f'Depth: {depth_value:.2f}m'
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

# 释放ZED相机并关闭窗口
zed.close()
cv2.destroyAllWindows()



###########################################################################################################
###########################################################################################################
###########################################################################################################


# import pyzed.sl as sl
# import torch
# import cv2
# import numpy as np
# import serial  # 导入串口通信库
# from pathlib import Path
# from models.experimental import attempt_load
# from utils.general import non_max_suppression, scale_coords, xyxy2xywh
# from utils.torch_utils import select_device
#
# # 初始化ZED相机
# zed = sl.Camera()
# init_params = sl.InitParameters()
# init_params.depth_mode = sl.DEPTH_MODE.PERFORMANCE  # 设置深度模式
# init_params.coordinate_units = sl.UNIT.METER  # 设置单位为米
# init_params.camera_resolution = sl.RESOLUTION.HD2K  # 设置相机分辨率为2K
# zed.open(init_params)
#
# # 设置设备
# device = select_device('0')  # '0' 表示使用第一个 GPU，如果没有 GPU 则使用 CPU
#
# # 加载本地权重文件
# weights_path = r'D:\python data\yolov5-5.0\yolov5-5.0\903bast.pt'  # 替换为本地权重文件的路径
# model = attempt_load(weights_path)  # 加载 YOLOv5 模型
# model = model.to(device)
# names = model.module.names if hasattr(model, 'module') else model.names  # 获取类别名称
#
# # 创建图像和深度测量对象
# image = sl.Mat()
# depth = sl.Mat()
#
# # 初始化串口通信
# ser = serial.Serial('COM1', 9600, timeout=1)  # 替换为实际串口号和波特率
#
# while True:
#     if zed.grab() == sl.ERROR_CODE.SUCCESS:
#         zed.retrieve_image(image, sl.VIEW.LEFT)  # 获取左视图图像
#         zed.retrieve_measure(depth, sl.MEASURE.DEPTH)  # 获取深度信息
#
#         # 将图像转换为OpenCV格式
#         frame = image.get_data()
#         frame = cv2.cvtColor(frame, cv2.COLOR_RGBA2RGB)
#
#         # 调整图像大小为32的倍数
#         img = cv2.resize(frame, (640, 352))
#         img = torch.from_numpy(img).to(device).float() / 255.0  # 转换为 Tensor 并归一化
#         img = img.permute(2, 0, 1).unsqueeze(0)  # 转换为 NCHW 格式
#
#         with torch.no_grad():  # 禁用梯度计算
#             pred = model(img)[0]  # 预测
#         pred = non_max_suppression(pred, 0.25, 0.45)  # 应用 NMS
#
#         # 在图像上绘制检测框和深度信息，并通过串口发送数据
#         for det in pred:
#             if len(det):
#                 det[:, :4] = scale_coords(img.shape[2:], det[:, :4], frame.shape).round()
#                 for *xyxy, conf, cls in det:
#                     x1, y1, x2, y2 = map(int, xyxy)
#                     label = f'{names[int(cls)]} {conf:.2f}'
#
#                     # 获取中心点的深度信息
#                     cx, cy = (x1 + x2) // 2, (y1 + y2) // 2
#                     err, depth_value = depth.get_value(cx, cy)
#                     if err == sl.ERROR_CODE.SUCCESS:
#                         depth_label = f'Depth: {depth_value:.2f}m'
#
#                         # 将中心坐标、物体类别名称和深度信息通过串口发送出去
#                         object_name = names[int(cls)]  # 获取物体名称
#                         data = f'{cx},{cy},{object_name},{depth_value:.2f}\n'
#                         ser.write(data.encode())
#                     else:
#                         depth_label = 'Depth: N/A'
#
#                     # 绘制检测框和标签
#                     cv2.rectangle(frame, (x1, y1), (x2, y2), (0, 255, 0), 2)
#                     cv2.putText(frame, label, (x1, y1 - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)
#                     cv2.putText(frame, depth_label, (x1, y2 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.5, (0, 255, 0), 2)
#
#         # 显示结果
#         cv2.imshow('YOLOv5 ZED Real-time Detection', frame)
#
#     # 按下 'q' 键退出
#     if cv2.waitKey(1) & 0xFF == ord('q'):
#         break
#
# # 关闭串口通信
# ser.close()
#
# # 释放ZED相机并关闭窗口
# zed.close()
# cv2.destroyAllWindows()
