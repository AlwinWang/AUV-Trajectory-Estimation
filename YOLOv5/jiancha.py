import os
import xml.etree.ElementTree as ET


def extract_categories_from_xml(xml_folder):
    categories = set()

    # 遍历文件夹中的所有 XML 文件
    for filename in os.listdir(xml_folder):
        if filename.endswith('.xml'):
            filepath = os.path.join(xml_folder, filename)
            tree = ET.parse(filepath)
            root = tree.getroot()

            # 提取物品类别
            for obj in root.findall('object'):
                name = obj.find('name').text
                categories.add(name)

    return categories


# 设置 XML 文件夹路径
xml_folder_path = r'D:\python data\yolov5-master\yolov5-master\data\mydata\Annotations'

# 提取并打印物品类别
categories = extract_categories_from_xml(xml_folder_path)
print(f'发现的物品类别有：{categories}')
print(f'类别总数：{len(categories)}')
