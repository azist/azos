/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Conf
{
  /// <summary>
  /// Designates entities that may be configured
  /// </summary>
  public interface IConfigurable
  {
    /// <summary>
    /// Configures an entity from supplied config node
    /// </summary>
    void Configure(IConfigSectionNode node);
  }


  /// <summary>
  /// Designates entities that may persist their parameters/state into configuration
  /// </summary>
  public interface IConfigurationPersistent
  {
    /// <summary>
    /// Persists relevant entities parameters/state into the specified configuration node
    /// </summary>
    ConfigSectionNode PersistConfiguration(ConfigSectionNode parentNode, string name);
  }


  /// <summary>
  /// Represents an entity that provides config node. It is primarily used for includes
  /// </summary>
  public interface IConfigNodeProvider : IConfigurable
  {
    /// <summary>
    /// Returns config node per optional context
    /// </summary>
    ConfigSectionNode ProvideConfigNode(object context = null);
  }


  /// <summary>
  /// Provides read-only configuration node abstraction for section and attribute nodes
  /// </summary>
  public interface IConfigNode : Collections.INamed
  {
    /// <summary>
    /// References configuration this node is under
    /// </summary>
    Configuration Configuration { get; }

    /// <summary>
    /// Determines whether this node really exists in configuration or is just a sentinel empty node
    /// </summary>
    bool Exists { get; }

    /// <summary>
    /// Returns verbatim (without variable evaluation) node value or null
    /// </summary>
    string VerbatimValue { get; }

    /// <summary>
    /// Returns null or value of this node with all variables evaluated
    /// </summary>
    string EvaluatedValue { get; }

    /// <summary>
    /// Returns null or value of this node with all variables evaluated
    /// </summary>
    string Value { get; }

    /// <summary>
    /// References parent node or empty node if this is the top-most node with no parent
    /// </summary>
    IConfigSectionNode Parent { get; }

    /// <summary>
    /// Returns a path from root to this node
    /// </summary>
    string RootPath { get; }

    /// <summary>
    /// Returns node value as string performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    string ValueAsString(string dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as byte[] performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    byte[] ValueAsByteArray(byte[] dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as int[] performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    int[] ValueAsIntArray(int[] dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as long[] performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    long[] ValueAsLongArray(long[] dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as float[] performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    float[] ValueAsFloatArray(float[] dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as double[] performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    double[] ValueAsDoubleArray(double[] dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as decimal[] performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    decimal[] ValueAsDecimalArray(decimal[] dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as short performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    short ValueAsShort(short dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as short? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    short? ValueAsNullableShort(short? dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as ushort performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    ushort ValueAsUShort(ushort dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as ushort? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    ushort? ValueAsNullableUShort(ushort? dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as byte performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    byte ValueAsByte(byte dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as byte? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    byte? ValueAsNullableByte(byte? dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as sbyte performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    sbyte ValueAsSByte(sbyte dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as sbyte? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    sbyte? ValueAsNullableSByte(sbyte? dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as int performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    int ValueAsInt(int dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as int? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    int? ValueAsNullableInt(int? dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as uint performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    uint ValueAsUInt(uint dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as uint? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    uint? ValueAsNullableUInt(uint? dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as long performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    long ValueAsLong(long dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as long? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    long? ValueAsNullableLong(long? dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as ulong performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    ulong ValueAsULong(ulong dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as ulong? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    ulong? ValueAsNullableULong(ulong? dflt = 0, bool verbatim = false);

    /// <summary>
    /// Returns node value as double performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    double ValueAsDouble(double dflt = 0d, bool verbatim = false);

    /// <summary>
    /// Returns node value as double? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    double? ValueAsNullableDouble(double? dflt = 0d, bool verbatim = false);

    /// <summary>
    /// Returns node value as float performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    float ValueAsFloat(float dflt = 0f, bool verbatim = false);

    /// <summary>
    /// Returns node value as float? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    float? ValueAsNullableFloat(float? dflt = 0f, bool verbatim = false);

    /// <summary>
    /// Returns node value as decimal performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    decimal ValueAsDecimal(decimal dflt = 0m, bool verbatim = false);

    /// <summary>
    /// Returns node value as decimal? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    decimal? ValueAsNullableDecimal(decimal? dflt = 0m, bool verbatim = false);

    /// <summary>
    /// Returns node value as bool performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    bool ValueAsBool(bool dflt = false, bool verbatim = false);

    /// <summary>
    /// Returns node value as bool? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    bool? ValueAsNullableBool(bool? dflt = false, bool verbatim = false);

    /// <summary>
    /// Returns node value as Guid performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    Guid ValueAsGUID(Guid dflt, bool verbatim = false);

    /// <summary>
    /// Returns node value as Guid? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    Guid? ValueAsNullableGUID(Guid? dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as GDID performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    GDID ValueAsGDID(GDID dflt, bool verbatim = false);

    /// <summary>
    /// Returns node value as GDID? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    GDID? ValueAsNullableGDID(GDID? dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as RGDID performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    RGDID ValueAsRGDID(RGDID dflt, bool verbatim = false);

    /// <summary>
    /// Returns node value as RGDID? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    RGDID? ValueAsNullableRGDID(RGDID? dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as DateTime performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    DateTime ValueAsDateTime(DateTime dflt, bool verbatim = false, System.Globalization.DateTimeStyles styles = CoreConsts.UTC_TIMESTAMP_STYLES);

    /// <summary>
    /// Returns node value as DateTime? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    DateTime? ValueAsNullableDateTime(DateTime? dflt = null, bool verbatim = false, System.Globalization.DateTimeStyles styles = CoreConsts.UTC_TIMESTAMP_STYLES);

    /// <summary>
    /// Returns node value as TimeSpan performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    TimeSpan ValueAsTimeSpan(TimeSpan dflt, bool verbatim = false);

    /// <summary>
    /// Returns node value as TimeSpan? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    TimeSpan? ValueAsNullableTimeSpan(TimeSpan? dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as TEnum performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    TEnum ValueAsEnum<TEnum>(TEnum dflt = default(TEnum), bool verbatim = false) where TEnum : struct;

    /// <summary>
    /// Returns node value as TEnum? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    TEnum? ValueAsNullableEnum<TEnum>(TEnum? dflt = null, bool verbatim = false) where TEnum : struct;

    /// <summary>
    /// Returns node value as Atom performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    Atom ValueAsAtom(Atom dflt, bool verbatim = false);

    /// <summary>
    /// Returns node value as Atom? performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>>
    Atom? ValueAsNullableAtom(Atom? dflt = null, bool verbatim = false);

    /// <summary>
    /// Returns node value as Uri performing conversion if necessary.
    /// The optional default is applied when conversion can not be made.
    /// The method evaluates variables embedded in the node literal value unless verbatim parameter
    /// is specified in which case the literal value is returned as-is without any evaluation
    /// </summary>
    Uri ValueAsUri(Uri dflt, bool verbatim = false);

    /// <summary>
    /// Tries to get value as specified type or throws if it can not be converted
    /// </summary>
    object ValueAsType(Type tp, bool verbatim = false, bool strict = true);

    /// <summary>
    /// Returns true when another node has the same name as this one
    /// </summary>
    bool IsSameName(IConfigNode other);

    /// <summary>
    /// Returns true when another name is the same as this node's name
    /// </summary>
    bool IsSameName(string other);
  }


  /// <summary>
  /// Provides read-only configuration section node abstraction
  /// </summary>
  public interface IConfigSectionNode : IConfigNode
  {
    /// <summary>
    /// Added in Feb 26, 2024 DKh:
    /// If set, provides an optional enumerable of type search paths which system may use
    /// to locate types by partial name, <see cref="FactoryUtils.Make{T}(IConfigSectionNode, Type, object[])"/> family of methods.
    /// By default, null which means no additional type paths
    /// </summary>
    IEnumerable<string> TypeSearchPaths { get; }

    /// <summary>
    /// Indicates whether this node has any child section nodes
    /// </summary>
    bool HasChildren { get; }

    /// <summary>
    /// Returns number of child section nodes
    /// </summary>
    int ChildCount { get; }

    /// <summary>
    /// Indicates whether this node has any associated attributes
    /// </summary>
    bool HasAttributes { get; }

    /// <summary>
    /// Returns number of child attribute nodes
    /// </summary>
    int AttrCount { get; }

    /// <summary>
    /// Enumerates all child nodes
    /// </summary>
    IEnumerable<IConfigSectionNode> Children { get; }

    /// <summary>
    /// Enumerates all attribute nodes
    /// </summary>
    IEnumerable<IConfigAttrNode> Attributes { get; }

    /// <summary>
    /// Retrieves section node by names, from left to right until existing node is found.
    /// If no existing node could be found then empty node instance is returned
    /// </summary>
    IConfigSectionNode this[params string[] names] { get; }

    /// <summary>
    /// Retrieves section node by index or empty node instance if section node with such index could not be found
    /// </summary>
    IConfigSectionNode this[int idx] { get; }

    /// <summary>
    /// Returns attribute node by its name or empty attribute if real attribute with such name does not exist
    /// </summary>
    IConfigAttrNode AttrByName(string name, bool autoCreate = false);

    /// <summary>
    /// Returns attribute node by its index or empty attribute if real attribute with such index does not exist
    /// </summary>
    IConfigAttrNode AttrByIndex(int idx);

    /// <summary>
    /// Navigates the path and return the appropriate node. Example '!/azos/logger/destination/$file-name'
    /// </summary>
    /// <param name="path">If path starts from '!' then exception will be thrown if such a node does not exist;
    ///  Use '/' as leading char for root,
    ///  '..' for step up,
    ///  '$' for attribute name. Multiple paths may be coalesced using '|' or ';'
    /// </param>
    IConfigNode Navigate(string path);

    /// <summary>
    /// Navigates the path and return the appropriate section node. Example '!/azos/logger/destination'
    /// </summary>
    /// <param name="path">If path starts from '!' then exception will be thrown if such a section node does not exist;
    ///  Use '/' as leading char for root,
    ///  '..' for step up. Multiple paths may be coalesced using '|' or ';'
    /// </param>
    IConfigSectionNode NavigateSection(string path);

    /// <summary>
    /// Evaluates a value string expanding all variables with var-paths relative to this node.
    /// Evaluates configuration variables such as "$(varname)" or "$(@varname)". Varnames are paths
    /// to other config nodes from the same configuration or variable names when prefixed with "~". If varname starts with "@" then it gets combined
    ///  with input as path string. "~" is used to qualify environment vars that get resolved through Configuration.EnvironmentVarResolver
    ///  Example: `....add key="Schema.$(/A/B/C/$attr)" value="$(@~HOME)bin\Transforms\"...`
    /// </summary>
    string EvaluateValueVariables(string value, bool recurse = true);

    /// <summary>
    /// Returns true when this and another nodes both have attribute "name" and their values are equal per case-insensitive culture-neutral comparison
    /// </summary>
    bool IsSameNameAttr(IConfigSectionNode other);

    /// <summary>
    /// Returns true when this node has an attribute called "name" and its value is equal to the supplied value per case-insensitive culture-neutral comparison
    /// </summary>
    bool IsSameNameAttr(string other);

    /// <summary>
    /// Serializes configuration tree rooted at this node into Laconic format and returns it as a string
    /// </summary>
    string ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions options = null);

    /// <summary>
    /// Converts this ConfigSectionNode to JSONDataMap. Contrast with ToConfigurationJSONDataMap
    /// Be careful: that this operation can "loose" data from ConfigSectionNode.
    /// In other words some ConfigSectionNode information can not be reflected in corresponding JSONDataMap, for example
    ///  this method overwrites duplicate key names and does not support section values
    /// </summary>
    JsonDataMap ToJSONDataMap();

    /// <summary>
    /// Returns the contents of this node per JSONConfiguration specification. Contrast with ToJSONDataMap
    /// </summary>
    JsonDataMap ToConfigurationJSONDataMap();

    /// <summary>
    /// Serializes configuration tree rooted at this node into JSON configuration format and returns it as a string
    /// </summary>
    string ToJSONString(Azos.Serialization.JSON.JsonWritingOptions options = null);

    /// <summary>
    /// Serializes configuration tree as XML
    /// </summary>
    System.Xml.XmlDocument ToXmlDoc(string xsl = null, string encoding = null);

    /// <summary>
    /// Serializes configuration tree as XML string with optional link to xsl file
    /// </summary>
    string ToXmlString(string xsl = null);

    /// <summary>
    /// Returns attribute values as string map
    /// </summary>
    Collections.StringMap AttrsToStringMap(bool verbatim = false);
  }


  /// <summary>
  /// Represents a read-only attribute of a attribute node
  /// </summary>
  public interface IConfigAttrNode : IConfigNode
  {

  }


  /// <summary>
  /// Represents an entity that provides a type-safe access to configuration settings that come from Configuration nodes.
  /// This class obviates the need for navigation between config nodes on every property get and facilitates faster access to some config parameters
  /// that need to be gotten efficiently, as they are now kept cached in RAM in native format (i.e. DateTime vs. string) as fields.
  /// Usually classes that implement this interface are singleton and they get registered with the application using IApplication.RegisterConfigSettings()
  /// method. Warning: the implementation must be thread-safe and allow property getters to keep reading while ConfigChanged() notification happens
  /// </summary>
  public interface IConfigSettings
  {
    /// <summary>
    /// Notifies the implementer that underlying source configuration has changed and memory-resident
    /// fields need to be re-read from config nodes. Usually this method is called by application container
    ///  when instance of this class has been registered with the application using IApplication.RegisterConfigSettings().
    /// Warning: the implementation must be thread-safe and allow getters to keep reading while notification happens
    /// </summary>
    /// <param name="application">Application which is sending the change</param>
    /// <param name="atNode">
    /// Passes the most top-level node that covers all of the changes that happened in the source config system.
    /// Usually this is a root config node. The capability of source config change detection on node level is not supported by all providers
    /// </param>
    void ConfigChanged(IApplication application, IConfigSectionNode atNode);
  }

}
