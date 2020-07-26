
import os
import numpy as np
import matplotlib.pyplot as plt
dirname = os.path.dirname(__file__)
RAW_PATH = os.path.join(dirname,'raw.txt')
DETECTED_PATH = os.path.join(dirname,'detected.txt')

with open(RAW_PATH, 'r') as raw_file:
    with open(DETECTED_PATH, 'r') as detected_file:

        raw_speed = np.loadtxt(raw_file, dtype=np.float32, delimiter=',')
        detected_speed = np.loadtxt(detected_file, dtype=np.float32, delimiter=',')

        raw_start_ind = next(
            i for i, x in enumerate(raw_speed[:,1]) if float(x) > 1)
        detected_start_ind = next(
            i for i, x in enumerate(detected_speed[:,1]) if float(x) > 1)

        raw_length = raw_speed.shape[0] - raw_start_ind
        detected_length = detected_speed.shape[0] - detected_start_ind

        raw_speed = raw_speed[raw_start_ind:]
        detected_speed = detected_speed[detected_start_ind:]
        detected_end = next(i for i, x in enumerate(detected_speed[::-1,0]) if x <= raw_speed[-1,0])
        detected_speed = detected_speed[:-detected_end-1]

        plt.plot(raw_speed[:,0],raw_speed[:,1])
        plt.plot(detected_speed[:,0], detected_speed[:,1])
        plt.show()
