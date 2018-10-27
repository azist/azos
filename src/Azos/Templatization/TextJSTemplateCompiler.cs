/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Azos.Conf;
using Azos.Data;

namespace Azos.Templatization
{
  /// <summary>
  /// Compiles templates based of text files that use js language syntax,
  /// compiles config to inline function
  /// </summary>
  /// <example>
  ///   argument[0] - root container 'id' or 'node'
  ///   argument[1] - context for variable evaluation
  ///   <code>
  ///   function() {
  ///     /*** #preprocessor_directive#
  ///     {
  ///       tag-name="inner text"
  ///       property-name="property value"
  ///       evaluted-property-name="@prop@ evaluated property"
  ///       on-event="function() { click(); }"
  ///     }
  ///     ***/
  ///   }
  ///   </code>
  /// </example>
  [Obsolete("This compiler is for legacy support only. Plan to convert to newer LJSCompiler syntax")]
  public class TextJSTemplateCompiler : TemplateCompiler
  {
    public const string CONFIG_DOM_GENERATOR_SECT = "dom-gen";
    public const string DOM_GENERATOR_SKIP_PRAGMA = " #skip#";

    #region .ctor
    public TextJSTemplateCompiler() : base() { }

    public TextJSTemplateCompiler(params ITemplateSource<string>[] sources) : base(sources) { }

    public TextJSTemplateCompiler(IEnumerable<ITemplateSource<string>> sources) : base(sources) { }
    #endregion

    #region Private/Prot Fields
    private DOMGenerator m_DOMGenerator;
    private string m_DomGenConfig;
    private Dictionary<string, string> m_MapLjsIdsToJsIds = new Dictionary<string, string>();
    #endregion

    #region Properties
    public override string LanguageName
    {
      get { return CoreConsts.JS_LANGUAGE; }
    }

    public override string LanguageSourceFileExtension
    {
      get { return CoreConsts.JS_EXTENSION; }
    }

    [Config(CONFIG_DOM_GENERATOR_SECT)]
    public DOMGenerator DOMGenerator
    {
      get { return m_DOMGenerator ?? DOMGenerator.Default; }
      set
      {
        EnsureNotCompiled();
        m_DOMGenerator = value;
      }
    }

    [Config("$"+CONFIG_DOM_GENERATOR_SECT)]
    public string DomGenConfig
    {
      get { return m_DomGenConfig; }
      set
      {
        EnsureNotCompiled();
        m_DomGenConfig = value;
        m_DOMGenerator = FactoryUtils.MakeAndConfigure<DOMGenerator>(m_DomGenConfig.AsLaconicConfig(),typeof(DOMGenerator));
      }
    }
    #endregion

    #region Protected
    protected override void DoCompileTemplateSource(CompileUnit unit)
    {
      var icname =  unit.TemplateSource.InferClassName();
      var text =  getSource(unit);

      var blockCount = 0;
      var result = transpile(text, "\"\\*{3}(.*?)\\*{3}\"", true, ref blockCount);
      result = transpile(result, @"\/\*{3}(.*?)\*{3}\/", false, ref blockCount);
      unit.CompiledSource = evaluateIds(result);
    }

    protected override void DoCompileCode()
    {
      throw new NotSupportedException("TextJSTemplateCompiler does not support compilation");
    }
    #endregion

    #region .pvt

    private string getSource(CompileUnit unit)
    {
      var source = unit.TemplateSource.GetSourceContent().ToString().Trim();
      var r = new Regex(@"\/\*{3}\@(.*?)\*{3}\/", RegexOptions.Singleline); //Include files, /***@ path ***/
      var path = string.Empty;
      try
      {
        return r.Replace(source, m => {
          path = m.Groups[1].AsString().Trim();
          return unit.TemplateSource.GetRelativeContent(path).AsString();
        });
      }
      catch (Exception error)
      {
        throw new TemplateParseException(StringConsts.TEMPLATE_JS_COMPILER_INCLUDE_ERROR.Args(path, error.Message), error);
      }
    }

    private string transpile(string source, string regex, bool wrapWithFunction, ref int blockCount)
    {
      var r = new Regex(regex, RegexOptions.Singleline);
      try
      {
        var bc = blockCount;
        var result = r.Replace(source, m =>
        {
          //check skip pragma
          var sourcePart = m.Groups[1].Value;

          var sourceLength = sourcePart.Length;
          var pragmaLength = DOM_GENERATOR_SKIP_PRAGMA.Length;
          if (sourceLength >= pragmaLength)
          {
            var checkedValue = sourcePart.TakeFirstChars(pragmaLength);
            if (checkedValue.EqualsOrdIgnoreCase(DOM_GENERATOR_SKIP_PRAGMA))
            {
               return m.Value.Remove(m.Value.IndexOf(checkedValue), pragmaLength);
            }
          }

          bc++;

          var spacesCount = 0;
          var idx = m.Index - 1;
          while (true)
          {
            var i = idx - spacesCount;
            if (i < 0) break;

            var c = source[i];
            if (c != ' ') break;
            spacesCount++;
          }

          var conf = sourcePart.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          if (!wrapWithFunction)
            return DOMGenerator.Generate(conf, spacesCount, ref bc, ref m_MapLjsIdsToJsIds);
          else
            return DOMGenerator.GenerateSEFFunction(conf, spacesCount, ref bc, ref m_MapLjsIdsToJsIds);
        });
        blockCount = bc;
        return result;
      }
      catch (Exception error)
      {
        throw new TemplateParseException(StringConsts.TEMPLATE_JS_COMPILER_CONFIG_ERROR + error.Message, error);
      }
    }

    private string evaluateIds(string value)
    {
      foreach (var pair in m_MapLjsIdsToJsIds)
      {
        value = value.Replace(pair.Key, pair.Value);
      }
      return value;
    }
    #endregion
  }
}
