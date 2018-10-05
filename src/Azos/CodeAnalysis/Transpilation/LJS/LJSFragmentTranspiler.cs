/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Text;

using Azos.Conf;
using Azos.CodeAnalysis.Laconfig;

namespace Azos.CodeAnalysis.Transpilation.LJS
{
  /// <summary>
  /// Transpiles a single LJS fragment
  /// </summary>
  public class LJSFragmentTranspiler : Transpiler<LJSParser>
  {
    public LJSFragmentTranspiler(LJSUnitTranspilationContext context, LJSParser parser,  MessageList messages = null, bool throwErrors = false)
             : base(context, parser, messages, throwErrors)
    {
    }

    public override Language Language => Parser.Language;
    public LJSUnitTranspilationContext UnitContext => (LJSUnitTranspilationContext)base.Context;

    public override string MessageCodeToString(int code)
    {
      return Parser.MessageCodeToString(code);
    }

    protected override void DoTranspile()
    {
      var ljsTree = Parser.ResultContext.ResultObject;
      ljsTree.TranspiledContent = DoTranspileTree(ljsTree);
    }

    protected virtual string DoTranspileTree(LJSTree tree)
    {
      var output = new StringBuilder();

      var idParent = UnitContext.GenerateID();
      output.AppendLine($"const {idParent} = arguments[0];");
      output.AppendLine($"if ({UnitContext.TypePrefix}.isString({idParent})) {idParent} = {UnitContext.DomPrefix}.id({idParent});");
      output.AppendLine();

      DoTranspileNode(output, 0, idParent, tree.Root);

      output.AppendLine($"return {idParent};");
      return output.ToString();
    }

    protected virtual void DoTranspileNode(StringBuilder output, int indentLevel, string idParent, LJSNode node)
    {
      if (node is LJSSectionNode nSection)
      {
        var id = DoEmitSectionNode(output, indentLevel, idParent, nSection);
        //indentLevel++;
        foreach(var child in nSection.Children)
          DoTranspileNode(output, indentLevel, id, child);
      }
      else if (node is LJSAttributeNode nAttr)
      {
        DoEmitAttributeNode(output, indentLevel, idParent, nAttr);
      }
      else if (node is LJSContentNode nContent)
      {
        DoEmitContentNode(output, indentLevel, idParent, nContent);
      }
      else if (node is LJSScriptNode nScript)
      {
        DoEmitScriptNode(output, indentLevel, idParent, nScript);
      }
      else
       EmitMessage(MessageType.Error,
                   -1,//todo Give proper error code
                   new Source.SourceCodeRef(UnitContext.UnitName),
                   token: node.StartToken,
                   text: "LJSNode type is unsupported: {0}".Args(node.GetType().DisplayNameWithExpandedGenericArgs()));

      output.AppendLine();
    }

    protected virtual void DoPad(StringBuilder output, int indentLevel)
    {
      for(var i=0; i<indentLevel*UnitContext.IndentWidth; i++) output.Append(' ');
    }

    protected virtual string DoEvaluateExpression(string source)
    {
      //todo 20180403 DKh Add Localizer support - if the value starts with @ then treat is a localizer key
      //tbd: where do we take schema and field names for localizer? Schema probably from Class name
      if (source.IsNullOrWhiteSpace()) return string.Empty;

      if (source.StartsWith("??")) return source.Substring(1).EscapeJSLiteral();//escape ?? -> ?

      if (source[0]=='?')return source.Substring(1);

      return "'{0}'".Args(source.EscapeJSLiteral());
    }


    protected virtual string DoEmitScriptNode(StringBuilder output, int indentLevel, string idParent, LJSScriptNode node)
    {
      DoPad(output, indentLevel);              // See  EscapeJSLiteral()
      output.AppendLine(node.Script);//script gets output as-is always on a separate line
      return null;
    }

    protected virtual string DoEmitAttributeNode(StringBuilder output, int indentLevel, string idParent, LJSAttributeNode node)
    {
      //proverit na ? js expression i escape js listeral
      DoPad(output, indentLevel);
      //     WVDOM.sa(divA, 'name-atr', value);
      output.AppendFormat("{0}.sa({1},{2},{3});\n", UnitContext.DomPrefix, idParent, DoEvaluateExpression(node.Name), DoEvaluateExpression(node.Value));
      return null;
    }

    protected virtual string DoEmitSectionNode(StringBuilder output, int indentLevel, string idParent, LJSSectionNode node)
    {
      var sid = UnitContext.GenerateID();         // See  EscapeJSLiteral()
      //proverit na ? js expression
      //proverit na nazvanie componenta
      //proverit node.TranspilerPragma na alias
      DoPad(output, indentLevel);
      if (char.IsUpper(node.Name[0]))
      {
        //чтобы создать класс мне нужно получить все атрибуты
      }
      else
      {
        output.AppendLine($"const {sid} = {UnitContext.DomPrefix}.ce({DoEvaluateExpression(node.Name)});");
        if (idParent.IsNotNullOrWhiteSpace())
        {
          DoPad(output, indentLevel);
          output.AppendLine($"{UnitContext.DomPrefix}.ac({idParent}, {sid});");
        }
      }

      return sid;
    }

    protected virtual string DoEmitContentNode(StringBuilder output, int indentLevel, string idParent, LJSContentNode node)
    {
      var sid = UnitContext.GenerateID();
      DoPad(output, indentLevel);

      output.AppendLine($"const {sid} = {UnitContext.DomPrefix}.ctn({DoEvaluateExpression(node.Content)});");
      if (idParent.IsNotNullOrWhiteSpace())
      {
        DoPad(output, indentLevel);
        output.AppendLine($"{UnitContext.DomPrefix}.ac({idParent}, {sid});");
      }
      return sid;
    }
  }
}
