/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Globalization;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Financial
{
  /// <summary>
  /// Represents monetary amount with currency ISO atom
  /// </summary>
  [Serializable]
  public struct Amount : IEquatable<Amount>, IComparable<Amount>, IJsonWritable, IJsonReadable, IRequiredCheck
  {
    private static readonly IFormatProvider INVARIANT = CultureInfo.InvariantCulture;

    public Amount(string iso, decimal value) : this(iso.IsNullOrWhiteSpace() ? Atom.ZERO : Atom.Encode(iso.ToLowerInvariant().Trim()), value)
    {
    }

    public Amount(Atom iso, decimal value)
    {
      ISO = iso;
      Value = value;
    }


    public readonly Atom ISO;
    public readonly decimal Value;

    public bool IsEmpty => ISO.IsZero && Value == default(decimal);

    public bool CheckRequired(string targetName) => !IsEmpty;

    /// <summary>
    /// Performs currency ISO equality comparison
    /// </summary>
    public bool IsSameCurrencyAs(Amount other) => ISO == other.ISO;


    public static Amount Parse(string val)
    {
      if (val==null) throw new FinancialException(StringConsts.ARGUMENT_ERROR + typeof(Amount).FullName + ".Parse(null)");

      try
      {
        var i = val.IndexOf(':');
        if (i<0)
        {
          var dv = decimal.Parse(val);
          return new Amount(null, dv);
        }
        else
        {
          var iso = val.Substring(i+1);
          iso = iso.Trim();
          if (iso.IsNullOrWhiteSpace()) Aver.Fail("Nothing after :");
          var dv = decimal.Parse( val.Substring(0, i), INVARIANT );
          return new Amount(iso, dv);
        }
      }
      catch
      {
        throw new FinancialException(StringConsts.FINANCIAL_AMOUNT_PARSE_ERROR.Args(val.TakeFirstChars(24, "..")));
      }
    }

    public static bool TryParse(string val, out Amount result)
    {
      result = new Amount();

      if (val==null) return false;

      var i = val.IndexOf(':');
      if (i<0)
      {
        decimal dv;
        if (!decimal.TryParse(val, out dv)) return false;
        result = new Amount(null, dv);
        return true;
      }
      else
      {
        var iso = i<val.Length-1 ? val.Substring(i+1) : string.Empty;
        if (i==0) return false;
        iso = iso.Trim();
        if (iso.IsNullOrWhiteSpace()) return false;
        if (!Atom.TryEncode(iso, out var isoa)) return false;
        decimal dv;
        if (!decimal.TryParse( val.Substring(0, i), NumberStyles.Any, INVARIANT, out dv )) return false;
        result = new Amount(isoa, dv);
        return true;
      }
    }

    #region Object overrides and intfs

    public override string ToString() => $"{Value.ToString("G", INVARIANT)}:{ISO}";

    public override int GetHashCode() => ISO.GetHashCode() ^ Value.GetHashCode();

    public override bool Equals(object obj) => obj is Amount other ? this.Equals(other) : false;

    public bool Equals(Amount other) => IsSameCurrencyAs(other) && Value == other.Value;


    public int CompareTo(Amount other) => this.Equals(other) ? 0 : this < other ? -1 : +1;

    void IJsonWritable.WriteAsJson(System.IO.TextWriter wri, int nestingLevel, JsonWritingOptions options)
    {
      JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("iso", ISO.Value), new DictionaryEntry("val", Value));
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        if (
            Atom.TryEncode(map["iso"].AsString(), out var iso) &&
            decimal.TryParse(map["val"].AsString(), NumberStyles.Any, INVARIANT, out var val)
           ) return (true, new Amount(iso, val));
      }

      return (false, null);
    }


    #endregion

    #region Operators

    public static bool operator ==(Amount left, Amount right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Amount left, Amount right)
    {
      return !Equals(left, right);
    }

    public static bool operator <(Amount left, Amount right)
    {
      return left.IsSameCurrencyAs(right) && (left.Value < right.Value);
    }

    public static bool operator >(Amount left, Amount right)
    {
      return left.IsSameCurrencyAs(right) && (left.Value > right.Value);
    }

    public static bool operator <=(Amount left, Amount right)
    {
      return left.IsSameCurrencyAs(right) && (left.Value <= right.Value);
    }

    public static bool operator >=(Amount left, Amount right)
    {
      return left.IsSameCurrencyAs(right) && (left.Value >= right.Value);
    }

    public static Amount operator +(Amount left, Amount right)
    {
      if (! left.IsSameCurrencyAs(right)) throw new FinancialException(StringConsts.FINANCIAL_AMOUNT_DIFFERENT_CURRENCIES_ERROR.Args('+', left, right));

      return new Amount(left.ISO, left.Value + right.Value);
    }

    public static Amount operator -(Amount left, Amount right)
    {
      if (! left.IsSameCurrencyAs(right)) throw new FinancialException(StringConsts.FINANCIAL_AMOUNT_DIFFERENT_CURRENCIES_ERROR.Args('-', left, right));

      return new Amount(left.ISO, left.Value - right.Value);
    }

    public static Amount operator *(Amount left, int right)
    {
      return new Amount(left.ISO, left.Value * right);
    }

    public static Amount operator *(int left, Amount right)
    {
      return new Amount(right.ISO, right.Value * left);
    }

    public static Amount operator *(Amount left, decimal right)
    {
      return new Amount(left.ISO, left.Value * right);
    }

    public static Amount operator *(decimal left, Amount right)
    {
      return new Amount(right.ISO, right.Value * left);
    }

    public static Amount operator *(Amount left, double right)
    {
      return new Amount(left.ISO, left.Value * (decimal)right);
    }

    public static Amount operator *(double left, Amount right)
    {
      return new Amount(right.ISO, right.Value * (decimal)left);
    }

    public static Amount operator /(Amount left, int right)
    {
      return new Amount(left.ISO, left.Value / right);
    }

    public static Amount operator /(Amount left, decimal right)
    {
      return new Amount(left.ISO, left.Value / right);
    }

    public static Amount operator /(Amount left, double right)
    {
      return new Amount(left.ISO, left.Value / (decimal)right);
    }
    #endregion
  }
}