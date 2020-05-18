using System;
using System.Collections.Generic;
using System.Text;
using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Data;
using Azos.Log;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class DocRecursiveGraphTests
  {
    [Run]
    public void NoRecursion()
    {
      using(var app = new AzosApplication(null, null))
      {
        var family = new Family{ People = new List<Person>{ new Person{ Family = null, PersonName = "Alex"} } };
        app.InjectInto(family);//no errors
        var ve = family.Validate();//no errors
        Aver.IsNull(ve);
        Aver.IsNotNull(family.m_Log);
      }
    }

    [Run]
    public void Recursion()
    {
      using (var app = new AzosApplication(null, null))
      {
        var family = new Family();
        family.People = new List<Person>();
        family.People.Add( new Person
         {
           Family = family,
           PersonName = "Alex"
         }
        );
      //  app.InjectInto(family);//still no errors
        var ve = family.Validate();//still no errors

        Aver.IsNull(ve);
        Aver.IsNotNull(family.m_Log);
      }
    }

    public class Family : TypedDoc
    {
      [Inject] public ILog m_Log;
      [Field] public List<Person> People{ get; set;}
    }

    public class Person : TypedDoc
    {
      [Field] public Family Family { get; set; }
      [Field] public string PersonName { get; set; }
    }

  }
}
