# RPC Data Pipe (API for [Data Access RPC](https://github.com/azist/azos/blob/master/src/Azos/Data/Access/Rpc/readme.md))


## Stored Procedures

---

### **Example #1**

The RPC Data Pipe will execute named stored procedures by adding `"sql-command-type": "proc"` in the Command Headers object.


=======================================

#### Request:

```rest
###
POST http://{{HOST}}:8103/data/rpc/handler/reader HTTP/1.2
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
        "Command": {
            "Headers": {
                "sql-command-type": "proc"
            },
        "Text": "Return_Subcontractors",
        "Parameters":
            [
                {"Name": "SiteID", "Value": "KAN01"}
            ]
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
        "SubID": "7a896aa2-3000-49b9-b957-4d79a28c0965",
        "Name": "Austin-Don Juan"
      },
      {
        "SubID": "ca3e075a-112e-40bc-bb1f-c2992360d375",
        "Name": "Lawson - Don Juan"
      }
    ]
  },
  "OK": true
}
```


---

=======================================

 ### Previous <- [Passing Parameters](ex4-passing-params.md)

 ### Next -> [Transactions](ex6-transactions.md)