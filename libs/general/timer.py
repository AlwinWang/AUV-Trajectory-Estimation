import numpy as np
from time import time


class Timers():
    def __init__(self, items=None):
        self.timers = {}
        if items is not None:
            self.add(items)

    def add(self, item):
        """add item to the timer
        Args:
            item (str/list): item name
        """
        if isinstance(item, list):
            for i in item:
                self.timers[i] = []
        elif isinstance(item, str):
            self.timers[item] = []
        else:
            assert False, "only list or str is accepted."
