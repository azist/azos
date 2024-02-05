/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;

namespace Azos.Scripting.Expressions
{
  /// <summary>
  /// Implements .Select(f)
  /// </summary>
  public class Select<TContext, TResult, TOperand> : UnaryOperator<TContext, IEnumerable<TResult>, IEnumerable<TOperand>>
  {
    public const string CONFIG_MAP_SECTION = "map";


    public Expression<TOperand, TResult> Map { get; set; }

    public sealed override IEnumerable<TResult> Evaluate(TContext context)
    {
      var operand = Operand.NonNull(nameof(Operand));
      var root = Map.NonNull(nameof(Map));

      var source = operand.Evaluate(context);
      return source.Select(e => root.Evaluate(e));
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      Map = Make<Expression<TOperand, TResult>>(node, CONFIG_MAP_SECTION);
    }
  }

  /// <summary>
  /// Implements .Where(f)
  /// </summary>
  public class Where<TContext, TOperand> : UnaryOperator<TContext, IEnumerable<TOperand>, IEnumerable<TOperand>>
  {
    public const string CONFIG_FILTER_SECTION = "filter";


    public Predicate<TOperand> Filter { get; set; }

    public sealed override IEnumerable<TOperand> Evaluate(TContext context)
    {
      var operand = Operand.NonNull(nameof(Operand));
      var filter = Filter.NonNull(nameof(Filter));

      var source = operand.Evaluate(context);
      return source.Where(e => filter.Evaluate(e));
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      Filter = Make<BoolFilter<TOperand>>(node, CONFIG_FILTER_SECTION);
    }
  }

  /// <summary>
  /// Implements .Any(f)
  /// </summary>
  public class Any<TContext, TOperand> : UnaryOperator<TContext, bool, IEnumerable<TOperand>>
  {
    public const string CONFIG_FILTER_SECTION = "filter";


    public Predicate<TOperand> Filter { get; set; }

    public sealed override bool Evaluate(TContext context)
    {
      var operand = Operand.NonNull(nameof(Operand));
      var filter = Filter.NonNull(nameof(Filter));

      var source = operand.Evaluate(context);
      return source.Any(e => filter.Evaluate(e));
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      Filter = Make<BoolFilter<TOperand>>(node, CONFIG_FILTER_SECTION);
    }
  }


  /// <summary>
  /// Implements .Skip(count)
  /// </summary>
  public class Skip<TContext, TOperand> : UnaryOperator<TContext, IEnumerable<TOperand>, IEnumerable<TOperand>>
  {
    [Config] public int Count { get; set; }

    public sealed override IEnumerable<TOperand> Evaluate(TContext context)
    {
      var operand = Operand.NonNull(nameof(Operand));

      var source = operand.Evaluate(context);
      return source.Skip(Count);
    }
  }


  /// <summary>
  /// Implements .First(f)
  /// </summary>
  public class First<TContext, TOperand> : UnaryOperator<TContext, TOperand, IEnumerable<TOperand>>
  {
    public const string CONFIG_FILTER_SECTION = "filter";


    public Predicate<TOperand> Filter { get; set; }

    public sealed override TOperand Evaluate(TContext context)
    {
      var operand = Operand.NonNull(nameof(Operand));
      var filter = Filter.NonNull(nameof(Filter));

      var source = operand.Evaluate(context);
      return source.First(e => filter.Evaluate(e));
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      Filter = Make<BoolFilter<TOperand>>(node, CONFIG_FILTER_SECTION);
    }
  }

  /// <summary>
  /// Implements .FirstOrDefault(f)
  /// </summary>
  public class FirstOrDefault<TContext, TOperand> : UnaryOperator<TContext, TOperand, IEnumerable<TOperand>>
  {
    public const string CONFIG_FILTER_SECTION = "filter";


    public Predicate<TOperand> Filter { get; set; }

    public sealed override TOperand Evaluate(TContext context)
    {
      var operand = Operand.NonNull(nameof(Operand));
      var filter = Filter.NonNull(nameof(Filter));

      var source = operand.Evaluate(context);
      return source.FirstOrDefault(e => filter.Evaluate(e));
    }

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      Filter = Make<BoolFilter<TOperand>>(node, CONFIG_FILTER_SECTION);
    }
  }

}
