
using System;

namespace Azos.Web.Shipping
{
  public struct Label
  {
    public Label(string id,
                 string url,
                 LabelFormat format,
                 string trackingNumber,
                 CarrierType carrier,
                 Azos.Financial.Amount rate) : this()
    {
      ID = id;
      CreateDate = App.TimeSource.UTCNow;
      URL = url;
      Format = format;
      TrackingNumber = trackingNumber;
      Carrier = carrier;
      Rate = rate;
    }

    public string ID { get; private set; } // system-inner label ID
    public DateTime CreateDate { get; private set; }

    public string URL { get; private set; }
    public LabelFormat Format { get; private set; }

    public string TrackingNumber { get; private set; }
    public CarrierType Carrier { get; private set; }
    public Azos.Financial.Amount Rate { get; private set; }
  }
}
