/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data.Business;

namespace Azos.Web.Messaging.Services
{
  /// <summary>
  /// Defines contract for sending web messages
  /// </summary>
  public interface IMessagingLogic : IBusinessLogic
  {
    /// <summary>
    /// Sends one message async optionally attaching the ad hoc properties.
    /// Returns a unique message Id which can be used to query the message (if system supports it)
    /// later via IMessageQueryLogic contract, or NULL if the message storage is not supported
    /// </summary>
    /// <remarks>
    /// The returned message id is the one assigned by doc storage service and it may or may not be the same as msg.ID.
    /// NULL is returned if the message is NOT saved anywhere
    /// </remarks>
    Task<string> SendOneAsync(Message one, MessageProps props = null);


    Task<string[]> SendManyAsync((Message msg, MessageProps props)[] many);
  }

  /// <summary>
  /// Provides functionality for message stores which provide message retrieval services
  /// </summary>
  public interface IMessageArchiveLogic : IBusinessLogic
  {
    //get list
    //get message by id / get props
    //get attachments (heavy)
  }

}
