import numpy as np
from Lab3 import MySimplexMethodStartPhase



if __name__ == "__main__":
    A = np.array([[1, 1, 1, ],
                  [2, 2, 2]])
    
    c = np.array([1, 0, 0])
    b = np.array([-1, 0])
    
    x, B, A, b = MySimplexMethodStartPhase(c,A,b)
    
    print(x)
    print(B)
    print(A)
    print(b)
