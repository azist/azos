/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Web.Shipping.Manual
{
  public class ManualSession : ShippingSession
  {
    public ManualSession(ShippingSystem shipSystem, ManualConnectionParameters cParams)
      : base(shipSystem, cParams)
    {
      m_ConnectionParams = cParams;
    }

    private readonly ManualConnectionParameters m_ConnectionParams;

    public ManualConnectionParameters ConnectionParams { get { return m_ConnectionParams; } }
  }
}
