# Azos.Data.Adlib - [A]morphous [D]ata [Lib]rary

Amorphous data library defines a way for working with schema-less document-oriented data
having documents persistently stored in named collections along with indexable ad-hoc 
attributes.

The system is build for horizontal scaling with sharding, optional data expiration, querying 
and reflection.

The service provides simple CRUD-like functionality based around [`Item.cs`](Item.cs)
which is an entity that gets stored.

Use [`ItemFilter.cs`](ItemFilter.cs) to POST data filter requests to server `filter`
endpoint


[`AdlibWebClientLogic.cs`](AdlibWebClientLogic.cs) is an implementation of `IAdlibLogic` which
delegates execution to a remote Http server via the circuit breaker/balancer circuitry.

## Sample API Use Cases

```
GET http://localhost:8080/adlib/store/spaces

{
	"data": [
		"s1",
		"s2"
	],
	"OK": true
}
```

```
GET http://localhost:8080/adlib/store/collections?space=s1

{
	"data": [
		"tezta"
	],
	"OK": true
}
```

```
POST http://localhost:8080/adlib/store/item
{
	item: {
		gdid: "0:2:300",
		space: "s1",
		collection: "dima1",
		shardtopic: null,
		createutc: 0,
		origin: "a",
		headers: "",
		contenttype: "bin",
		content: "another",
		tags: [{p: "lname", s: "Gurariy"}]
	}
}

//got:
{
	"OK": true,
	"change": "Inserted",
	"status": 200,
	"affected": 1,
	"message": "Inserted",
	"data": {
		"crud": {
			"TotalDocumentsAffected": 1,
			"TotalDocumentsUpdatedAffected": 0,
			"Upserted": null,
			"WriteErrors": null
		},
		"gdid": "0:2:500",
		"id": "dima1.gi@s1::0:2:500"
	}
}

```


```
 PUT http://localhost:8080/adlib/store/item
 {
	item: {
		gdid: "0:9:2101",
		space: "s1",
		collection: "dima1",
		shardtopic: null,
		createutc: 0,
		origin: "a",
		headers: "",
		contenttype: "bin",
		content: "AP8BAABHKuRAAAAU",
		tags: [{p: "lname", s: "Lenin"},{p: "nick", s: "Mushroooomz"}]
	}
}

//got:
{
	"OK": true,
	"change": "Updated",
	"status": 200,
	"affected": 1,
	"message": "Updated",
	"data": {
		"crud": {
			"TotalDocumentsAffected": 1,
			"TotalDocumentsUpdatedAffected": 0,
			"Upserted": [
				{
					"ID": {
						"$type": "UserDefined",
						"$binary": "AAAAAJAAAAAAAAg1"
					},
					"Index": 0
				}
			],
			"WriteErrors": null
		},
		"gdid": "0:9:2101",
		"id": "dima1.gi@s1::0:9:2101"
	}
}
```


```
DELETE http://localhost:8080/adlib/store/item?id=tezta.gi%40s1%3A%3A16711936%3A0%3A20031883161108500

//got:
{
	"OK": true,
	"change": "Deleted",
	"status": 200,
	"affected": 0,
	"message": "Deleted",
	"data": {
		"crud": {
			"TotalDocumentsAffected": 0,
			"TotalDocumentsUpdatedAffected": 0,
			"Upserted": null,
			"WriteErrors": null
		},
		"gdid": "16711936:0:20031883161108500",
		"id": "tezta.gi@s1::16711936:0:20031883161108500"
	}
}

```
  
```
POST http://localhost:8080/adlib/store/filter

{
	filter: {
		//gdid: "16711936:0:20031883161108500",
		space: "s1",
		collection: "tezta",
		fetchtags: true,
		tagfilter: {
     "Operator": "or",
     "LeftOperand": {
      "Operator": "=",
      "LeftOperand": { "Identifier": "nick"},
      "RightOperand": { "Value": "Dick"}
      },
			"RightOperand": {
				"Operator": "=",
				"LeftOperand": { "Identifier": "nick"},
				"RightOperand": { "Value": "Mushroom"}
			}
    }
  }
}	

//got:

{
	"data": [
		{
			"Space": "s1",
			"Collection": "tezta",
			"Gdid": "16711936:0:20034997817704449",
			"Id": "tezta.gi@s1::16711936:0:20034997817704449",
			"ShardTopic": null,
			"CreateUtc": "0",
			"Origin": "a",
			"Headers": "",
			"ContentType": "bin",
			"Tags": [
				{
					"p": "lname",
					"s": "Lenin"
				},
				{
					"p": "nick",
					"s": "Mushroom"
				}
			],
			"Content": null
		}
	],
	"OK": true
}


```

