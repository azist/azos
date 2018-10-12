
using Azos.Conf;
using Azos.Apps;

namespace Azos.IO.Net.Gate
{
  /// <summary>
  /// Allow/Deny
  /// </summary>
  public enum GateAction {Deny=0, Allow}

  /// <summary>
  /// Stipulates general contract for nrtwork gates - entities similar to firewall.
  /// Network gates allow/deny in/out traffic based on a set of rules
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
    /// <summary>
    /// Default instance of INetGate implementation that allows all traffic
    /// </summary>
    public static readonly NOPNetGate Instance = new NOPNetGate();

    protected NOPNetGate():base() {}

    public bool Enabled {get{return false;}}


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
