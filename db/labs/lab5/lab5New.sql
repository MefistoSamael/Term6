-- 1
CREATE TABLE TestTable1 (
    id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    value NUMBER NOT NULL
);

CREATE TABLE TestTable2 (
    id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    datetime TIMESTAMP NOT NULL
);

CREATE TABLE TestTable3 (
    id NUMBER PRIMARY KEY,
    name VARCHAR2(100) NOT NULL,
    fk_id INT,
    FOREIGN KEY (fk_id) REFERENCES TestTable2(id)
    ON DELETE CASCADE
);

CREATE TABLE TablesLog(
    id NUMBER PRIMARY KEY,
    operation_time TIMESTAMP,
    table_id NUMBER,
    table_number NUMBER
);

CREATE TABLE LastReport (
    id NUMBER PRIMARY KEY,
    datetime TIMESTAMP
);

INSERT INTO LastReport VALUES(1, NULL);



begin
GenerateLoggingTable('TESTTABLE1');
GenerateLoggingTable('TESTTABLE2');
GenerateLoggingTable('TESTTABLE3');
end;

CREATE OR REPLACE TRIGGER LoggingTable1_logger
BEFORE INSERT OR DELETE
ON LOGGINGFORTESTTABLE1 FOR EACH ROW
DECLARE
    log_id NUMBER;
BEGIN
    SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM TablesLog;

    IF INSERTING THEN
        INSERT INTO TablesLog (id, operation_time, table_id, table_number)
        VALUES (log_id, CURRENT_TIMESTAMP, :NEW.id, 1);
    ELSIF DELETING THEN
        DELETE FROM TablesLog WHERE table_id = :OLD.id AND table_number = 1;
    END IF;
END;
/

CREATE OR REPLACE TRIGGER LoggingTable2_logger
BEFORE INSERT OR DELETE
ON LOGGINGFORTESTTABLE2 FOR EACH ROW
DECLARE
    log_id NUMBER;
BEGIN
    SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM TablesLog;

    IF INSERTING THEN
        INSERT INTO TablesLog (id, operation_time, table_id, table_number)
        VALUES (log_id, CURRENT_TIMESTAMP, :NEW.id, 2);
    ELSIF DELETING THEN
        DELETE FROM TablesLog WHERE table_id = :OLD.id AND table_number = 2;
    END IF;
END;
/

CREATE OR REPLACE TRIGGER LoggingTable3_logger
BEFORE INSERT OR DELETE
ON LOGGINGFORTESTTABLE3 FOR EACH ROW
DECLARE
    log_id NUMBER;
BEGIN
    SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM TablesLog;

    IF INSERTING THEN
        INSERT INTO TablesLog (id, operation_time, table_id, table_number)
        VALUES (log_id, CURRENT_TIMESTAMP, :NEW.id, 3);
    ELSIF DELETING THEN
        DELETE FROM TablesLog WHERE table_id = :OLD.id AND table_number = 3;
    END IF;
END;
/

CREATE OR REPLACE TRIGGER Trigger_TESTTABLE1
    BEFORE INSERT OR UPDATE
    ON TESTTABLE1 FOR EACH ROW
    DECLARE
        log_id NUMBER;
    BEGIN
        SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM LOGGINGFORTESTTABLE1;

        IF INSERTING THEN
            INSERT INTO LOGGINGFORTESTTABLE1 (ID, OPERATION, DATETIME, NEW_ID, OLD_ID, NEW_NAME, OLD_NAME, NEW_VALUE, OLD_VALUE)
            VALUES (log_id, 'INSERT', CURRENT_TIMESTAMP, :NEW.ID, NULL, :NEW.NAME, NULL, :NEW.VALUE, NULL);
        ELSIF UPDATING THEN
            INSERT INTO LOGGINGFORTESTTABLE1 (ID, OPERATION, DATETIME, NEW_ID, OLD_ID, NEW_NAME, OLD_NAME, NEW_VALUE, OLD_VALUE)
            VALUES (log_id, 'UPDATE', CURRENT_TIMESTAMP, :NEW.ID, :OLD.ID, :NEW.NAME, :OLD.NAME, :NEW.VALUE, :OLD.VALUE);
        END IF;
    END;
/

CREATE OR REPLACE TRIGGER Deletion_Trigger_TESTTABLE1
    AFTER delete
    ON TESTTABLE1 FOR EACH ROW
    DECLARE
        log_id NUMBER;
    BEGIN
        SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM LOGGINGFORTESTTABLE1;

        INSERT INTO LOGGINGFORTESTTABLE1 (ID, OPERATION, DATETIME, NEW_ID, OLD_ID, NEW_NAME, OLD_NAME, NEW_VALUE, OLD_VALUE)
        VALUES (log_id, 'DELETE', CURRENT_TIMESTAMP, NULL, :OLD.ID, NULL, :OLD.NAME, NULL, :OLD.VALUE);
    END;
/

CREATE OR REPLACE TRIGGER Trigger_TESTTABLE2
    BEFORE INSERT OR UPDATE OR DELETE
    ON TESTTABLE2 FOR EACH ROW
    DECLARE
        log_id NUMBER;
    BEGIN
        SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM LOGGINGFORTESTTABLE2;

        IF INSERTING THEN
            INSERT INTO LOGGINGFORTESTTABLE2 (ID, OPERATION, DATETIME, NEW_ID, OLD_ID, NEW_NAME, OLD_NAME, NEW_DATETIME, OLD_DATETIME)
            VALUES (log_id, 'INSERT', CURRENT_TIMESTAMP, :NEW.ID, NULL, :NEW.NAME, NULL, :NEW.DATETIME, NULL);
        ELSIF UPDATING THEN
            INSERT INTO LOGGINGFORTESTTABLE2 (ID, OPERATION, DATETIME, NEW_ID, OLD_ID, NEW_NAME, OLD_NAME, NEW_DATETIME, OLD_DATETIME)
            VALUES (log_id, 'UPDATE', CURRENT_TIMESTAMP, :NEW.ID, :OLD.ID, :NEW.NAME, :OLD.NAME, :NEW.DATETIME, :OLD.DATETIME);
        END IF;
    END;
/

CREATE OR REPLACE TRIGGER Deletion_Trigger_TESTTABLE2
AFTER DELETE
ON TESTTABLE2 FOR EACH ROW
DECLARE
    log_id NUMBER;
BEGIN
    SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM LOGGINGFORTESTTABLE2;

    INSERT INTO LOGGINGFORTESTTABLE2 (ID, OPERATION, DATETIME, NEW_ID, OLD_ID, NEW_NAME, OLD_NAME, NEW_DATETIME, OLD_DATETIME)
    VALUES (log_id, 'DELETE', CURRENT_TIMESTAMP, NULL, :OLD.ID, NULL, :OLD.NAME, NULL, :OLD.DATETIME);
END;
/

CREATE OR REPLACE TRIGGER Trigger_TESTTABLE3
    BEFORE INSERT OR UPDATE OR DELETE
    ON TESTTABLE3 FOR EACH ROW
    DECLARE
        log_id NUMBER;
    BEGIN
        SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM LOGGINGFORTESTTABLE3;

        IF INSERTING THEN
            INSERT INTO LOGGINGFORTESTTABLE3 (ID, OPERATION, DATETIME, NEW_ID, OLD_ID, NEW_NAME, OLD_NAME, NEW_FK_ID, OLD_FK_ID)
            VALUES (log_id, 'INSERT', CURRENT_TIMESTAMP, :NEW.ID, NULL, :NEW.NAME, NULL, :NEW.FK_ID, NULL);
        ELSIF UPDATING THEN
            INSERT INTO LOGGINGFORTESTTABLE3 (ID, OPERATION, DATETIME, NEW_ID, OLD_ID, NEW_NAME, OLD_NAME, NEW_FK_ID, OLD_FK_ID)
            VALUES (log_id, 'UPDATE', CURRENT_TIMESTAMP, :NEW.ID, :OLD.ID, :NEW.NAME, :OLD.NAME, :NEW.FK_ID, :OLD.FK_ID);
        END IF;
    END;
/

CREATE OR REPLACE TRIGGER Deletion_Trigger_TESTTABLE3
    AFTER DELETE
    ON TESTTABLE3 FOR EACH ROW
    DECLARE
        log_id NUMBER;
    BEGIN
        SELECT NVL(MAX(ID), 0) + 1 INTO log_id FROM LOGGINGFORTESTTABLE3;

        INSERT INTO LOGGINGFORTESTTABLE3 (ID, OPERATION, DATETIME, NEW_ID, OLD_ID, NEW_NAME, OLD_NAME, NEW_FK_ID, OLD_FK_ID)
        VALUES (log_id, 'DELETE', CURRENT_TIMESTAMP, NULL, :OLD.ID, NULL, :OLD.NAME, NULL, :OLD.FK_ID);
    END;
/

-- 3
CREATE OR REPLACE PACKAGE recovery_package AS
    PROCEDURE Tables_recovery(p_datetime TIMESTAMP);
    PROCEDURE Tables_recovery(p_seconds INT);
END recovery_package;
/

CREATE OR REPLACE PACKAGE BODY recovery_package AS

    PROCEDURE Tables_recovery(p_datetime TIMESTAMP) AS
        Table1Record LoggingForTESTTABLE1%ROWTYPE;
        Table2Record LoggingForTESTTABLE2%ROWTYPE;
        Table3Record LoggingForTESTTABLE3%ROWTYPE;
    BEGIN
        FOR action IN (SELECT * FROM TablesLog WHERE p_datetime < operation_time ORDER BY id DESC)
        LOOP
            -- Если действие для первой таблицы
            IF action.table_number = 1 THEN
                SELECT * INTO Table1Record FROM LoggingForTESTTABLE1 WHERE id = action.table_id;

                IF Table1Record.operation = 'INSERT' THEN
                    DELETE FROM TestTable1 WHERE id = Table1Record.new_ID;
                END IF;

                IF Table1Record.operation = 'UPDATE' THEN
                    UPDATE TestTable1 SET
                        id = Table1Record.old_ID,
                        name = Table1Record.old_NAME,
                        value = Table1Record.old_VALUE
                    WHERE id = Table1Record.new_ID;
                END IF;

                IF Table1Record.operation = 'DELETE' THEN
                    INSERT INTO TestTable1 VALUES (Table1Record.old_ID, Table1Record.old_NAME, Table1Record.old_VALUE);
                END IF;

            -- Если действие для второй таблицы
            ELSIF action.table_number = 2 THEN
                SELECT * INTO Table2Record FROM LoggingForTESTTABLE2 WHERE id = action.table_id;

                IF Table2Record.operation = 'INSERT' THEN
                    DELETE FROM TestTable2 WHERE id = Table2Record.new_ID;
                END IF;

                IF Table2Record.operation = 'UPDATE' THEN
                    UPDATE TestTable2 SET
                        id = Table2Record.old_ID,
                        name = Table2Record.old_NAME,
                        datetime = Table2Record.old_DATETIME
                    WHERE id = Table2Record.new_ID;
                END IF;

                IF Table2Record.operation = 'DELETE' THEN
                    INSERT INTO TestTable2 VALUES (Table2Record.old_ID, Table2Record.old_NAME, Table2Record.old_DATETIME);
                END IF;

            -- Если действие для третьей таблицы
            ELSIF action.table_number = 3 THEN
                SELECT * INTO Table3Record FROM LoggingForTESTTABLE3 WHERE id = action.table_id;

                IF Table3Record.operation = 'INSERT' THEN
                    DELETE FROM TestTable3 WHERE id = Table3Record.new_ID;
                END IF;

                IF Table3Record.operation = 'UPDATE' THEN
                    UPDATE TestTable3 SET
                        id = Table3Record.old_ID,
                        name = Table3Record.old_NAME,
                        fk_id = Table3Record.old_FK_ID
                    WHERE id = Table3Record.new_ID;
                END IF;

                IF Table3Record.operation = 'DELETE' THEN
                    INSERT INTO TestTable3 VALUES (Table3Record.old_ID, Table3Record.old_NAME, Table3Record.old_FK_ID);
                END IF;
            END IF;
        END LOOP;

        DELETE FROM LoggingForTESTTABLE1 WHERE datetime > p_datetime;
        DELETE FROM LoggingForTESTTABLE2 WHERE datetime > p_datetime;
        DELETE FROM LoggingForTESTTABLE3 WHERE datetime > p_datetime;
    END Tables_recovery;

    PROCEDURE Tables_recovery(p_seconds INT) AS
    BEGIN
        Tables_recovery(CURRENT_TIMESTAMP - INTERVAL '1' SECOND * p_seconds);
    END Tables_recovery;

END recovery_package;
/

-- 4
CREATE OR REPLACE DIRECTORY my_directory AS '/opt/oracle';
GRANT READ, WRITE ON DIRECTORY my_directory TO PUBLIC;

CREATE OR REPLACE PACKAGE create_report_package AS
    FUNCTION create_report(title VARCHAR2, insert_count1  NUMBER, update_count1  NUMBER, delete_count1  NUMBER,
     insert_count2  NUMBER, update_count2  NUMBER, delete_count2  NUMBER,
     insert_count3  NUMBER, update_count3  NUMBER, delete_count3  NUMBER) RETURN VARCHAR2;
    PROCEDURE create_report_for_TestTable(p_datetime TIMESTAMP);
    PROCEDURE create_report_for_TestTable;
END create_report_package;
/

CREATE OR REPLACE PACKAGE BODY create_report_package AS
    FUNCTION create_report (title IN VARCHAR2,
     insert_count1 IN NUMBER, update_count1 IN NUMBER, delete_count1 IN NUMBER,
     insert_count2 IN NUMBER, update_count2 IN NUMBER, delete_count2 IN NUMBER,
     insert_count3 IN NUMBER, update_count3 IN NUMBER, delete_count3 IN NUMBER)
    RETURN VARCHAR2 IS
        result VARCHAR(4000);
    BEGIN
        result :=  '<!DOCTYPE html>
                    <html lang="en">
                    <head>
                        <meta charset="UTF-8">
                        <meta name="viewport" content="width=device-width, initial-scale=1.0">
                        <title>Report</title>
                        <style>
                            body {
                                font-family: Arial, sans-serif; /* Updated font */
                            }
                            table {
                                border-collapse: separate;
                                border-spacing: 0;
                                width: 100%; /* Full width */
                                margin: 20px 0;
                                border-radius: 10px;
                                overflow: hidden; /* Helps in applying border-radius */
                            }
                            th, td {
                                border-right: 1px solid #dddddd; /* Lighter border color */
                                padding: 12px 15px; /* Increased padding */
                                text-align: left;
                            }
                            th:last-child, td:last-child {
                                border-right: none;
                            }
                            th {
                                background-color: #4CAF50; /* Dark green header */
                                color: white;
                            }
                            tr:nth-child(even) {
                                background-color: #f9f9f9; /* Zebra striping for rows */
                            }
                            tr:hover {
                                background-color: #f1f1f1; /* Hover effect */
                            }
                        </style>
                    </head>
                    <body>
                    <h2>' || title || '</h2>
                    <table>
                        <tr>
                            <th>Operation</th>
                            <th>Count</th>
                        </tr>
                        <tr>
                            <td>INSERTs count into tab1</td>
                            <td>' || insert_count1 || '</td>
                        </tr>
                        <tr>
                            <td>UPDATEs count into tab1</td>
                            <td>' || update_count1 || '</td>
                        </tr>
                        <tr>
                            <td>DELETEs count into tab1</td>
                            <td>' || delete_count1 || '</td>
                        </tr>
                        <tr>
                            <td></td>
                            <td></td>
                        </tr>
                        <tr>
                            <td>INSERTs count into tab2</td>
                            <td>' || insert_count2 || '</td>
                        </tr>
                        <tr>
                            <td>UPDATEs count into tab2</td>
                            <td>' || update_count2 || '</td>
                        </tr>
                        <tr>
                            <td>DELETEs count into tab2</td>
                            <td>' || delete_count2 || '</td>
                        </tr>
                        <tr>
                            <td></td>
                            <td></td>
                        </tr>
                        <tr>
                            <td>INSERTs count into tab3</td>
                            <td>' || insert_count3 || '</td>
                        </tr>
                        <tr>
                            <td>UPDATEs count into tab3</td>
                            <td>' || update_count3 || '</td>
                        </tr>
                        <tr>
                            <td>DELETEs count into tab3</td>
                            <td>' || delete_count3 || '</td>
                        </tr>
                    </table>
                    </body>
                    </html>';

        DBMS_OUTPUT.PUT_LINE(result);
        RETURN result;
    END create_report;


    PROCEDURE create_report_for_TestTable(p_datetime TIMESTAMP) AS
        v_file_handle UTL_FILE.FILE_TYPE;
        report VARCHAR2(4000);
        title VARCHAR2(100);
        insert_count1 NUMBER;
        update_count1 NUMBER;
        delete_count1 NUMBER;
        insert_count2 NUMBER;
        update_count2 NUMBER;
        delete_count2 NUMBER;
        insert_count3 NUMBER;
        update_count3 NUMBER;
        delete_count3 NUMBER;
        result VARCHAR(4000);
    BEGIN
        title := 'Since ' || p_datetime;
        SELECT COUNT(*) INTO insert_count1 FROM LoggingForTestTable1 WHERE operation = 'INSERT' AND p_datetime <= datetime;
        SELECT COUNT(*) INTO update_count1 FROM LoggingForTestTable1 WHERE operation = 'UPDATE' AND p_datetime <= datetime;
        SELECT COUNT(*) INTO delete_count1 FROM LoggingForTestTable1 WHERE operation = 'DELETE' AND p_datetime <= datetime;

        SELECT COUNT(*) INTO insert_count2 FROM LoggingForTestTable2 WHERE operation = 'INSERT' AND p_datetime <= datetime;
        SELECT COUNT(*) INTO update_count2 FROM LoggingForTestTable2 WHERE operation = 'UPDATE' AND p_datetime <= datetime;
        SELECT COUNT(*) INTO delete_count2 FROM LoggingForTestTable2 WHERE operation = 'DELETE' AND p_datetime <= datetime;

        SELECT COUNT(*) INTO insert_count3 FROM LoggingForTestTable3 WHERE operation = 'INSERT' AND p_datetime <= datetime;
        SELECT COUNT(*) INTO update_count3 FROM LoggingForTestTable3 WHERE operation = 'UPDATE' AND p_datetime <= datetime;
        SELECT COUNT(*) INTO delete_count3 FROM LoggingForTestTable3 WHERE operation = 'DELETE' AND p_datetime <= datetime;

        result := create_report(title, insert_count1, update_count1, delete_count1, insert_count2, update_count2, delete_count2, insert_count3, update_count3, delete_count3);

        v_file_handle := UTL_FILE.FOPEN('MY_DIRECTORY', 'report.html', 'W');
        UTL_FILE.PUT_LINE(v_file_handle, result);
        UTL_FILE.FCLOSE(v_file_handle);

        UPDATE LastReport SET datetime = CURRENT_TIMESTAMP WHERE ROWNUM = 1;
    END create_report_for_TestTable;

    PROCEDURE create_report_for_TestTable AS
        v_time TIMESTAMP;
    BEGIN
        SELECT datetime INTO v_time FROM LastReport WHERE ROWNUM = 1;

        -- Если таблица со временем пустая, то берем минимальное из нынешних логов
        IF v_time IS NULL THEN
            DBMS_OUTPUT.PUT_LINE('If');
            SELECT MIN(datetime) INTO v_time FROM (
                SELECT datetime  FROM LOGGINGFORTESTTABLE1
                UNION ALL
                SELECT datetime  FROM LOGGINGFORTESTTABLE2
                UNION ALL
                SELECT datetime FROM LOGGINGFORTESTTABLE3);
        END IF;

        create_report_for_TestTable(v_time);
    END create_report_for_TestTable;
END create_report_package;
/


-- for testing
INSERT INTO TestTable1 VALUES(1, 'first table 1', 10);
INSERT INTO TestTable1 VALUES(2, 'first table 2', 11);
INSERT INTO TestTable1 VALUES(3, 'first table 3', 12);

INSERT INTO TestTable2 VALUES(1, 'second table 1', '11.11.11');
INSERT INTO TestTable2 VALUES(2, 'second table 2', '11.09.01');
INSERT INTO TestTable2 VALUES(3, 'second table 3', '12.02.02');

INSERT INTO TestTable3 VALUES(1, 'third table 1', 1);
INSERT INTO TestTable3 VALUES(2, 'third table 2', 1);
INSERT INTO TestTable3 VALUES(3, 'third table 3', 1);

UPDATE TestTable1 SET value = 20 WHERE Id = 2;

UPDATE TestTable2 SET name = 'second table second row new' WHERE Id = 2;

UPDATE TestTable3 SET name = 'third table second row new' WHERE Id = 2;


DELETE FROM TestTable2 WHERE Id = 3;

DELETE FROM TestTable1 WHERE Id = 1;

DELETE FROM TestTable3 WHERE Id = 1;

DELETE FROM TestTable2 WHERE Id = 1;

BEGIN
    --recovery_package.Tables_recovery(5);
    recovery_package.Tables_recovery(TO_TIMESTAMP('09.05.24 13:42:49'));
END;
/

begin
create_report_package.create_report_for_TestTable();
end;


drop table TablesLog;
drop table TestTable1;
drop table TestTable3;
drop table TestTable2;
drop table LastReport;
drop table LOGGINGFORTESTTABLE1;
drop table LOGGINGFORTESTTABLE2;
drop table LOGGINGFORTESTTABLE3;




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