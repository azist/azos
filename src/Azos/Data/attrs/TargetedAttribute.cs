/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;

namespace Azos.Data
{
  /// <summary>
  /// Provides a base for attributes which are targeted for particular technology (e.g. "ORACLE", "RIAK" etc.)
  /// </summary>
  [Serializable]
  public abstract class TargetedAttribute : Attribute
  {
      public const string ANY_TARGET = "*";

      public TargetedAttribute(string targetName, string metadata)
      {
          TargetName = targetName.IsNullOrWhiteSpace() ? ANY_TARGET : targetName;
          m_MetadataContent = metadata;
      }

      /// <summary>
      /// Returns the name of target, i.e. the name of database engine i.e. "ORACLE11g" or "MySQL"
      /// </summary>
      public readonly string TargetName;

      /// <summary>
      /// Returns metadata content string in Laconic format or null. Root not is not specified. I.e.: 'a=1 b=true c{...}'
      /// </summary>
      public string MetadataContent {get{ return m_MetadataContent;}}

      protected string m_MetadataContent;

      [NonSerialized]
      private IConfigSectionNode m_Metadata;

      /// <summary>
      /// Returns structured metadata or null if there is no metadata defined
      /// </summary>
      public IConfigSectionNode Metadata
      {
        get
        {
          if (MetadataContent.IsNullOrWhiteSpace()) return null;
          if (m_Metadata==null)//not thread safe but its ok, in the worst case 2nd copy will be made
            m_Metadata = ParseMetadataContent(m_MetadataContent);

          return m_Metadata;
        }
      }

      /// <summary>
      /// Parses content with or without root node
      /// </summary>
      public static ConfigSectionNode ParseMetadataContent(string content)
      {
        try
        {
            content = content ?? string.Empty;
            var root = ("meta{"+content+"}").AsLaconicConfig(handling: ConvertErrorHandling.Throw);

            //Unwrap extra "meta" root node like:  meta{ meta{ a=1 } } -> meta{ a=1 }
            //if someone wrote metadata with `meta` wrap
            if (!root.HasAttributes && root.ChildCount==1)
            {
              var subroot = root["meta"];
              if (subroot.Exists) return subroot;
            }

            return root;
        }
        catch(Exception error)
        {
            throw new DataException(StringConsts.CRUD_FIELD_ATTR_METADATA_PARSE_ERROR.Args(error.ToMessageWithType(), content), error);
        }
      }

  }
}
