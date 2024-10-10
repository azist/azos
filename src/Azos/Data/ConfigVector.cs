/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Represents a piece of configuration content along with configuration object model
  /// which is lazily parsed upon first access then cached. This instance is NOT thread safe
  /// and designed for use in data document fields
  /// </summary>
  [Serializable]
  public sealed class ConfigVector : IJsonWritable, IJsonReadable, IRequiredCheck, ILengthCheck, IValidatable
  {
    public ConfigVector(){ }
    public ConfigVector(string content) => Content = content;
    public ConfigVector(IConfigSectionNode node) => Node = node;

    private string m_Content;

    [NonSerialized]
    private IConfigSectionNode m_Node;

    /// <summary>
    /// Json configuration content which gets stored by this field
    /// </summary>
    public string Content
    {
      get => m_Content;
      set
      {
        if (m_Content.EqualsOrdSenseCase(value)) return;
        m_Content = value;
        m_Node = null;//reset the cached value
      }
    }

    /// <summary>
    /// Returns parsed content. If textual content is null then returns null.
    /// Throws <see cref="ConfigException"/>  If textual content is un-parsable
    /// </summary>
    public IConfigSectionNode Node
    {
      get
      {
        if (m_Node == null)
        {
          Exception jsonError = null;
          try
          {
            m_Node = m_Content.AsJSONConfig(wrapRootName: "r", handling: ConvertErrorHandling.Throw);
          }
          catch(Exception error)
          {
            jsonError = error;
          }

          if (m_Node == null)
          {
            try
            {
              m_Node = m_Content.AsLaconicConfig(wrapRootName: "r", handling: ConvertErrorHandling.Throw);
            }
            catch (Exception error)
            {
              var both = new ValidationBatchException(jsonError);
              both.Batch.Add(error);
              throw new ConfigException("Invalid {0} content: {1}".Args(nameof(ConfigVector), error.ToMessageWithType()), both);
            }
          }

        }

        return m_Node;
      }
      set
      {
        if (object.ReferenceEquals(m_Node, value)) return;

        m_Node = value;

        if (value != null && value.Exists)
          m_Content = value.ToJSONString(JsonWritingOptions.Compact);
        else
          m_Content = null;
      }
    }

    (bool match, IJsonReadable self) IJsonReadable.ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is string s)
      {
        m_Content = s;
        return (true, this);
      }

      if (data is JsonDataMap map)
      {
        m_Content = map.ToJson(JsonWritingOptions.Compact);
        return (true, this);
      }

      return (false, this);
    }

    void IJsonWritable.WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options)
     => JsonWriter.EncodeString(wri, m_Content, options);

    public bool CheckRequired(string targetName) => m_Content.IsNotNullOrWhiteSpace();
    public bool CheckMinLength(string targetName, int minLength) => m_Content != null && m_Content.Length >= minLength;
    public bool CheckMaxLength(string targetName, int maxLength) => m_Content != null && m_Content.Length <= maxLength;

    public override string ToString() => m_Content;

    public ValidState Validate(ValidState state, string scope = null)
    {
      if (Content.IsNotNullOrWhiteSpace())
      {
        try
        {
          var node = Node;
        }
        catch(Exception error)
        {
          state = new ValidState(state, new FieldValidationException(nameof(ConfigVector), scope.Default("<cfg>"), "Invalid value", error));
        }
      }
      return state;
    }

    public static implicit operator ConfigVector(string v) => new ConfigVector(v);
    public static implicit operator ConfigVector(ConfigSectionNode v) => new ConfigVector(v);

    public static implicit operator string(ConfigVector v) => v?.Content;
    public static implicit operator ConfigSectionNode(ConfigVector v) => v?.Node as ConfigSectionNode;
  }
}
