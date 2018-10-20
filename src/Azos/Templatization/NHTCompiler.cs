
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;

namespace Azos.Templatization
{
  /// <summary>
  /// Compiles templates based of text files that use C# language syntax,
  /// First compiles by the TextJSTemplateCompiler then by the TextCSTemplateCompiler
  /// </summary>
  public class NHTCompiler : TextCSTemplateCompiler
  {
    #region CONSTS
    public const string CONFIG_JS_COMPILER_SECT = "jsopt";
    #endregion

    #region .ctor
    public NHTCompiler() : base () { }

    public NHTCompiler(params ITemplateSource<string>[] sources) : base (sources) { }

    public NHTCompiler(IEnumerable<ITemplateSource<string>> sources) : base (sources) { }
    #endregion

    #region Fields
    private string m_JSConfig;
    #endregion

    #region Props
    public override string LanguageName
    {
      get { return CoreConsts.CS_LANGUAGE; }
    }

    public override string LanguageSourceFileExtension
    {
      get { return CoreConsts.CS_EXTENSION; }
    }

    [Config("$jsopt")]
    public string JSConfigStr
    {
      get { return m_JSConfig; }
      set
      {
        EnsureNotCompiled();
        m_JSConfig = value;
      }
    }
    #endregion

    #region Protected

    protected override string GetCompileUnitSourceContent(CompileUnit unit, out string className)
    {
      className = unit.TemplateSource.InferClassName();

      var cmp = getJSCompiler();
      cmp.IncludeTemplateSource(unit.TemplateSource);
      cmp.Compile();
      return cmp.First().CompiledSource;
    }
    #endregion

    #region .pvt
    public TextJSTemplateCompiler getJSCompiler()
    {
      if (m_JSConfig.IsNotNullOrWhiteSpace())
      {
        var conf = m_JSConfig.AsLaconicConfig();
        if (conf != null && conf.Exists)
          return FactoryUtils.MakeAndConfigure<TextJSTemplateCompiler>(conf, typeof(TextJSTemplateCompiler));
      }
      return new TextJSTemplateCompiler();
    }
    #endregion
  }
}
