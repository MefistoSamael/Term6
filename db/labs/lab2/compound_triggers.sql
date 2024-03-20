create or replace  TRIGGER validate_student_id
FOR INSERT OR UPDATE ON Students
FOLLOWS generate_student_id
COMPOUND TRIGGER

  TYPE StudentIdList IS TABLE OF NUMBER INDEX BY PLS_INTEGER;
  IdList StudentIdList;

  BEFORE STATEMENT IS
  BEGIN
    IdList.DELETE;
  END BEFORE STATEMENT;

  BEFORE EACH ROW IS
  BEGIN
    IF INSERTING OR UPDATING THEN
      IdList(IdList.COUNT + 1) := :NEW.id;
    END IF;
  END BEFORE EACH ROW;

  AFTER STATEMENT IS
    IdCount NUMBER;
    DuplicateIdException EXCEPTION;

  BEGIN
    FOR i IN 1 .. IdList.COUNT LOOP
      SELECT COUNT(*)
      INTO IdCount
      FROM students
      WHERE id = IdList(i);

      IF IdCount > 1 THEN
        dbms_output.put_line('This student id exists ' || IdList(i));
        RAISE  DuplicateIdException;
      END IF;
    END LOOP;
  END AFTER STATEMENT;

END validate_student_id;
/


-- уникальность Id Groups
CREATE OR REPLACE TRIGGER validate_group_id
FOR INSERT OR UPDATE ON Groups
FOLLOWS generate_group_id
COMPOUND TRIGGER

TYPE GroupIdList IS TABLE OF NUMBER INDEX BY PLS_INTEGER;
v_group_id_list GroupIdList;

BEFORE STATEMENT IS
BEGIN
    v_group_id_list.DELETE;
END BEFORE STATEMENT;

BEFORE EACH ROW IS
BEGIN
    IF INSERTING OR UPDATING THEN
        v_group_id_list(v_group_id_list.COUNT + 1) := :NEW.id;
    END IF;
END BEFORE EACH ROW;

AFTER STATEMENT IS
    v_id_count NUMBER;
    DuplicateGroupIdException EXCEPTION;

BEGIN
    FOR i IN 1 .. v_group_id_list.COUNT LOOP
        SELECT COUNT(*)
        INTO v_id_count
        FROM Groups
        WHERE id = v_group_id_list(i);

        IF v_id_count > 1 THEN
            dbms_output.put_line('This group id exists ' || v_group_id_list(i));
            RAISE DuplicateGroupIdException;
        END IF;
    END LOOP;
END AFTER STATEMENT;

END validate_group_id;
/


--Уникальность имени группы
CREATE OR REPLACE TRIGGER validate_group_name
FOR INSERT OR UPDATE ON Groups
COMPOUND TRIGGER

TYPE GroupNamesList IS TABLE OF VARCHAR2(40) INDEX BY PLS_INTEGER;
v_group_names_list GroupNamesList;

BEFORE STATEMENT IS
BEGIN
    v_group_names_list.DELETE;
END BEFORE STATEMENT;

BEFORE EACH ROW IS
BEGIN
    IF INSERTING OR UPDATING THEN
        v_group_names_list(v_group_names_list.COUNT + 1) := :NEW.name;
    END IF;
END BEFORE EACH ROW;

AFTER STATEMENT IS
    v_name_count NUMBER;
    DuplicateGroupNameException EXCEPTION;

BEGIN
    FOR i IN 1 .. v_group_names_list.COUNT LOOP
        SELECT COUNT(*)
        INTO v_name_count
        FROM Groups
        WHERE name = v_group_names_list(i);

        IF v_name_count > 1 THEN
            dbms_output.put_line('This group name exists ' || v_group_names_list(i));
            RAISE DuplicateGroupNameException;
        END IF;
    END LOOP;
END AFTER STATEMENT;

END validate_group_name;
/