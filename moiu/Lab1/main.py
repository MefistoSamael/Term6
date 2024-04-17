import numpy as np
from Lab3 import MySimplexMethodStartPhase
from Lab4 import dual_simplex_method



if __name__ == "__main__":
    A = np.array([[-2,-1,-4,1,0 ],
                  [-2,-2,-2,0,1]])
    
    c = np.array([-4,-3,-7,0,0])
    b = np.array([-1, -3/2])
    B = [4,5]
    
    
    result = dual_simplex_method(c, A, b, B)
    print("\n\n\n result")
    print(result)
