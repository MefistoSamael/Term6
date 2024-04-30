-- TestTable2.datetime -> TestTable2.datetime_field
-- LoggingForTestTable2.new_datetime -> LoggingForTestTable2.new_datetime_field
-- LoggingForTestTable2.old_datetime -> LoggingForTestTable2.old_datetime_field


-- 1
CREATE TABLE TestTable1 (
    id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    MY_value NUMBER NOT NULL
);

CREATE TABLE TestTable2 (
    id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    MY_datetime_field TIMESTAMP NOT NULL
);

CREATE TABLE TestTable3 (
    id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    fk_id INT,
    FOREIGN KEY (fk_id) REFERENCES TestTable2(id)
);

-- 2
CREATE TABLE LoggingForTestTable1 (
    id NUMBER PRIMARY KEY,

    operation VARCHAR2(50) NOT NULL,
    datetime TIMESTAMP NOT NULL,

    new_id NUMBER,
    old_id NUMBER,
    new_name VARCHAR2(100),
    old_name VARCHAR2(100),
    new_value NUMBER,
    old_value NUMBER
);

CREATE TABLE LoggingForTestTable2 (
    id NUMBER PRIMARY KEY,

    operation VARCHAR2(50) NOT NULL,
    datetime TIMESTAMP NOT NULL,

    new_id NUMBER,
    old_id NUMBER,
    new_name VARCHAR2(100),
    old_name VARCHAR2(100),
    new_datetime_field TIMESTAMP,
    old_datetime_field TIMESTAMP
);

CREATE TABLE LoggingForTestTable3 (
    id NUMBER PRIMARY KEY,

    operation VARCHAR2(50) NOT NULL,
    datetime TIMESTAMP NOT NULL,

    new_id NUMBER,
    old_id NUMBER,
    new_name VARCHAR2(100),
    old_name VARCHAR2(100),
    new_fk_id INT,
    old_fk_id INT
);


CREATE OR REPLACE PROCEDURE create_logging_trigger(table_name IN VARCHAR2, logging_table_name IN VARCHAR2) IS
    v_sql VARCHAR2(1000);
BEGIN
    v_sql := 'CREATE OR REPLACE TRIGGER ' || table_name || '_trigger
              BEFORE INSERT OR UPDATE OR DELETE 
              ON ' || table_name || ' FOR EACH ROW
              DECLARE
                  log_id NUMBER;
              BEGIN
                  SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM ' || logging_table_name || ';

                  IF INSERTING THEN
                      INSERT INTO ' || logging_table_name || ' (id, operation, datetime';

                  DBMS_OUTPUT.PUT_LINE(v_sql);
                  DBMS_OUTPUT.PUT_LINE('loop');

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      DBMS_OUTPUT.PUT_LINE('col_name ' || col.column_name);
                      v_sql := v_sql || ', new_' || col.column_name;
                  END LOOP;

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      v_sql := v_sql || ', old_' || col.column_name;
                  END LOOP;

                  v_sql := v_sql || ') VALUES (log_id, ''INSERT'', SYSTIMESTAMP';

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      v_sql := v_sql || ', :NEW.' || col.column_name;
                  END LOOP;

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      v_sql := v_sql || ', NULL';
                  END LOOP;

                  v_sql := v_sql || ');';

                  DBMS_OUTPUT.PUT_LINE('before first execute');
                  DBMS_OUTPUT.PUT_LINE(v_sql);

                  EXECUTE IMMEDIATE v_sql;

                  v_sql := 'IF UPDATING THEN
                                INSERT INTO ' || logging_table_name || ' (id, operation, datetime';

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      v_sql := v_sql || ', new_' || col.column_name || ', old_' || col.column_name;
                  END LOOP;

                  v_sql := v_sql || ') VALUES (log_id, ''UPDATE'', SYSTIMESTAMP';

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      v_sql := v_sql || ', :NEW.' || col.column_name || ', :OLD.' || col.column_name;
                  END LOOP;

                  v_sql := v_sql || ');';

                  EXECUTE IMMEDIATE v_sql;

                  v_sql := 'ELSIF DELETING THEN
                                INSERT INTO ' || logging_table_name || ' (id, operation, datetime';

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      v_sql := v_sql || ', old_' || col.column_name;
                  END LOOP;

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      v_sql := v_sql || ', NULL';
                  END LOOP;

                  v_sql := v_sql || ') VALUES (log_id, ''DELETE'', SYSTIMESTAMP';

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      v_sql := v_sql || ', :OLD.' || col.column_name;
                  END LOOP;

                  FOR col IN (SELECT column_name, data_type FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
                      v_sql := v_sql || ', NULL';
                  END LOOP;

                  v_sql := v_sql || ');';

                  EXECUTE IMMEDIATE v_sql || '
              END IF;
          END;';
          DBMS_OUTPUT.PUT_LINE(v_sql);
END create_logging_trigger;
/

BEGIN
    create_logging_trigger('TestTable1', 'LoggingForTestTable1');
    create_logging_trigger('TestTable2', 'LoggingForTestTable2');
    create_logging_trigger('TestTable3', 'LoggingForTestTable3');
END;
/

CREATE OR REPLACE PROCEDURE restore_data(table_name IN VARCHAR2, logging_table_name IN VARCHAR2, p_datetime IN VARCHAR) IS
    v_sql VARCHAR2(1000);
BEGIN
    FOR action IN (SELECT * FROM logging_table_name WHERE p_datetime < datetime ORDER BY id DESC) LOOP
        v_sql := 'IF action.operation = ''INSERT'' THEN
                      DELETE FROM ' || table_name || ' WHERE id = action.new_id;
                  ELSIF action.operation = ''UPDATE'' THEN
                      UPDATE ' || table_name || ' SET ';

        FOR col IN (SELECT column_name FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
            v_sql := v_sql || col.column_name || ' = action.old_' || col.column_name || ', ';
        END LOOP;

        v_sql := RTRIM(v_sql, ', ') || ' WHERE id = action.new_id;
                  ELSIF action.operation = ''DELETE'' THEN
                      INSERT INTO ' || table_name || ' (id';

        FOR col IN (SELECT column_name FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
            v_sql := v_sql || ', ' || col.column_name;
        END LOOP;

        v_sql := v_sql || ') VALUES (action.old_id';

        FOR col IN (SELECT column_name FROM user_tab_cols WHERE table_name = table_name AND column_name NOT IN ('ID', 'DATETIME')) LOOP
            v_sql := v_sql || ', action.old_' || col.column_name;
        END LOOP;

        v_sql := v_sql || ');';
        EXECUTE IMMEDIATE v_sql;
    END LOOP;

    DELETE FROM logging_table_name WHERE p_datetime < datetime;
END restore_data;
/

CREATE OR REPLACE PACKAGE restore_package AS
    PROCEDURE restore_data_in_table(table_name IN VARCHAR2, logging_table_name IN VARCHAR2, p_datetime IN VARCHAR);
    PROCEDURE restore_data_in_table(table_name IN VARCHAR2, logging_table_name IN VARCHAR2, p_interval IN INTERVAL DAY TO SECOND);
END restore_package;
/

CREATE OR REPLACE PACKAGE BODY restore_package AS
    PROCEDURE restore_data_in_table(table_name IN VARCHAR2, logging_table_name IN VARCHAR2, p_datetime IN VARCHAR) AS
    BEGIN
        restore_data(table_name, logging_table_name, p_datetime);
    END restore_data_in_table;

    PROCEDURE restore_data_in_table(table_name IN VARCHAR2, logging_table_name IN VARCHAR2, p_interval IN INTERVAL DAY TO SECOND) AS
        v_current_time TIMESTAMP;
        v_target_time TIMESTAMP;
    BEGIN
        SELECT CURRENT_TIMESTAMP INTO v_current_time FROM DUAL;
        v_target_time := v_current_time - p_interval - INTERVAL '0 03:00:00.000000' DAY TO SECOND;
        restore_data_in_table(table_name, logging_table_name, TO_CHAR(v_target_time));
    END restore_data_in_table;
END restore_package;
/
