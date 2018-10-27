/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;
using System.Threading.Tasks;

using Azos.Collections;
using Azos.Scripting;

namespace Azos.Tests.Unit.Collections
{

                       public class NamedClazz : INamed
                       {
                         public NamedClazz(string name, int data)
                         {
                           m_Name = name;
                           m_Data = data;
                         }
                         private string m_Name;
                         private int m_Data;
                         public string Name { get { return m_Name; } }
                         public int Data { get { return m_Data; } }
                       }

                       public class OrderedClazz : NamedClazz, IOrdered
                       {
                         public OrderedClazz(string name, int order, int data) : base (name, data)
                         {
                           m_Order = order;
                         }
                         private int m_Order;
                         public int Order{ get { return m_Order; } }
                       }



  [Runnable(TRUN.BASE, 0)]
  public class RegistryTests
  {
    [Run]
    public void Registry()
    {
       var reg = new Registry<NamedClazz>();
       Aver.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Aver.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Aver.IsFalse(  reg.Register( new NamedClazz("Apple", 3) ) );

       Aver.AreEqual(2, reg.Count);

       Aver.AreEqual(1, reg["Apple"].Data);
       Aver.AreEqual(2, reg["Banana"].Data);
       Aver.AreObjectsEqual(null, reg["Grapes"]);

       Aver.IsFalse( reg.Unregister(new NamedClazz("I was never added before", 1)) );
       Aver.AreEqual(2, reg.Count);

       Aver.IsTrue( reg.Unregister(new NamedClazz("Apple", 1)) );
       Aver.AreEqual(1, reg.Count);
       Aver.AreObjectsEqual(null, reg["Apple"]);
       Aver.AreEqual(2, reg["Banana"].Data);
    }


    [Run]
    public void CaseInsensitiv()
    {
       var reg = new Registry<NamedClazz>();//INSENSITIVE
       Aver.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Aver.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Aver.IsFalse(  reg.Register( new NamedClazz("APPLE", 3) ) );

       Aver.AreEqual(2, reg.Count);

       Aver.AreEqual(1, reg["Apple"].Data);
       Aver.AreEqual(2, reg["Banana"].Data);
       Aver.AreEqual(1, reg["APPLE"].Data);

       Aver.IsFalse( reg.Unregister(new NamedClazz("I was never added before", 1)) );
       Aver.AreEqual(2, reg.Count);

       Aver.IsTrue( reg.Unregister(new NamedClazz("ApPle", 1)) );
       Aver.AreEqual(1, reg.Count);
       Aver.AreObjectsEqual(null, reg["Apple"]);
       Aver.AreEqual(2, reg["Banana"].Data);
    }

    [Run]
    public void CaseSensitive()
    {
       var reg = new Registry<NamedClazz>(true);//SENSITIVE!!!!!!!!!!!!!!!!!!!!!!!!!!!
       Aver.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Aver.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Aver.IsTrue(  reg.Register( new NamedClazz("APPLE", 3) ) );

       Aver.AreEqual(3, reg.Count);

       Aver.AreEqual(1, reg["Apple"].Data);
       Aver.AreEqual(2, reg["Banana"].Data);
       Aver.AreEqual(3, reg["APPLE"].Data);

       Aver.IsFalse( reg.Unregister(new NamedClazz("I was never added before", 1)) );
       Aver.AreEqual(3, reg.Count);

       Aver.IsFalse( reg.Unregister(new NamedClazz("AppLE", 1)) );
       Aver.AreEqual(3, reg.Count);
       Aver.IsTrue( reg.Unregister(new NamedClazz("APPLE", 3)) );
       Aver.AreEqual(2, reg.Count);
       Aver.AreEqual(1, reg["Apple"].Data);
       Aver.AreEqual(2, reg["Banana"].Data);
       Aver.AreObjectsEqual(null, reg["APPLE"]);
    }


    [Run]
    public void Registry_UnregisterByName()
    {
       var reg = new Registry<NamedClazz>();
       Aver.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Aver.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Aver.IsFalse(  reg.Register( new NamedClazz("Apple", 3) ) );

       Aver.AreEqual(2, reg.Count);

       Aver.AreEqual(1, reg["Apple"].Data);
       Aver.AreEqual(2, reg["Banana"].Data);
       Aver.AreObjectsEqual(null, reg["Grapes"]);

       Aver.IsFalse( reg.Unregister("I was never added before") );
       Aver.AreEqual(2, reg.Count);

       Aver.IsTrue( reg.Unregister("Apple") );
       Aver.AreEqual(1, reg.Count);
       Aver.AreObjectsEqual(null, reg["Apple"]);
       Aver.AreEqual(2, reg["Banana"].Data);
    }

    [Run]
    public void Registry_Clear()
    {
       var reg = new Registry<NamedClazz>();
       Aver.IsTrue(  reg.Register( new NamedClazz("Apple", 1) ) );
       Aver.IsTrue(  reg.Register( new NamedClazz("Banana", 2) ) );
       Aver.IsFalse(  reg.Register( new NamedClazz("Apple", 3) ) );

       Aver.AreEqual(2, reg.Count);

       Aver.AreEqual(1, reg["Apple"].Data);
       Aver.AreEqual(2, reg["Banana"].Data);
       Aver.AreObjectsEqual(null, reg["Grapes"]);

       reg.Clear();

       Aver.AreEqual(0, reg.Count);
       Aver.AreObjectsEqual(null, reg["Apple"]);
       Aver.AreObjectsEqual(null, reg["Banana"]);
    }



    [Run]
    public void OrderedRegistry()
    {
       var reg = new OrderedRegistry<OrderedClazz>();
       Aver.IsTrue(  reg.Register( new OrderedClazz("Apple",  8,  1) ) );
       Aver.IsTrue(  reg.Register( new OrderedClazz("Banana", -2,  2) ) );
       Aver.IsFalse(  reg.Register( new OrderedClazz("Apple", 22, 3) ) );

       Aver.AreEqual(2, reg.Count);

       Aver.AreEqual(1, reg["Apple"].Data);
       Aver.AreEqual(2, reg["Banana"].Data);
       Aver.AreObjectsEqual(null, reg["Grapes"]);

       var ordered = reg.OrderedValues.ToArray();
       Aver.AreEqual(2, ordered.Length);
       Aver.AreEqual("Banana", ordered[0].Name);
       Aver.AreEqual("Apple", ordered[1].Name);

       Aver.IsTrue( reg.Register( new OrderedClazz("Zukini", 0, 180) )  );

       ordered = reg.OrderedValues.ToArray();
       Aver.AreEqual(3, ordered.Length);
       Aver.AreEqual("Banana", ordered[0].Name);
       Aver.AreEqual("Zukini", ordered[1].Name);
       Aver.AreEqual("Apple", ordered[2].Name);


       Aver.IsFalse( reg.Unregister(new OrderedClazz("I was never added before", 1, 1)) );
       Aver.AreEqual(3, reg.Count);

       Aver.IsTrue( reg.Unregister(new OrderedClazz("Apple", 2, 1)) );
       Aver.AreEqual(2, reg.Count);
       Aver.AreObjectsEqual(null, reg["Apple"]);
       Aver.AreEqual(2, reg["Banana"].Data);
       Aver.AreEqual(180, reg["Zukini"].Data);
    }

    [Run]
    public void OrderedRegistry_Clear()
    {
       var reg = new OrderedRegistry<OrderedClazz>();
       Aver.IsTrue(  reg.Register( new OrderedClazz("Apple",  8,  1) ) );
       Aver.IsTrue(  reg.Register( new OrderedClazz("Banana", -2,  2) ) );
       Aver.IsFalse(  reg.Register( new OrderedClazz("Apple", 22, 3) ) );

       var ordered = reg.OrderedValues.ToArray();
       Aver.AreEqual(2, ordered.Length);
       Aver.AreEqual("Banana", ordered[0].Name);
       Aver.AreEqual("Apple", ordered[1].Name);

       reg.Clear();
       Aver.AreEqual(0, reg.Count);
       Aver.AreEqual(0, reg.OrderedValues.Count());


    }


    [Run]
    public void Registry_Parallel()
    {
       var reg = new Registry<NamedClazz>();

       var CNT = 250000;

       Parallel.For(0, CNT, (i)=>
       {
           reg.Register(new NamedClazz("Name_{0}".Args(i % 128), i));
           var item = reg["Name_{0}".Args(i % 128)];//it may be null
           reg.Unregister("Name_{0}".Args((i-2) %128));
       });

       Aver.Pass("No exceptions thrown during multithreaded parallel work");
    }



    [Run]
    public void OrderedRegistry_Parallel()
    {
       var reg = new OrderedRegistry<OrderedClazz>();

       var CNT = 250000;

       Parallel.For(0, CNT, (i)=>
       {
           reg.Register(new OrderedClazz("Name_{0}".Args(i % 128), i % 789, i));
           var item = reg["Name_{0}".Args(i % 128)];//it may be null
           reg.Unregister("Name_{0}".Args((i-2) %128));
       });

       Aver.Pass("No exceptions thrown during multithreaded parallel work");
    }




    [Run]
    public void OrderedRegistry_RegisterOrReplace()
    {
       var reg = new OrderedRegistry<OrderedClazz>();
       Aver.IsTrue(  reg.Register( new OrderedClazz("Apple",  8,  1) ) );
       Aver.IsTrue(  reg.Register( new OrderedClazz("Banana", -2,  2) ) );
       Aver.IsTrue(  reg.Register( new OrderedClazz("Grapes", 0,  3) ) );
       Aver.IsFalse(  reg.Register( new OrderedClazz("Apple", 22, 12345) ) );

       var ordered = reg.OrderedValues.ToArray();
       Aver.AreEqual(3, ordered.Length);
       Aver.AreEqual("Banana", ordered[0].Name);
       Aver.AreEqual("Grapes", ordered[1].Name);
       Aver.AreEqual("Apple", ordered[2].Name);

       Aver.AreEqual( 1, reg["Apple"].Data);

       Aver.IsFalse(  reg.RegisterOrReplace( new OrderedClazz("Apple", 22, 12345) ) );

       ordered = reg.OrderedValues.ToArray();
       Aver.AreEqual(3, ordered.Length);
       Aver.AreEqual("Banana", ordered[0].Name);
       Aver.AreEqual("Grapes", ordered[1].Name);
       Aver.AreEqual("Apple", ordered[2].Name);

       Aver.AreEqual( 12345, reg["Apple"].Data);//got replaced


       Aver.IsTrue(  reg.RegisterOrReplace( new OrderedClazz("Peach", 99, -234) ) );

       ordered = reg.OrderedValues.ToArray();
       Aver.AreEqual(4, ordered.Length);
       Aver.AreEqual("Banana", ordered[0].Name);
       Aver.AreEqual("Grapes", ordered[1].Name);
       Aver.AreEqual("Apple", ordered[2].Name);
       Aver.AreEqual("Peach", ordered[3].Name);

       Aver.AreEqual( 12345, reg["Apple"].Data);//got replaced before
       Aver.AreEqual( -234, reg["Peach"].Data);

    }


    [Run]
    public void OrderedRegistry_GetOrRegister()
    {
       var reg = new OrderedRegistry<OrderedClazz>();

       bool wasAdded;
       var obj1 = reg.GetOrRegister<object>("Apple", (_) => new OrderedClazz("Apple",  8,  1), null, out wasAdded);
       Aver.AreEqual( 8, obj1.Order );
       Aver.IsTrue( wasAdded );

       var obj2 = reg.GetOrRegister<object>("Yabloko", (_) => new OrderedClazz("Yabloko",  3,  2), null, out wasAdded);
       Aver.AreEqual( 3, obj2.Order );
       Aver.IsTrue( wasAdded );

       Aver.IsFalse( object.ReferenceEquals( obj1, obj2 ) );

       var obj3 = reg.GetOrRegister<object>("Apple", (_) => new OrderedClazz("Apple",  123,  111), null, out wasAdded);
       Aver.AreEqual( 8, obj3.Order );
       Aver.IsFalse( wasAdded );

       Aver.IsTrue( object.ReferenceEquals( obj1, obj3 ) );


       var ordered = reg.OrderedValues.ToArray();
       Aver.AreEqual(2, ordered.Length);
       Aver.AreEqual("Yabloko", ordered[0].Name);
       Aver.AreEqual("Apple", ordered[1].Name);
    }


  }
}
