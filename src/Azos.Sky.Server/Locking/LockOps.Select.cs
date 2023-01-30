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

          public static SelectConstantValueOp SelectConstantValue(string intoName, object value)
          {
            return new SelectConstantValueOp(intoName, value);
          }
          /// <summary>
          /// Returns the constant value as a named key which is returned back to the client
          /// </summary>
          [Serializable]
          public sealed class SelectConstantValueOp : SelectOp
          {
            internal SelectConstantValueOp(string intoName, object value) : base(intoName)
            {
              Value = value;
            }
            public readonly object Value;

            public override void Prepare(EvalContext ctx, string path)
            {
              base.Prepare(ctx, path);
            }

            public override void Execute(EvalContext ctx)
            {
              ctx.AddData( IntoName, Value );
            }
          }




          public static SelectOperatorValueOp SelectOperatorValue(string intoName, OperatorOp operand)
          {
            return new SelectOperatorValueOp(intoName, operand);
          }
          /// <summary>
          /// Returns the value of operator as a named key which is returned back to the client
          /// </summary>
          [Serializable]
          public sealed class SelectOperatorValueOp : SelectOp
          {
            internal SelectOperatorValueOp(string intoName, OperatorOp operand) : base(intoName)
            {
              if (operand==null)
                    throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+"SelectOperatorValue(operand=null)");

              Operand = operand;
            }
            public readonly OperatorOp Operand;

            public override void Prepare(EvalContext ctx, string path)
            {
              base.Prepare(ctx, path);
              Operand.Prepare(ctx, m_Path);
            }

            public override void Execute(EvalContext ctx)
            {
              var val = Operand.GetValue(ctx);
              if (ctx.Aborted) return;
              ctx.AddData( IntoName, val );
            }
          }


          public static SelectVarValueOp SelectVarValue(string intoName,
                                                        string table,
                                                        string var,
                                                        bool ignoreThisSession=true,
                                                        bool abortIfNotFound=false,
                                                        bool selectMany=false)
          {
              return new SelectVarValueOp(intoName, table, var, ignoreThisSession, abortIfNotFound, selectMany);
          }

          /// <summary>
          /// Returns the value of a named variable either a a single or Variable[] object.
          /// Optionally aborts if variable was not set
          /// </summary>
          [Serializable]
          public sealed class SelectVarValueOp : SelectOp
          {
            internal SelectVarValueOp(string intoName, string table, string var, bool ignoreThisSession, bool abortIfNotFound, bool selectMany) : base(intoName)
            {
              if (table.IsNullOrWhiteSpace() || var.IsNullOrWhiteSpace())
                    throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+"SelectVarValue(table|name=null|empty)");

              Table = table;
              Var = var;
              IgnoreThisSession = ignoreThisSession;
              AbortIfNotFound = abortIfNotFound;
              SelectMany = selectMany;
            }
            public readonly string Table; [NonSerialized]internal Server.Table _Table;
            public readonly string Var;
            public readonly bool IgnoreThisSession;
            public readonly bool AbortIfNotFound;
            public readonly bool SelectMany;


            public override void Prepare(EvalContext ctx, string path)
            {
              base.Prepare(ctx, path);
              _Table = ctx.GetExistingOrMakeTableByName(Table);
            }

            public override void Execute(EvalContext ctx)
            {
              var val = _Table.GetVariable(SelectMany, ctx, Var, IgnoreThisSession);

              if (AbortIfNotFound && val==null)
                ctx.Abort( m_Path );
              else
                ctx.AddData( IntoName, val );
            }
          }




  }

}
