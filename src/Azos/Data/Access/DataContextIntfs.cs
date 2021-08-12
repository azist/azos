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
    IRegistry<IDataStore> Contexts { get; }
  }


  public interface IDataContextHubImplementation : IDataContextHub, IDataStoreImplementation { }
}
