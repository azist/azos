/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Provides access to volume metadata config content
  /// </summary>
  public struct VolumeMetadata
  {
    public const string CONFIG_VOLUME_SECTION = "volume";
    public const string CONFIG_SYS_SECTION = "system";
    public const string CONFIG_VERSION_MAJOR_ATTR = "ver-major";
    public const string CONFIG_VERSION_MINOR_ATTR = "ver-minor";
    public const string CONFIG_ID_ATTR = "id";
    public const string CONFIG_LABEL_ATTR = "label";
    public const string CONFIG_DESCRIPTION_ATTR = "description";
    public const string CONFIG_CHANNEL_ATTR = "channel";
    public const string CONFIG_SCHEME_ATTR = "scheme";
    public const string CONFIG_ENCRYPTION_SECTION = "encryption";
    public const string CONFIG_COMPRESSION_SECTION = "compression";


    internal VolumeMetadata(ConfigSectionNode data)
    {
      Data = data.NonEmpty(nameof(data));
    }

    public readonly IConfigSectionNode Data;


    public IConfigSectionNode SectionSystem => Data[CONFIG_SYS_SECTION];
    public IConfigSectionNode SectionEncryption => SectionSystem[CONFIG_ENCRYPTION_SECTION];
    public IConfigSectionNode SectionCompression => SectionSystem[CONFIG_COMPRESSION_SECTION];


    public int VersionMajor => SectionSystem.Of(CONFIG_VERSION_MAJOR_ATTR).ValueAsInt();
    public int VersionMinor => SectionSystem.Of(CONFIG_VERSION_MINOR_ATTR).ValueAsInt();
    public Guid Id => SectionSystem.Of(CONFIG_ID_ATTR).ValueAsGUID(Guid.Empty);
    public string Label => SectionSystem.Of(CONFIG_LABEL_ATTR).Value;
    public string Description => SectionSystem.Of(CONFIG_DESCRIPTION_ATTR).Value;
    public Atom Channel => SectionSystem.Of(CONFIG_CHANNEL_ATTR).ValueAsAtom(Atom.ZERO);


    public bool IsEncrypted => SectionEncryption.Of(CONFIG_SCHEME_ATTR).Exists;
    public bool IsCompressed => SectionCompression.Of(CONFIG_SCHEME_ATTR).Exists;
  }

  /// <summary>
  /// Builds VolumeMetadata instances
  /// </summary>
  public struct VolumeMetadataBuilder
  {
    public VolumeMetadataBuilder(string rootName = null)
    {
      Root = Configuration.NewEmptyRoot(rootName.Default(VolumeMetadata.CONFIG_VOLUME_SECTION));
      SectionSystem = Root.AddChildNode(VolumeMetadata.CONFIG_SYS_SECTION);
      SectionSystem.AddAttributeNode(VolumeMetadata.CONFIG_ID_ATTR, Guid.NewGuid());
    }

    public readonly ConfigSectionNode Root;
    private readonly ConfigSectionNode  SectionSystem;

    public VolumeMetadata Built => new VolumeMetadata(Root);

    public bool Assigned => Root != null;

    public VolumeMetadataBuilder SetVersion(int major, int minor)
    {
      SectionSystem.AttrByName(VolumeMetadata.CONFIG_VERSION_MAJOR_ATTR, true).Value = major.ToString();
      SectionSystem.AttrByName(VolumeMetadata.CONFIG_VERSION_MINOR_ATTR, true).Value = minor.ToString();
      return this;
    }

    public VolumeMetadataBuilder SetChannel(Atom channel)
    {
      if (!channel.IsZero)
        SectionSystem.AttrByName(VolumeMetadata.CONFIG_CHANNEL_ATTR, true).Value = channel.Value;
      return this;
    }

    public VolumeMetadataBuilder SetLabel(string label)
    {
      if (label.IsNotNullOrWhiteSpace())
        SectionSystem.AttrByName(VolumeMetadata.CONFIG_LABEL_ATTR, true).Value = label;
      return this;
    }

    public VolumeMetadataBuilder SetDescription(string description)
    {
      if (description.IsNotNullOrWhiteSpace())
        SectionSystem.AttrByName(VolumeMetadata.CONFIG_DESCRIPTION_ATTR, true).Value = description;
      return this;
    }

    public VolumeMetadataBuilder SetCompression(Action<ConfigSectionNode> compressionBuilder)
    {
      if (compressionBuilder==null) return this;
      var compression = Root.AddChildNode(VolumeMetadata.CONFIG_COMPRESSION_SECTION);
      compressionBuilder(compression);
      return this;
    }

    public VolumeMetadataBuilder SetEncryption(Action<ConfigSectionNode> encryptionBuilder)
    {
      if (encryptionBuilder == null) return this;
      var encryption = Root.AddChildNode(VolumeMetadata.CONFIG_ENCRYPTION_SECTION);
      encryptionBuilder(encryption);
      return this;
    }

    public VolumeMetadataBuilder SetCompressionScheme(string scheme)
    {
      if (scheme.IsNullOrWhiteSpace()) return this;
      return SetCompression(node => node.AddAttributeNode(VolumeMetadata.CONFIG_SCHEME_ATTR, scheme));
    }

    public VolumeMetadataBuilder SetEncryptionScheme(string scheme)
    {
      if (scheme.IsNullOrWhiteSpace()) return this;
      return SetEncryption(node => node.AddAttributeNode(VolumeMetadata.CONFIG_SCHEME_ATTR, scheme));
    }

  }

}
