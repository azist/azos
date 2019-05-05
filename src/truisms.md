# Software Engineering Myths and Truisms
Back to 
- [Documentation Index](/src/documentation-index.md)
- [Azos Design Philosophy](/src/philosophy.md)

Author: Dmitriy R Khmaladze

**TLDR**
>No pattern/principle or process may substitute for engineering experience and talent. Experience comes with time
>and practice and can not be acquired just by reading books. Purism is good in academia and when applied judiciously
>when it makes sense, and one needs to have an experience to judge when it does or does not make sense. Too many framework/library
>choices for developers really impede performance and lead to creation of over-engineered systems.


Every competent software engineer knows common design principles such as SOLID, DRY, IoC, DI etc.
This article is not to criticize those principles which are absolutely necessary to write "sane" code.
We are going to try to look at the problem of modern software development from a bit of a different angle.

In the past 15 years of my professional career I had a privilege working on a number of systems having really large code bases, mostly
business-oriented data-driven software with large backing data stores and various service layers. Those projects
had 10s and sometimes 100+ engineers supporting them in a corporate landscape. What strikes me, is that in those
companies, which are not connected to each other, and even use different technologies (.Net, Java, Node.js) **developers redo almost
the same things over and over** - whilst using the most advanced contemporary frameworks (Spring, Castle, Service Stack et.al.)
In spite of using large frameworks, each project that I have seen also uses a plethora of 3rd party solutions like 
(without particular categorization): NLog, Log4Net, Hibernate, Akka, Kafka, Rabbit MQ, Autofac, and many others in **the same
solution**. I would say that probably over 70% of the code is repetitive (violation of DRY) with much boilerplate to adapt to those different
sub-frameworks. 

The saddest thing - most technology people think it is ~~**good and "eclectic" to have so "many tools"**~~. 
If only could the companies (business) comprehend the technology enough to straighten those "technologists" out. 

Simply put, there is a **lot of puristic BS in modern frameworks** that try to fit all possible combinations at the expense of verbose code and
overall complexity. Lets take [DI *(dependency injection)*](https://en.wikipedia.org/wiki/Dependency_injection) as an example. DI is powerful 
and great, yet how many times have you questioned the need for new object allocation? It turns out, that while writing business-oriented
applications manual object allocation and class proliferation in general is not a good thing. Classes make sense for business state and logic,
but not for senseless stubs with 1 method (and no state). See [Azos Dependency Injection](/src/Azos/Apps/Injection) for details.

Why does this happen?
- Too many choices - you can do the same thing in 1000s of different ways, even in the same solutions
- Purism - taken literally, e.g. single responsibility principle - lets create classes with no state and only 1 method, make them non-static so that they are "testable" and mockable implementing (useless) interfaces
- Complex framework yet too of a low level for business problems (not a full solution) - leaving many choices to developers
- Politics + People don't care

Here is a short list of SOLID principles misuse which is prevalent almost everywhere:
- Single Responsibility Principle: create 1 class with 1 method and no state + another accompanying class with no methods to hold state
- Interface segregation: create interface for every such class described above. The interface does not extract any common behavior and is really senseless (1:1=intf:class)
- Dependency Inversion - in spite of aforementioned classes effectively being static code, create "testable" code - implement mocks for even POCO data holder classes

What is the real problem with the abuse outlined above? Well, it is really a **senseless abuse which creates 100s of useless classes**,
interfaces, and unit **tests which do not really test anything of value**.

The **infinite level of abstraction** in business apps is a good example of purism. For example, **in the .Net framework itself abstraction is capped** - a `String` 
class is used in many APIs, not an `IString` or `ICharSequence` and puristically this is a bad design, but it is a good 
design from the practicality standpoint.




**One** of Real Examples:
>Around 2012 I consulted a large company having 50K+ employees. In one system I have seen around 100 Mvc controllers each post method containing similar code which had around 
>5 accompanying classes for every data model type *(model binder, validator, storage adapter, UI presenter)*. Without getting in too
> much detail of that particular case, I could conclude that whoever architected that solution was an incorrigible purist with little 
>knowledge of real-world software development, then **10s of developers followed the pattern for a few years**
>creating around **3000 classes** + unit tests. The solution had around 650 assemblies and took around 30 minutes
>to open and "build all" in Visual Studio!!! What strikes me, is that the largest part of that code was "infrastructure"/system - it
>did not even deal with their business domain. At the end of the day I was able to extract common business behavior into 2 classes
>and get rid of "custom infrastructure rule engine" completely. The code base shrunk to 1 assembly (instead of 650) and <200 classes,
>having classes represent 100+ different business rules as stipulated by the state governments.


## Problems with General-Purpose Object Allocation in Business Apps

**TLDR**
>Business apps are nothing more but manipulators of model data usually supplied by Http post, validated, transformed
>and stored into a DB store (which usually uses auto-generated SQL/CRUD). The complexity of classes/inheritance and object graph
>allocation is mostly needed in system code (such as Azos framework itself), not a business code (such as XYZ system built on Azos)
>as most business code is procedural/linear logic operating on business-models. Most entities come into business code already pre-allocated by
>the system (controllers, service modules, entity mapping, model bindings)

Composition Root/Chassis:
>Object should be composed together as close as possible to the application's entry point.
>A Composition Root is a (preferably) unique location in an application where modules are composed together.
><p style="text-align: right"><a href="http://blog.ploeh.dk/2011/07/28/CompositionRoot/">Composition Root Pattern by Mark Seemann</a></p>


DI is good, in general, what is not good is that in the hands of purists and careless developers general purpose DI availability creates 
a temptation to add extra classes and allocate too many objects for tasks which **do not need object instances in principle**. 
Of course those classes may be created without DI - just by calling `new`, but this is a tight coupling code smell, therefore once
DI-anything becomes "free", developers are tempted to start abusing single responsibility principle by adding myriads of classes with
little purpose. An example of this abuse (taken from real apps): *MyModelX, MyModelXValidator, MyModelXEmailMustBeUnqueValidationRule, MyModelXSomethingElseValidationRule,
MyModelXControllerBinder, MyModelXStore, MyModelXDefaults* etc. now, many times developers also create a 1:1 interface for every class so they "can be mocked".
This is really a crazy impractical design instigated by DI-anything ubiquity and purism.

Lets analyze what objects are needed at runtime and how they get allocated.
The following lists objects that get allocated in a typical app:

- App component graph - allocated by the system at start, app components are app-scoped objects like logger, service stack etc. (system code)
- Logging - allocated at app start (system code)
- Instrumentation - allocated at app start (system code)
- Data Store/Access Repository -  allocated at app start (system code), data store may need to cache precomputed values
- Security Manager - allocated at app start (system code)
- Domain-specific business logic - modules (strategies/logic) allocated at app start (system code)
- Web call POST/PUT payload -> Domain model - allocated by MVC controller/model binder (system code)
- Data Access model - allocated by datastore/access layer (system code)
- Custom data structures/e.g. tree Nodes - **allocated by user code**
- Domain model object - allocated by datastore/object mapper (system code mostly) **and user code**
- Deserialization: stream-> object: - allocated by serializer (system code)

As we can see in the list above, **most of the object allocations are done by the system(framework)** code. Business logic 
developers should concentrate on implementing:

- Domain models - objects that reflect business (e.g. User, Account, Doctor etc...)
- Domain logic - objects that contain business rules. The practice has shown that the majority of business logic is linear and procedural, hence it makes sense to put those functions in "logic modules" that get loaded in the app context and used as a "service"

**Business logic is mostly procedural in its core**, as it takes "business models", validates them and transacts
the db/store/queue. A business-oriented data driven application is all about good UX front end and matching backend that
takes data (usually via web API facade), binds it to server models, validates, transforms and stores those models either
directly into the store or via some kind of queue/CQRS/event sourcing etc.

It became apparent that **frivolous object allocations should NOT be encouraged** altogether in business logic.

While writing software for an end-user problem, If you are spending too much time doing system architecture (inventing non-domain classes) on the
server side - then there is something wrong with your framework which you base your architecture on.
**Business app must ONLY contain domain objects and business logic that operates on them.**



## A few Software Engineering Truisms
Most "well known" facts and principles such as: **SOLID**, data **normalization**, IoC etc. are rooted in puristic world view and
actually pose a great threat to the project when applied carelessly, without proper engineering experience. Unfortunately **there are
no rules that can replace the true engineering experience and sheer talent**. A good solutions is **always a balance in the mix of good 
software engineering techniques** (some mentioned above) **and practicality**, where one must know what this or that would take in
real life to write and maintain (including test/retest).

Does this mean that all design patterns and well-known software engineering principles are bad? Absolutely not! As a matter of fact,
those practices hold true for 99% of cases, however you must be careful and **not blindly follow the rules** as your case
may not be the best case for those rules.

A few examples of software "truisms" which are usually taken as incontrovertible facts of life/nature.
These are just a few tacit assumptions that one must be careful with:

#### In OOP everything should be a class with a "Single" responsibility
...so people create classes like `Adder<int,int>` etc.

**In Reality**:
Too many people take this literally and start creating senseless classes, classes without state. What appeals to many that those classes
are easy to test, however the purpose of the class is senseless to begin with so there is no benefit in testing of senseless code.
The most trivial requirement turns into creation of 10s (if not hundreds) of useless types each containing one method and no state.
In reality the creation of new object instances should be minimized/regulated by architecture.

#### State is bad, everything is to be State-Less
...so everything can scale as there is no binding between worker/machine/state. Make everything stateless! 

**In Reality**:
Humans use computers for their ability to record state/data. You would not be able to find a single stateless system in the world -
it would be of no use. In reality the state is stowed into a central database (a bottle-neck!!!). When you work in "stateless"
environment you are really keeping the state somewhere else, for example Google Docs keeps state in the cloud, but there is state. There is always
state and sometimes it is a **GOOD and EXPECTED design to store the state in-memory** on the particular server and use the benefit of
 [**locality of reference**](https://en.wikipedia.org/wiki/Locality_of_reference). The question of state-full or state-less
is really a question of how the state is abstracted and where the state is kept. There is always state in one or another way.

#### The more unit tests-the better
...so your solution is safer

**In Reality**:
Unit tests are software - they may contain bugs. A passing unit test is of no value if it does not assert good business cases
including edge cases. Unit testing which relies on too much mocking is actually testing that mocking as well. So again this is about balance - the system
has to be designed in such a way that testing can be done with the **LEAST amount of mocking** required - this does not mean that classes may not have dependencies,
to the opposite - the dependencies are to be tested first, then composed for more complex "integration" tests. For example, the best practical
**test of database access code** is to NOT use mocking, but have proper **local database deployment scripts and let the tests run on empty local DB**.
It is better to have maintainable amount of tests which **exhaustively test the purpose of every function**. Auto-generated
tests in this regard are absolutely of no practical use. Example: create a unit test for `StrToInt(string)` function - good test coverage should test for:
negative values, values with hex/bin prefixes (if supported), null and empty input, overflown values, values with spaces, especially in between the minus sign etc.
Now consider this `EvaluateExpression(string, val[])` function: `"x<5 || x!=-23".EvaluateExpression(x: 3)` - how many unit tests will it take to cover it? - probably more than 50.
Another important concern of modern unit testing deals with asynchronous processing and multi-threading. Many supposedly thread-safe functions may fail while being called in parallel.


#### Premature Optimization is Bad
...is the root of all evil, as developer spend time and build more complex code for problems that do not exist in reality or are 
not as critical as originally anticipated. This rule is **almost** always true, but not always.
A good example of premature optimization is someone thinking about processor false cache lane sharing while writing a web-service
code that queries employee roster via SQL - those are just incomparable problems on a time scale as the later deals with micro optimizing 
tight code in the nano-second range. However, there are many cases when things need to be carefully optimized up front.

**In Reality**:
one has to be very attentive to details and **consider use-cases ahead of time**, especially while developing libraries. Consider these 
two versions of code:
```CSharp
  if (rowset.RowCount==0) sendNoDataNotification(...);
  -vs -
  if (rowset.IsEmpty) sendNoDataNotification(...);
```
The first line would probably have to fetch 100(s) rows (as an example) in callers memory from the server to get the RowCount, whereas the
second line would not fetch anything or 1 row at most because `IsEmpty` was purposely created to avoid this unneeded fetch all - it is
an optimization property, which logically does exactly the same thing as `RowCount!=0` - the difference is in the how it does it.


#### Black Box/Encapsulation
...is exercised so we should not care about how things are done. Liskov substitution principle says: you should be able to swap B for C if both
do the same thing implementing the same interface.

**In Reality**:
good engineers always use Reflector/ILSpy (or ref source) to see how different classes render their services. For example, 
`ConcurrentDictionary` is built for multi-threaded operations, but did you know that `Dictionary` is actually much more performant
in cases when its reference is swapped atomically in place and mutations are infrequent (so the code is thread safe)? 
The point is: you should always see what is happening inside (especially when writing tight code) and although two classes implement the same interface their side effects may 
lie in the performance spectrum. 


#### Always use Exceptions

...in programming languages with native exception support (C++, Java, C# et.al.) one must not rely on custom function return codes and always
use exceptions as a sole mechanism of error signaling and detection.

**In Reality:**
exceptions work **5x-10x slower** than functions returning flags - this is very important for writing **tight code** such as the code in 
serializers, in-memory data structures (such as nested tries/grids) and other structures with large number of processing elements.
Many times one need to "bail out" from the inner processing loop (e.g. a data cube cell contains null) - and this is a semi-exception as
it is somewhat anticipated. By returning a boolean flag which collapses nested call you can achieve orders of magnitude increase in 
performance. Another example is data validation of models `model.Validate(): Exception`. When you need to validate models at multi-thousand 
rate a second per worker (thread/task) it is better to return exception as an instance, this way you can rethrow them or handle with a simple
 if statement which will be MUCH faster.












----

External resources:
- [Opinionated Framework (SO)](https://stackoverflow.com/questions/802050/what-is-opinionated-software)
- [Pattern: Microservice Chassis](https://microservices.io/patterns/microservice-chassis.html)
- [Service Location (Wikipedia)](https://en.wikipedia.org/wiki/Service_locator_pattern)
- [Dependency Injection (Wikipedia)](https://en.wikipedia.org/wiki/Dependency_injection)
- [IoC with Service Location / Dependency Injection by Martin Fowler](https://martinfowler.com/articles/injection.html)

Back to [Documentation Index](/src/documentation-index.md)