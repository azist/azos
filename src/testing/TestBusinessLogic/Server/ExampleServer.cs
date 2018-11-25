/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos;
using Azos.Log;
using System;

namespace TestBusinessLogic.Server
{
  /// <summary>
  /// Provides implementation for IExampleContract used for testing
  /// </summary>
  [Azos.Glue.ThreadSafe]
  public class ExampleServer : IExampleContract // no inheritance from special type is needed, just implement the contract
  {
    //used by serializer (if the contract is stateful)
    private ExampleServer() { }

    //used manually for unit testing, this .ctor will never be called by Glue
    //instead of IApplication you could inject whatever particular dependency you need, e.g. IMyAccountingLogic
    //public ExampleServer(IMyAccountingLogic accounting) => m_Accounting = accounting;
    public ExampleServer(IApplication app) => m_App = app;

    //Mark as [NonSer] just in case this becomes a stateful server ever
    [NonSerialized] private IApplication m_App;

    //instead of IApplication you could inject whatever particular dependency you need, e.g. IMyAccountingLogic
    //e.g. use Azos.Glue.ServerCall.App.GetAccountingLogic() extension method to return a custom typed IMyAccountingLogic:
    //public IMyAccountingLogic Accounting => m_Accounting ?? Azos.Glue.ServerCall.App.GetAccountingLogic();
    public IApplication App => m_App ?? (m_App = Azos.Glue.ServerCall.App);

    //In this business-logic-oriented method we may need dependencies. Those dependencies
    //are to be used via properties resolvable via App chassis ambient context
    public object ExampleMethod(string name)
    {
      //return Accounting.GetBalance(name);
      App.Log.Write( new Message{ Text = name });
      return name;
    }
  }
}
