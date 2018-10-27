/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Web.Shipping
{
  [Serializable]
  public class ShippingException : Exception
  {
    #region Inner

    public enum ErrorReason
    {
      Error = 0,
      PaidServices = 10
    }

    #endregion

    public static ShippingException ComposeError(string header, Exception inner)
    {
        var webError = inner as System.Net.WebException;
        if (webError == null || webError.Response==null)
            return new ShippingException(header, inner);

        var response = (HttpWebResponse)webError.Response;
        var reason = ErrorReason.Error;

        var responseMessage = string.Empty;
        try
        {
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                var responseStr = reader.ReadToEnd();
                var responseJSON = responseStr.JSONToDataObject() as JSONDataMap;
                if (responseJSON != null)
                {
                    var details = responseJSON["detail"].AsString();
                    if (details.IsNotNullOrWhiteSpace())
                      responseMessage = details.IsNullOrWhiteSpace() ? responseJSON.ToJSON() : details;
                    else if (responseJSON.ContainsKey("__all__"))
                    {
                      var all = responseJSON["__all__"] as JSONDataArray;
                      if (all != null && all.Any())
                        responseMessage = all[0].AsString();
                    }
                    else
                      responseMessage = responseJSON.ToJSON();
                }
            }
        }
        catch (Exception)
        {
        }

        if (responseMessage.Contains("billing") || responseMessage.Contains("payment"))
          reason = ErrorReason.PaidServices;

        var statusCode = (int)response.StatusCode;
        var message = "{0} ({1})".Args(responseMessage, statusCode);

        return new ShippingException(message, inner) { Reason = reason };
    }

    #region .ctor

      public ShippingException()
      {
      }

      public ShippingException(string message)
        : base(message)
      {
      }

      public ShippingException(string message, Exception inner)
        : base(message, inner)
      {
      }

      protected ShippingException(SerializationInfo info, StreamingContext context)
        : base(info, context)
      {
      }

      public ErrorReason Reason { get; set; }

    #endregion
  }

  [Serializable]
  public class ValidateShippingAddressException : ShippingException
  {
    #region .ctor

      public ValidateShippingAddressException()
        : base()
      {
      }

      public ValidateShippingAddressException(string message, string details)
        : base(message)
      {
        m_Details = details;
      }

      public ValidateShippingAddressException(string message, string details, Exception inner)
        : base(message, inner)
      {
        m_Details = details;
      }

      protected ValidateShippingAddressException(SerializationInfo info, StreamingContext context)
        : base(info, context)
      {
      }

    #endregion

    private readonly string m_Details;

    public string Details { get { return m_Details; } }
  }
}
