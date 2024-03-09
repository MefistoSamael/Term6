import numpy as np
from Lab2 import MySimplexMethodMainPhase



if __name__ == "__main__":
    A = np.array([[-1, 1, 1, 0, 0],
                  [1, 0, 0, 1, 0],
                  [0, 1, 0, 0, 1]])
    
    c = np.array([1, 1, 0, 0, 0])
    x = np.array([0, 0, 1, 3, 2])
    B = [3, 4, 5]
    
    result = MySimplexMethodMainPhase(c,x,A,B)
    
    print(result)
