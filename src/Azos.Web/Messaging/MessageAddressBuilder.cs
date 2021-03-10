/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Web.Messaging
{

  /// <summary>
  /// Represents events fired on MessageBuilder change
  /// </summary>
  public delegate void MessageBuilderChangeEventHandler(MessageAddressBuilder builder);


  /// <summary>
  /// Facilitates the conversion of config into stream of Addressee entries
  /// </summary>
  public sealed class MessageAddressBuilder : IEnumerable<MessageAddressBuilder.Addressee>
  {
    /// <summary>
    /// Provides data for an addressee:
    ///   {Name, Channel, Address (per channel)}, example {"Frank Borland", "UrgentSMTP", "frankb@xyz.com"}.
    /// Note: The format of channel address string depends on the channel which this addressee points to
    /// </summary>
    public struct Addressee : IJsonWritable, IJsonReadable
    {
      public const string JSON_NAME = "n";
      public const string JSON_CHANNEL = "c";
      public const string JSON_ADDRESS = "a";

      public static Addressee From(JsonDataMap map)
        => new Addressee(map[JSON_NAME].AsString(), map[JSON_CHANNEL].AsString(), map[JSON_ADDRESS].AsString());

      public Addressee(string name, string channel, string address)
      {
        Name = name;
        Channel = channel;
        Address = address;
      }


      public readonly string Name;
      public readonly string Channel;
      public readonly string Address;

      public bool Assigned => Address.IsNotNullOrWhiteSpace();

      public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
       => JsonWriter.WriteMap(wri, nestingLevel, options,
                                new DictionaryEntry(JSON_NAME, Name),
                                new DictionaryEntry(JSON_CHANNEL, Channel),
                                new DictionaryEntry(JSON_ADDRESS, Address));

      public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
        => data is JsonDataMap map ? (true, Addressee.From(map))
                                   : (false, this);
    }

    public MessageAddressBuilder(string name, string channelName, string channelAddress)
      : this(new Addressee(name, channelName, channelAddress))
    {
    }

    public MessageAddressBuilder(Addressee addressee) : this(null)
    {
      Add(addressee);
    }

    public MessageAddressBuilder(string json, MessageBuilderChangeEventHandler onChange = null)
    {
      if (json.IsNotNullOrWhiteSpace())
      {
        var array = JsonReader.DeserializeDataObject(json, true) as JsonDataArray;
        if (array != null)
        {
          foreach(var elm in array)
          {
            if (elm is JsonDataMap map)
              m_Data.Add(Addressee.From(map));
          }
        }
      }

      if (onChange != null)
       MessageBuilderChange+=onChange;
    }



    private List<Addressee> m_Data = new List<Addressee>(4);


    public IEnumerable<Addressee> All => m_Data;

    /// <summary>
    /// Subscribe to get change notifications
    /// </summary>
    public event MessageBuilderChangeEventHandler MessageBuilderChange;


    public override string ToString() => JsonWriter.Write(this, JsonWritingOptions.Compact);

    public bool MatchNamedChannel(IEnumerable<string> channelNames)
    {
      if (channelNames == null || !channelNames.Any()) return false;
      var adrChannelNames = m_Data.Select(a => a.Channel);
      return adrChannelNames.Any(c => channelNames.Any(n => n.EqualsOrdIgnoreCase(c)));
    }

    public IEnumerable<Addressee> GetMatchesForChannels(IEnumerable<string> channelNames)
    {
      if (channelNames == null || !channelNames.Any()) return Enumerable.Empty<Addressee>();
      return m_Data.Where(a => channelNames.Any(n => n.EqualsOrdIgnoreCase(a.Channel)));
    }

    public Addressee GetFirstOrDefaultMatchForChannels(IEnumerable<string> channelNames)
    {
      return GetMatchesForChannels(channelNames).FirstOrDefault();
    }

    public void Add(string name, string channelName, string channelAddress)
      => Add(new Addressee(name, channelName, channelAddress));

    public void Add(Addressee addressee)
    {
      m_Data.Add(addressee);
      MessageBuilderChange?.Invoke(this);
    }

    public void AddMany(IEnumerable<Addressee> many)
    {
      if (many==null) return;

      foreach(var one in many)
        m_Data.Add(one);

      MessageBuilderChange?.Invoke(this);
    }

    public IEnumerator<Addressee> GetEnumerator() => m_Data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_Data.GetEnumerator();
  }
}
