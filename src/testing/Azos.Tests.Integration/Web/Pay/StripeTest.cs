/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Apps;
using Azos.Scripting;
using Azos.Web;
using Azos.Web.Pay;
using Azos.Web.Pay.Stripe;

namespace Azos.Tests.Integration.Web.Pay
{
  [Runnable]
  public class StripeTest : ExternalCfg
  {
    [Run]
    public void Charge()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCommon(sess);
        }
      }
    }

    [Run]
    [Aver.Throws(typeof(PaymentStripeException), Message = "declined")]
    public void ChargeCardDeclined()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardDeclined(sess);
        }
      }
    }

    [Run]
    [Aver.Throws(typeof(PaymentStripeException), Message = "card number is incorrect")]
    public void ChargeCardLuhnErr()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardLuhnErr(sess);
        }
      }
    }

    [Run]
    [Aver.Throws(typeof(PaymentStripeException), Message = "expiration year is invalid")]
    public void ChargeCardExpYearErr()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardExpYearErr(sess);
        }
      }
    }

    [Run]
    [Aver.Throws(typeof(PaymentStripeException), Message = "expiration month is invalid")]
    public void ChargeCardExpMonthErr()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardExpMonthErr(sess);
        }
      }
    }

    [Run]
    [Aver.Throws(typeof(PaymentStripeException), Message = "security code is invalid")]
    public void ChargeCardVCErr()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeCardVCErr(sess);
        }
      }
    }

    //   refund reason
    //   actualaccountdata: +zip
    //   charge: + other address attributes
    [Run]
    public void ChargeWithBillingAddressInfo()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = getPaySystem();

        using (var sess = ps.StartSession())
        {
          PayTestCommon.ChargeWithBillingAddressInfo(sess);
        }
      }
    }

    [Run]
    public void CaptureImplicitTotal()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.CaptureImplicitTotal(pss);
        }
      }
    }

    [Run]
    public void CaptureExplicitTotal()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.CaptureExplicitTotal(pss);
        }
      }
    }

    [Run]
    public void CapturePartial()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.CapturePartial(pss);
        }
      }
    }

    [Run]
    public void RefundFullImplicit()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.RefundFullImplicit(pss);
        }
      }
    }

    [Run]
    public void RefundFullExplicit()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.RefundFullExplicit(pss);
        }
      }
    }

    [Run]
    public void RefundFullTwoParts()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.RefundFullTwoParts(pss);
        }
      }
    }

    [Run]
    public void TransferToBank()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.TransferToBank(pss);
        }
      }
    }

    [Run]
    public void TransferToCard()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.TransferToCard(pss);
        }
      }
    }

    [Run]
    public void TransferToCardWithBillingAddressInfo()
    {
      var conf = LACONF.AsLaconicConfig();

      using (new ServiceBaseApplication(null, conf))
      {
        var ps = PaySystem.Instances["stripe"];
        using (var pss = ps.StartSession())
        {
          PayTestCommon.TransferToCardWithBillingAddressInfo(pss);
        }
      }
    }

    private PaySystem getPaySystem()
    {
      var paymentSection = LACONF.AsLaconicConfig()[WebSettings.CONFIG_WEBSETTINGS_SECTION][Azos.Web.Pay.PaySystem.CONFIG_PAYMENT_PROCESSING_SECTION];

      var stripeSection = paymentSection.Children.First(p => p.AttrByName("name").Value == "Stripe");

      var ps = PaySystem.Make<StripeSystem>(null, stripeSection);

      return ps;
    }
  }
}
