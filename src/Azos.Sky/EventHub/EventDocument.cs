/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Abstract base declaration of event document
  /// </summary>
  public abstract class EventDocument : AmorphousTypedDoc
  {
    /// <summary>
    /// EventDocuments enable amorphous data by default
    /// </summary>
    public override bool AmorphousDataEnabled => true;

    /// <summary>
    /// Specifies parameters how this event should be routed for processing
    /// </summary>
    public abstract Route ProcessingRoute {  get; }
  }
}
