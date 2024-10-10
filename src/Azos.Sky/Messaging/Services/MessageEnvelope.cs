/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using Azos.Data.Business;
using Azos.Apps.Injection;
using Azos.Time;
using Azos.Serialization.JSON;
using Azos.Data;
using Azos.Serialization.Bix;

namespace Azos.Sky.Messaging.Services
{
  /// <summary>
  /// Embodies a communication message (such as an email) with additional properties which can be used
  /// for optional message content derivation (e.g. template expansion) and other purposes
  /// </summary>
  [Schema(Description = "Embodies a communication message (such as an email) with additional properties")]
  [Bix("a332f054-7ef3-4d30-933f-b0aae64b2ec1")]
  public sealed class MessageEnvelope : PersistedModel<ChangeResult>
  {
    [Inject] IMessagingLogic m_MessagingLogic;
    [Inject] ITimeSource m_TimeSource;

    //Used for Multipart encoding, must be enabled
    public override bool AmorphousDataEnabled => true;

    /// <summary>
    /// Required message content
    /// </summary>
    [Field(required: true, Description = "Required message content")]
    public Message Content { get; set; }

    /// <summary>
    /// Optional message config properties which can be used for message content derivation (e.g. template expansion) and other purposes
    /// </summary>
    [Field(Description = "Optional message envelope config properties which can be used for message content derivation (e.g. template expansion) and other purposes")]
    public ConfigVector Props { get; set; }


    public override ValidState Validate(ValidState state, string scope = null)
    {
      state = base.Validate(state, scope);
      if (state.ShouldStop) return state;

      state = m_MessagingLogic.CheckPreconditions(this, state);

      return state;
    }

    protected override ValidState DoBeforeValidateOnSave()
    {
      if (Content != null)
      {
        Content.CreateDateUTC = m_TimeSource.UTCNow; //always default time
      }

      return base.DoBeforeValidateOnSave();
    }

    protected override async Task<SaveResult<ChangeResult>> DoSaveAsync()
    {
      var mid = await m_MessagingLogic.SendAsync(this).ConfigureAwait(false);

      var change = new ChangeResult(ChangeResult.ChangeType.Inserted,
                                    affectedCount: 1,
                                    msg: new { sent_id = mid }.ToJson(),
                                    data: mid);

      return new SaveResult<ChangeResult>(change);
    }


    /// <summary>
    /// Custom binder inserts bin[] content supplied at the top-map level as provided using multipart data encoding.
    /// The binder "inserts" the byte[] inside the message attachment graph
    /// </summary>
    public override (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (!(data is JsonDataMap map)) return (false, this);
      Content = JsonReader.ToDoc<Message>(map, fromUI, options);

      for (var i = 0; i < Content.Attachments.Length; i++)
      {
        var attachment = Content.Attachments[i];
        if (attachment.Content != null && attachment.Content.Length > 0) continue;
        var oob = AmorphousData[$"attachment-{i}"] as byte[];//out-of-band content attachment
        if (oob != null) attachment.Content = oob;
      }

      return (true, this);
    }
  }
}
