#pragma
modify=tbl_patient
key=counter
load=doctor_phone,doctor_id
ignore=marker
@last_name=lname
@first_name=fname
.last_name=This is description of last name

SELECT
 'X' AS MARKER,
 T1."COUNTER",
 T1."SSN",
 T1."LNAME" AS LAST_NAME,
 T1."FNAME" AS FIRST_NAME,
 T1."DOB",
 T1."ADDRESS1",
 T1."ADDRESS2",
 T1."CITY",
 T1."STATE",
 T1."ZIP",
 T1."PHONE",
 T1."AMOUNT",
 T1."NOTE",

 T1."C_DOCTOR",
 T2."PHONE" AS DOCTOR_PHONE,
 T2."NPI"	AS DOCTOR_ID
FROM
 TBL_PATIENT T1
  LEFT OUTER JOIN TBL_DOCTOR T2 ON T1.C_DOCTOR = T2.COUNTER
WHERE
 T1.LNAME LIKE :LN