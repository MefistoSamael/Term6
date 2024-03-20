--Задание 1
CREATE TABLE Students(
    id NUMBER NOT NULL,
    name VARCHAR2(30) NOT NULL,
    group_id NUMBER NOT NULL);
/

CREATE TABLE Groups(
    id NUMBER NOT NULL,
    name VARCHAR2(40) NOT NULL,
    c_val NUMBER NOT NULL);
/


--Задание 2
CREATE SEQUENCE student_id_sequence
START WITH 1
INCREMENT BY 1;
/

CREATE OR REPLACE TRIGGER generate_student_id
BEFORE INSERT ON Students
FOR EACH ROW
BEGIN
    IF :new.id IS NULL THEN
        :new.id := student_id_sequence.nextval;
    END IF;
END;
/


CREATE SEQUENCE group_id_sequence
START WITH 1
INCREMENT BY 1;
/

CREATE OR REPLACE TRIGGER generate_group_id
BEFORE INSERT ON Groups
FOR EACH ROW
BEGIN
    IF :new.id IS NULL THEN
        :new.id := group_id_sequence.nextval;
    END IF;
END;
/

CREATE TABLE cascade (
    id NUMBER PRIMARY KEY,
    is_cascade BOOLEAN
);
/

INSERT INTO cascade VALUES (1, FALSE);
/

--Задание 3
CREATE OR REPLACE TRIGGER fk_group_delete_cascade
 AFTER DELETE ON Groups
 FOR EACH ROW
BEGIN
    UPDATE cascade SET is_cascade = TRUE WHERE id = 1;
    DELETE FROM Students WHERE group_id = :OLD.id;
    UPDATE cascade SET is_cascade = FALSE WHERE id = 1;
END;
/


--Задание 4
CREATE TABLE student_journal (
    id NUMBER PRIMARY KEY,
    operation VARCHAR2(10),
    op_time TIMESTAMP,
    s_id NUMBER,
    s_name VARCHAR2(30),
    s_group_id NUMBER,
    n_s_id NUMBER,
    n_s_name VARCHAR2(30),
    n_s_group_id NUMBER
);
/

CREATE SEQUENCE stud_journal_id_seq
START WITH 1
INCREMENT BY 1;
/

CREATE OR REPLACE TRIGGER students_journal
 AFTER INSERT OR UPDATE OR DELETE ON Students
 FOR EACH ROW
BEGIN
    IF INSERTING THEN
        INSERT INTO student_journal VALUES (stud_journal_id_seq.nextval, 'INSERT', CURRENT_TIMESTAMP, :NEW.id, :NEW.name, :NEW.group_id, NULL, NULL, NULL);
    END IF;

    IF UPDATING THEN
        INSERT INTO student_journal VALUES (stud_journal_id_seq.nextval, 'UPDATE', CURRENT_TIMESTAMP, :OLD.id, :OLD.name, :OLD.group_id, :NEW.id, :NEW.name, :NEW.group_id);
    END IF;
        
    IF DELETING THEN
        INSERT INTO student_journal VALUES (stud_journal_id_seq.nextval, 'DELETE', CURRENT_TIMESTAMP, :OLD.id, :OLD.name, :OLD.group_id, NULL, NULL, NULL);
    END IF;
END;
/


--Задание 5
CREATE OR REPLACE PROCEDURE rollback_students(r_time TIMESTAMP) IS
BEGIN
    FOR action IN (SELECT * FROM student_journal WHERE r_time < op_time ORDER BY id DESC)
    LOOP
        CASE action.operation
            WHEN 'INSERT' THEN
                DELETE Students WHERE id = action.s_id;

            WHEN 'UPDATE' THEN
                UPDATE Students
                SET id = action.s_id,
                    name = action.s_name,
                    group_id = action.s_group_id
                WHERE id = action.n_s_id;

            WHEN 'DELETE' THEN
                INSERT INTO Students VALUES (action.s_id, action.s_name, action.s_group_id);
        END CASE;
    END LOOP;
END;
/

CREATE OR REPLACE NONEDITIONABLE PROCEDURE rollback_students_by_offset(offset NUMBER) IS
BEGIN
    rollback_students(TO_TIMESTAMP(CURRENT_TIMESTAMP - INTERVAL '1'MINUTE * offset ));
END;
/


--Задание 6
CREATE OR REPLACE TRIGGER c_val_trigger
AFTER INSERT OR UPDATE OR DELETE ON Students
FOR EACH ROW
DECLARE
    v_is_cascade BOOLEAN;
BEGIN 
    IF UPDATING THEN
        UPDATE Groups
        SET c_val = c_val + 1
        WHERE id = :NEW.group_id;

        UPDATE Groups
        SET c_val = c_val - 1
        WHERE id = :OLD.group_id;
    ELSIF INSERTING THEN
        UPDATE Groups
        SET c_val = c_val + 1
        WHERE id = :NEW.group_id;
    ELSIF DELETING THEN
        SELECT is_cascade INTO v_is_cascade FROM cascade WHERE id = 1;

        IF v_is_cascade THEN
            RETURN;
        ELSE
            UPDATE Groups
            SET c_val = c_val - 1
            WHERE id = :OLD.group_id;
        END IF;
    END IF;
END;
/

DELETE FROM Students;
DELETE FROM Groups;
DELETE FROM student_journal;


INSERT INTO Groups(id, name, c_val) VALUES (1, '153502', 0);
INSERT INTO Groups(id, name, c_val) VALUES (2, '153503', 0);
INSERT INTO Groups(name, c_val) VALUES ( '153504', 0);
SELECT * FROM Groups;

INSERT INTO Students(id, name, group_id) VALUES (1, 'Anton', 1);
INSERT INTO Students(name, group_id) VALUES ('Boris', 1);
INSERT INTO Students(name, group_id) VALUES ('Vlad', 2);
SELECT * FROM Students;

--проверка невозможности вставки группы с тем же айди
INSERT INTO Groups(id, name, c_val) VALUES (1, '153505', 0);

--проверка невозможности вставки группы с тем же именем
INSERT INTO Groups(name, c_val) VALUES ('153502', 0);


--проверка уникальности id студента
INSERT INTO Students(id, name, group_id) VALUES (1, 'Vassya', 1);

--проверка каскадного удаления
delete from groups where name = '153503';
SELECT * FROM Students;

--проверка журналирования
SELECT * FROM student_journal;

--проверка возврата по временной метке
call rollback_students(TO_TIMESTAMP ('23.02.24 14:10:10.123000', 'DD.MM.RRRR HH24:MI:SS.FF'));

--проверка возврата по смещению
call rollback_students_by_offset(сам вставь);

DELETE FROM Students;
DELETE FROM Groups;
DELETE FROM student_journal;


