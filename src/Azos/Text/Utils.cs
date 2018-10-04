
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Azos.Text
{
  public static class Utils
  {
    private const char SPACE = ' ';
    private static readonly char[] FIELD_NAME_DELIMETERS = new char[] {' ', '-', '_'};

    /// <summary>
    /// Parses database field names (column names) and converts parts to human-readable description
    ///  like:
    ///  "FIRST_NAME" -> "First Name",
    ///  "FirstName" -> "First Name",
    ///  "CHART_OF_ACCOUNTS" -> "Chart of Accounts"
    /// </summary>
    public static string ParseFieldNameToDescription(this string fieldName, bool capitalize)
    {
      if (fieldName.IsNullOrWhiteSpace()) return string.Empty;

      var builder = new StringBuilder();
      char prev = fieldName[0];
      builder.Append(prev);

      var length = fieldName.Length;
      for (int i = 1; i < length; i++)
      {
        var curr = fieldName[i];
        if (
            !FIELD_NAME_DELIMETERS.Contains(prev) &&
            !FIELD_NAME_DELIMETERS.Contains(curr) &&
            (charCaseTransition(prev, curr) || charDigitTransition(prev, curr))
           )
        {
            builder.Append(SPACE);
        }

        builder.Append(curr);
        prev = curr;
      }

      var name = builder.ToString();
      var segs = name.Split(FIELD_NAME_DELIMETERS, StringSplitOptions.RemoveEmptyEntries);
      var result = capitalize ?
                      segs.Select(s => s.Trim().ToLowerInvariant().CapitalizeFirstChar()).Aggregate((s1,s2) => s1+SPACE+s2) :
                      segs.Select(s => s.Trim().ToLowerInvariant()).Aggregate((s1,s2) => s1+SPACE+s2);

      return result;
    }


    private static Regex urlCheckRegex;

    /// <summary>
    /// Checks URL string for validity
    /// </summary>
    public static bool IsURLValid(this string url)
    {
      if (string.IsNullOrEmpty(url)) return false;


      lock (typeof(Utils))
        if (urlCheckRegex == null)
        {
          string pattern = @"^(http|https)\://[a-zA-Z0-9\-\.]+(:[a-zA-Z0-9]*)?(/[a-zA-Z0-9\-\._]*)*$";
          urlCheckRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

      Match m = urlCheckRegex.Match(url);
      return m.Success;
    }

    /// <summary>
    /// Puts every sentence on a separate line
    /// </summary>
    public static string MakeSentenceLines(this string text)
    {
      StringBuilder result = new StringBuilder();

      for (int i = 0; i < text.Length; i++)
      {
        Char c = text[i];
        result.Append(c);
        if ((c == '.') ||
            (c == ';') ||
            (c == '?') ||
            (c == '!'))
          result.Append("\n");
      }

      return result.ToString();
    }



    /// <summary>
    /// Returns a captured wildcard segment from string. Pattern uses '*' for match capture by default and may contain a single capture
    /// </summary>
    public static string CapturePatternMatch(this string str,     //     Pages/Dima/Welcome
                                                  string pattern,
                                                  char wc ='*',
                                                  StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)

    {
       var i = pattern.IndexOf(wc);
       if (i<0) return string.Empty;

       var pleft = pattern.Substring(0, i);
       var pright = (i+1<pattern.Length)? pattern.Substring(i+1) : string.Empty;

       if (pleft.Length>0)
       {
         if (!str.StartsWith(pleft, comparisonType)) return string.Empty;
         str = str.Substring(pleft.Length);
       }

       if (pright.Length>0)
       {
         if (!str.EndsWith(pright, comparisonType)) return string.Empty;
         str = str.Substring(0, str.Length - pright.Length);
       }

       return str;
    }


    /// <summary>
    /// Returns true if supplied string matches pattern that can contain multiple char span (*) wildcards and
    /// multiple single-char (?) wildcards
    /// </summary>
    public static bool MatchPattern(this string str,     //     some address
                                         string pattern, //     some*e?s
                                         char wc ='*',
                                         char wsc ='?',
                                         bool senseCase = false)
    {
      var snws = str.IsNullOrWhiteSpace();
      var pnws = pattern.IsNullOrWhiteSpace();

      if (snws && pnws) return true;
      if (snws || pnws) return false;


      int istr = 0, ipat = 0;

      var pistr = str.Length;
      var pipat = pattern.Length;
      while(istr<str.Length)
      {
        for(; istr<str.Length && ipat<pattern.Length; istr++, ipat++)
        {
          var pc = pattern[ipat];
          if (pc==wc) //*
          {
            if (ipat==pattern.Length-1) return true;//final char in pattern is *
            pistr = istr;
            pipat = ipat;
            istr--;
            continue;
          }

          if (pc==wsc) continue;
          if (charEqual(pc, str[istr], senseCase)) continue;

          ipat = pipat-1; //same pattern
          istr = pistr; //next char
        }

        //eat trailing ****
        while(ipat<pattern.Length && pattern[ipat]==wc) ipat++;

        if (istr==str.Length && ipat==pattern.Length) return true;
        ipat = pipat;
        pistr++;
        istr = pistr;
      }
      return false;
    }



    private static bool charEqual(char a, char b, bool senseCase)
    {
      return senseCase ? a==b : Char.ToUpperInvariant(a)==Char.ToUpperInvariant(b);
    }

    private static bool charCaseTransition(char prev, char curr)
    {
      return  char.IsLower(prev) && char.IsUpper(curr);
    }

    private static bool charDigitTransition(char prev, char curr)
    {
      return char.IsDigit(prev) ^ char.IsDigit(curr);
    }
  }
}
