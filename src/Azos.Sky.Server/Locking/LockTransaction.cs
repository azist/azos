/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;

namespace Azos.Sky.Locking
{
  /// <summary>
  /// Represents a transaction that gets atomically applied to the remote locking server
  /// </summary>
  [Serializable]
  public sealed class LockTransaction : IEnumerable<LockOp.StatementOp>
  {
    /// <summary>
    /// Represents transaction that pings any server regardless of reliability guarantees
    /// </summary>
    public static readonly LockTransaction PingAnyReliability = new LockTransaction(0, 0.0d);


    /// <summary>
    /// Creates a new transaction object for execution on a remote lock server
    /// </summary>
    public LockTransaction(
                            string description,
                            string namespaceName,
                            uint minRuntimeSec,
                            double minTrustLevel,
                            params LockOp.StatementOp[] statements
                          )
    {
      if (description.IsNullOrWhiteSpace())
       throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(description=null|empty)");

      if (namespaceName.IsNullOrWhiteSpace())
       throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(namespaceName=null|empty)");

      if (minTrustLevel < 0d || minTrustLevel > 1.0d)
       throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(minTrustLevel must be 0d..1d)");

      if (statements==null || statements.Length<1)
       throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(statements=null|empty)");

      ID = Guid.NewGuid();
      Description = description;
      Namespace = namespaceName;
      MinimumRequiredRuntimeSec = minRuntimeSec;
      MinimumRequiredTrustLevel = minTrustLevel;
      Statements = statements;
    }


    /// <summary>
    /// Creates PING transaction - a transaction that just touches the session and does nothing else
    /// </summary>
    public LockTransaction(
                            uint minRuntimeSec,
                            double minTrustLevel
                          )
    {
      if (minTrustLevel < 0d || minTrustLevel > 1.0d)
       throw new LockingException(ServerStringConsts.ARGUMENT_ERROR+GetType().FullName+".ctor(minTrustLevel must be 0d..1d)");

      ID = Guid.NewGuid();
      MinimumRequiredRuntimeSec = minRuntimeSec;
      MinimumRequiredTrustLevel = minTrustLevel;
    }

    /// <summary>
    /// Unique ID assigned to this transaction at create time
    /// </summary>
    public readonly Guid ID;

    /// <summary>
    /// Provides Textual description for transaction
    /// </summary>
    public readonly string Description;

    /// <summary>
    /// Name of data/memory partition in lock server where all transactions get serialized, hence
    ///  where all changes are guaranteed to be atomic and coherent
    /// </summary>
    public readonly string Namespace;


    /// <summary>
    /// Specifies te minimum time that the server had to run for for transaction to succeed
    /// </summary>
    public readonly uint MinimumRequiredRuntimeSec;

    /// <summary>
    /// Specifies 0..1 minimum trust that the server has to have for transaction to succeed.
    /// If the server's level of trust is below this number, then transaction fails
    /// </summary>
    public readonly double MinimumRequiredTrustLevel;

    /// <summary>
    /// The list of statemnts that get executed in order
    /// </summary>
    public readonly LockOp.StatementOp[] Statements;



    public IEnumerator<LockOp.StatementOp> GetEnumerator()
    {
      return ((IEnumerable<LockOp.StatementOp>)Statements).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return Statements.GetEnumerator();
    }

  }

}
