/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

namespace Azos.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Parses Laconfig lexer output into laconic configuration node graph
  /// </summary>
  public sealed partial class LaconfigParser : Parser<LaconfigLexer>
  {
    public LaconfigParser(LaconfigData context, LaconfigLexer input, MessageList messages = null, bool throwErrors = false) :
        base(context, new LaconfigLexer[] { input }, messages, throwErrors)
    {
      m_Lexer = Input.First();
    }

    private LaconfigLexer m_Lexer;

    public LaconfigLexer Lexer => m_Lexer;

    public LaconfigData ResultContext => Context as LaconfigData;

    public override Language Language => LaconfigLanguage.Instance;

    public override string MessageCodeToString(int code) => ((LaconfigMsgCode)code).ToString();

    protected override void DoParse()
    {
      try
      {
        var config = ResultContext.ResultObject;

        tokens = Lexer.GetEnumerator();
        fetchPrimary();
        doRoot(config);

        if (config.Root.Exists)
          config.Root.ResetModified();

      }
      catch (abortException)
      {

      }
    }

  }
}
