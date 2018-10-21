
using System;
using System.Text;

namespace Azos.Web.IO
{
  public static class RFC3986
  {
    #region CONSTS

      private const string     ESCAPE = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
      private const string ESCAPE_URL = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~/:";

    #endregion

    #region Static

      /// <summary>
      /// Encodes URL string according to RFC3986
      /// </summary>
      public static string URLEncode(string input)
      {
        return Encode(input, ESCAPE_URL);
      }

      /// <summary>
      /// Encodes string according to RFC3986
      /// </summary>
      public static string Encode(string input)
      {
        return Encode(input, ESCAPE);
      }

      private static string Encode(string input, string escape)
      {
        if (string.IsNullOrEmpty(input))
          return string.Empty;

        StringBuilder b = new StringBuilder(input.Length * 2); // approximate estimation
        byte[] bb = Encoding.UTF8.GetBytes(input);

        foreach(char ch in bb)
        {
          if (escape.IndexOf(ch) != -1)
          {
            b.Append((char)ch);
          }
          else
          {
            b.Append('%');
            b.Append(Convert.ToString(ch, 16).ToUpper());
          }
        }
        return b.ToString();
      }

    #endregion

  } //RFC3986

}
