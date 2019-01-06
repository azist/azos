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


                      public static SetVarOp SetVar(string table,
                                      string var,
                                      object value,
                                      string description = null,
                                      DateTime? expirationTimeUTC = null,
                                      bool allowDuplicates = false)
                      {
                        return new SetVarOp(table, var, value, description, expirationTimeUTC, allowDuplicates);
                      }

                      /// <summary>
                      /// Sets the named variable in table, return true if var was set, false if conflict happened
                      /// </summary>
                      [Serializable]
                      public sealed class SetVarOp : ChangeOp
                      {
                        internal SetVarOp(string table,
                                      string var,
                                      object value,
                                      string description,
                                      DateTime? expirationTimeUTC,
                                      bool allowDuplicates) : base(table, var)
                        {
                            Value = value;
                            Description = description;
                            ExpirationTimeUTC = expirationTimeUTC;
                            AllowDuplicates = allowDuplicates;
                        }

                        public override bool GetValue(EvalContext ctx)
                        {
                          return _Table.SetVariable(ctx, Var, Value, Description, ExpirationTimeUTC, AllowDuplicates);
                        }

                         public readonly object Value;
                         public readonly string Description;
                         public readonly DateTime? ExpirationTimeUTC;
                         public readonly bool AllowDuplicates;

                      }


                      public static DeleteVarOp DeleteVar(string table, string var, object value = null)
                      {
                        return new DeleteVarOp(table, var, value);
                      }
                      /// <summary>
                      /// Deletes the named variable in table that this session has created, true if it was deleted, false if it did not exist
                      /// or was not created by this session
                      /// </summary>
                      [Serializable]
                      public sealed class DeleteVarOp : ChangeOp
                      {
                        internal DeleteVarOp(string table, string var, object value) : base(table, var)
                        {
                           Value = value;
                        }

                        public readonly object Value;

                        public override bool GetValue(EvalContext ctx)
                        {
                          return _Table.DeleteVariable(ctx, Var, Value);
                        }
                      }



  }

}
