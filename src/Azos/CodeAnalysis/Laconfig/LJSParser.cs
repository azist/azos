/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

namespace Azos.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Parses Laconfig lexer output into Laconic Java Script node graph embodies by LJSData
  /// </summary>
  public sealed partial class LJSParser : Parser<LaconfigLexer>
  {
    public LJSParser(LaconfigLexer input, MessageList messages = null, bool throwErrors = false) :
        this(new LJSData(), input, messages, throwErrors)
    {
    }

    public LJSParser(LJSData context, LaconfigLexer input, MessageList messages = null, bool throwErrors = false) :
        base(context, new LaconfigLexer[] { input }, messages, throwErrors)
    {
      m_Lexer = Input.First();
    }

    private LaconfigLexer m_Lexer;

    public LaconfigLexer Lexer => m_Lexer;

    public LJSData ResultContext => Context as LJSData;

    public override Language Language => LJSLanguage.Instance;

    public override string MessageCodeToString(int code)
    => ((LaconfigMsgCode)code).ToString();

    protected override void DoParse()
    {
      try
      {
        var tree = ResultContext.ResultObject;

        tokens = Lexer.GetEnumerator();
        fetchPrimary();
        doRoot(tree);

      }
      catch (abortException)
      {

      }
    }

  }
}
