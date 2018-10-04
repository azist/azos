using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.CodeAnalysis.Transpilation
{
  /// <summary>
  /// Transpiles content into another form
  /// </summary>
  public abstract class Transpiler<TParser> : CommonCodeProcessor, ITranspiler where TParser : IParser
  {
    protected Transpiler(IAnalysisContext context, TParser parser,  MessageList messages = null, bool throwErrors = false)
             : base( context,  messages, throwErrors)
    {
      m_Parser = parser;
    }

    private bool m_HasTranspiled;
    private TParser m_Parser;

    public IParser SourceParser => m_Parser;
    public TParser Parser       => m_Parser;


    /// <summary>
    /// Indicates whether Transpile() already happened
    /// </summary>
    public bool HasTranspiled { get {return m_HasTranspiled;} }

    /// <summary>
    /// Performs transpilation if it has not been performed yet
    /// </summary>
    public void Transpile()
    {
      try
      {
          DoTranspile();
      }
      finally
      {
          m_HasTranspiled = true;
      }
    }


    /// <summary>
    /// Override to perform actual transpilation
    /// </summary>
    protected abstract void DoTranspile();
  }
}
