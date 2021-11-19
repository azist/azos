# RPC Data Pipe (API for [Data Access RPC](https://github.com/azist/azos/blob/master/src/Azos/Data/Access/Rpc/readme.md))


## Passing Parameters

---

### **Example #1**

The RPC Data Pipe will accept underlying provider parameters as an array of `Name`/`Value` pair objects.
The example below passes a **`@typ`** parameter with the value of **Sunroom%** to the configured SQL Server.

> Note* - parameter prefix (e.g. `@`) may vary by configured datastore provider

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
			Text: "SELECT TOP 3 AppointmentId, AppointmentType, SalesmanId FROM Appointments WHERE AppointmentType LIKE @typ",
			Parameters:
			 [
				 {"Name": "typ", "Value": "Sunroom%"}
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
        "AppointmentId": 1,
        "AppointmentType": "Sunroom",
        "SalesmanId": "e97cf86f-a105-4159-a4ad-904587d97568"
      },
      {
        "AppointmentId": 1,
        "AppointmentType": "Sunroom",
        "SalesmanId": "be1fec74-c465-4a21-8d6e-e5c9e4d62b80"
      },
      {
        "AppointmentId": 1,
        "AppointmentType": "Sunroom",
        "SalesmanId": "20babe47-19e3-4351-8f39-fba45c938b21"
      }
    ]
  },
  "OK": true
}
```


---

=======================================

 ### Previous <- [Additional RequestHeaders](ex3-additional-headers.md)

 ### Next -> [Stored Procedures](ex5-stored-procedures.md)