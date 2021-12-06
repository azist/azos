# RPC Data Pipe (API for [Data Access RPC](https://github.com/azist/azos/blob/master/src/Azos/Data/Access/Rpc/readme.md))


## Transactions

---

### **Example #1**

The RPC Data Pipe exposes an additional `transaction` method endpoint for executing
CREATE, UPDATE, and DELETE transactions that do not return rowset data. The method
request accepts an array of Commands whereas the prior `reader` examples only accept
a single Command object in the request body.


=======================================

#### Request:

```rest
###
POST http://{{HOST}}:8103/data/rpc/handler/transaction HTTP/1.2
Authorization: {{AUTH}}
Accept: application/json
Content-Type: application/json

{
    "request": {
        "RequestHeaders": {
            "rows-as-map": true,
            "pretty": true,
            "no-schema": true,
        },
        "Commands":
        [ 
            {
                "Headers": { "sql-command-type": "proc" },
                "Text": "UpdateComments",
                "Parameters":
                [
                    {"Name": "userflag", "Value": 1},
                    {"Name": "workerid", "Value": "bb77220e-ac64-44dd-8491-4df819664779"},
                    {"Name": "commtype", "Value": "R"},
                    {"Name": "typeid", "Value": "8c1e5498-d8fd-492d-ac6a-516cff7ccf98"},
                    {"Name": "commdate", "Value": "2021-08-23T09:05:48.547"},
                    {"Name": "comments", "Value": "should we test"},
                    {"Name": "commid", "Value": "7497bbe3-f94e-4176-bc00-029178737104"}
                ]
            },
            {
                "Headers": { "sql-command-type": "proc" },
                "Text": "UpdateComments",
                "Parameters":
                [
                    {"Name": "userflag", "Value": 1},
                    {"Name": "workerid", "Value": "bb77220e-ac64-44dd-8491-4df819664779"},
                    {"Name": "commtype", "Value": "R"},
                    {"Name": "typeid", "Value": "8c1e5498-d8fd-492d-ac6a-516cff7ccf98"},
                    {"Name": "commdate", "Value": "2021-08-23T09:05:58.547"},
                    {"Name": "comments", "Value": "test"},
                    {"Name": "commid", "Value": "7497bbe3-f94e-4176-bc00-029178737104"}
                ]
            }
        ]		
    }
}
```

=======================================

#### Response:


```rest
{
  "OK": false,
  "change": "Undefined",
  "status": 200,
  "affected": 0,
  "message": "Processed in 369 ms",
  "data": [
    {
      "OK": true,
      "change": "Processed",
      "status": 0,
      "affected": -1,
      "message": "Done in 39 ms",
      "data": null
    },
    {
      "OK": true,
      "change": "Processed",
      "status": 0,
      "affected": -1,
      "message": "Done in 41 ms",
      "data": null
    }
  ]
}
```

The response body now contains a `data` array that contains a 
collection of ChangeResult JSON Objects describing the change, status, etc.

---

=======================================

 ### Previous <- [Stored Procedures](ex5-stored-procedures.md)
