/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Financial;
using Azos.Scripting;
using Azos.Time;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class DocValidationRequiredStructsTests
  {
    [Run]
    public void Test_GDID()
    {
      var doc = new GdidDoc
      {
        NGdidNotRequired = null,
        NGdidRequired = new GDID(1,2),
        GdidNotRequired = GDID.ZERO,
        GdidRequired = new GDID(3,4)
      };

      var ve = doc.Validate();
      Aver.IsNull(ve);

      doc.NGdidRequired = GDID.ZERO;
      ve = doc.Validate();
      Aver.IsNotNull(ve);
      ve.See("NGdidRequired = GDID.ZERO:\n");

      doc.NGdidRequired = null;
      ve = doc.Validate();
      Aver.IsNotNull(ve);
      ve.See("doc.NGdidRequired = null:\n");

      doc.NGdidRequired = new GDID(9,10);
      ve = doc.Validate();
      Aver.IsNull(ve);

      doc.NGdidNotRequired = GDID.ZERO;
      ve = doc.Validate();
      Aver.IsNull(ve);

      doc.GdidRequired = GDID.ZERO;
      ve = doc.Validate();
      Aver.IsNotNull(ve);
      ve.See("doc.GdidRequired = GDID.ZERO:\n");

      doc.GdidRequired = new GDID(5,7);
      ve = doc.Validate();
      Aver.IsNull(ve);

      doc.GdidNotRequired = new GDID(100,100);
      ve = doc.Validate();
      Aver.IsNull(ve);

      doc.GdidNotRequired = GDID.ZERO;
      ve = doc.Validate();
      Aver.IsNull(ve);
    }


    [Run]
    public void Test_Amount()
    {
      var doc = new AmountDoc
      {
        NAmountNotRequired = null,
        NAmountRequired = new Amount("usd", 2),
        AmountNotRequired = new Amount(),
        AmountRequired = new Amount("cad", 3.4m)
      };

      var ve = doc.Validate();
      Aver.IsNull(ve);

      doc.NAmountRequired = new Amount();
      ve = doc.Validate();
      Aver.IsNotNull(ve);
      ve.See("doc.NAmountRequired = new Amount():\n");

      doc.NAmountRequired = null;
      ve = doc.Validate();
      Aver.IsNotNull(ve);
      ve.See("doc.NAmountRequired = null:\n");

      doc.NAmountRequired = new Amount("xxx", 122);
      ve = doc.Validate();
      Aver.IsNull(ve);

      doc.NAmountNotRequired = new Amount();
      ve = doc.Validate();
      Aver.IsNull(ve);

      doc.AmountRequired = new Amount();
      ve = doc.Validate();
      Aver.IsNotNull(ve);
      ve.See("doc.AmountRequired = new Amount():\n");

      doc.AmountRequired = new Amount("cad", 700);
      ve = doc.Validate();
      Aver.IsNull(ve);

      doc.AmountNotRequired = new Amount("cad", 500);
      ve = doc.Validate();
      Aver.IsNull(ve);

      doc.AmountNotRequired = new Amount();
      ve = doc.Validate();
      Aver.IsNull(ve);
    }

    public class GdidDoc : TypedDoc
    {
      [Field(required: false)]
      public GDID? NGdidNotRequired { get; set; }
      [Field(required: true)]
      public GDID? NGdidRequired { get; set; }

      [Field(required: false)]
      public GDID GdidNotRequired { get; set; }
      [Field(required: true)]
      public GDID GdidRequired{ get; set;}
    }

    public class AmountDoc : TypedDoc
    {
      [Field(required: false)]
      public Amount? NAmountNotRequired { get; set; }
      [Field(required: true)]
      public Amount? NAmountRequired { get; set; }

      [Field(required: false)]
      public Amount AmountNotRequired { get; set; }
      [Field(required: true)]
      public Amount AmountRequired { get; set; }
    }

    public class DateRangeDoc : TypedDoc
    {
      [Field(required: false)]
      public DateRange? NDateRangeNotRequired { get; set; }
      [Field(required: true)]
      public DateRange? NDateRangeRequired { get; set; }

      [Field(required: false)]
      public DateRange DateRangeNotRequired { get; set; }
      [Field(required: true)]
      public DateRange DateRangeRequired { get; set; }
    }

    public class HourListDoc : TypedDoc
    {
      [Field(required: false)]
      public HourList? NHourListNotRequired { get; set; }
      [Field(required: true)]
      public HourList? NHourListRequired { get; set; }

      [Field(required: false)]
      public HourList HourListNotRequired { get; set; }
      [Field(required: true)]
      public HourList HourListRequired { get; set; }
    }

  }
}
