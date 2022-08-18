/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data.Business;
using Azos.Apps.Injection;
using Azos.Time;
using Azos.Serialization.JSON;
using Azos.Data;
using System.Threading.Tasks;

namespace Azos.Sky.Messaging.Services
{

  public sealed class MessageEnvelope : PersistedModel<ChangeResult>
  {

    [Inject] IMessagingLogic m_MessagingLogic;
    [Inject] ITimeSource m_TimeSource;

    public override bool AmorphousDataEnabled => true;



    [Field(required: true)] public Message Content { get; set; }

    [Field] public JsonDataMap Props { get; set; }

    public MessageProps GetMessageProps() => Props != null ? new MessageProps(Props) : null;

    public override ValidState Validate(ValidState state, string scope = null)
    {
      state = base.Validate(state, scope);
      if (state.ShouldStop) return state;

      try
      {
        GetMessageProps();
      }
      catch (Exception error)
      {
        state = new ValidState(state, new FieldValidationException(nameof(MessageEnvelope),
                                                                   nameof(Props),
                                                                   "Map contains unacceptable key/value pairs. The keys are to be 8 or less ASCII only chars, and values either string, bool, or ints/longs ", error));
      }
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
      var mid = await m_MessagingLogic.SendAsync(Content, GetMessageProps()).ConfigureAwait(false);

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
