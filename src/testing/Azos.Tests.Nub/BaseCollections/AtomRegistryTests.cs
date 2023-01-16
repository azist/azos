/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;
using System.Threading.Tasks;

using Azos.Collections;
using Azos.Scripting;

namespace Azos.Tests.Nub.BaseCollections
{
  public class AtomNamedClazz : IAtomNamed
  {
    public AtomNamedClazz(Atom name, int data)
    {
      m_Name = name;
      m_Data = data;
    }
    private Atom m_Name;
    private int m_Data;
    public Atom Name { get { return m_Name; } }
    public int Data { get { return m_Data; } }
  }

  public class OrderedAtomClazz : AtomNamedClazz, IOrdered
  {
    public OrderedAtomClazz(Atom name, int order, int data) : base(name, data)
    {
      m_Order = order;
    }
    private int m_Order;
    public int Order { get { return m_Order; } }
  }


  [Runnable]
  public class AtomRegistryTests
  {
    [Run]
    public void Registry()
    {
      var reg = new AtomRegistry<AtomNamedClazz>();

      //WARNING!!!!!

      //Never write real code where you would use Atom.Encode()
      //We are doing it here for unit testing only!!!!!!!!!!
      // In real code, the atoms MUST BE CONSTANTS - that s the whole point of using atoms!!!!!!

      Aver.IsTrue(reg.Register(new AtomNamedClazz(Atom.Encode("Apple"), 1)));
      Aver.IsTrue(reg.Register(new AtomNamedClazz(Atom.Encode("Banana"), 2)));
      Aver.IsFalse(reg.Register(new AtomNamedClazz(Atom.Encode("Apple"), 3)));

      Aver.AreEqual(2, reg.Count);

      Aver.AreEqual(1, reg[Atom.Encode("Apple")].Data);
      Aver.AreEqual(2, reg[Atom.Encode("Banana")].Data);
      Aver.AreObjectsEqual(null, reg[Atom.Encode("Grapes")]);

      Aver.IsFalse(reg.Unregister(new AtomNamedClazz(Atom.Encode("Zizikaka"), 1)));
      Aver.AreEqual(2, reg.Count);

      Aver.IsTrue(reg.Unregister(new AtomNamedClazz(Atom.Encode("Apple"), 1)));
      Aver.AreEqual(1, reg.Count);
      Aver.AreObjectsEqual(null, reg[Atom.Encode("Apple")]);
      Aver.AreEqual(2, reg[Atom.Encode("Banana")].Data);
    }

    [Run]
    public void Registry_UnregisterByName()
    {
      var reg = new AtomRegistry<AtomNamedClazz>();
      Aver.IsTrue(reg.Register(new AtomNamedClazz(Atom.Encode("Apple"), 1)));
      Aver.IsTrue(reg.Register(new AtomNamedClazz(Atom.Encode("Banana"), 2)));
      Aver.IsFalse(reg.Register(new AtomNamedClazz(Atom.Encode("Apple"), 3)));

      Aver.AreEqual(2, reg.Count);

      Aver.AreEqual(1, reg[Atom.Encode("Apple")].Data);
      Aver.AreEqual(2, reg[Atom.Encode("Banana")].Data);
      Aver.AreObjectsEqual(null, reg[Atom.Encode("Grapes")]);

      Aver.IsFalse(reg.Unregister(Atom.Encode("zizikaka")));
      Aver.AreEqual(2, reg.Count);

      Aver.IsTrue(reg.Unregister(Atom.Encode("Apple")));
      Aver.AreEqual(1, reg.Count);
      Aver.AreObjectsEqual(null, reg[Atom.Encode("Apple")]);
      Aver.AreEqual(2, reg[Atom.Encode("Banana")].Data);
    }

    [Run]
    public void Registry_Clear()
    {
      var reg = new AtomRegistry<AtomNamedClazz>();
      Aver.IsTrue(reg.Register(new AtomNamedClazz(Atom.Encode("Apple"), 1)));
      Aver.IsTrue(reg.Register(new AtomNamedClazz(Atom.Encode("Banana"), 2)));
      Aver.IsFalse(reg.Register(new AtomNamedClazz(Atom.Encode("Apple"), 3)));

      Aver.AreEqual(2, reg.Count);

      Aver.AreEqual(1, reg[Atom.Encode("Apple")].Data);
      Aver.AreEqual(2, reg[Atom.Encode("Banana")].Data);
      Aver.AreObjectsEqual(null, reg[Atom.Encode("Grapes")]);

      reg.Clear();

      Aver.AreEqual(0, reg.Count);
      Aver.AreObjectsEqual(null, reg[Atom.Encode("Apple")]);
      Aver.AreObjectsEqual(null, reg[Atom.Encode("Banana")]);
    }

    [Run]
    public void OrderedRegistry()
    {
      var reg = new OrderedAtomRegistry<OrderedAtomClazz>();
      Aver.IsTrue(reg.Register(new OrderedAtomClazz(Atom.Encode("Apple"), 8, 1)));
      Aver.IsTrue(reg.Register(new OrderedAtomClazz(Atom.Encode("Banana"), -2, 2)));
      Aver.IsFalse(reg.Register(new OrderedAtomClazz(Atom.Encode("Apple"), 22, 3)));

      Aver.AreEqual(2, reg.Count);

      Aver.AreEqual(1, reg[Atom.Encode("Apple")].Data);
      Aver.AreEqual(2, reg[Atom.Encode("Banana")].Data);
      Aver.AreObjectsEqual(null, reg[Atom.Encode("Grapes")]);

      var ordered = reg.OrderedValues.ToArray();
      Aver.AreEqual(2, ordered.Length);
      Aver.AreEqual(Atom.Encode("Banana"), ordered[0].Name);
      Aver.AreEqual(Atom.Encode("Apple"), ordered[1].Name);

      Aver.IsTrue(reg.Register(new OrderedAtomClazz(Atom.Encode("Zukini"), 0, 180)));

      ordered = reg.OrderedValues.ToArray();
      Aver.AreEqual(3, ordered.Length);
      Aver.AreEqual(Atom.Encode("Banana"), ordered[0].Name);
      Aver.AreEqual(Atom.Encode("Zukini"), ordered[1].Name);
      Aver.AreEqual(Atom.Encode("Apple"), ordered[2].Name);


      Aver.IsFalse(reg.Unregister(new OrderedAtomClazz(Atom.Encode("zizikaka"), 1, 1)));
      Aver.AreEqual(3, reg.Count);

      Aver.IsTrue(reg.Unregister(new OrderedAtomClazz(Atom.Encode("Apple"), 2, 1)));
      Aver.AreEqual(2, reg.Count);
      Aver.AreObjectsEqual(null, reg[Atom.Encode("Apple")]);
      Aver.AreEqual(2, reg[Atom.Encode("Banana")].Data);
      Aver.AreEqual(180, reg[Atom.Encode("Zukini")].Data);
    }

    [Run]
    public void OrderedRegistry_Clear()
    {
      var reg = new OrderedAtomRegistry<OrderedAtomClazz>();
      Aver.IsTrue(reg.Register(new OrderedAtomClazz(Atom.Encode("Apple"), 8, 1)));
      Aver.IsTrue(reg.Register(new OrderedAtomClazz(Atom.Encode("Banana"), -2, 2)));
      Aver.IsFalse(reg.Register(new OrderedAtomClazz(Atom.Encode("Apple"), 22, 3)));

      var ordered = reg.OrderedValues.ToArray();
      Aver.AreEqual(2, ordered.Length);
      Aver.AreEqual(Atom.Encode("Banana"), ordered[0].Name);
      Aver.AreEqual(Atom.Encode("Apple"), ordered[1].Name);

      reg.Clear();
      Aver.AreEqual(0, reg.Count);
      Aver.AreEqual(0, reg.OrderedValues.Count());
    }

    [Run]
    public void Registry_Parallel()
    {
      var reg = new AtomRegistry<AtomNamedClazz>();

      var CNT = 250000;

      Parallel.For(0, CNT, (i) =>
      {
        reg.Register(new AtomNamedClazz(Atom.Encode("N_{0}".Args(i % 128)), i));
        var item = reg[Atom.Encode("N_{0}".Args(i % 128))];//it may be null
        reg.Unregister(Atom.Encode("N_{0}".Args((i - 2) % 128)));
      });

      Aver.Pass("No exceptions thrown during multithreaded parallel work");
    }

    [Run]
    public void OrderedRegistry_Parallel()
    {
      var reg = new OrderedAtomRegistry<OrderedAtomClazz>();

      var CNT = 250000;

      Parallel.For(0, CNT, (i) =>
      {
        reg.Register(new OrderedAtomClazz(Atom.Encode("N_{0}".Args(i % 128)), i % 789, i));
        var item = reg[Atom.Encode("Name_{0}".Args(i % 128))];//it may be null
         reg.Unregister(Atom.Encode("Name_{0}".Args((i - 2) % 128)));
      });

      Aver.Pass("No exceptions thrown during multithreaded parallel work");
    }

    [Run]
    public void OrderedRegistry_RegisterOrReplace()
    {
      var reg = new OrderedAtomRegistry<OrderedAtomClazz>();
      Aver.IsTrue(reg.Register(new OrderedAtomClazz(Atom.Encode("Apple"), 8, 1)));
      Aver.IsTrue(reg.Register(new OrderedAtomClazz(Atom.Encode("Banana"), -2, 2)));
      Aver.IsTrue(reg.Register(new OrderedAtomClazz(Atom.Encode("Grapes"), 0, 3)));
      Aver.IsFalse(reg.Register(new OrderedAtomClazz(Atom.Encode("Apple"), 22, 12345)));

      var ordered = reg.OrderedValues.ToArray();
      Aver.AreEqual(3, ordered.Length);
      Aver.AreEqual(Atom.Encode("Banana"), ordered[0].Name);
      Aver.AreEqual(Atom.Encode("Grapes"), ordered[1].Name);
      Aver.AreEqual(Atom.Encode("Apple"), ordered[2].Name);

      Aver.AreEqual(1, reg[Atom.Encode("Apple")].Data);

      Aver.IsFalse(reg.RegisterOrReplace(new OrderedAtomClazz(Atom.Encode("Apple"), 22, 12345)));

      ordered = reg.OrderedValues.ToArray();
      Aver.AreEqual(3, ordered.Length);
      Aver.AreEqual(Atom.Encode("Banana"), ordered[0].Name);
      Aver.AreEqual(Atom.Encode("Grapes"), ordered[1].Name);
      Aver.AreEqual(Atom.Encode("Apple"), ordered[2].Name);

      Aver.AreEqual(12345, reg[Atom.Encode("Apple")].Data);//got replaced

      Aver.IsTrue(reg.RegisterOrReplace(new OrderedAtomClazz(Atom.Encode("Peach"), 99, -234)));

      ordered = reg.OrderedValues.ToArray();
      Aver.AreEqual(4, ordered.Length);
      Aver.AreEqual(Atom.Encode("Banana"), ordered[0].Name);
      Aver.AreEqual(Atom.Encode("Grapes"), ordered[1].Name);
      Aver.AreEqual(Atom.Encode("Apple"),  ordered[2].Name);
      Aver.AreEqual(Atom.Encode("Peach"),  ordered[3].Name);

      Aver.AreEqual(12345, reg[Atom.Encode("Apple")].Data);//got replaced before
      Aver.AreEqual(-234, reg[Atom.Encode("Peach")].Data);
    }

    [Run]
    public void OrderedRegistry_GetOrRegister()
    {
      var reg = new OrderedAtomRegistry<OrderedAtomClazz>();

      bool wasAdded;
      var obj1 = reg.GetOrRegister<object>(Atom.Encode("Apple"), (_) => new OrderedAtomClazz(Atom.Encode("Apple"), 8, 1), null, out wasAdded);
      Aver.AreEqual(8, obj1.Order);
      Aver.IsTrue(wasAdded);

      var obj2 = reg.GetOrRegister<object>(Atom.Encode("Yabloko"), (_) => new OrderedAtomClazz(Atom.Encode("Yabloko"), 3, 2), null, out wasAdded);
      Aver.AreEqual(3, obj2.Order);
      Aver.IsTrue(wasAdded);

      Aver.IsFalse(object.ReferenceEquals(obj1, obj2));

      var obj3 = reg.GetOrRegister<object>(Atom.Encode("Apple"), (_) => new OrderedAtomClazz(Atom.Encode("Apple"), 123, 111), null, out wasAdded);
      Aver.AreEqual(8, obj3.Order);
      Aver.IsFalse(wasAdded);

      Aver.IsTrue(object.ReferenceEquals(obj1, obj3));

      var ordered = reg.OrderedValues.ToArray();
      Aver.AreEqual(2, ordered.Length);
      Aver.AreEqual(Atom.Encode("Yabloko"), ordered[0].Name);
      Aver.AreEqual(Atom.Encode("Apple"), ordered[1].Name);
    }

  }
}
