/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;
using System.IO;

using Azos.CodeAnalysis.Source;
using Azos.Scripting;

namespace Azos.Tests.Nub.CodeAnalysis
{
  [Runnable]
  public class FileSourceTests : IRunnableHook
  {
    private const string FN = "nub-test-utf8-text-file.txt"; //2,294 bytes

    private string m_Master;
    private IApplication m_App;

    public void Prologue(Runner runner, FID id)
    {
      m_App = runner.App;
      m_Master = File.ReadAllText(FN);
    }

    public bool Epilogue(Runner runner, FID id, Exception error) => false;

    private string read(ISourceText src)
    {
      var sb = new StringBuilder();
      while(!src.EOF) sb.Append(src.ReadChar());
      return sb.ToString();
    }


    [Run]
    public void DefaultCtor_01()
    {
      using var sut = new FileSource(FN);
      var got = read(sut);

      Aver.AreEqual(m_Master, got);
    }

    [Run]
    public void LineComparison()
    {
      using var sut = new FileSource(FN);
      var got = read(sut);

      var lines = got.SplitLines();

      Aver.AreEqual(42, lines.Length);


      Aver.AreEqual("1: This file has UNICODE Characters", lines[1 - 1]);

      Aver.AreEqual(95, lines[9 - 1].IndexOf("我能吞下玻璃而不傷身體"));

      Aver.AreEqual("18: ЭЮЯ", lines[18-1]);
      Aver.AreEqual("ვეპხის ტყაოსანი შოთა რუსთაველი", lines[38-1]);

      Aver.AreEqual("42: eof.", lines[42 - 1]);
    }
  }
}
