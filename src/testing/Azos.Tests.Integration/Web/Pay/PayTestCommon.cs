/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Web.Pay;

namespace Azos.Tests.Integration.Web.Pay
{
  public class PayTestCommon
  {
    #region Tests
      public static void ChargeCommon(PaySession sess)
      {
        Aver.IsNotNull(sess);

        var ta = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new Azos.Financial.Amount("usd", 15.75M), true, "test payment");

        Aver.IsNotNull(ta);
      }

      public static void ChargeCardDeclined(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_DECLINED, Account.EmptyInstance,
          new Azos.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardLuhnErr(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_LUHN_ERR, Account.EmptyInstance,
          new Azos.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardExpYearErr(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_EXP_YEAR_ERR, Account.EmptyInstance,
          new Azos.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardExpMonthErr(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_EXP_MONTH_ERR, Account.EmptyInstance,
            new Azos.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeCardVCErr(PaySession sess)
      {
        var ta = sess.Charge(FakePaySystemHost.CARD_CVC_ERR, Account.EmptyInstance,
            new Azos.Financial.Amount("usd", 15.75M), true, "test payment");
      }

      public static void ChargeWithBillingAddressInfo(PaySession sess)
      {
        Aver.IsNotNull(sess);

        var ta = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS, Account.EmptyInstance,
          new Azos.Financial.Amount("usd", 15.75M), true, "test payment");

        Aver.IsNotNull(ta);
      }

      public static void CaptureImplicitTotal(PaySession sess)
      {
        var amount = new Azos.Financial.Amount("usd", 17.25M);
        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT,
          amount, false, "test payment");

        Aver.AreEqual(new Azos.Financial.Amount("usd", .0M), charge.AmountCaptured);

        sess.Capture(charge);

        Aver.AreEqual(amount, charge.AmountCaptured);
      }

      public static void CaptureExplicitTotal(PaySession sess)
      {
        var amount = new Azos.Financial.Amount("usd", 17.25M);

        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, amount, false, "test payment");

        Aver.AreEqual(new Azos.Financial.Amount("usd", .0M), charge.AmountCaptured);

        sess.Capture(charge, amount.Value);

        Aver.AreEqual(amount, charge.AmountCaptured);
      }

      public static void CapturePartial(PaySession sess)
      {
        var chargeAmount = new Azos.Financial.Amount("usd", 17.25M);

        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT,
          chargeAmount, false, "test payment");

        Aver.AreEqual(new Azos.Financial.Amount("usd", .0M), charge.AmountCaptured);

        var captureAmount = 10.00M;
        sess.Capture(charge, amount: captureAmount);

        Aver.AreObjectsEqual(captureAmount, charge.AmountCaptured);
      }

      public static void RefundFullImplicit(PaySession sess)
      {
        var amountToRefund = new Azos.Financial.Amount("usd", 17.25M);

        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance, amountToRefund, true, "test payment");

        Aver.AreEqual(new Azos.Financial.Amount("usd", .0M), charge.AmountRefunded);

        sess.StoreTransaction(charge);

        sess.Refund(charge);

        Aver.AreEqual(amountToRefund, charge.AmountRefunded);
      }

      public static void RefundFullExplicit(PaySession sess)
      {
        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new Azos.Financial.Amount("usd", 20.00M), true, "Refund Full Explicit Charge");

        sess.Refund(charge, 20.00M);
      }

      public static void RefundFullTwoParts(PaySession sess)
      {
        var charge = sess.Charge(FakePaySystemHost.CARD_ACCOUNT_STRIPE_CORRECT, Account.EmptyInstance,
          new Azos.Financial.Amount("usd", 20.00M), true, "Refund Full Explicit Charge");
        sess.Refund(charge, 15.00M, "fraudulent");
        sess.Refund(charge, 5.00M, "requested_by_customer");
      }

      public static void TransferToBank(PaySession sess)
      {
        var transferTA = sess.Transfer(Account.EmptyInstance, FakePaySystemHost.BANK_ACCOUNT_STRIPE_CORRECT, new Azos.Financial.Amount("usd", 183.90M));

        Aver.IsNotNull(transferTA);
      }

      public static void TransferToCard(PaySession sess)
      {
          var transferTA = sess.Transfer(Account.EmptyInstance, FakePaySystemHost.CARD_DEBIT_ACCOUNT_STRIPE_CORRECT,
            new Azos.Financial.Amount("usd", 27.00M));

          Aver.IsNotNull(transferTA);
      }

      public static void TransferToCardWithBillingAddressInfo(PaySession sess)
      {
        var transferTA = sess.Transfer(Account.EmptyInstance, FakePaySystemHost.CARD_DEBIT_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS,
          new Azos.Financial.Amount("usd", 55.00M));

        Aver.IsNotNull(transferTA);
      }

    #endregion
  }
}
