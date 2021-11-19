# RPC Data Pipe (API for [Data Access RPC](https://github.com/azist/azos/blob/master/src/Azos/Data/Access/Rpc/readme.md))


## Additional RequestHeaders

---

### **Example #1**

The responses shown in ALL of the earlier examples have been formatted for display purposes 
and by default the VS Code Rest Client automatically formats the responses. However the 
actual raw response body is returned as shown below.


### Actual Raw Response Text:
```rest
{"data":{"Rows":[[3,4]],"Instance":"4005082a-311b-4955-9564-61745aabbe19","Type":"Azos.Data.Rowset","IsTable":false,"Schema":{"Name":"JSON-1275186659","FieldDefs":[{"Name":"NobelPrizeInMath","Order":0,"Type":"int","Nullable":false,"GetOnly":false,"IsKey":false,"IsRequired":false,"Visible":true},{"Name":"EqualsFour","Order":1,"Type":"int","Nullable":false,"GetOnly":false,"IsKey":false,"IsRequired":false,"Visible":true}]}},"OK":true}
```

In order to opt-in to formatted JSON response body content you can specify 
the `"pretty": true` RequestHeaders value.

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
          "rows-as-map": true,
          "pretty": true
      },
		Command: {
			Text: "select 1+2 as NobelPrizeInMath, 2+2 as EqualsFour"
		}
	}
}
```

=======================================

#### Response:


```rest
{
  "data": 
    {
      "Rows": [
          {
            "NobelPrizeInMath": 3, 
            "EqualsFour": 4
          }], 
      "Instance": "db37d862-bf7b-48b0-95f0-d8d0f876fbb0", 
      "Type": "Azos.Data.Rowset", 
      "IsTable": false, 
      "Schema": 
        {
          "Name": "JSON-1655865352", 
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
              }]
        }
    }, 
  "OK": true
}
```

---


### **Example #2**

Under many scenarios it may be beneficial for readability and performance to 
reduce the message size and complexity for known object schemas that rarely
change. You can opt-out of returning the schema values in the JSON response
by setting the `"no-schema": true` RequestHeaders value.

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
          "rows-as-map": true,
          "pretty": true,
          "no-schema": true
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
  "data": 
    {
      "Rows": [
          {
            "NobelPrizeInMath": 3, 
            "EqualsFour": 4
          }]
    }, 
  "OK": true
}
```


---

=======================================

 ### Previous <- [Mapping Rows as JSON Objects](ex2-mapping-rows.md)

 ### Next -> [Passing Parameters](ex4-passing-params.md)