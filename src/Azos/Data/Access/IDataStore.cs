/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Data.Access
{
  /// <summary>
  /// Represents a store that can save and retrieve data
  /// </summary>
  public interface IDataStore : IApplicationComponent
  {
    /// <summary>
    /// Returns the name of the underlying store technology, example: "ORACLE", "MongoDB" etc.
    /// This property is used by some metadata-based validation logic which is target-dependent
    /// </summary>
    string TargetName { get; }

    /// <summary>
    /// Tests connectivity/operation and throws an exception if connection could not be established
    /// </summary>
    void TestConnection();

    /// <summary>
    /// Provides default timeout imposed on execution of commands/calls. Expressed in milliseconds.
    /// A value less or equal to zero indicates no timeout
    /// </summary>
    int DefaultTimeoutMs { get; }
  }


  /// <summary>
  /// Represents a store that can save and retrieve data
  /// </summary>
  public interface IDataStoreImplementation : IDataStore, IDisposable, IConfigurable, IInstrumentable
  {
    /// <summary>
    /// Defines log level for data stores
    /// </summary>
    StoreLogLevel DataLogLevel { get; set; }

    /// <summary>
    /// Provides default timeout imposed on execution of commands/calls. Expressed in milliseconds.
    /// A value less or equal to zero indicates no timeout
    /// </summary>
    new int DefaultTimeoutMs { get; set; }
  }
}
