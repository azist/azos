# Data Access RPC

RPC stands for **R**emote **P**rocedure **C**all
https://en.wikipedia.org/wiki/Remote_procedure_call

The purpose of this library is to provide a direct pass-through RPC-style access to other systems. 
Such need typically arises when one needs to consume data in legacy systems.

The RPC is typically exposed via a web API endpoint which is connected to `IRpcServer`-implementing module
which in turn typically uses `ICrudDataStore` interfacing to a legacy database (e.g. Ms SQL).

