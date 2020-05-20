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
        Aver.IsNotNull(family.People[0].m_Log);

        Aver.AreEqual(1, family.DI_COUNT);
        Aver.AreEqual(1, family.VAL_COUNT);

        Aver.AreEqual(1, family.People[0].DI_COUNT);
        Aver.AreEqual(1, family.People[0].VAL_COUNT);
      }
    }

    [Run]
    public void Recursion_1()
    {
      using (var app = new AzosApplication(null, null))
      {
        var family = new Family();
        family.People = new List<Person>();//via transitive list
        family.People.Add( new Person
         {
           Family = family,
           PersonName = "Alex"
         }
        );
        app.InjectInto(family);//still no errors
        var ve = family.Validate();//still no errors

        Aver.IsNull(ve);
        Aver.IsNotNull(family.m_Log);
        Aver.IsNotNull(family.People[0].m_Log);

        Aver.AreEqual(1, family.DI_COUNT);
        Aver.AreEqual(1, family.VAL_COUNT);

        Aver.AreEqual(1, family.People[0].DI_COUNT);
        Aver.AreEqual(1, family.People[0].VAL_COUNT);
      }
    }

    [Run]
    public void Recursion_2()
    {
      using (var app = new AzosApplication(null, null))
      {
        var family = new Family();
        family.Another = family;

        app.InjectInto(family);//still no errors
        var ve = family.Validate();//still no errors

        Aver.IsNull(ve);
        Aver.IsNotNull(family.m_Log);

        Aver.AreEqual(1, family.DI_COUNT);
        Aver.AreEqual(2, family.VAL_COUNT);//because validate don't lock out the root method, instead it locks out the inner method

        app.InjectInto(family);//still no errors
        ve = family.Validate();//still no errors

        Aver.IsNull(ve);
        Aver.IsNotNull(family.m_Log);

        Aver.AreEqual(2, family.DI_COUNT);
        Aver.AreEqual(4, family.VAL_COUNT);
      }
    }

    [Run]
    public void Recursion_3()
    {
      using (var app = new AzosApplication(null, null))
      {
        var family1 = new Family();
        var family2 = new Family();
        var man1 = new Person
        {
          Family = family1,
          PersonName = "Man1"
        };

        family1.Another = family2;
        family2.Another = family1;
        family1.People = new List<Person>();//via transitive list
        family1.People.Add(man1);//multiple times
        family1.People.Add(man1);
        family1.People.Add(man1);
        family1.People.Add(man1);
        family1.People.Add(man1);

        family2.People = new List<Person>();//via transitive list
        family2.People.Add(man1);//again the same instance



        app.InjectInto(family1);//still no errors
        var ve = family1.Validate();//still no errors

        Aver.IsNull(ve);
        Aver.IsNotNull(family1.m_Log);
        Aver.IsNotNull(family2.m_Log);
        Aver.IsNotNull(man1.m_Log);

        Aver.AreEqual(1, family1.DI_COUNT);
        Aver.AreEqual(1, family2.DI_COUNT);
        Aver.AreEqual(1, man1.DI_COUNT);

        Aver.AreEqual(2, man1.VAL_COUNT);

        app.InjectInto(family2);//still no errors
        ve = family2.Validate();//still no errors

        Aver.IsNull(ve);
        Aver.IsNotNull(family1.m_Log);
        Aver.IsNotNull(family2.m_Log);
        Aver.IsNotNull(man1.m_Log);

        Aver.AreEqual(2, family1.DI_COUNT);
        Aver.AreEqual(2, family2.DI_COUNT);
        Aver.AreEqual(2, man1.DI_COUNT);

        Aver.AreEqual(4, man1.VAL_COUNT);
      }
    }


    public class BAZE : TypedDoc
    {
      public int VAL_COUNT;
      public int DI_COUNT;

      public override ValidState Validate(ValidState state, string scope = null)
      {
        VAL_COUNT++;
        return base.Validate(state, scope);
      }

      protected override bool DoInjectApplication(IApplicationDependencyInjector injector)
      {
        DI_COUNT++;
        return base.DoInjectApplication(injector);
      }

    }

    public class Family : BAZE
    {
      [Inject] internal ILog m_Log;
      [Field] public Family Another { get; set; }
      [Field] public List<Person> People{ get; set;}
    }

    public class Person : BAZE
    {
      [Inject] internal ILog m_Log;
      [Field] public Family Family { get; set; }
      [Field] public string PersonName { get; set; }
    }

  }
}
