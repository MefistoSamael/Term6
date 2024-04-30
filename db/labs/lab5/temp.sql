CREATE OR REPLACE PROCEDURE GenerateLoggingTable(p_table_name VARCHAR2) AS
    v_logging_table_name VARCHAR2(100);
    v_column_list VARCHAR2(1000);
BEGIN
    -- Determine the logging table name based on the existing table name
    v_logging_table_name := 'LoggingFor' || p_table_name;

    -- Build column list for logging table
    v_column_list := '
id NUMBER PRIMARY KEY,
operation VARCHAR2(50) NOT NULL,
datetime TIMESTAMP NOT NULL,' || CHR(10);

    -- Add columns from the existing table to the column list
    FOR column_info IN (
        SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH
        FROM USER_TAB_COLUMNS
        WHERE TABLE_NAME = p_table_name
    ) LOOP
            v_column_list := v_column_list 
                            || 'new_' || column_info.COLUMN_NAME || ' ' || column_info.DATA_TYPE ||
                             CASE
                                WHEN column_info.DATA_TYPE = 'VARCHAR2' THEN '(' || column_info.DATA_LENGTH || ')'
                                ELSE NULL
                             END || ',' || CHR(10)
                            || 'old_' || column_info.COLUMN_NAME || ' ' || column_info.DATA_TYPE ||
                             CASE
                                WHEN column_info.DATA_TYPE = 'VARCHAR2' THEN '(' || column_info.DATA_LENGTH || ')'
                                ELSE NULL
                             END || ',' || CHR(10);
    END LOOP;
    v_column_list := SUBSTR(v_column_list, 1, LENGTH(v_column_list) - 2) || CHR(10);

    -- Create logging table dynamically
    EXECUTE IMMEDIATE 'CREATE TABLE ' || v_logging_table_name || ' (' || v_column_list || ')';
END;
/



-- Create a procedure to generate triggers for logging
CREATE OR REPLACE PROCEDURE CreateLoggingTriggers(p_table_name VARCHAR2) AS
    v_trigger_name VARCHAR2(100);
    v_logging_table_name VARCHAR2(100);
    v_logging_id_seq_name VARCHAR2(100);
    v_column_list VARCHAR2(1000);
    v_ins_values_list VARCHAR2(1000);
    v_upd_values_list VARCHAR2(1000);
    v_del_values_list VARCHAR2(1000);
BEGIN
    -- Determine the logging table name and ID sequence name based on the table name
    v_logging_table_name := 'LOGGINGFOR' || p_table_name;
    v_logging_id_seq_name := 'SEQ_' || v_logging_table_name;

    -- Determine the trigger name based on the table name
    v_trigger_name := 'Trigger_' || p_table_name;

    -- Build column list for logging table insert statements
    EXECUTE IMMEDIATE 'DROP TABLE IF EXISTS ' || v_logging_table_name;
    GenerateLoggingTable(p_table_name);

    -- cloumn names
    FOR col IN (SELECT column_name, column_id FROM USER_TAB_COLUMNS
                WHERE table_name = v_logging_table_name ORDER by column_id) loop
        v_column_list := v_column_list || col.column_name || ', ';
    END LOOP;
    v_column_list := SUBSTR(v_column_list, 1, LENGTH(v_column_list) - 2);

    -- column values
    -- for insert statement
    FOR col IN (SELECT column_name, column_id FROM USER_TAB_COLUMNS
                WHERE table_name = v_logging_table_name
                AND column_name not in ('ID', 'OPERATION', 'DATETIME')
                ORDER by column_id) loop
        v_ins_values_list := v_ins_values_list || 
        CASE
            WHEN INSTR(col.column_name, 'OLD') > 0 THEN 'NULL'
            ELSE  ':NEW.' || SUBSTR(col.column_name, 5, LENGTH(col.column_name))
        END;
        v_ins_values_list := v_ins_values_list || ', ';
    END LOOP;
    v_ins_values_list := SUBSTR(v_ins_values_list, 1, LENGTH(v_ins_values_list) - 2);

    -- for update statement
    FOR col IN (SELECT column_name, column_id FROM USER_TAB_COLUMNS
                WHERE table_name = v_logging_table_name
                AND column_name not in ('ID', 'OPERATION', 'DATETIME')
                ORDER by column_id) loop
        v_upd_values_list := v_upd_values_list || 
        CASE
            WHEN INSTR(col.column_name, 'OLD') > 0 THEN ':OLD.' || SUBSTR(col.column_name, 5, LENGTH(col.column_name))
            ELSE  ':NEW.' || SUBSTR(col.column_name, 5, LENGTH(col.column_name))
        END;
        v_upd_values_list := v_upd_values_list || ', ';
    END LOOP;
    v_upd_values_list := SUBSTR(v_upd_values_list, 1, LENGTH(v_upd_values_list) - 2);

    -- for delete statement
    FOR col IN (SELECT column_name, column_id FROM USER_TAB_COLUMNS
                WHERE table_name = v_logging_table_name
                AND column_name not in ('ID', 'OPERATION', 'DATETIME')
                ORDER by column_id) loop
        v_del_values_list := v_del_values_list || 
        CASE
            WHEN INSTR(col.column_name, 'OLD') > 0 THEN ':OLD.' || SUBSTR(col.column_name, 5, LENGTH(col.column_name))
            ELSE  'NULL'
        END;
        v_del_values_list := v_del_values_list || ', ';
    END LOOP;
    v_del_values_list := SUBSTR(v_del_values_list, 1, LENGTH(v_del_values_list) - 2);
    

    -- Create trigger dynamically
    EXECUTE IMMEDIATE '
    CREATE OR REPLACE TRIGGER ' || v_trigger_name || '
    BEFORE INSERT OR UPDATE OR DELETE 
    ON ' || p_table_name || ' FOR EACH ROW
    DECLARE
        log_id NUMBER;
    BEGIN
        SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM ' || v_logging_table_name || ';
    
        IF INSERTING THEN
            INSERT INTO ' || v_logging_table_name || ' (' || v_column_list || ') 
            VALUES (log_id, ''INSERT'', SYSTIMESTAMP, ' ||  v_ins_values_list || ');
        ELSIF UPDATING THEN
            INSERT INTO ' || v_logging_table_name || ' (' || v_column_list || ') 
            VALUES (log_id, ''UPDATE'', SYSTIMESTAMP, ' ||  v_upd_values_list || ');
        ELSIF DELETING THEN
            INSERT INTO ' || v_logging_table_name || ' (' || v_column_list || ') 
            VALUES (log_id, ''DELETE'', SYSTIMESTAMP, ' ||  v_del_values_list || ');
        END IF;
    END;';
END;
/
