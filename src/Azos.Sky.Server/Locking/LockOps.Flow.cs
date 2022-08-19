/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Sky.Locking.Server;

namespace Azos.Sky.Locking
{
  /// <summary>
  /// Represents operations of locking transaction
  /// </summary>
  public static partial class LockOp
  {

              public static AbortOp Abort() { return new AbortOp();}
              /// <summary>
              /// Unconditionally aborts the processing
              /// </summary>
              [Serializable]
              public sealed class AbortOp : FlowOp
              {
                internal AbortOp() { }

                public override void Execute(EvalContext ctx)
                {
                  ctx.Abort(this.m_Path);
                }
              }


              public static AssertOp Assert(OperatorOp condition) { return new AssertOp(condition);}
              /// <summary>
              /// Asserts the condition and aborts if it is not true
              /// </summary>
              [Serializable]
              public sealed class AssertOp : FlowOp
              {
                internal AssertOp(OperatorOp condition)
                {
                 if (condition==null)
                        throw new LockingException(StringConsts.ARGUMENT_ERROR+"Assert(condition=null)");

                 Condition = condition;
                }

                public readonly OperatorOp Condition;

                public override void Prepare(EvalContext ctx, string path)
                {
                  base.Prepare(ctx, path);
                  Condition.Prepare(ctx, m_Path);
                }

                public override void Execute(EvalContext ctx)
                {
                  var val = Condition.GetValue(ctx);
                  if (ctx.Aborted) return;
                  if (!val) ctx.Abort( m_Path );
                }
              }

              public static AnywayContinueAfterOp AnywayContinueAfter(ChangeOp operation, bool resetAbort = false)
              {
                 return new AnywayContinueAfterOp(operation, resetAbort);
              }
              /// <summary>
              /// Ignores the result of a change operation whether it returns true or false and continues
              /// </summary>
              [Serializable]
              public sealed class AnywayContinueAfterOp : FlowOp
              {
                internal AnywayContinueAfterOp(ChangeOp operation, bool resetAbort)
                {
                 if (operation==null)
                        throw new LockingException(StringConsts.ARGUMENT_ERROR+"AnywayContinueAfter(operation=null)");
                 Operation = operation;
                }

                public readonly ChangeOp Operation;
                public readonly bool ResetAbort;

                public override void Prepare(EvalContext ctx, string path)
                {
                  base.Prepare(ctx, path);
                  Operation.Prepare(ctx, m_Path);
                }

                public override void Execute(EvalContext ctx)
                {
                  Operation.GetValue(ctx);
                  //ignore the resulting abort
                  if (ResetAbort) ctx.ResetAbort();
                }
              }


              public static BlockOp Block(params StatementOp[] statements )
              {
                 return new BlockOp(statements);
              }
              /// <summary>
              /// Block of statements
              /// </summary>
              [Serializable]
              public sealed class BlockOp : FlowOp
              {
                internal BlockOp(StatementOp[] statements)
                {
                 if (statements==null || statements.Length<1)
                        throw new LockingException(StringConsts.ARGUMENT_ERROR+"BlockOp(statements=null|0)");
                 Statements = statements;
                }

                public readonly StatementOp[] Statements;

                public override void Prepare(EvalContext ctx, string path)
                {
                  base.Prepare(ctx, path);
                  foreach(var statement in Statements) statement.Prepare(ctx, m_Path);
                }

                public override void Execute(EvalContext ctx)
                {
                  foreach( var statement in Statements)
                  {
                    statement.Execute(ctx);
                    if (ctx.Aborted) return;
                  }
                }
              }



        public static IfOp If(OperatorOp condition, StatementOp then, StatementOp elze = null)
        {
            return new IfOp(condition, then, elze);
        }
        /// <summary>
        /// Conditional statement
        /// </summary>
        [Serializable]
        public sealed class IfOp : FlowOp
        {
          internal IfOp(OperatorOp condition, StatementOp then, StatementOp elze)
          {
            if (condition==null || then==null)
                  throw new LockingException(StringConsts.ARGUMENT_ERROR+"IfOp(condition|then==null)");
            Condition = condition;
            Then = then;
            Else = elze;
          }

          public readonly OperatorOp Condition;
          public readonly StatementOp Then;
          public readonly StatementOp Else;

          public override void Prepare(EvalContext ctx, string path)
          {
            base.Prepare(ctx, path);

            Condition.Prepare(ctx, m_Path);
            Then.Prepare(ctx, m_Path);
            if (Else!=null)
             Else.Prepare(ctx, m_Path);
          }

          public override void Execute(EvalContext ctx)
          {
            var result = Condition.GetValue(ctx);
            if (ctx.Aborted) return;

            if (result)
             Then.Execute(ctx);
            else
            {
              if (Else!=null) Else.Execute(ctx);
            }
          }
        }







  }

}
