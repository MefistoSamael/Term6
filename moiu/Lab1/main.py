from itertools import product
from copy import deepcopy
from collections import Counter
import numpy as np


def print_if_main(*args):
    if __name__ == "__main__":
        print(*args)


def print_graph(a, b, i=None, j=None):
    print_if_main("\nТекущая ситуация в пунктах производства и потребления:")

    str_a = '\t'.join([f"a{i + 1} • {ai}" for i, ai in enumerate(a)])
    str_b = '\t'.join([f"b{i + 1} • {bi}" for i, bi in enumerate(b)])
    if i is not None:
        str_a += f"\t|\ti = {i}"
    if j is not None:
        str_b += f"\t|\tj = {j}"
    str_a += f"\t\t (пункты производства)"
    str_b += f"\t\t (пункты потребления)"
    print_if_main(f"{str_a}\n\n"
                  f"{str_b}")


def balance_condition(a, b, c):
    print_if_main("Проверка условия баланса:\n")
    if (diff := sum(a) - sum(b)) > 0:
        print_if_main(f"Σa[i] > Σb[j]. Добавляем фиктивный пункт потребления b{len(b) + 1} = {diff}.")
        b_new = np.append(b, diff)
        a_new = a
        c_new = np.append(c, [[0] * c.shape[0]], axis=1)
    elif diff < 0:
        print_if_main(f"Σa[i] < Σb[j]. Добавляем фиктивный пункт производства a{len(a) + 1} = {diff}.")
        b_new = b
        a_new = np.append(a, -diff)
        c_new = np.append(c, [[0] * c.shape[1]], axis=0)
    else:
        print_if_main(f"Σa[i] = Σb[j]. Условие соблюдается.")
        a_new = a
        b_new = b
        c_new = c
    return a_new, b_new, c_new


def north_west_corner(a, b):
    print_if_main("________________________________________")
    print_if_main("\nФАЗА 1. (метод северо-западного угла):")
    m, n = len(a), len(b)
    X = np.zeros((m, n))
    B = []
    B_str = ', '.join(B)
    print_if_main(f"\nЗадаем нулевую матрицу плана перевозок размера {m}x{n}:"
                  f"\nX = \n{X}\nB = {'{'}{B_str}{'}'}")

    i, j = 1, 1
    step = 1
    while True:
        print_graph(a, b, i, j)
        B_old = B.copy()
        B.append((i, j))
        print_if_main("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
        print_if_main(f"Шаг {step}. Помещаем позицию (i, j) = ({i}, {j}) в B."
                      f"\n\tB = {B_old} -> B = {B}")
        print_if_main(f"\tВ a{i} = {a[i - 1]} единиц.\n"
                      f"\tВ b{j} = {b[j - 1]} единиц.")

        if a[i - 1] - b[j - 1] <= 0:
            print_if_main(f"\tОтправляем из a{i} в b{j} {a[i - 1]} единиц.")
            diff = a[i - 1]
            b[j - 1] -= diff
            a[i - 1] = 0
            X[i - 1][j - 1] = diff
        else:
            print_if_main(f"\tОтправляем из a{i} в b{j} {b[j - 1]} единиц.")
            diff = b[j - 1]
            a[i - 1] -= diff
            b[j - 1] = 0
            X[i - 1][j - 1] = diff

        if a[i - 1] == 0:
            if i + 1 <= m:
                i += 1
                print_if_main(f"\tПункт а{i - 1} пуст. Переводим указатели (i, j) = ({i - 1}, {j}) -> "
                              f"(i, j) = ({i}, {j})")
        if b[j - 1] == 0:
            if j + 1 <= n:
                j += 1
                print_if_main(f"\tПункт b{j - 1} заполнен. Переводим указатели (i, j) = ({i}, {j - 1}) -> "
                              f"(i, j) = ({i}, {j})")

        print_if_main(f"X = \n{X}")
        if i == m and a[i - 1] == 0 and j == n and b[i - 1] == 0:
            print_graph(a, b, i, j)
            print_if_main("Оба указателя указывают на нулевые пункты. "
                          "Правее ни один из указателей сдвинуть нельзя.\n"
                          "Завершение метода...")
            break
        step += 1

    return X, B


def potential_method(X, B, c):
    print_if_main("________________________________________")
    print_if_main("ФАЗА 2. (метод потенциалов):")
    m, n = len(c), len(c[0])
    u_str = ', '.join([f"u{i}" for i in range(1, m + 1)])
    v_str = ', '.join([f"v{i}" for i in range(1, n + 1)])
    print_if_main("\nЗададим переменные потенциалы\n"
                  f"{u_str} - для пунктов производства\n"
                  f"{v_str} - для пунктов потребления")

    iteration = 1
    while True:
        print_if_main("- - - - - - - - - - - - - - - - - - - - - - - -")
        print_if_main(f"ИТЕРАЦИЯ #{iteration}\n")
        print_if_main("Составим систему уравнений вида\n"
                      "\tu[i] + v[j] = c[i][j], где (i, j) ∊ B")

        sys_str = ''
        sys = [[0 for i in range(m + n)] for j in range(m + n)]
        c_vals = [0 for i in range(m + n)]
        for step, (i, j) in enumerate(B):
            sys_str += f"\t\tu{i} + v{j} = {c[i - 1][j - 1]}\n"
            sys[step][i - 1] = 1
            sys[step][m + j - 1] = 1
            c_vals[step] = c[i - 1][j - 1]
        sys[-1][0] = 1
        c_vals[-1] = 0
        sys_str += f"\t\tu1 = 0\n"
        print_if_main(f"Полученная система: \n{sys_str}")
        sol = np.linalg.solve(sys, c_vals)
        u = sol[:m]
        v = sol[m:]
        u_sol_str = ', '.join(f"u{i + 1} = {val}" for i, val in enumerate(u))
        v_sol_str = ', '.join(f"v{i + 1} = {val}" for i, val in enumerate(v))
        print_if_main(f"Решение системы:\n{u_sol_str}\n{v_sol_str}")

        print_if_main("\nПроверяем условие оптимальности текущего базисного плана\n"
                      "∀(i, j) ∈ N проверяем неравенство u[i] + v[j] <= c[i][j]:")
        i_s = list(range(1, m + 1))
        j_s = list(range(1, n + 1))
        all_positions = list(product(i_s, j_s))
        non_basis_positions = [pair for pair in all_positions if pair not in B]

        optimal = True
        i, j = None, None
        for i, j in non_basis_positions:
            if u[i - 1] + v[j - 1] <= c[i - 1][j - 1]:
                print_if_main(f"для ({i}, {j}): u{i} + v{j} = {u[i - 1]} + {v[j - 1]} "
                              f"<= c[{i}][{j}] = {c[i - 1][j - 1]}")
            else:
                optimal = False
                print_if_main(f"для ({i}, {j}): u{i} + v{j} = {u[i - 1]} + "
                              f"{v[j - 1]} > c[{i}][{j}] = {c[i - 1][j - 1]}")
                print_if_main("Условие не выполняется. Дальше можно не проверять.")
                i, j = i, j
                break

        if optimal:
            print_if_main("Неравенство выполняется ∀(i, j) ∈ N.\nТекущий план перевозок X является оптимальным!")
            return X, B
        else:
            print_if_main("Текущий план перевозок не является оптимальным. Продолжаем...\n")

        print_if_main(f"Делаем базисной первую позицию (i, j) = ({i}, {j}), "
                      f"на которой не выполнилось условие оптимальности.")
        B_old = B.copy()
        B.append((i, j))
        print_if_main(f"\nB = {B_old} -> B = {B}")

        X_temp = deepcopy(X)
        B_temp = B.copy()

        B_temp = deepcopy(B)
        while True:
            i_list = [i for (i, j) in B_temp]
            j_list = [j for (i, j) in B_temp]

            i_counter = Counter(i_list)
            j_counter = Counter(j_list)

            i_del = [i for i in i_counter if i_counter[i] == 1]
            j_del = [j for j in j_counter if j_counter[j] == 1]

            if not i_del and not j_del:
                break
            B_temp = [(i, j) for (i, j) in B_temp if i not in i_del
                      and j not in j_del]

        print_if_main("\nОставляем среди базисных вершин плана перевозок Х только угловые вершины. Их позиции"
                      "\nВ = ", B_temp)

        B_copy = deepcopy(B_temp)
        plus, minus = [], []
        plus.append(B_copy.pop())

        while B_copy:
            if len(plus) > len(minus):
                for index, (i, j) in enumerate(B_copy):
                    if plus[-1][0] == i or plus[-1][1] == j:
                        minus.append(B_copy.pop(index))
                        break
            else:
                for index, (i, j) in enumerate(B_copy):
                    if minus[-1][0] == i or minus[-1][1] == j:
                        plus.append(B_copy.pop(index))
                        break

        signs = [(pair, '+') if pair in plus else (pair, '-') for pair in B_temp]
        print_if_main("\nПроставляем у оставшихся базисных вершин знаки +/-:\n"
                      f"Позиции и их знаки:\n {signs}")

        minuses = [X[i - 1][j - 1] for (i, j), sign in signs if sign == '-']
        print_if_main(f"\nРассмотрим вершины с минусами: {minuses}")

        theta = min(minuses)
        print_if_main(f"Выберем среди них минимальное число θ:\nθ = {theta}")

        print_if_main("Выполним операции сложения/вычитания (в соответствии со знаком) "
                      "между угловыми вершинами и числом θ:")

        for (i, j), sign in signs:
            if sign == '+':
                X[i - 1][j - 1] += theta
            else:
                X[i - 1][j - 1] -= theta

        print_if_main(f"X =\n{X}")

        i_del, j_del = None, None
        for (i, j) in B:
            if X[i - 1][j - 1] == 0:
                i_del, j_del = i, j
                break

        print_if_main(f"Удалим из набора базисных ту позицию, "
                      f"которая после преобразования стала нулевой ( {i_del, j_del} ):")

        B_old = B.copy()
        B.remove((i_del, j_del))
        print_if_main(f"B = {B_old} -> B = {B}")
        print_if_main("\nВозвращаемся в начало...")

        iteration += 1


def matrix_transportation_problem(a, b, c):
    a, b, c = balance_condition(a, b, c)
    X, B = north_west_corner(a, b)
    X, B = potential_method(X, B, c)
    return X, B


if __name__ == "__main__":
    a = np.array([100, 300, 300])
    b = np.array([300, 200, 200])
    c = np.array([[8, 4, 1],
                  [8, 4, 3],
                  [9, 7, 5]])

    a = np.array([50, 50, 100])
    b = np.array([40, 90, 70])
    c = np.array([[2, 5, 3],
                  [4, 3, 2],
                  [5, 1, 2]])

    ans = matrix_transportation_problem(a, b, c)
    cost = 0
    for i in range(len(c)):
        for j in range(len(c[i])):
            cost += ans[0][i][j] * c[i][j]
    print(f"\n\n\nОТВЕТ!!!\nОптимальный план перевозок: X = \n{ans[0]}\nB = {ans[1]}." if ans is not None else
          "Что-то пошло не так! :(")
    print(f"Итоговая стоимость перевозок = {cost}")
