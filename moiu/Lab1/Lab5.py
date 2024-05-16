import numpy as np

def first_phase(a: np.ndarray, b: np.ndarray, c: np.ndarray) -> tuple[np.ndarray, list, np.ndarray, np.ndarray, np.ndarray]:
    # Balance the supply and demand
    a_total, b_total = np.sum(a), np.sum(b)
    if a_total != b_total:
        difference = a_total - b_total
        if difference > 0:
            b = np.append(b, difference)
            c = np.hstack((c, np.zeros((len(b), 1))))
        else:
            a = np.append(a, -difference)
            c = np.vstack((c, np.zeros((len(a), 1))))
    
    n, m = len(a), len(b)
    X = np.zeros((n, m))
    i, j = 0, 0
    B = []

    while i < n and j < m:
        if b[j] > a[i]:
            X[i, j] = a[i]
            b[j] -= a[i]
            a[i] = 0
            i += 1
        else:
            X[i, j] = b[j]
            a[i] -= b[j]
            b[j] = 0
            j += 1

        B.append((i, j))

    return X, B, a, b, c

def potentials_method(a: np.ndarray, b: np.ndarray, c: np.ndarray) -> np.ndarray:
    X, B, a, b, c = first_phase(a, b, c)
    n, m = len(a), len(b)

    while True:
        x, y = [], []
        for i, j in B:
            u, v = [0] * n, [0] * m
            u[i], v[j] = 1, 1
            x.append(u + v)
            y.append(c[i, j])

        x.append([1] + [0] * (n + m - 1))
        y.append(0)

        result = np.linalg.solve(x, y)
        u, v = result[:n], result[n:]

        new_position = None
        for i in range(n):
            for j in range(m):
                if (i, j) not in B and u[i] + v[j] > c[i, j]:
                    new_position = (i, j)
                    break
            if new_position:
                break

        if not new_position:
            return X
        
        B.append(new_position)

        corner_B = B[:]
        while True:
            changes = False
            for i in range(n):
                connected = [j for j in range(m) if (i, j) in corner_B]
                if len(connected) <= 1:
                    for j in connected:
                        corner_B.remove((i, j))
                        changes = True

            for j in range(m):
                connected = [i for i in range(n) if (i, j) in corner_B]
                if len(connected) <= 1:
                    for i in connected:
                        corner_B.remove((i, j))
                        changes = True

            if not changes:
                break

        marked_B = {position: None for position in corner_B}
        marked_B[new_position] = True
        add_plus_or_minus(new_position, marked_B)

        theta = min(X[i, j] for i, j in marked_B if not marked_B[(i, j)])
        for i, j in marked_B:
            if marked_B[(i, j)]:
                X[i, j] += theta
            else:
                X[i, j] -= theta

        B.remove(next((pos for pos in marked_B if not marked_B[pos]), None))

def add_plus_or_minus(position: tuple[int, int], B: dict) -> None:
    for i, j in list(B.keys()):
        if (position[0] == i or position[1] == j) and B[(i, j)] is None:
            B[(i, j)] = not B[position]
            add_plus_or_minus((i, j), B)