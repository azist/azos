# Azos Design Philosophy
Back to [Documentation Index](/src/documentation-index.md)

This section describes design choices and patterns used in Azos framework. We also touch upon
a few common software myths and truisms.


## Overview
Azos project originated as **purposely "opinionated"** framework [~what is opinionated software?~](https://stackoverflow.com/questions/802050/what-is-opinionated-software)
as it prescribes **a certain way of application structuring**. This is done to reap the benefit of
 simplicity of the SDLC as a whole, and short time to market while writing real-world complex business/data-driven applications.

The opinion described in this paragraph has been forming for the past 15+ years based on observations of corporate code bases in 10s of large and medium scale organizations
having 10s to 100s of developers working on those systems. It has been concluded that **modern framework landscape is still of a too low level
for average developer to grasp and apply properly**. Another detrimental factor is **fragmentation and too many choices**. I have seen solutions
using 3 different logging frameworks in a single console app. Multiple iOC containers in the same web system. The saddest thing - they think
it is ~~**good and "eclectic" to have so "many tools"**~~. If only could the companies(business) comprehend the technology enough to straighten those "technologists" out. 

Simply put, there is a **lot of puristic BS in modern frameworks** that try to fit all possible combinations at the expense of verbose code and
overall complexity. Lets take DI as an example. 

## Problems with General-Purpose Object Allocations
DI is good, in general, what is not good is that in the hands of inapt developers general purpose DI availability creates 
a temptation to allocate too many objects for tasks which do not need object instances in principle. For example, the following 
lists objects that need to get allocated in a typical app:

- App component graph - allocated by the system at start (system code)
- Logging - allocated at app start (system code)
- Instrumentation - allocated at app start (system code)
- Data Store/Access Repository -  allocated at app start (system code)
- Security Manager - allocated at app start (system code)
- Domain-specific business logic - modules allocated at app start (system code)
- Web call POST/PUT payload -> Domain model - allocated by MVC controller/model binder (system code)
- Data Access model - allocated by datastore/access layer (system code)
- Custom data structures/Nodes - allocated by user code
- Domain model object - allocated by datastore/object mapper (system code mostly) and user code
- Deserialization: stream-> object: - allocated by serializer (system code)

As we can see in the list above, most of the object allocations are done by the system(framework) code. Business logic 
developers should concentrate on implementing:

- Domain models - objects that reflect business (e.g. User, Account, Doctor etc...)
- Domain logic - objects that contain business rules. The practice has shown that the majority of business logic is linear and procedural, hence it makes sense to put those functions in "logic modules" that get loaded in the app context and used a s a "service"

Business logic is mostly procedural in its core, and it takes "business models", validates them and transacts
the db/store/queue.

It became apparent that **frivolous object allocations should NOT be encouraged** altogether in business logic.




## A few Software Engineering Truisms
Most "well known" facts and principles such as: **SOLID**, data **normalization**, IoC etc. are rooted in puristic world view and
actually pose a great threat to the project when applied carelessly, without proper engineering experience. Unfortunately **there are
no rules that can replace the true engineering experience and sheer talent**. A good solutions is **always a balance in the mix of good 
software engineering techniques** (some mentioned above) **and practicality**, where one must know what this or that would take in
real life to write and maintain (including test/retest).

Does this mean that all design patterns and well-known software engineering principles are bad? Absolutely not! As a matter of fact,
those practices hold true for 99% of cases, however you must be careful and **not blindly follow the rules** as your case
may not be the best case for those rules.

A few examples of software "truisms" which are usually taken as incontrovertible facts of life/nature:

#### In OOP everything should be a class with "Single" responsibility
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
 [**locality of reference**](https://en.wikipedia.org/wiki/Locality_of_reference).

#### The more unit tests-the better
...so your solution is safer

**In Reality**:
Unit tests are software - they may contain bugs. A passing unit test is of no value if it does not assert good business cases
including edge cases. Unit testing which relies on too much mocking is actually testing that mocking as well. So again this is about balance - the system
has to be designed in such a way that testing can be done with the **LEAST amount of mocking** required - this does not mean that classes may not have dependencies,
to the opposite - the dependencies are to be tested first the composed for more complex "integration" tests. For example, the best practical
**test of database access code** is to NOT use mocking, but have proper **local database deployment scripts and let the tests run on empty local DB**.
It is better to have maintainable amount of tests which **exhaustively test the purpose of every function**. Auto-generated
tests in this regard are absolutely of no practical use. Example: create a unit test for `StrToInt(string)` function - good test coverage should test for:
negative values, values with hex/bin prefixes (if supported), null and empty input, overflown values, values with spaces, especially in between the minus sign etc.
Now consider this `EvaluateExpression(string, val[])` function: `"x<5 || x!=-23".EvaluateExpression(x: 3)` - how many unit tests will it take to cover it? - probably more than 50.
Another important concern of modern unit testing deals with asynchronous processing and multi-threading. Many supposedly thread-safe functions may fail while called in parallel.


#### Premature Optimization is Bad
...is the root of all evil, as developer spend time and build more complex code for problems that do not exist in reality or are 
not as critical as originally anticipated. This rule is **almost** always true, but not always.
A good example of premature optimization is someone thinking about processor false cache lane sharing while writing a web-service
code that queries employee roster via SQL - those are just incomparable problems on a time scale as the later deals with micro optimizing 
tight code in the nano-second range. However, there are many cases when things need to be carefully optimized up front.

**In Reality**:
one has to be very attentive to details and think ahead of possible use-cases, especially while developing libraries. Consider these 
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
in cases when its reference is swapped atomically in place (so the code is thread safe)? The point is: you should always see what is happening inside
 and although two classes implement the same interface their side effects may lie in the performance spectrum. 


#### Always use Exceptions

...in programming languages with native exception support (C++, Java, C# et.al.) one must not rely on custom function return codes and always
use exceptions as a sole mechanism of error signaling and detection.

**In Reality:**
exceptions work **5x-10x slower** than functions returning flags - this is very important for writing **tight code** such as the one in 
serializers, in-memory data structures (such as nested tries/grids) and other structures with large number of processing elements.
Many times one need to "bail out" from the inner processing loop (e.g. a data cube cell contains null) - and this is a semi-exception as
it is somewhat anticipated. By returning a boolean flag which collapses nested call you can achieve orders of magnitude increase in 
performance.












----

External resources:
- [Opinionated Framework (SO)](https://stackoverflow.com/questions/802050/what-is-opinionated-software)
- [Pattern: Microservice Chassis](https://microservices.io/patterns/microservice-chassis.html)
- [Service Location (Wikipedia)](https://en.wikipedia.org/wiki/Service_locator_pattern)
- [Dependency Injection (Wikipedia)](https://en.wikipedia.org/wiki/Dependency_injection)
- [IoC with Service Location / Dependency Injection by Martin Fowler](https://martinfowler.com/articles/injection.html)

Back to [Documentation Index](/src/documentation-index.md)