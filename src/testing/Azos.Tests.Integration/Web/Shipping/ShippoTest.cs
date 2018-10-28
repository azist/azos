/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Web.Shipping;

namespace Azos.Tests.Integration.Web.Shipping
{
  [Runnable]
  public class ShippoTest : IRunnableHook
  {
    public const string LACONF = @"
    nfx
    {
      starters
      {
        starter
        {
          name='Shipping Processing 1'
          type='Azos.Web.Shipping.ShippingSystemStarter, Azos.Web'
          application-start-break-on-exception=true
        }
      }

      web-settings
      {
        shipping-processing
        {
          shipping-system-host
          {
            name='PrimaryShippingSystemHost'
            type='Azos.Tests.Integration.Web.Shipping.FakeShippingSystemHost, Azos.Tests.Integration'
          }

          shipping-system
          {
            name='Shippo'
            type='Azos.Web.Shipping.Shippo.ShippoSystem, Azos.Web'
            auto-start=true

            default-session-connect-params
            {
              name='ShippoDefaultConnection'
              type='Azos.Web.Shipping.Shippo.ShippoConnectionParameters, Azos.Web'

              private-token=$(~SHIPPO_PRIVATE_TOKEN)
              public-token=$(~SHIPPO_PUBLIC_TOKEN)
            }

            carriers
            {
              carrier
              {
                carrier-type='USPS'
                name=$(~SHIPPO_USPS_CARRIER_ID)
                nls-name { eng{n='USPS' d='United States Postal Service'} }

                services
                {
                  service
                  {
                    name=usps_priority
                    nls-name { eng{n='Priority Mail' d='USPS Priority Mail'} }
                    price-category=Standard
                  }

                  service
                  {
                    name=usps_priority_express
                    nls-name { eng{n='Priority Mail Express' d='USPS Priority Mail Express'} }
                    price-category=Expedited
                  }
                  
                  service
                  {
                    name=usps_parcel_select
                    nls-name { eng{n='Parcel Select' d='USPS Parcel Select'} }
                    price-category=Standard
                  }
                  
                  service
                  {
                    name=usps_first
                    nls-name { eng{n='First-Class Package/Mail Parcel' d='USPS First-Class Package/Mail Parcel'} }
                    price-category=Saver
                  }
                }

                packages
                {
                  package
                  {
                    name=USPS_FlatRateEnvelope
                    package-type=Envelope
                    nls-name { eng{n='Flat Rate Envelope' d='USPS Flat Rate Envelope'} }
                  }
                }
              }// carrier USPS

              carrier
              {
                carrier-type='DHLExpress'
                name=$(~SHIPPO_DHL_EXPRESS_CARRIER_ID)
                nls-name { eng{n='DHL Express' d='DHL Express'} }

                services
                {
                  service
                  {
                    name=dhl_express_domestic_express_doc
                    nls-name { eng{n='Domestic Express Doc' d='DHL Express Domestic Express Doc'} }
                    price-category=Standard
                  }
                }
              }// carrier DHL EXPRESS

            }// carriers

          }
        }
      }
    }";

    private ServiceBaseApplication m_App;
    private ShippingSystem m_ShippingSystem;

    public ShippingSystem ShippingSystem
    {
      get
      {
        if (m_ShippingSystem == null)
          m_ShippingSystem = Azos.Web.Shipping.ShippingSystem.Instances["Shippo"];

        return m_ShippingSystem;
      }
    }


    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      var config = LACONF.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      m_App = new ServiceBaseApplication(null, config);
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }


    [Run]
    public void CreateLabel_USPS()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var usps = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.USPS);
        var method = usps.Services["usps_priority"];
        var template = usps.Packages["USPS_FlatRateEnvelope"];

        var shipment = getDefaultShipment(usps, method, template);
        var label = session.CreateLabel(null, shipment);

        Aver.IsNotNull(label);
        Aver.IsTrue(CarrierType.USPS == label.Carrier);
        Aver.IsTrue(LabelFormat.PDF == label.Format);
        Aver.AreEqual("USD", label.Rate.CurrencyISO);
        Aver.IsTrue(label.TrackingNumber.IsNotNullOrEmpty());
        Aver.IsTrue(label.URL.IsNotNullOrEmpty());
      }
    }

    [Run]
    public void CreateReturnLabel_USPS()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var usps = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.USPS);
        var method = usps.Services["USPS_PRIORITY"];
        var template = usps.Packages["USPS_FLAT_RATE_ENVELOPE_TEMPLATE"];

        var shipment = getDefaultShipment(usps, method, template);
        shipment.ReturnAddress = getDefaultAddress();
        var label = session.CreateLabel(null, shipment);

        shipment.LabelIDForReturn = label.ID;
        var retLabel = session.CreateLabel(null, shipment);

        Aver.IsNotNull(retLabel);
        Aver.IsTrue(CarrierType.USPS == retLabel.Carrier);
        Aver.IsTrue(LabelFormat.PDF == retLabel.Format);
        Aver.AreEqual("USD", retLabel.Rate.CurrencyISO);
        Aver.IsTrue(retLabel.URL.IsNotNullOrEmpty());
        Aver.IsTrue(retLabel.TrackingNumber.IsNotNullOrEmpty());
      }
    }

    [Run]
    public void EstimateShippingCost_USPS_FlatRateTemplate()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var usps = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.USPS);
        var method = usps.Services["USPS_PRIORITY"];
        var template = usps.Packages["USPS_FLAT_RATE_ENVELOPE_TEMPLATE"];

        var shipment = getDefaultShipment(usps, method, template);
        var estShipment = session.EstimateShippingCost(null, shipment);

        Aver.IsNotNull(estShipment);
        Aver.AreEqual(estShipment.Cost.Value.CurrencyISO, "USD");
        Aver.IsTrue(estShipment.Cost.Value.Value > 0);
      }
    }

    [Run]
    public void EstimateShippingCost_USPS_NoTemplate()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var usps = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.USPS);

        var method = usps.Services["USPS_PRIORITY"];
        var shipment = getDefaultShipment(usps, method);
        var estShipment = session.EstimateShippingCost(null, shipment);

        Aver.IsNotNull(estShipment.Cost);
        Aver.AreEqual(estShipment.Cost.Value.CurrencyISO, "USD");
        Aver.IsTrue(estShipment.Cost.Value.Value>0);

        method = usps.Services["USPS_PRIORITY_EXPRESS"];
        shipment = getDefaultShipment(usps, method);
        estShipment = session.EstimateShippingCost(null, shipment);

        Aver.IsNotNull(estShipment.Cost);
        Aver.AreEqual(estShipment.Cost.Value.CurrencyISO, "USD");
        Aver.IsTrue(estShipment.Cost.Value.Value>0);

        method = usps.Services["USPS_PARCEL_SELECT"];
        shipment = getDefaultShipment(usps, method);
        estShipment = session.EstimateShippingCost(null, shipment);

        Aver.IsNotNull(estShipment.Cost);
        Aver.AreEqual(estShipment.Cost.Value.CurrencyISO, "USD");
        Aver.IsTrue(estShipment.Cost.Value.Value>0);

        method = usps.Services["USPS_FIRST"];
        shipment = getDefaultShipment(usps, method);
        estShipment = session.EstimateShippingCost(null, shipment);

        Aver.IsNotNull(estShipment.Cost);
        Aver.AreEqual(estShipment.Cost.Value.CurrencyISO, "USD");
        Aver.IsTrue(estShipment.Cost.Value.Value>0);
      }
    }

    [Run]
    public void EstimateShippingCost_DHLExpress()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var dhl = session.GetShippingCarriers(null).First(c => c.Type==CarrierType.DHLExpress);
        var method = dhl.Services["dhl_express_domestic_express_doc"];

        var shipment = getDefaultShipment(dhl, method);
        var amount = session.EstimateShippingCost(null, shipment);

        Aver.IsNotNull(amount);
        Aver.IsTrue(amount.IsAlternative);
      }
    }

    [Run]
    public void ValidateAddress_Valid()
    {
      using (var session = ShippingSystem.StartSession())
      {
        ValidateShippingAddressException error;

        var address = getDefaultAddress();
        var corrAddress = session.ValidateAddress(null, address, out error);

        Aver.IsNull(error);
      }
    }

    [Run]
    public void ValidateAddress_Valid_Corrected()
    {
      using (var session = ShippingSystem.StartSession())
      {
        ValidateShippingAddressException error;

        var address = getDefaultAddress();
        address.City = "Los Angeles";
        var corrAddress = session.ValidateAddress(null, address, out error);

        Aver.IsNull(error);
        Aver.AreEqual("los alamos", corrAddress.City.ToLower());
      }
    }

    [Run]
    public void ValidateAddress_Invalid()
    {
      using (var session = ShippingSystem.StartSession())
      {
        ValidateShippingAddressException error;

        var address = getDefaultAddress();
        address.Line1 = "Starokubanskaya 33";
        var corrAddress = session.ValidateAddress(null, address, out error);
        Aver.IsNotNull(error);
        Aver.IsNull(corrAddress);
        Aver.IsTrue(error.Message.Contains("The address as submitted could not be found"));

        address = getDefaultAddress();
        address.Line1 = "587000 Kiva Street";
        corrAddress = session.ValidateAddress(null, address, out error);
        Aver.IsNotNull(error);
        Aver.IsNull(corrAddress);
        Aver.IsTrue(error.Message.Contains("The address as submitted could not be found"));

        address = getDefaultAddress();
        address.City = "New York";
        address.Postal = "350011";
        corrAddress = session.ValidateAddress(null, address, out error);
        Aver.IsNotNull(error);
        Aver.IsNull(corrAddress);
        Aver.IsTrue(error.Message.Contains("The City in the address submitted is invalid"));
      }
    }

    [Run]
    public void ValidateAddress_Crazy()
    {
      // Shippo API can pass STATUS_INVALID or
      // (!!!!!) STATUS_VALID with error in 'messages' with 'code'="Invalid"
      var crazy = new Address
      {
        PersonName = "Stan Ulam",
        Country = "USA",
        Region = "NM",
        City = "NEW YORK",
        EMail = "s-ulam@myime.com",
        Line1 = "2500TH ST AVE",
        Postal = "10017000000",
        Phone = "(333) 777-77-77"
      };

      using (var session = ShippingSystem.StartSession())
      {
        ValidateShippingAddressException error;
        var corrAddress = session.ValidateAddress(null, crazy, out error);

        Aver.IsNotNull(error);
        Aver.IsNull(corrAddress);
      }
    }

    [Run]
    public void Test_ShippoFixedBugs()
    {
      using (var session = ShippingSystem.StartSession())
      {
        var address = new Address
        {
          PersonName = "Stan Ulam",
          Country = "USA",
          Region = "IN",
          City = "INDIANAPOLIS",
          EMail = "s-ulam@myime.com",
          Line1 = "1004 HOSBROOK ST",
          Postal = "46203-1012",
          Phone = "(333) 777-77-77"
        };

        ValidateShippingAddressException error;
        var corrAddress = session.ValidateAddress(null, address, out error);
        Aver.IsNull(error);
        Aver.IsNotNull(corrAddress);

        address.City = "INDIANAPOLIs";
        corrAddress = session.ValidateAddress(null, address, out error);
        Aver.IsNull(error);
        Aver.IsNotNull(corrAddress);

        corrAddress = session.ValidateAddress(null, corrAddress, out error);
        Aver.IsNull(error);
        Aver.IsNotNull(corrAddress);

        address = new Address
        {
          PersonName = "Stan Ulam",
          Country = "USA",
          Region = "NM",
          City = "LOS ALAMOS",
          EMail = "s-ulam@myime.com",
          Line1 = "587 KIVA ST",
          Postal = "87544",
          Phone = "(333) 777-77-77"
        };

        corrAddress = session.ValidateAddress(null, address, out error);
        Aver.IsNull(error);
        Aver.IsNotNull(corrAddress);

        corrAddress = session.ValidateAddress(null, corrAddress, out error);
        Aver.IsNull(error);
        Aver.IsNotNull(corrAddress);
      }
    }

    [Run]
    public void Test_TrackingNumber()
    {
      // just a stub
      // we can not track sandbox shipments
      using (var session = ShippingSystem.StartSession())
      {
        var track = session.TrackShipment(null, "USPS", "12334fsfsdfw3r32");
      }
    }


    private Address getDefaultAddress()
    {
      var result = new Address
      {
        PersonName = "Stan Ulam",
        Country = "USA",
        Region = "NM",
        City = "LOS ALAMOS",
        EMail = "s-ulam@myime.com",
        Line1 = "587 KIVA ST",
        Postal = "87544",
        Phone = "(333) 777-77-77"
      };

      return result;
    }

    private Shipment getDefaultShipment(ShippingCarrier carrier, ShippingCarrier.Service method, ShippingCarrier.Package template = null)
    {
      var result = new Shipment
      {
        Length = 0.5M,
        Width = 0.5M,
        Height = 0.5M,
        DistanceUnit = Azos.Standards.Distance.UnitType.In,
        Weight = 0.3M,
        WeightUnit = Azos.Standards.Weight.UnitType.Lb,
        Carrier = carrier,
        Service = method,
        Package = template,
        FromAddress = new Address
        {
          PersonName = "J. London",
          Country = "US",
          Region = "NY",
          City = "New York",
          EMail = "jlondon@myime.com",
          Line1 = "183 Canal Street",
          Postal = "10013",
          Phone = "(111) 222-33-44"
        },
        ToAddress = new Address
        {
          PersonName = "A. Einstein",
          Country = "US",
          Region = "CA",
          City = "Los Angeles",
          EMail = "aeinstein@myime.com",
          Line1 = "1782 West 25th Street",
          Postal = "90018",
          Phone = "(111) 333-44-55"
        }
      };

      return result;
    }
  }
}
