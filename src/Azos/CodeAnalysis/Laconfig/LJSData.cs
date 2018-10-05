/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.CodeAnalysis.Laconfig
{
  /// <summary>
  /// Represents Laconic Java Script parser result - the Laconic Java Script Document Object Model
  /// </summary>
  public sealed class LJSData : ObjectResultAnalysisContext<LJSTree>
  {

      public LJSData() : base(null)
      {
        m_ResultObject = new LJSTree();
      }

       public LJSData(IAnalysisContext ctx) : base(ctx)
      {
        m_ResultObject = new LJSTree();
      }


      public override Language Language => LJSLanguage.Instance;


      public override string MessageCodeToString(int code)
      {
          return ((LaconfigMsgCode)code).ToString();
      }
  }

  /// <summary>
  /// Represents the result of parsing of LJS content into Laconic Java Script Document Object Model
  /// </summary>
  public sealed class LJSTree //wrapper class reserved for tree-wide attrs (if any)
  {
    /// <summary>Tree root </summary>
    public LJSSectionNode Root { get; internal set; }

    /// <summary> Attaches arbitrary data, such as the one used by the generator </summary>
    public object Data{ get; set;}

    /// <summary>
    /// Returns The tree transpiled by the transpiler
    /// </summary>
    public string TranspiledContent{ get; set;}
  }

  public abstract class LJSNode
  {
    /// <summary> The first laconic content token that starts this node </summary>
    public  LaconfigToken StartToken { get; internal set; }

    /// <summary> Parent node that this node is in</summary>
    public  LJSSectionNode   Parent { get; internal set; }

    /// <summary> Node name - name of attribute or section </summary>
    public  string Name { get; internal set; }

    /// <summary> Attaches arbitrary data, such as the one used by the generator </summary>
    public object Data { get; set; }

    /// <summary> Prints tree into string builder </summary>
    public abstract void Print(StringBuilder builder, int indent);
  }

  public sealed class LJSAttributeNode : LJSNode
  {
    /// <summary>The value of attribute node</summary>
    public string Value { get; internal set; }

    public override void Print(StringBuilder builder, int indent)
    {
      builder.Append(new string(' ',indent *2));
      builder.AppendLine("{0} {1} -> {2}".Args("Attr", StartToken.Type, Value));
    }
  }

  public sealed class LJSSectionNode : LJSNode
  {
    /// <summary>
    /// The name assigned to this section node like div='pragma1'{} to be used by the script/generator,
    /// most likely used for assigning a deterministic variable name in java script to the element
    /// </summary>
    public string TranspilerPragma { get; internal set; }
    /// <summary> All nodes in order of declaration: sections, content, script, attributes</summary>
    public LJSNode[] Children { get; internal set; }

    public override void Print(StringBuilder builder, int indent)
    {
      builder.Append(new string(' ',indent *2));
      builder.AppendLine("{0} {1} -> {2} = {3}".Args("Section", StartToken.Type,  Name, TranspilerPragma));
      foreach(var c in Children)
        c.Print(builder, indent+1);
    }
  }

  /// <summary> Represents textual content block, such as:   div{ content block text }</summary>
  public sealed class LJSContentNode : LJSNode
  {
    /// <summary>Textual content</summary>
    public string Content { get; internal set; }

    public override void Print(StringBuilder builder, int indent)
    {
      builder.Append(new string(' ',indent *2));
      builder.AppendLine("{0} {1} -> {2}".Args("Content", StartToken.Type, Content));
    }
  }

  /// <summary> Represents script textual content block, such as: # let x =1;</summary>
  public sealed class LJSScriptNode : LJSNode
  {
    /// <summary>Textual script content</summary>
    public string Script { get; internal set; }

    public override void Print(StringBuilder builder, int indent)
    {
      builder.Append(new string(' ',indent *2));
      builder.AppendLine("{0} {1} -> {2}".Args("Script", StartToken.Type, Script));
    }
  }




}
