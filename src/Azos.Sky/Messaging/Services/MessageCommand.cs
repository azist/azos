/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data.Business;
using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using System.Threading.Tasks;
using Azos.Collections;
using Azos.Conf;
using Azos.Apps;

namespace Azos.Sky.Messaging.Services
{
  /// <summary>
  /// Represents a command object embedded in a <see cref="MessageEnvelope"/> message sent to the server for pre-processing,
  /// such as get CMS template and fetch content into a document which should be rendered and attached as a PDF file (as an example)
  /// </summary>
  [Bix("a71f0325-bad3-4eb4-bc1a-ab84cfa57a62")]
  [Schema(Description = "Represents a command object embedded in a MessageEnvelope message sent to the server for pre-processing. " +
                        "Command processing yields another MessageEnvelope populated with actual message content")]
  public sealed class MessageCommand  : FragmentModel
  {
    /// <summary>
    /// Minimum length of messaging command name is 5 characters
    /// </summary>
    public const int NAME_MIN_LEN = 5;

    /// <summary>
    /// Maximum length of messaging command name is 512 characters
    /// </summary>
    public const int NAME_MAX_LEN = 512;

    /// <summary>
    /// Command name which is used for server handler matching
    /// </summary>
    [Field(required: true, minLength: NAME_MIN_LEN, maxLength: NAME_MAX_LEN, description: "Command name which is used for server handler matching")]
    public string Name { get; set; }

    /// <summary>
    /// Optional parameter collection to pass to command. Some commands do not require any parameters
    /// </summary>
    [Field(required: true, description: "Command parameters - arguments for execution by the server")]
    public JsonDataMap Parameters { get; set; }
  }


  /// <summary>
  /// Abstraction of entities which handle commands
  /// </summary>
  public abstract class CommandHandler : ApplicationComponent<IMessagingLogic>, INamed
  {
    public const string CONFIG_HANDLER_SECTION = "handler";

    public CommandHandler(IMessagingLogic logic, IConfigSectionNode cfg) : base(logic)
    {
      ConfigAttribute.Apply(this, cfg);
      m_Name.NonBlankMinMax(MessageCommand.NAME_MIN_LEN, MessageCommand.NAME_MAX_LEN, "Configure handler name");
    }

    [Config] private string m_Name;

    public string Name => m_Name;

    /// <summary>
    /// Handles the command by creating a new <see cref="MessageEnvelope"/> instance
    /// which represents the resulkt of command excution, such as a MessageEnvelope
    ///  with its Content property set with a newly create Message with expanded adresses, content and possibly attachements
    /// </summary>
    public abstract Task<MessageEnvelope> HandleCommandAsync(MessageEnvelope originalEnvelope, MessageCommand command);
  }

}
