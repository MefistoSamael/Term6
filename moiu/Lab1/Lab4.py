import numpy as np

from Lab1 import MyMatrixInversion



def dual_simplex_method(c: np.ndarray, A: np.ndarray, b:np.ndarray, B:list):
    firstLap = True
    
    zz = 1
    m, n = A.shape
    while True:
        print(f"------------Lap number {zz}")
        N = [i for i in range(1, n+1) if not i in B]
        
        # step 1
        A_B = A[:, np.array(B) - 1]
        if firstLap:
            A_I_B = np.linalg.inv(A_B)
        else:
            A_I_B = MyMatrixInversion(A_I_B, A[:, int(j_0) - 1], k + 1)
        print("A_B")
        print(A_B)
        print("A_I_B")
        print(A_I_B)
        
        #step 2
        c_B = c[np.array(B) - 1]
        print("c_B")
        print(c_B)
        
        #step 3
        y_T = c_B.T.dot(A_I_B)
        print("y_T")
        print(y_T)
        
        #step 4
        kp_B = A_I_B.dot(b)
        
        kp = np.zeros(n)
        zu = 0
        for i in range(1, n+1):
            if i in B:
                kp[B[zu] - 1] = kp_B[zu]
                zu+=1
        
        print("kp")
        print(kp)
        
        print("kp_B")
        print(kp_B)
        
        # step 5
        if np.all(kp >= 0):
            return kp
        
        #step 6
        #j_k = np.where(kp < 0)[0][0] ниже - для теста
        j_k = np.where(kp < 0)[0][-1]
        
        k = B.index(j_k + 1)
        print("j_k")
        print(j_k + 1)
        print("k")
        print(k + 1)
        
        #step 7
        d_y = A_I_B[k, :]
        mu = [(j, d_y.T.dot(A[:, j - 1])) for j in N]
        mu = np.array(mu)
        print("d_y")
        print(d_y)
        print("mu")
        print(mu)
        
        #step 8
        if np.all(mu >=0):
            raise Exception("problem is not feasible")
        
        #step 9
        sigma = [(j, (c[int(j - 1)] - A[:, int(j - 1)].T.dot(y_T)) / mu_j) for j,mu_j in mu if mu_j < 0]
        print("sigma")
        print(sigma)
        
        #step 10
        j_0, _ = min(sigma, key=lambda x: x[1])
        print("j_0")
        print(j_0)
        
        #step 11
        B[k] = int(j_0)
        
        print("B")
        print(B)


        firstLap = False
        zz+=1




