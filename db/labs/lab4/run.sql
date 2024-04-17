-- CREATE PART
DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "CREATE",
    "table": "Test1",
    "columns": [
        {
            "name": "Id",
            "datatype": "INT",
            "constraint": "NOT NULL"
        },
        {
            "name": "Name",
            "datatype": "VARCHAR2(100)"
        }
    ],
    "primary": "Id"
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "CREATE",
    "table": "Test2",
    "columns": [
        {
            "name": "Id",
            "datatype": "INT",
            "constraint": "NOT NULL"
        },
        {
            "name": "Name",
            "datatype": "VARCHAR2(100)"
        },
        {
            "name": "Test1Id",
            "datatype": "INT"
        }
    ],
    "primary": "Id",
    "foreign": [
        {
            "column": "Test1Id",
            "refcolumn": "Id",
            "reftable": "Test1"
        }
    ]
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

-- INSERT PART
DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "INSERT",
    "table": "Test1",
    "columns": [
        "Name"
    ],
    "values": [
        [
            "t1"
        ]
    ]
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "INSERT",
    "table": "Test1",
    "columns": [
        "Name"
    ],
    "values": [
        [
            "t2"
        ]
    ]
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "INSERT",
    "table": "Test2",
    "columns": [
        "Name",
        "Test1Id"
    ],
    "values": [
        [
            "m1",
            1
        ]
    ]
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "INSERT",
    "table": "Test2",
    "columns": [
        "Name",
        "Test1Id"
    ],
    "values": [
        [
            "m2",
            2
        ]
    ]
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

-- SELECT PART 1

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);

    TYPE GenericCursor IS REF CURSOR;
    v_cursor GenericCursor;
    
    id INT;
    name VARCHAR2(50);
BEGIN
    json_data := '{
        "type": "SELECT",
        "columns": [
            "Id",
            "Name"
        ],
        "tables": [
            "Test1"
        ]
    }'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
    
    IF v_cursor IS NOT NULL THEN
        LOOP
            FETCH v_cursor INTO id, name;
            EXIT WHEN v_cursor%NOTFOUND;
            DBMS_OUTPUT.PUT_LINE('Result: ' || id || ', ' || name);
        END LOOP;
        CLOSE v_cursor;
    END IF;
END;
/

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);

    TYPE GenericCursor IS REF CURSOR;
    v_cursor GenericCursor;
    
    id INT;
    name VARCHAR2(50);
BEGIN
    json_data := '{
        "type": "SELECT",
        "columns": [
            "Id",
            "Name"
        ],
        "tables": [
            "Test2"
        ]
    }'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
    
    IF v_cursor IS NOT NULL THEN
        LOOP
            FETCH v_cursor INTO id, name;
            EXIT WHEN v_cursor%NOTFOUND;
            DBMS_OUTPUT.PUT_LINE('Result: ' || id || ', ' || name);
        END LOOP;
        CLOSE v_cursor;
    END IF;
END;
/

-- UPDATE PART

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "UPDATE",
    "table": "Test2",
    "set": [
        {
            "column": "Name",
            "value": "m0"
        }
    ],
    "filters": [
        {
            "type": "WHERE",
            "operator": "OR",
            "body": [
                "Id = 1",
                {
                    "type": "=",
                    "body": {
                        "value": "Id",
                        "condition": {
                            "type": "SELECT",
                            "columns": [
                                "MAX(Id)"
                            ],
                            "tables": [
                                "Test1"
                            ]
                        }
                    }
                }
            ]
        }
    ]
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/
-- SELECT PART 2

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);

    TYPE GenericCursor IS REF CURSOR;
    v_cursor GenericCursor;
    
    id INT;
    name VARCHAR2(50);
BEGIN
    json_data := '{
        "type": "SELECT",
        "columns": [
            "Id",
            "Name"
        ],
        "tables": [
            "Test2"
        ]
    }'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
    
    IF v_cursor IS NOT NULL THEN
        LOOP
            FETCH v_cursor INTO id, name;
            EXIT WHEN v_cursor%NOTFOUND;
            DBMS_OUTPUT.PUT_LINE('Result: ' || id || ', ' || name);
        END LOOP;
        CLOSE v_cursor;
    END IF;
END;
/

-- DELETE PART

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "DELETE",
    "table": "Test2",
    "filters": [
        {
            "type": "WHERE",
            "body": [
                "Name = ''m0''"
            ]
        }
    ]
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

-- SELECT PART 3

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);

    TYPE GenericCursor IS REF CURSOR;
    v_cursor GenericCursor;
    
    id INT;
    name VARCHAR2(50);
BEGIN
    json_data := '{
        "type": "SELECT",
        "columns": [
            "Id",
            "Name"
        ],
        "tables": [
            "Test2"
        ]
    }'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
    
    IF v_cursor IS NOT NULL THEN
        LOOP
            FETCH v_cursor INTO id, name;
            EXIT WHEN v_cursor%NOTFOUND;
            DBMS_OUTPUT.PUT_LINE('Result: ' || id || ', ' || name);
        END LOOP;
        CLOSE v_cursor;
    END IF;
END;
/

-- INSER PART 2

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "INSERT",
    "table": "Test2",
    "columns": [
        "Name",
        "Test1Id"
    ],
    "values": [
        [
            "m1",
            1
        ]
    ]
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "INSERT",
    "table": "Test2",
    "columns": [
        "Name",
        "Test1Id"
    ],
    "values": [
        [
            "m2",
            2
        ]
    ]
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

--SELECT PART 4

-- SMALL SELECT
DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);

    TYPE GenericCursor IS REF CURSOR;
    v_cursor GenericCursor;
    
    id INT;
    name VARCHAR2(50);
BEGIN
    json_data := '{
        "type": "SELECT",
        "columns": [
            "Id",
            "Name"
        ],
        "tables": [
            "Test1"
        ]
    }'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
    
    IF v_cursor IS NOT NULL THEN
        LOOP
            FETCH v_cursor INTO id, name;
            EXIT WHEN v_cursor%NOTFOUND;
            DBMS_OUTPUT.PUT_LINE('Result: ' || id || ', ' || name);
        END LOOP;
        CLOSE v_cursor;
    END IF;
END;
/

-- HUGE SELECT
DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);

    TYPE GenericCursor IS REF CURSOR;
    v_cursor GenericCursor;
    
    id INT;
    name VARCHAR2(50);
BEGIN
    json_data := '{
        "type": "SELECT",
        "columns": [
            "Test1.Id",
            "Test2.Name"
        ],
        "tables": [
            "Test1"
        ],
        "joins": [
            {
                "table": "Test2",
                "condition": [
                    "Test1.ID = Test2.ID"
                ]
            }
        ],
        "filters": [
            {
                "type": "WHERE",
                "operator": "AND",
                "body": [
                    "Test1.ID = 1",
                    {
                        "type": "NOT IN",
                        "body": {
                            "value": 3,
                            "condition": {
                                "type": "SELECT",
                                "columns": [
                                    "ID"
                                ],
                                "tables": [
                                    "Test2"
                                ]
                            }
                        }
                    }
                ]
            }
        ]
    }'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
    
    IF v_cursor IS NOT NULL THEN
        LOOP
            FETCH v_cursor INTO id, name;
            EXIT WHEN v_cursor%NOTFOUND;
            DBMS_OUTPUT.PUT_LINE('Result: ' || id || ', ' || name);
        END LOOP;
        CLOSE v_cursor;
    END IF;
END;
/

-- DROP PART

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "DROP",
    "table": "Test2"
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/

DECLARE
    json_data CLOB;
    sql_result VARCHAR2(4000);
    v_cursor SYS_REFCURSOR;
BEGIN
    -- JSON Object from lab4_tests.json
    json_data := '{
    "type": "DROP",
    "table": "Test1"
}'; 

    JSON_ORM(json_data, sql_result, v_cursor); 
    DBMS_OUTPUT.PUT_LINE(sql_result);
END;
/