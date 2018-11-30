/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos;
using Azos.Log;
using Azos.Apps.Injection;

namespace TestBusinessLogic.Server
{
  /// <summary>
  /// Provides implementation for IExampleContract used for testing
  /// </summary>
  public class ExampleServer : IExampleContract // no inheritance from special type is needed, just implement the contract
  {
    //used by serializer (if the contract is stateful)
    private ExampleServer() { }

    //used manually for unit testing, this .ctor will never be called by Glue
    //instead of IApplication you could inject whatever particular dependency you need, e.g. IMyAccountingLogic
    //public ExampleServer(IMyAccountingLogic accounting) => m_Accounting = accounting;
    public ExampleServer(IApplication app, ILog log)
    {
      m_App = app;
      m_Log = log;
    }

    //Mark as [NonSer] just in case this becomes a stateful server ever
    [Inject] private IApplication m_App;
    [Inject] private ILog m_Log;
    //[Inject] private IAccountingModule m_Accounting;
    //[InjectModule] private IAccountingModule m_Accounting;
    //[Inject(Name="AccountingLogic")] private IAccountingModule m_Accounting;
    //[Inject(Type=typeof(IUSAccountingLoogic)] private IAccountingModule m_Accounting;

    //In this business-logic-oriented method we may need dependencies
    public object ExampleMethod(string name)
    {
      //return Accounting.GetBalance(name);
      m_Log.Write( new Message{ Text = name });
      //m_Log.Write( new Message { Text = m_Accounting.GetBalance(name) });
      return name;
    }

    public SimplePersonDoc ProcessPerson(SimplePersonDoc person)
    {
      var error = person.Validate(m_App);//notice the use of extension method
      if (error!=null) throw error;
      return person;
    }
  }

}
