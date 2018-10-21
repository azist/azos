
using Azos.Apps;
using Azos.Conf;

namespace Azos.Web.Shipping
{
  /// <summary>
  /// Represents a process-global host for a shipping systems
  /// </summary>
  public class ShippingSystemHost : ServiceWithInstrumentationBase<object>, IShippingSystemHostImplementation
  {
    #region .ctor

      protected ShippingSystemHost(string name, IConfigSectionNode node) : this(name, node, null) { }

      protected ShippingSystemHost(string name, IConfigSectionNode node, object director) : base(director)
      {
        if (node != null) Configure(node);
        if (name.IsNotNullOrWhiteSpace()) this.Name = name;
      }

    #endregion

    #region Properties

      public override bool InstrumentationEnabled
      {
        get { return false; }
        set { }
      }

    #endregion
  }
}
