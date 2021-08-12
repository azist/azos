/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Collections;

namespace Azos.Data.Access
{
  /// <summary>
  /// Provides higher-order database context hosting services.
  /// This entity unifies data access to various named contexts
  /// </summary>
  public interface IDataContextHub : IDataStore
  {
    /// <summary> Registry of data contexts </summary>
    IRegistry<IDataContext> Contexts { get; }
  }


  public interface IDataContextHubImplementation : IDataContextHub, IDataStoreImplementation { }

  /// <summary>
  /// A general purpose data context is a named higher-order IDataStore-based concept
  /// </summary>
  public interface IDataContext : IDataStore, INamed
  {
  }

  public interface IDataContextImplementation : IDataContext, IDataStoreImplementation { }

  /// <summary>
  /// Data context capable of processing Crud operations such as sending queries
  /// </summary>
  public interface ICrudDataContext : IDataContext, ICrudDataStore
  {
  }

  public interface ICrudDataContextImplementation : ICrudDataContext, IDataContextImplementation { }
}
