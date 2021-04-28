/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Scripting;

using Azos.Serialization.CSV;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class CSVParserTests
  {
    private bool same(IEnumerable<string> got, params string[] elm) { return got.SequenceEqual(elm); }

    [Run]
    public void ParseCSVRow_SimpleRecord()
    {
      Aver.IsTrue(same("aaa,bbb,ccc,".ParseCSVRow(), "aaa", "bbb", "ccc", ""));
      Aver.IsTrue(same("aaa bbb,ccc,".ParseCSVRow(), "aaa bbb", "ccc", ""));
      Aver.IsTrue(same("  aaa  ,  bbb,ccc  ,  ".ParseCSVRow(), "  aaa  ", "  bbb", "ccc  ", "  "));
      Aver.IsTrue(same("  aaa bbb,ccc ddd  ".ParseCSVRow(), "  aaa bbb", "ccc ddd  "));
    }

    [Run]
    public void ParseCSVRow_SimpleRecordTrim()
    {
      Aver.IsTrue(same("  aaa  ,  bbb,ccc  ,  ".ParseCSVRow(true), "aaa", "bbb", "ccc", ""));
      Aver.IsTrue(same("  aaa bbb,ccc ddd  ".ParseCSVRow(true), "aaa bbb", "ccc ddd"));
    }

    [Run]
    public void ParseCSVRow_QuotedRecord()
    {
      Aver.IsTrue(same("\"aaa\",\"bbb\",\"ccc\",\"\"".ParseCSVRow(), "aaa", "bbb", "ccc", ""));
      Aver.IsTrue(same("\"aaa,bbb\",\"ccc,\",\",ddd\"".ParseCSVRow(), "aaa,bbb", "ccc,", ",ddd"));
      Aver.IsTrue(same(@"  ""aaa""  ,  ""bbb"",  ""  ,  """.ParseCSVRow(), "  \"aaa\"  ", "  \"bbb\"", "  \"  ", "  \""));
      Aver.IsTrue(same(@"""  aaa  "",""  bbb"",""ccc  "",""  """.ParseCSVRow(), "  aaa  ", "  bbb", "ccc  ", "  "));
      Aver.IsTrue(same(@"  ""  aaa  ""  ,  ""  bbb  "",  ""  ""  ,  ""  """.ParseCSVRow(), "  \"  aaa  \"  ", "  \"  bbb  \"", "  \"  \"  ", "  \"  \""));
      Aver.IsTrue(same(@"""aaa  ,  bbb"",""ccc,  "",""  ,ddd""".ParseCSVRow(), "aaa  ,  bbb", "ccc,  ", "  ,ddd"));
      Aver.IsTrue(same(
@"""aaa
bbb"",""
ccc"",""ddd
""".ParseCSVRow(),
@"aaa
bbb",
@"
ccc",
@"ddd
"));
      Aver.IsTrue(same(
@"""aaa
bbb
ccc"",""
""".ParseCSVRow(),
@"aaa
bbb
ccc",
@"
"));
    }

    [Run]
    public void ParseCSVRow_QuotedRecordTrim()
    {
      Aver.IsTrue(same(@"  ""aaa""  ,  ""bbb"",""ccc""  ,  """"  ,  """",""""  ".ParseCSVRow(true), "aaa", "bbb", "ccc", "", "", ""));
      Aver.IsTrue(same(@"""  aaa  "",""  bbb"",""ccc  "",""  """.ParseCSVRow(true), "  aaa  ", "  bbb", "ccc  ", "  "));
      Aver.IsTrue(same(@"  ""  aaa  ""  ,  ""  bbb  "",""  ccc  ""  ,  ""  ""  ,  ""  "",""  ""  ".ParseCSVRow(true), "  aaa  ", "  bbb  ", "  ccc  ", "  ", "  ", "  "));
      Aver.IsTrue(same(@"""aaa  ,  bbb"",""ccc,  "",""  ,ddd""".ParseCSVRow(true), "aaa  ,  bbb", "ccc,  ", "  ,ddd"));
    }

    [Run]
    public void ParseCSVRow_QuotedRecordWithQuote()
    {
      Aver.IsTrue(same("\"\"\"\",\"  \"\"  \",\"  \"\"\",\"\"\"  \"".ParseCSVRow(), "\"", "  \"  ", "  \"", "\"  "));
      Aver.IsTrue(same("\",\"\",\",\"  \"\",  \",\",  \"\"\",\"\"\"  ,\"".ParseCSVRow(), ",\",", "  \",  ", ",  \"", "\"  ,"));
    }

    [Run]
    public void ParseCSVRow_SimpleRecordWithQuote()
    {
      Aver.IsTrue(same("aaa\"bbb,ccc\"".ParseCSVRow(), "aaa\"bbb", "ccc\""));
    }

    [Run]
    public void ParseCSVRow_BreakOnLineBreaks()
    {
      Aver.IsTrue(same(
@"aaa,bbb,ccc,
ddd".ParseCSVRow(), "aaa", "bbb", "ccc", ""));
      Aver.IsTrue(same(
@"aaa,bbb,ccc
ddd".ParseCSVRow(), "aaa", "bbb", "ccc"));
      Aver.IsTrue(same("aaa,bbb,ccc,\nddd".ParseCSVRow(), "aaa", "bbb", "ccc", ""));
      Aver.IsTrue(same("aaa,bbb,ccc\r\nddd".ParseCSVRow(), "aaa", "bbb", "ccc"));
    }

    [Run]
    public void ParseCSV()
    {
      var csv =
@"aaa,aaa,aaa,
aaa aaa,aaa,,
  aaa  ,  aaa,aaa  ,
  aaa aaa,aaa aaa  ,,
""aaa"",""aaa"",""aaa"",""""
""aaa,aaa"",""aaa,"","",aaa""
  ""aaa""  ,  ""aaa"",  ""aaa  ,aaa  ""
""  aaa  "",""  aaa"",""aaa  "",""  ""
  ""  aaa  ""  ,  ""  aaa  "",  "" aaa ""  ,  "" aaa ""
""aaa  ,  aaa"",""aaa,  "",""  ,aaa""
""aaa
aaa"",""
aaa"",""aaa
""
""aaa
aaa
aaa"",""
""";
      foreach (var row in csv.ParseCSV(trim: true, skipHeader: true, columns: 3, skipIfMore: true, addIfLess: true))
      {
        var count = 0;
        foreach (var value in row)
        {
          Aver.IsTrue(value.IsNullOrWhiteSpace() || value.Contains("aaa"));
          count++;
        }
        count.See();
      }
      foreach (var row in csv.ParseCSV(skipHeader: true, columns: 3, skipIfMore: true, addIfLess: true))
      {
        var count = 0;
        foreach (var value in row)
        {
          Aver.IsTrue(value.IsNullOrWhiteSpace() || value.Contains("aaa"));
          count++;
        }
        count.See();
      }
      foreach (var row in csv.ParseCSV(columns: 3, skipIfMore: true, addIfLess: true))
      {
        var count = 0;
        foreach (var value in row)
        {
          Aver.IsTrue(value.IsNullOrWhiteSpace() || value.Contains("aaa"));
          count++;
        }
        count.See();
      }
    }

  }
}
