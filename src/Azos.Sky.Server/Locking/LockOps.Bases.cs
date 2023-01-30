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
          [Serializable]
          public abstract class Op
          {
            [NonSerialized]
            protected string m_Path;

            /// <summary>
            /// Invoked by the server to prepare this node before execution/before lock
            /// </summary>
            public virtual void Prepare(EvalContext ctx, string path) { m_Path = path + this.GetType().Name + "/";  }
          }

          [Serializable]
          public abstract class OperatorOp : Op
          {
            public abstract bool GetValue(EvalContext ctx);
          }

              [Serializable]
              public abstract class BinaryOperatorOp : OperatorOp
              {
                public BinaryOperatorOp(OperatorOp lop, OperatorOp rop)
                {
                  if (lop==null||rop==null)
                   throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+"BinOp(lop|rop=null)");

                  LeftOperand = lop;
                  RightOperand = rop;
                }
                public readonly OperatorOp LeftOperand;
                public readonly OperatorOp RightOperand;

                public override void Prepare(EvalContext ctx, string path)
                {
                  base.Prepare(ctx, path);
                  LeftOperand.Prepare(ctx, m_Path);
                  RightOperand.Prepare(ctx, m_Path);
                }
              }

              [Serializable]
              public abstract class UnaryOperatorOp : OperatorOp {}


      [Serializable]
      public abstract class StatementOp : Op
      {
          public abstract void Execute(EvalContext ctx);
      }

          [Serializable]
          public abstract class FlowOp : StatementOp
          {
            public FlowOp() { }
          }

      /// <summary>
      /// Represents a change to the state, returns true if change succeeds, false otherwise
      /// </summary>
      [Serializable]
      public abstract class ChangeOp : UnaryOperatorOp
      {
        public ChangeOp(string table, string var) : base()
        {
          if (table.IsNullOrWhiteSpace() || var.IsNullOrWhiteSpace())
                throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+"Change(table|var==null|empty)");

          Table = table;
          Var = var;
        }

          public readonly string Table; [NonSerialized]internal Server.Table _Table;
          public readonly string Var;

          public override void Prepare(EvalContext ctx, string path)
          {
            base.Prepare(ctx, path);
            _Table = ctx.GetExistingOrMakeTableByName(Table);
          }
      }


      [Serializable]
      public abstract class SelectOp : StatementOp
      {
        public SelectOp(string intoName)
        {
          if (intoName.IsNullOrWhiteSpace() )
                throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+"Select(intoName=null|empty)");

          IntoName = intoName;
        }
        public readonly string IntoName;
      }

  }

}
