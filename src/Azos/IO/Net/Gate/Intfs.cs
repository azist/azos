/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Apps;

namespace Azos.IO.Net.Gate
{
  /// <summary>
  /// Allow/Deny
  /// </summary>
  public enum GateAction {Deny=0, Allow}

  /// <summary>
  /// Stipulates general contract for network gates - entities similar to firewall.
  /// Unlike network firewalls, gates allow/deny in/out traffic based on a set of rules
  /// fed of the application state
  /// </summary>
  public interface INetGate : IApplicationComponent
  {

    /// <summary>
    /// When gate is not enabled it allows all traffic bypassing any rules
    /// </summary>
    bool Enabled {get; }

    /// <summary>
    /// Checks whether the specified traffic is allowed or denied
    /// </summary>
    GateAction CheckTraffic(ITraffic traffic);


    /// <summary>
    /// Checks whether the specified traffic is allowed or denied.
    /// Returns the rule that determined the allow/deny outcome or null when no rule matched
    /// </summary>
    GateAction CheckTraffic(ITraffic traffic, out Rule rule);

    /// <summary>
    /// Increases the named variable in the network scope which this specified traffic falls under
    /// </summary>
    void IncreaseVariable(TrafficDirection direction, string address, string varName, int value);

    /// <summary>
    /// Sets the named variable in the network scope which this specified traffic falls under
    /// </summary>
    void SetVariable(TrafficDirection direction, string address, string varName, int value);
  }

  public interface INetGateImplementation : INetGate, IConfigurable
  {

  }


  /// <summary>
  /// Represents an implementation of INetGate that allows all traffic
  /// </summary>
  public class NOPNetGate : ApplicationComponent,  INetGate
  {

    protected NOPNetGate(IApplication app) : base(app) {}

    public bool Enabled => false;

    public override string ComponentLogTopic => CoreConsts.IO_TOPIC;

    public GateAction CheckTraffic(ITraffic traffic)
    {
      return GateAction.Allow;
    }

    public GateAction CheckTraffic(ITraffic traffic, out Rule rule)
    {
      rule = null;
      return GateAction.Allow;
    }

    public void IncreaseVariable(TrafficDirection direction, string address, string varName, int value)
    {
    }

    public void SetVariable(TrafficDirection direction, string address, string varName, int value)
    {
    }
  }

}
