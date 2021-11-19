# RPC Data Pipe (API for [Data Access RPC](https://github.com/azist/azos/blob/master/src/Azos/Data/Access/Rpc/readme.md))

RPC stands for **R**emote **P**rocedure **C**all
https://en.wikipedia.org/wiki/Remote_procedure_call

The purpose of this API is to provide a direct pass-through RPC-style access to other systems. Such need typically arises when one needs to consume data in legacy systems. The RPC is Data Pipe API exposes `IRpcServer` data store logic to a legacy database (e.g. Ms SQL). The implementation provides general interim data access with additional basic Azos framework features (e.g. security, logging, configuration, etc.). 

This architectural proxy data API is meant to address migration of legacy systems via ["The Strangler Ivy Pattern"](https://docs.microsoft.com/en-us/azure/architecture/patterns/strangler-fig) and/or to address near EOL/third party systems via ["Anti-Corruption Layer Pattern"](https://docs.microsoft.com/en-us/azure/architecture/patterns/anti-corruption-layer). It is not recommended for [greenfield](https://en.wikipedia.org/wiki/Greenfield_project) system architecture.   

---


# RPC Data Pipe Getting Started Samples:

| File Name | Title |
|---|---|
|[ex1-connect-test.md](ex1-connect-test.md)| Test Connectivity |
|[ex2-mapping-rows.md](ex2-mapping-rows.md)| Mapping Rows as JSON Objects |
|[ex3-additional-headers.md](ex3-additional-headers.md)| Additional RequestHeaders |
|[ex4-passing-params.md](ex4-passing-params.md)| Passing Parameters |
|[ex5-stored-procedures.md](ex5-stored-procedures.md)| Stored Procedures |
|[ex6-transactions.md](ex6-transactions.md)| Transactions |

=======================================

## Configuration

You will need to need to create an application using Azos.Wave and expose the `Azos.Data.Access.Rpc.Server.Handler` API Controller.
See the [Handler.cs](https://github.com/azist/azos/blob/master/src/Azos.Wave/Data/Rpc/Handler.cs) API Controller class. You will also
need access to a supported relational database with test data. The setup for this is beyond the scope of these examples.


## Getting Started

You will need an HTTP REST client development tool. The sample API calls here were created using VS Code with 
the Rest Client Extension. You can however use whatever tool you like (curl, Insomnia, PostMan, etc.)

> You can download the Rest Client Extension for VS Code [Here!](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

## Setup - REST File Parameters

---

### `*.rest` File Host Parameters

You can modify the default host parameters to specify a target host and basic auth header.

```rest
# ==================== Host Parameters =========================

@HOST = localhost 
@AUTH = Basic [username/pwd in Base64]

```

---

=======================================

 ### Next -> [Testing Connectivity](ex1-connect-test.md)