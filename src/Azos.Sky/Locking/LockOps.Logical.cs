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


                    public static AndOp And(OperatorOp lop, OperatorOp rop) { return new AndOp(lop, rop);}
                    [Serializable]
                    public sealed class AndOp : BinaryOperatorOp
                    {
                      internal AndOp(OperatorOp lop, OperatorOp rop): base(lop, rop){}

                      public override bool GetValue(EvalContext ctx)
                      { //common code not refactored to parent class for speed (less virt calls)
                        var lv = LeftOperand.GetValue(ctx);
                        if (ctx.Aborted) return false;
                        var rv = RightOperand.GetValue(ctx);
                        if (ctx.Aborted) return false;
                        return lv && rv;
                      }
                    }

                    public static OrOp Or(OperatorOp lop, OperatorOp rop) { return new OrOp(lop, rop);}
                    [Serializable]
                    public sealed class OrOp  : BinaryOperatorOp
                    {
                      internal OrOp(OperatorOp lop, OperatorOp rop) : base(lop, rop){}

                      public override bool GetValue(EvalContext ctx)
                      { //common code not refactored to parent class for speed (less virt calls)
                        var lv = LeftOperand.GetValue(ctx);
                        if (ctx.Aborted) return false;
                        var rv = RightOperand.GetValue(ctx);
                        if (ctx.Aborted) return false;
                        return lv || rv;
                      }
                    }

                    public static XorOp Xor(OperatorOp lop, OperatorOp rop) { return new XorOp(lop, rop);}
                    [Serializable]
                    public sealed class XorOp  : BinaryOperatorOp
                    {
                      internal XorOp(OperatorOp lop, OperatorOp rop) : base(lop, rop){}

                      public override bool GetValue(EvalContext ctx)
                      { //common code not refactored to parent class for speed (less virt calls)
                        var lv = LeftOperand.GetValue(ctx);
                        if (ctx.Aborted) return false;
                        var rv = RightOperand.GetValue(ctx);
                        if (ctx.Aborted) return false;
                        return lv ^ rv;
                      }
                    }

                    public static NotOp Not(OperatorOp operand) { return new NotOp(operand);}
                    /// <summary>
                    /// Reverses the boolean value of the operand
                    /// </summary>
                    public sealed class NotOp : UnaryOperatorOp
                    {
                      internal NotOp(OperatorOp operand)
                      {
                        if (operand==null)
                          throw new LockingException(StringConsts.ARGUMENT_ERROR+"Not(operand=null)");

                        Operand = operand;
                      }
                      public readonly OperatorOp Operand;

                      public override bool GetValue(EvalContext ctx)
                      {
                        var v = Operand.GetValue(ctx);
                        if (ctx.Aborted) return false;
                        return !v;
                      }

                      public override void Prepare(EvalContext ctx, string path)
                      {
                        base.Prepare(ctx, path);
                        Operand.Prepare(ctx, m_Path);
                      }
                    }


                    public static ExistsOp Exists(string table, string var, object value = null, bool ignoreThisSession=true)
                    {
                      return new ExistsOp(table, var, value, ignoreThisSession);
                    }
                    /// <summary>
                    /// Returns true if the named variable exists.
                    /// </summary>
                    [Serializable]
                    public sealed class ExistsOp : UnaryOperatorOp
                    {
                      internal ExistsOp(string table, string var, object value, bool ignoreThisSession)
                      {
                        if (table.IsNullOrWhiteSpace() || var.IsNullOrWhiteSpace())
                          throw new LockingException(StringConsts.ARGUMENT_ERROR+"Exists(table|var=null|empty)");

                        Table = table;
                        Var = var;
                        IgnoreThisSession = ignoreThisSession;
                      }
                      public readonly string Table; [NonSerialized]internal Server.Table _Table;
                      public readonly string Var;
                      public readonly object Value;
                      public readonly bool IgnoreThisSession;

                      public override void Prepare(EvalContext ctx, string path)
                      {
                        base.Prepare(ctx, path);
                        _Table = ctx.GetExistingOrMakeTableByName(Table);
                      }

                      public override bool GetValue(EvalContext ctx)
                      {
                        return _Table.Exists(ctx, Var, Value, IgnoreThisSession);
                      }
                    }

                    public static TrueOp True{ get{ return new TrueOp();}}
                    /// <summary>
                    /// Dummy operator that always returns true
                    /// </summary>
                    [Serializable]
                    public sealed class TrueOp : UnaryOperatorOp
                    {
                      internal TrueOp() {}
                      public override bool GetValue(EvalContext ctx){ return true;}
                    }

                    public static FalseOp False{ get{ return new FalseOp();}}
                    /// <summary>
                    /// Dummy operator that always returns false
                    /// </summary>
                    [Serializable]
                    public sealed class FalseOp : UnaryOperatorOp
                    {
                      internal FalseOp() {}
                      public override bool GetValue(EvalContext ctx){ return false;}
                    }



  }

}
