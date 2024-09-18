/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data.Business;
using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Sky.Messaging.Services
{
  /// <summary>
  /// Represents a command object embedded in a <see cref="MessageEnvelope"/> message sent to the server for pre-processing,
  /// such as get CMS template and fetch content into a document which should be rendered and attached as a PDF file (as an example)
  /// </summary>
  [Bix("a71f0325-bad3-4eb4-bc1a-ab84cfa57a62")]
  [Schema(Description = "Represents a command object embedded in a MessageEnvelope message sent to the server for pre-processing. " +
                        "Command processing yields another MessageEnvelope populated with actual message content")]
  public sealed class MessageCommand : FragmentModel
  {
    /// <summary>
    /// Maximum length of messaging command name is 512 characters
    /// </summary>
    public const int MAX_NAME_LEN = 512;

    /// <summary>
    /// Command name which is used for server handler matching
    /// </summary>
    [Field(required: true, maxLength: MAX_NAME_LEN, description: "Command name which is used for server handler matching")]
    public string Name { get; set; }

    /// <summary>
    /// Optional parameter collection to pass to command. Some commands do not require any parameters
    /// </summary>
    [Field(required: true, description: "Command parameters - arguments for execution by the server")]
    public JsonDataMap Parameters { get; set; }
  }
}
