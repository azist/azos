/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

namespace Azos.CodeAnalysis
{
  /// <summary>
  /// Performs parsing of token streams provided by lexers
  /// </summary>
  public abstract class Parser<TLexer> : CommonCodeProcessor, IParser where TLexer : ILexer
  {
    protected Parser(IAnalysisContext context, IEnumerable<TLexer> input, MessageList messages = null, bool throwErrors = false) :
      base(context, messages, throwErrors)

    {
      m_Input = input.ToList();
    }

    private bool m_HasParsed;
    private List<TLexer> m_Input;

    /// <summary>
    /// Returns lexers that feed this parser
    /// </summary>
    public IEnumerable<TLexer> Input => m_Input;

    /// <summary>
    /// Lists source lexers that supply token stream for parsing
    /// </summary>
    public IEnumerable<ILexer> SourceInput => (IEnumerable<ILexer>)m_Input;

    /// <summary>
    /// Indicates whether Parse() already happened
    /// </summary>
    public bool HasParsed => m_HasParsed;

    /// <summary>
    /// Performs parsing if it has not been performed yet
    /// </summary>
    public void Parse()
    {
      try
      {
        DoParse();
      }
      finally
      {
        m_HasParsed = true;
      }
    }

    /// <summary>
    /// Override to perform actual parsing
    /// </summary>
    protected abstract void DoParse();

  }
}
