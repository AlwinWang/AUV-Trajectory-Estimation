import cv2
import numpy as np


def convert_sparse3D_to_depth(kp, XYZ, height, width):
    """Convert sparse 3D keypoint to depth map
    Args:
        kp (Nx2): keypoints
        XYZ (3xN): 3D coorindates for the keypoints
        height (int): image height
        width (int): image width
    Returns:
        depth (HxW array): depth map
    """
    depth = np.zeros((height, width))
    kp_int = kp.astype(np.int)
    y_idx = (kp_int[:, 0] >= 0) * (kp_int[:, 0] < width)
    kp_int = kp_int[y_idx]
    x_idx = (kp_int[:, 1] >= 0) * (kp_int[:, 1] < height)
    kp_int = kp_int[x_idx]

    XYZ = XYZ[:, y_idx]
    XYZ = XYZ[:, x_idx]

    depth[kp_int[:, 1], kp_int[:, 0]] = XYZ[2]
    return depth


def triangulation(kp1, kp2, T_1w, T_2w):
    """Triangulation to get 3D points
    Args:
        kp1 (Nx2): keypoint in view 1 (normalized)
        kp2 (Nx2): keypoints in view 2 (normalized)
        T_1w (4x4): pose of view 1 w.r.t  i.e. T_1w (from w to 1)
        T_2w (4x4): pose of view 2 w.r.t world, i.e. T_2w (from w to 2)
    Returns:
        X (3xN): 3D coordinates of the keypoints w.r.t world coordinate
        X1 (3xN): 3D coordinates of the keypoints w.r.t view1 coordinate
        X2 (3xN): 3D coordinates of the keypoints w.r.t view2 coordinate
    """
    kp1_3D = np.ones((3, kp1.shape[0]))
    kp2_3D = np.ones((3, kp2.shape[0]))
    kp1_3D[0], kp1_3D[1] = kp1[:, 0].copy(), kp1[:, 1].copy()
    kp2_3D[0], kp2_3D[1] = kp2[:, 0].copy(), kp2[:, 1].copy()
    X = cv2.triangulatePoints(T_1w[:3], T_2w[:3], kp1_3D[:2], kp2_3D[:2])
    X /= X[3]
    X1 = T_1w[:3] @ X
    X2 = T_2w[:3] @ X
    return X[:3], X1, X2


def unprojection_kp(kp, kp_depth, cam_intrinsics):
    """Convert kp to XYZ
    Args:
        kp (Nx2 array): [x, y] keypoints
        kp_depth (Nx2 array): keypoint depth
        cam_intrinsics (Intrinsics): camera intrinsics
    Returns:
        XYZ (Nx3): 3D coordinates
    """
    N = kp.shape[0]
    XYZ = np.ones((N, 3, 1))
    XYZ[:, :2, 0] = kp
    
    inv_K = np.ones((1, 3, 3))
    inv_K[0] = cam_intrinsics.inv_mat
    inv_K = np.repeat(inv_K, N, axis=0)

    XYZ = np.matmul(inv_K, XYZ)[:, :, 0]
    XYZ[:, 0] = XYZ[:, 0] * kp_depth
    XYZ[:, 1] = XYZ[:, 1] * kp_depth
    XYZ[:, 2] = XYZ[:, 2] * kp_depth
    return XYZ
