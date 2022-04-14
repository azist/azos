/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading;
using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Scripting.Dsl
{
  /// <summary>
  /// Emits a log message
  /// </summary>
  public sealed class Log : Step
  {
    public Log(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx){ }

    [Config] public Azos.Log.MessageType MsgType{ get; set;}
    [Config] public string From { get; set; }
    [Config] public string Text { get; set; }
    [Config] public string Pars { get; set; }


    protected override string DoRun(JsonDataMap state)
    {
      WriteLog(MsgType, Eval(From, state), Eval(Text, state), null, null, Eval(Pars, state));
      return null;
    }
  }

  /// <summary>
  /// Emits a log message
  /// </summary>
  public sealed class See : Step
  {
    public See(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public string Text { get; set; }
    [Config] public string Format { get; set; }


    protected override string DoRun(JsonDataMap state)
    {
      if (Text.IsNotNullOrWhiteSpace()) Conout.See(Eval(Text, state));

      if (Format.IsNotNullOrWhiteSpace())
      {
        var got = StepRunnerVarResolver.FormatString(Eval(Format, state), Runner, state);
        Conout.See(got);
      }
      return null;
    }
  }

  /// <summary>
  /// Runs a step with a delay in seconds
  /// </summary>
  public sealed class Delay : Step
  {
    public Delay(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public string Seconds { get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      var secTimeout = Eval(Seconds, state).AsDouble(0.0);
      if (secTimeout <= 0.0) secTimeout = 1.0;

      var time = Timeter.StartNew();

      while (time.ElapsedSec < secTimeout && Runner.IsRunning)
        Thread.Sleep(50);

      return null;
    }
  }


  /// <summary>
  /// Loads a module and resolves dependencies
  /// </summary>
  public sealed class LoadModule : Step
  {
    public const string CONFIG_MODULE_SECTION = "module";

    /// <summary>
    /// Tries to find a module of a specified type with optional name on a call stack of frames.
    /// Returns null if such module is not found
    /// </summary>
    public static TModule TryGet<TModule>(string name = null) where TModule : class, IModule
     => StepRunner.Frame.Current?.All.FirstOrDefault(o => (o is TModule m) && (name.IsNullOrWhiteSpace() || m.Name.EqualsOrdIgnoreCase(name))) as TModule;

    /// <summary>
    /// Tries to find a module of a specified type with optional name on a call stack of frames.
    /// Throws if such module is not found and dependency could not be satisfied
    /// </summary>
    public static TModule Get<TModule>(string name = null) where TModule : class, IModule
     => TryGet<TModule>(name).NonNull("Satisfied dependency on `{0}('{1}')` loaded by `{2}` step".Args(
                             typeof(TModule).DisplayNameWithExpandedGenericArgs(),
                             name.Default("<null>"),
                             nameof(LoadModule)));


    public LoadModule(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config(CONFIG_MODULE_SECTION)] public IConfigSectionNode Module { get; set; }

    protected override string DoRun(JsonDataMap state)
    {
      Module.NonEmpty(CONFIG_MODULE_SECTION);
      var module = FactoryUtils.MakeAndConfigureComponent<IModuleImplementation>(App, Module);
      module.ApplicationAfterInit();
      StepRunner.Frame.Current.Owned.Add(module);
      return null;
    }
  }

  /// <summary>
  /// Impersonates a session with credentials
  /// </summary>
  public class Impersonate : Step
  {
    public Impersonate(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public string Id { get; set; }
    [Config] public string Pwd { get; set; }
    [Config] public string Auth { get; set; }

    /// <summary>
    /// Override to create custom impersonation session type. By default BaseSession is used.
    /// You can also override this to set specific session context before injecting it into ExecutionContext
    /// </summary>
    protected virtual ISession MakeImpersonationSession() => new BaseSession(Guid.NewGuid(), App.Random.NextRandomUnsignedLong);


    protected override string DoRun(JsonDataMap state)
    {
      var credentials = Auth.IsNotNullOrWhiteSpace() ? IDPasswordCredentials.FromBasicAuth(Eval(Auth, state))
                                                     : new IDPasswordCredentials(Eval(Id, state),
                                                                                 Eval(Pwd, state));

      var user = App.SecurityManager.Authenticate(credentials);
      var session = MakeImpersonationSession();
      session.User = user;
      Azos.Apps.ExecutionContext.__SetThreadLevelSessionContext(session);
      return null;
    }
  }

  /// <summary>
  /// Sets ambient session data context name
  /// </summary>
  public class SetDataContextName : Step
  {
    public SetDataContextName(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config] public string DataContext { get; set; }

    /// <summary>
    /// Override to create custom impersonation session type. By default BaseSession is used.
    /// You can also override this to set specific session context before injecting it into ExecutionContext
    /// </summary>
    protected virtual ISession MakeImpersonationSession() => new BaseSession(Guid.NewGuid(), App.Random.NextRandomUnsignedLong);


    protected override string DoRun(JsonDataMap state)
    {
      var session = Ambient.CurrentCallSession;
      if (session is NOPSession)
      {
        session = MakeImpersonationSession();
        Azos.Apps.ExecutionContext.__SetThreadLevelSessionContext(session);
      }
      session.DataContextName = Eval(DataContext, state);

      return null;
    }
  }

}
