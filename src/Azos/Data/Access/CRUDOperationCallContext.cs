/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Threading;

namespace Azos.Data.Access
{

  /// <summary>
  /// Establishes an async-safe context which surrounds CRUD operations. You can derive your own classes,
  /// the .ctor must be chained. The context flows between async operations and can be nested. Logical flow
  /// must be observed: a call to .ctor must be balanced with eventual call to .Dispose() (which may be async).
  /// This class is used to pass some out-of-band information to CRUD operations without changing the caller interface,
  /// i.e. to swap database connection string
  /// </summary>
  public class CRUDOperationCallContext : DisposableObject
  {
      private static AsyncLocal<Stack<CRUDOperationCallContext>> ats_Instances = new AsyncLocal<Stack<CRUDOperationCallContext>>();

      /// <summary>
      /// Returns the current set CRUDOperationCallContext or null
      /// </summary>
      public static CRUDOperationCallContext Current
      {
        get
        {
          return ats_Instances.Value!=null && ats_Instances.Value.Count>0 ? ats_Instances.Value.Peek() : null;
        }
      }

      public CRUDOperationCallContext()
      {
        if (ats_Instances.Value==null)
           ats_Instances.Value = new Stack<CRUDOperationCallContext>();

        ats_Instances.Value.Push(this);
      }

      protected override void Destructor()
      {
        if (ats_Instances.Value.Count>0)
        {
          if (ats_Instances.Value.Pop()==this)
          {
            if (ats_Instances.Value.Count == 0)
              ats_Instances.Value = null;

            return;
          }
        }

        throw new DataAccessException(StringConsts.CRUD_OPERATION_CALL_CONTEXT_SCOPE_MISMATCH_ERROR);
      }


      /// <summary>
      /// Used to override store's default database connection string
      /// </summary>
      public string ConnectString{ get; set;}

      /// <summary>
      /// Used to override store's default database name - used by some stores, others take db name form connect string
      /// </summary>
      public string DatabaseName{ get; set;}
  }

}
