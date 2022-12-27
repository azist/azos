# Data Access RPC Endpoint

## Data Access RPC

The purpose of this API is to provide a direct pass-through RPC-style access to other systems. 
Such need typically arises when one needs to consume data in legacy systems. 
The RPC is Data Pipe API exposes `IRpcServer` data store logic to a legacy 
database (e.g. Ms SQL). 

## Endpoints

### /data/rpc/handler/reader POST

`{$title}`
`{$description}`

**Request Sample:**
```
POST https://{{HOST}}/data/rpc/handler/reader HTTP/1.2
Authorization: {{AUTH}}
wv-data-ctx: myctx
Accept: application/json
Content-Type: application/json

{ "request": 
  {
      "requestHeaders": {
          "rows-as-map": true,
          "pretty": true,
          "no-schema": false
      },
      "command": {
          "headers": {
              "sql-command-type": "proc"
          },
          "text": "dpipe_ReferralSearch",
          "parameters": [
              {
                  "name": "FirstName",
                  "value": ""
              },
              {
                  "name": "LastName",
                  "value": ""
              },
              {
                  "name": "BusinessUnit",
                  "value": "IND01"
              },
              {
                  "name": "Postal",
                  "value": ""
              }
          ]
      }
  }
}
```

**Response Sample:**
```
{
    "data": {
      "Rows": [
        {
          "CustomerID": "c2de9d0e-2553-477e-8cc5-000032fad21f",
          "FirstName": "Ralph",
          "LastName": "Banion",
          "Address1": "6456 Arlington Way",
          "Address2": "",
          "City": "Indianapolis",
          "County": "Marion",
          "State": "IN",
          "Postal": "46237",
          "Email": "rbanion@customshoes.com",
          "Phone": "3174566593",
          "Business_Unit": "IND01"
        },
        {
          "CustomerID": "e432f0ef-cb63-4114-93cd-0000a5eef802",
          "FirstName": "John",
          "LastName": "Smith",
          "Address1": "7809 E 65th St",
          "Address2": "",
          "City": "Indianapolis",
          "County": "Marion",
          "State": "IN",
          "Postal": "46256",
          "Email": null,
          "Phone": "3178453340",
          "Business_Unit": "IND01"
        }
      ],
      "Instance": "3fb9aa40-2e12-4fe0-8096-b3fc819a53ee",
      "Type": "Azos.Data.Rowset",
      "IsTable": false,
      "Schema": {
        "Name": "JSON-1436433374",
        "FieldDefs": [
          {
            "Name": "CustomerID",
            "Order": 0,
            "Type": "object",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "FirstName",
            "Order": 1,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "LastName",
            "Order": 2,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "Address1",
            "Order": 3,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "Address2",
            "Order": 4,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "City",
            "Order": 5,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "County",
            "Order": 6,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "State",
            "Order": 7,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "Postal",
            "Order": 8,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "Email",
            "Order": 9,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "Phone",
            "Order": 10,
            "Type": "string",
            "Nullable": false,
            "GetOnly": false,
            "IsKey": false,
            "IsRequired": false,
            "Visible": true
          },
          {
            "Name": "Business_Unit",
            "Order": 11,
            "Type": "string",
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

### /data/rpc/handler/transaction POST

`{$title}`
`{$description}`

**Request Sample:**
```
POST https://{{HOST}}/data/rpc/handler/transaction HTTP/1.2
Authorization: {{AUTH}}
wv-data-ctx: myctx
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
                "Text": "dpipe_UpdateComments",
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
                "Text": "dpipe_UpdateComments",
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

**Response Sample:**
```
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