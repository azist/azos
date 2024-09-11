/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;


using Azos.Apps;
using Azos.Collections;
using Azos.Conf;

namespace Azos.Time
{
  /// <summary>
  /// Describes an entity that provides the localized time
  /// </summary>
  public interface ILocalizedTimeProvider
  {
      /// <summary>
      /// Returns the location
      /// </summary>
      TimeLocation TimeLocation { get; }


      /// <summary>
      /// Returns current time localized per TimeLocation
      /// </summary>
      DateTime LocalizedTime { get; }

      /// <summary>
      /// Converts universal time to local time as of TimeLocation property
      /// </summary>
      DateTime UniversalTimeToLocalizedTime(DateTime utc);

      /// <summary>
      /// Converts localized time to UTC time as of TimeLocation property
      /// </summary>
      DateTime LocalizedTimeToUniversalTime(DateTime local);
  }


  /// <summary>
  /// Defines time source - an entity that supplies local and UTC times. A concrete implementation
  ///  may elect to get accurate times from the network or other external precision time sources (i.e. NASA atomic clock).
  /// The split-second time accuracy is necessary in some distributed algorithms such as the ones that use precision time stamps
  /// to established the relative order of distributed events during data reconciliation (e.g. multi-master replication)
  /// </summary>
  public interface ITimeSource : IApplicationComponent, ILocalizedTimeProvider
  {
      /// <summary>
      /// Returns local time stamp, Alias to this.LocalizedTime
      /// </summary>
      DateTime Now { get; }

      /// <summary>
      /// Returns current UTC time stamp
      /// </summary>
      DateTime UTCNow { get; }
  }

  /// <summary>
  /// Denotes an implementation for an app-global time source - an entity that supplies time in this application instance
  /// </summary>
  public interface ITimeSourceImplementation : ITimeSource, IDisposable, IConfigurable
  {

  }


  /// <summary>
  /// Denotes a contract for an app-global event timer - an entity that fires requested events
  /// </summary>
  public interface IEventTimer : IApplicationComponent, Azos.Instrumentation.IInstrumentable
  {
      /// <summary>
      /// Gets the granularity of event firing resolution
      /// </summary>
      int ResolutionMs { get; }

      /// <summary>
      /// Lists all events in the instance
      /// </summary>
      IRegistry<Event> Events { get;}
  }


  /// <summary>
  /// Denotes an implementation for an app-global event timer - an entity that fires requested events
  /// </summary>
  public interface IEventTimerImplementation : IEventTimer, IDisposable, IConfigurable
  {
      new int ResolutionMs { get; set; }

      /// <summary>
      /// Internal call, developers - do not call
      /// </summary>
      void __InternalRegisterEvent(Event evt);

      /// <summary>
      /// Internal call, developers - do not call
      /// </summary>
      void __InternalUnRegisterEvent(Event evt);
  }
}
