/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Serialization.Bix;
using Azos.Sky.EventHub;

namespace Azos.AuthKit.Events
{
  /// <summary>
  /// Triggered by login activity such as an attempt to perform user log-on, bad login attempt,
  /// password reset etc...
  /// </summary>
  [Bix("ada5df48-c892-4043-9d7c-3990fed8b548")]
  //[Event(CorporateConsts.EVT_NS_CORPORATE, CorporateConsts.EVT_QUEUE_ALL, DataLossMode.Default)]
  public sealed class LoginEvent : EventDocument
  {
    public override ShardKey GetEventPartition() => throw new NotImplementedException();


  }
}
