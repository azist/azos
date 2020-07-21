/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;


using Azos.Apps;
using Azos.Time;
using Azos.Instrumentation;
using Azos.Serialization.BSON;
using Azos.Data;

namespace Azos.Log
{
  /// <summary>
  /// Describes an entity that accepts log messages
  /// </summary>
  public interface ILog : IApplicationComponent, ILocalizedTimeProvider
  {
      void Write(Message msg);
      void Write(Message msg, bool urgent);
      void Write(MessageType type, string text, string topic = null, string from = null);
      void Write(MessageType type, string text, bool urgent, string topic = null, string from = null);

      Message LastWarning     { get; }
      Message LastError       { get; }
      Message LastCatastrophe { get; }
  }

  /// <summary>
  /// Describes entity capable of being written log information to
  /// </summary>
  public interface ILogImplementation : ILog, IDisposable, Conf.IConfigurable, IInstrumentable
  {
  }

  /// <summary>
  /// Marker interface for entities that can be stored in archives, such as access/telemetry logs
  /// </summary>
  public interface IArchiveLoggable : IDataDoc
  {
    /// <summary>
    /// Gets archive dimension content for later retrieval of entity by key, e.g. a user ID may be
    /// used to retrieve log messages related to a user.
    /// In most cases CSV, JSON or Laconic content is stored, the format depends on a concrete system
    /// which uses this content as an opaque field
    /// </summary>
    string ArchiveDimensions { get; }
  }



}
