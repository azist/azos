/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class DefaultAppVarResolverTests
  {
    [Run]
    public void Test_CoreConsts()
    {
      string got;
      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "CoreConsts." + nameof(CoreConsts.ASSERT_TOPIC), out got));
      Aver.AreEqual(CoreConsts.ASSERT_TOPIC, got);

      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "CoreConsts." + nameof(CoreConsts.UNIT_NAME_CHANNEL), out got));
      Aver.AreEqual(CoreConsts.UNIT_NAME_CHANNEL, got);

      Aver.IsFalse(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "CoreConsts.", out got));
      Aver.IsTrue(got.IsNullOrWhiteSpace());
    }

    [Run]
    public void Test_GUID()
    {
      string got;
      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "INSTANCE", out got));
      Aver.AreEqual( NOPApplication.Instance.InstanceId, got.AsGUID(Guid.Empty) );

      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "INSTANCEX", out got));
      Aver.AreEqual(NOPApplication.Instance.InstanceId, got.AsGUID(Guid.Empty));
    }

    [Run]
    public void Test_FID()
    {
      string got;
      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "FID", out got));
      Aver.IsTrue( got.IsNotNullOrWhiteSpace() );
      got.See();

      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "FIDX", out got));
      Aver.IsTrue(got.IsNotNullOrWhiteSpace());
      got.See();
    }

    [Run]
    public void Test_StartTime()
    {
      string got;
      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, nameof(IApplication.StartTime), out got));
      Aver.IsTrue( (NOPApplication.Instance.StartTime - got.AsDateTime()).TotalSeconds < 1);
    }

    [Run]
    public void Test_NonExisting()
    {
      string got;
      Aver.IsFalse(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "dsfsadfsadfsa", out got));
      Aver.AreEqual("", got);
    }

    [Run]
    public void Test_Counter()
    {
      string got;
      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "Counter.Abc", out got));
      Aver.IsTrue(got.IsNotNullOrWhiteSpace());
      Aver.AreEqual(1, got.AsInt());

      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "Counter.Abc", out got));
      Aver.IsTrue(got.IsNotNullOrWhiteSpace());
      Aver.AreEqual(2, got.AsInt());

      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "Counter.Abc", out got));
      Aver.IsTrue(got.IsNotNullOrWhiteSpace());
      Aver.AreEqual(3, got.AsInt());

      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "Counter.Bac", out got));
      Aver.IsTrue(got.IsNotNullOrWhiteSpace());
      Aver.AreEqual(1, got.AsInt());

      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "Counter.Abc", out got));
      Aver.IsTrue(got.IsNotNullOrWhiteSpace());
      Aver.AreEqual(4, got.AsInt());

      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "Counter.Bubu", out got));
      Aver.IsTrue(got.IsNotNullOrWhiteSpace());
      Aver.AreEqual(1, got.AsInt());

      Aver.IsTrue(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "Counter.Bac", out got));
      Aver.IsTrue(got.IsNotNullOrWhiteSpace());
      Aver.AreEqual(2, got.AsInt());

      Aver.IsFalse(DefaultAppVarResolver.ResolveNamedVar(NOPApplication.Instance, "Counter.", out got));
      Aver.IsTrue(got.IsNullOrWhiteSpace());
    }

  }
}