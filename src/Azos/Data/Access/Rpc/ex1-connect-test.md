# RPC Data Pipe (API for [Data Access RPC](https://github.com/azist/azos/blob/master/src/Azos/Data/Access/Rpc/readme.md))



## Test Connectivity

---

### **Example #1**

To ensure that you can connect and that the server is functioning as well as the 
supplied credentials are accepted you can run a sample `SQL` statement that 
relies on no underlying DDL object in the database.

=======================================

#### Request:

```rest
###
POST http://{{HOST}}:8103/data/rpc/handler/reader HTTP/1.2
Authorization: {{AUTH}}
Accept: application/json
Content-Type: application/json

{
	request: {
		Command: {
			Text: "select 1+2 as NobelPrizeInMath, 2+2 as EqualsFour"
		}
	}
}
```

=======================================

#### Response:


```json

{
  "data": {
    "Rows": [
      [
        3,
        4
      ]
    ],
    "Instance": "ec11859b-eef4-45a5-86e4-13e9ff4d2fbe",
    "Type": "Azos.Data.Rowset",
    "IsTable": false,
    "Schema": {
      "Name": "JSON1802119865",
      "FieldDefs": [
        {
          "Name": "NobelPrizeInMath",
          "Order": 0,
          "Type": "int",
          "Nullable": false,
          "GetOnly": false,
          "IsKey": false,
          "IsRequired": false,
          "Visible": true
        },
        {
          "Name": "EqualsFour",
          "Order": 1,
          "Type": "int",
          "Nullable": false,
          "GetOnly": false,
          "IsKey": false,
          "IsRequired": false,
          "Visible": true
        }
      ]
    }
  },
  "OK": true
}
```

The response will include the default message structure as shown above. 
The data section will contain a `Rows` array value when any data is 
returned from the query. We executed a `SQL` script that will 
only ever return 1 row and alway 1 row. This row contains an array of 
values where each value is an array. 

```json
"Rows": [
    [
        3,
        4
    ]
]
```

The rectangular multidimensional array is functionally equivalent to 
a table in a database. This is the default format when no `RequestHeaders` 
is passed in the Request. By supplying the `RequestHeaders` options you can 
customize how the Response object is formatted (in later examples).

For additional information on the `Schema` and other values see [Azos Data Documentation](https://github.com/azist/azos/tree/master/src/Azos/Data) 

---

=======================================

 ### Previous <- [README](readme.md)

 ### Next -> [Mapping Rows as JSON Objects](ex2-mapping-rows.md)