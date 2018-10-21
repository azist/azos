
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Web.IO;

namespace Azos.Web.Social
{
  public partial class Twitter : SocialNetwork
  {
    #region CONSTS

    #endregion

    #region Inner Types

    #endregion

    #region Static

      public static string GetRawOAuthHeaderStr(Dictionary<string, string> dictionary)
      {
        StringBuilder b = new StringBuilder();
        bool firstValue = true;

        foreach (var item in GetEncodedPairs(dictionary))
        {
          if (firstValue)
            firstValue = false;
          else
            b.Append('&');

          b.AppendFormat("{0}={1}", item.Item1, item.Item2);
        }

        return b.ToString();
      }

      public static string GetOAuthHeaderString(Dictionary<string, string> dictionary)
      {
        StringBuilder b = new StringBuilder();
        bool firstValue = true;

        foreach (var item in GetEncodedPairs(dictionary))
        {
          if (firstValue)
            firstValue = false;
          else
            b.Append(", ");

          b.AppendFormat("{0}=\"{1}\"", item.Item1, item.Item2);
        }

        return b.ToString();
      }

      private static IEnumerable<Tuple<string, string>> GetEncodedPairs(Dictionary<string, string> dictionary)
      {
        foreach (var kvp in dictionary.OrderBy(kvp => kvp.Key))
        {
          string encodedKey = RFC3986.Encode(kvp.Key);
          string encodedValue = RFC3986.Encode(kvp.Value);

          yield return new Tuple<string, string>(encodedKey, encodedValue);
        }
      }

      public static string AddMethodAndBaseURL(string headerStr, HTTPRequestMethod method, string baseURL)
      {
        StringBuilder b = new StringBuilder();

        b.Append(method.ToString()); b.Append('&');
        b.Append(RFC3986.Encode(baseURL)); b.Append('&');
        b.Append(RFC3986.Encode(headerStr));

        return b.ToString();
      }

      public static string CalculateSignature(string src, string secretKey, string tokenSecret = null)
      {
        TwitterCryptor cryptor = new TwitterCryptor( secretKey, tokenSecret);
        string signature = cryptor.GetHashString(src);
        return signature;
      }

    #endregion

  } //TwitterRequestExtensions

}
