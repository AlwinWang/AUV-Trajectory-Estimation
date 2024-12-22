import argparse
import cv2
import matplotlib.pyplot as plt
import numpy as np
import os

from Estimation_modules import VisualEstimation as ES
from libs.general.utils import *
from libs.utils import load_kitti_odom_intrinsics

""" Argument Parsing """
parser = argparse.ArgumentParser(description='Trajectory')
parser.add_argument("-s", "--seq", type=str,
                    default=None, help="sequence")
parser.add_argument("-c", "--configuration", type=str,
                    default=None,
                    help="custom configuration file")
parser.add_argument("-d", "--default_configuration", type=str,
                    default="options/kitti/kitti_default_configuration.yml",
                    help="default configuration files")
args = parser.parse_args()

""" Read configuration """
default_config_file = args.default_configuration
config_files = [default_config_file, args.configuration]
cfg = merge_cfg(config_files)
if args.seq is not None:
    cfg.seq = args.seq
cfg.seq = str(cfg.seq)

continue_flag = input("Save result in {}? [y/n]".format(cfg.result_dir))
if continue_flag == "y":
    mkdir_if_not_exists(cfg.result_dir)
else:
    exit()

""" basic setup """
SEED = cfg.seed
np.random.seed(SEED)

""" Main """
es = ES(cfg)
es.setup()
es.main()

cfg_path = os.path.join(cfg.result_dir, "configuration_{}.yml".format(cfg.seq))
save_cfg(config_files, file_path=cfg_path)
