# RPC Data Pipe (API for [Data Access RPC](https://github.com/azist/azos/blob/master/src/Azos/Data/Access/Rpc/readme.md))


## Mapping Rows as JSON Objects

---

### **Example #1**

By adding the RequestHeaders value of `"rows-as-map": true` as shown below 
you can optionally choose to have the Rows formated as JSON objects. The 
default value for **rows-as-map** is false and is optional.

> Note - Setting `"rows-as-map": true` will be generally result in slower performance

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
      RequestHeaders: {
          "rows-as-map": true
      },
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
      {
        "NobelPrizeInMath": 3,
        "EqualsFour": 4
      }
    ],
    "Instance": "b78f72e3-a836-43c7-80b0-ef37aceaf551",
    "Type": "Azos.Data.Rowset",
    "IsTable": false,
    "Schema": {
      "Name": "JSON149197822",
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

The response will now contain a `Rows` array value that contains a 
collection of JSON Objects. 

```json
    "Rows": [
      {
        "NobelPrizeInMath": 3,
        "EqualsFour": 4
      }
    ],
```



---

=======================================

 ### Previous <- [Test Connectivity](ex1-connect-test.md)

 ### Next -> [Additional RequestHeaders](ex3-additional-headers.md)