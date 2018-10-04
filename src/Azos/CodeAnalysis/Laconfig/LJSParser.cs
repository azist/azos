
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Parses Laconfig lexer output into Laconic Java Script node graph embodies by LJSData
  /// </summary>
  public sealed partial class LJSParser : Parser<LaconfigLexer>
  {

    public LJSParser(LaconfigLexer input,  MessageList messages = null, bool throwErrors = false) :
        this(new LJSData(), input, messages, throwErrors)
    {
    }


    public LJSParser(LJSData context, LaconfigLexer input,  MessageList messages = null, bool throwErrors = false) :
        base(context, new LaconfigLexer[]{ input }, messages, throwErrors)
    {
        m_Lexer = Input.First();
    }

    private LaconfigLexer m_Lexer;

    public LaconfigLexer Lexer         { get { return m_Lexer;} }
    public LJSData       ResultContext { get{ return Context as LJSData;} }

    public override Language Language  { get { return LJSLanguage.Instance; } }

    public override string MessageCodeToString(int code)
    {
      return ((LaconfigMsgCode)code).ToString();
    }


    protected override void DoParse()
    {
      try
      {
        var tree = ResultContext.ResultObject;

        tokens = Lexer.GetEnumerator();
        fetchPrimary();
        doRoot( tree );

      }
      catch(abortException)
      {

      }
    }
  }
}
