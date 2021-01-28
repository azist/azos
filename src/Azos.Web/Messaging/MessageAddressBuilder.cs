/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Data;

namespace Azos.Web.Messaging
{
  /// <summary>
  /// Represents events fired on MessageBuilder change
  /// </summary>
  public delegate void MessageBuilderChangeEventHandler(MessageAddressBuilder builder);

  /// <summary>
  /// Facilitates the conversion of config into stream of Addressee entries
  /// </summary>
  public sealed class MessageAddressBuilder
  {
    #region CONSTS
    public const string CONFIG_ROOT_SECT     = "as";
    public const string CONFIG_A_SECT        = "a";
    public const string ATTR_NAME            = "nm";
    public const string ATTR_CHANNEL_NAME    = "cn";
    public const string ATTR_CHANNEL_ADDRESS = "ca";
    #endregion

    /// <summary>
    /// Provides data for an addressee:
    ///   {Name, Channel, Address (per channel)}, example {"Frank Borland", "UrgentSMTP", "frankb@xyz.com"}.
    /// Note: The format of channel address string depends on the channel which this addressee points to
    /// </summary>
    public struct Addressee
    {
      public Addressee(string name, string channelName, string channelAddress)
      {
        Name = name;
        ChannelName = channelName;
        ChannelAddress = channelAddress;
      }

      public readonly string Name;
      public readonly string ChannelName;
      public readonly string ChannelAddress;

      public bool Assigned => this.ChannelAddress.IsNotNullOrWhiteSpace();
    }

    public static string OneAddressee(string name, string channelName, string channelAddress)
    {
      var b = new MessageAddressBuilder(name, channelName, channelAddress);
      return b.ToString();
    }

    public MessageAddressBuilder(string name, string channelName, string channelAddress)
      : this(new Addressee(name, channelName, channelAddress))
    {
    }

    public MessageAddressBuilder(Addressee addressee) : this(null)
    {
      AddAddressee(addressee);
    }

    public MessageAddressBuilder(string config, MessageBuilderChangeEventHandler onChange = null)
    {
      if (config.IsNullOrWhiteSpace())
      {
        var c = new MemoryConfiguration();
        c.Create(CONFIG_ROOT_SECT);
        m_Config = c.Root;
      }
      else
       m_Config = config.AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      if (onChange!=null)
       MessageBuilderChange+=onChange;
    }


    private ConfigSectionNode m_Config;


    /// <summary>
    /// Subscribe to get change notifications
    /// </summary>
    public event MessageBuilderChangeEventHandler MessageBuilderChange;

    /// <summary>
    /// Enumerates all Addressee instances
    /// </summary>
    public IEnumerable<Addressee> All
    {
      get
      {
        return  m_Config.Children
                        .Where(c => c.IsSameName(CONFIG_A_SECT))
                        .Select(c => new Addressee(
                                         c.AttrByName(ATTR_NAME).ValueAsString(),
                                         c.AttrByName(ATTR_CHANNEL_NAME).ValueAsString(),
                                         c.AttrByName(ATTR_CHANNEL_ADDRESS).ValueAsString()
                                       )
                               );
      }
    }

    public override string ToString()
      => m_Config.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact);

    public bool MatchNamedChannel(IEnumerable<string> channelNames)
    {
      if (channelNames == null || !channelNames.Any()) return false;
      var adrChannelNames = All.Select(a => a.ChannelName);
      return adrChannelNames.Any(c => channelNames.Any(n => n.EqualsOrdIgnoreCase(c)));
    }

    public IEnumerable<Addressee> GetMatchesForChannels(IEnumerable<string> channelNames)
    {
      if (channelNames == null || !channelNames.Any()) return Enumerable.Empty<Addressee>();
      return All.Where(a => channelNames.Any(n => n.EqualsOrdIgnoreCase(a.ChannelName)));
    }

    public Addressee GetFirstOrDefaultMatchForChannels(IEnumerable<string> channelNames)
    {
      return GetMatchesForChannels(channelNames).FirstOrDefault();
    }

    public void AddAddressee(string name, string channelName, string channelAddress)
    {
      AddAddressee(new Addressee(name, channelName, channelAddress));
    }

    public void AddAddressee(Addressee addressee)
    {
      var aSection = m_Config.AddChildNode(CONFIG_A_SECT);
      aSection.AddAttributeNode(ATTR_NAME, addressee.Name);
      aSection.AddAttributeNode(ATTR_CHANNEL_NAME, addressee.ChannelName);
      aSection.AddAttributeNode(ATTR_CHANNEL_ADDRESS, addressee.ChannelAddress);

      MessageBuilderChange?.Invoke(this);
    }
  }
}
