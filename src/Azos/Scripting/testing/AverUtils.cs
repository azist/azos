/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Scripting
{
  /// <summary>
  /// Provides utilities for writing complex averments such as data doc diffing etc.
  /// </summary>
  public static class AverUtils
  {
    /// <summary>
    /// Avers that the TDoc is logically no different from the expected one supplied as JSON content
    /// </summary>
    public static void AverNoDiff<TDoc>(this string expectJson, TDoc got) where TDoc : TypedDoc
    {
      var expect = JsonReader.ToDoc<TDoc>(expectJson);
      AverNoDiff(expect, got);
    }

    /// <summary>
    /// Compares two TDoc data documents
    /// </summary>
    public static DocLogicalComparer.Result CompareTo<TDoc>(this string expectJson, TDoc got) where TDoc : TypedDoc
    {
      var expect = JsonReader.ToDoc<TDoc>(expectJson);
      return expect.CompareTo(got);
    }

    /// <summary>
    /// Avers that the TDoc supplied as JSON content is logically no different from the expected one
    /// </summary>
    public static void AverNoDiff<TDoc>(this TDoc expect, string gotJson) where TDoc : TypedDoc
    {
      var got = JsonReader.ToDoc<TDoc>(gotJson);
      AverNoDiff(expect, got);
    }

    /// <summary>
    /// Compares two TDoc data documents
    /// </summary>
    public static DocLogicalComparer.Result CompareTo<TDoc>(this TDoc expect, string gotJson) where TDoc : TypedDoc
    {
      var got = JsonReader.ToDoc<TDoc>(gotJson);
      return expect.CompareTo(got);
    }


    /// <summary>
    /// Compares two TDoc data documents
    /// </summary>
    public static DocLogicalComparer.Result CompareTo<TDoc>(this TDoc expect, TDoc got) where TDoc : Doc
    {
      var cmp = new DocLogicalComparer()
      {
        LoopByA = true,
        LoopByB = true,
        LoopByAmorphous = true,
        FindMissingInAmorphous = true
      };

      var result = cmp.Compare(expect, got);

      return result;
    }

    /// <summary>
    /// Avers that two TDoc data documents are not logically different
    /// </summary>
    public static void AverNoDiff<TDoc>(this TDoc expect, TDoc got) where TDoc : Doc
    {
      var result = expect.CompareTo(got);
      Aver.IsTrue(result.AreSame, "Documents do not match: {0} differences found".Args(result.Differences.Count()));
    }
  }
}
