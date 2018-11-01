# Immediate Todo Stickers

##### 2018-11-01 DKh
* Runnable - consider invoking async methods and properly awaiting them - 
 this is needed for example to make sure that tests epilog is ran to completion on 
a thread pool and properly awaited for

* Glue Server call context - get rid of ThreadStatic
* Revise Azos.Web.WebClient to use proper async model
* ReflectionUtils -add method to test a method for being async
