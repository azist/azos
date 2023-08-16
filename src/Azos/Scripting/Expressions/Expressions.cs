/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Scripting.Expressions
{
  public abstract class Expression : IExpression
  {
    public void Configure(IConfigSectionNode node)
    {
      if (node==null || !node.Exists) return;
      ConfigAttribute.Apply(this, node);
      DoConfigure(node);
    }

    protected virtual void DoConfigure(IConfigSectionNode node)
    {
    }

    public abstract object EvaluateObject(object context);

    /// <summary> Makes entity with descriptive error </summary>
    protected virtual T Make<T>(IConfigSectionNode parent, string section) where T : IConfigurable
    {
      try
      {
        return FactoryUtils.MakeAndConfigure<T>(parent.NonNull(nameof(parent))[section.NonBlank(nameof(section))]);
      }
      catch(ConfigException error)
      {
        throw new ScriptingException("Expression `{0}` declaration error while trying to make `{1}` around:  ... {2} ...".Args(GetType().Name, section, parent.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact)), error);
      }
    }

    /// <summary> Makes entity with descriptive error </summary>
    protected virtual T Make<T>(IConfigSectionNode node) where T : IConfigurable
    {
      node.NonNull(nameof(node));
      try
      {
        return FactoryUtils.MakeAndConfigure<T>(node);
      }
      catch (ConfigException error)
      {
        throw new ScriptingException("Expression `{0}` declaration error while trying to make instance around:  ... {1} ...".Args(GetType().Name, node.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact)), error);
      }
    }
  }

  public abstract class Expression<TContext, TResult> : Expression
  {
    public sealed override object EvaluateObject(object context)
    => Evaluate((TContext)context);

    public abstract TResult Evaluate(TContext context);
  }


}
