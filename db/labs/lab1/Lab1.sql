DROP TABLE MyTable;

-- Задание 1
CREATE TABLE MyTable(
    id NUMBER PRIMARY KEY,
    val NUMBER NOT NULL
);
/

-- Задание 2
BEGIN
FOR i IN 1 .. 10 LOOP
	INSERT INTO MyTable values (i, ROUND(dbms_random.value(1,1000)));
END LOOP;
END;
/


--проверка
SELECT * from MyTable;
/

-- Задание 3
CREATE OR REPLACE FUNCTION CheckEvenOddCount
RETURN VARCHAR2
IS
    even_count NUMBER := 0;
    odd_count NUMBER := 0;
BEGIN
    -- Подсчет количества четных и нечетных значений
    FOR rec IN (SELECT val FROM MyTable)
    LOOP
        IF MOD(rec.val, 2) = 0 THEN
            even_count := even_count + 1;
        ELSE
            odd_count := odd_count + 1;
        END IF;
    END LOOP;

    -- Возвращаем результат на основе подсчета
    IF even_count > odd_count THEN
        RETURN 'TRUE';
    ELSIF even_count < odd_count THEN
        RETURN 'FALSE';
    ELSE
        RETURN 'EQUAL';
    END IF;
END;
/


--проверка
select CheckEvenOddCount() from dual
/

-- Задание 4
create or replace FUNCTION GenerateInsert(inputId integer) return string IS
    tableValue integer;
BEGIN
    SELECT val INTO tableValue FROM mytable WHERE id = inputId;

    return 'INSERT INTO MyTable(id, val) VALUES (' || inputId || ', ' || tableValue || ');';
END;
/


--проверка
select GenerateInsert(3) from dual
/

-- Задание 5

-- вставка
CREATE OR REPLACE PROCEDURE InsertIntoMyTable(
    p_id NUMBER,
    p_val NUMBER
)
IS
BEGIN
    INSERT INTO MyTable(id, val) VALUES (p_id, p_val);
END InsertIntoMyTable;
/

-- обновление
CREATE OR REPLACE PROCEDURE UpdateMyTable(
    p_id NUMBER,
    p_new_val NUMBER
)
IS
BEGIN
    UPDATE MyTable SET val = p_new_val WHERE id = p_id;
END UpdateMyTable;
/

-- удаление 
CREATE OR REPLACE PROCEDURE DeleteFromMyTable(
    p_id NUMBER
)
IS
BEGIN
    DELETE FROM MyTable WHERE id = p_id;
END DeleteFromMyTable;
/


-- проверка
EXEC InsertIntoMyTable(25, 100);
select * from mytable;
EXEC UpdateMyTable(25, 150);
select * from mytable;
EXEC DeleteFromMyTable(25);
select * from mytable;
/

--Задание 6
CREATE OR REPLACE FUNCTION CalculateTotalCompensation(
    p_monthly_salary NUMBER,
    p_annual_bonus_percentage NUMBER
)
RETURN NUMBER
IS
    v_annual_bonus_percentage NUMBER;
    v_total_compensation NUMBER;
    annual_bonus_percentage_exception EXCEPTION;
    monthly_salary_exception EXCEPTION;
BEGIN
    -- проверка процента годовых премиальных
    IF p_annual_bonus_percentage <= 0 OR p_annual_bonus_percentage > 100 THEN
        RAISE annual_bonus_percentage_exception;
    END IF;

    -- проверка месячной зарплаты
    IF p_monthly_salary <= 0 THEN
        RAISE monthly_salary_exception;
    END IF;    

    -- преобразование процента к дробному виду
    v_annual_bonus_percentage := p_annual_bonus_percentage / 100;

    -- вычисление общего вознаграждения
    v_total_compensation := (1 + v_annual_bonus_percentage) * 12 * p_monthly_salary;

    RETURN v_total_compensation;
EXCEPTION
    WHEN annual_bonus_percentage_exception THEN
        dbms_output.put_line('Некорректное значение процента годовых премиальных');
        RAISE annual_bonus_percentage_exception;
    WHEN monthly_salary_exception THEN
        dbms_output.put_line('Некорректное значение месячной зарплаты');
        RAISE monthly_salary_exception;
END CalculateTotalCompensation;
/

EXECUTE dbms_output.put_line(CalculateTotalCompensation(2, 8));
/