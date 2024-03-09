from numpy import *
import numpy

# AInv - inverted matrix to A
# x - column vector
# i - column that will be replaced (indexing from 1)
def MyMatrixInversion(AInv, x, i):
    n = x.shape[0]
    
    # STEP 1
    l = AInv.dot(x)
    if l[i-1] == 0:
       print("unable to find inverted matrix")
       return
    #print(l) 

    # STEP 2
    lWave = numpy.array(l)
    lWave[i-1] = -1
    #print(lWave)
    #print(l)

    # STEP 3
    lTick = numpy.dot((-1/l[i-1]),lWave)
    #print(lTick)

    # STEP 4
    Q = numpy.eye(n)
    Q[:, i-1] = lTick
    #print(Q)

    AnswerMatrix = numpy.zeros((n,n))
    
    # STEP 5
    for j in range(0, n):
        for k in range (0, n):
            if (j == i-1):
                AnswerMatrix[j, k] = Q[j,j] * AInv [j, k]
            else:    
                AnswerMatrix[j, k] = Q[j,j] * AInv [j , k] + Q[j , i - 1] * AInv[i - 1,k]
            
    return AnswerMatrix
