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
