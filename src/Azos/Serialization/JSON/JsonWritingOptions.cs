/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;

namespace Azos.Serialization.JSON
{

    /// <summary>
    /// Specifies the purpose of JSON serialization so the level of detail may be dynamically adjusted.
    /// Depending on this parameter IJSONWritable implementors may include additional details
    /// that are otherwise not needed
    /// </summary>
    public enum JsonSerializationPurpose
    {
       Unspecified = 0,

       /// <summary>
       /// UI Interface feeding - include only the data needed to show to the user
       /// </summary>
       UIFeed,

       /// <summary>
       /// Include as much data as possible for remote object reconstruction
       /// </summary>
       Marshalling
    }


    /// <summary>
    /// Specifies how JSON should be written as text. Use JSONWritingOptions.Compact or JSONWritingOptions.PrettyPrint
    ///  static properties for typical options
    /// </summary>
    public class JsonWritingOptions : IConfigurable
    {
        private static JsonWritingOptions s_Compact =   new JsonWritingOptions(isSystem: true);

        private static JsonWritingOptions s_CompacRowsAsMap =  new JsonWritingOptions(isSystem: true) {m_RowsAsMap = true};

        private static JsonWritingOptions s_CompactASCII = new JsonWritingOptions(isSystem: true) { m_ASCIITarget = true };

        private static JsonWritingOptions s_PrettyPrint =  new JsonWritingOptions(isSystem: true)
        {
          m_IndentWidth = 2,
          m_ObjectLineBreak = true,
          m_MemberLineBreak = true,
          m_SpaceSymbols = true,
          m_ASCIITarget = false
        };

        private static JsonWritingOptions s_PrettyPrintASCII =  new JsonWritingOptions(isSystem: true)
        {
          m_IndentWidth = 2,
          m_ObjectLineBreak = true,
          m_MemberLineBreak = true,
          m_SpaceSymbols = true,
          m_ASCIITarget = true
        };

        private static JsonWritingOptions s_PrettyPrintRowsAsMap =  new JsonWritingOptions(isSystem: true)
        {
          m_IndentWidth = 2,
          m_ObjectLineBreak = true,
          m_MemberLineBreak = true,
          m_SpaceSymbols = true,
          m_ASCIITarget = false,
          m_RowsAsMap = true
        };


        /// <summary>
        /// Writes JSON without line breaks between members and no indenting. Suitable for data transmission
        /// </summary>
        public static JsonWritingOptions Compact => s_Compact;

        /// <summary>
        /// Writes JSON without line breaks between members and no indenting writing rows as maps(key:values) instead of arrays. Suitable for data transmission
        /// </summary>
        public static JsonWritingOptions CompactRowsAsMap => s_CompacRowsAsMap;

        /// <summary>
        /// Writes JSON without line breaks between members and no indenting escaping any characters
        ///  with codes above 127 suitable for ASCII transmission
        /// </summary>
        public static JsonWritingOptions CompactASCII => s_CompactASCII;

        /// <summary>
        /// Writes JSON suitable for printing/screen display
        /// </summary>
        public static JsonWritingOptions PrettyPrint => s_PrettyPrint;

        /// <summary>
        /// Writes JSON suitable for printing/screen display
        ///  with codes above 127 suitable for ASCII transmission
        /// </summary>
        public static JsonWritingOptions PrettyPrintASCII => s_PrettyPrintASCII;

        /// <summary>
        /// Writes JSON suitable for printing/screen display writing rows as maps(key:values) instead of arrays
        /// </summary>
        public static JsonWritingOptions PrettyPrintRowsAsMap => s_PrettyPrintRowsAsMap;

        public JsonWritingOptions(){ }
        internal JsonWritingOptions(bool isSystem) { m_IsSystem = isSystem; }

        public JsonWritingOptions(JsonWritingOptions other)
        {
          if (other==null) return;

          this.NLSMapLanguageISO        = other.NLSMapLanguageISO;
          this.NLSMapLanguageISODefault = other.NLSMapLanguageISODefault;
          this.IndentWidth              = other.IndentWidth;
          this.SpaceSymbols             = other.SpaceSymbols;
          this.ObjectLineBreak          = other.ObjectLineBreak;
          this.MemberLineBreak          = other.MemberLineBreak;
          this.ASCIITarget              = other.ASCIITarget;
          this.ISODates                 = other.ISODates;
          this.MaxNestingLevel          = other.MaxNestingLevel;
          this.RowsAsMap                = other.RowsAsMap;
          this.RowsetMetadata           = other.RowsetMetadata;
          this.Purpose                  = other.Purpose;
          this.MapSkipNulls             = other.MapSkipNulls;
          this.RowMapTargetName         = other.RowMapTargetName;
        }


        private bool m_IsSystem;
        private T nonsys<T>(T v) => m_IsSystem ? throw new AzosException(StringConsts.IMMUTABLE_SYS_INSTANCE.Args(nameof(JsonWritingOptions))) : v;

        private string m_NLSMapLanguageISO;
        private string m_NLSMapLanguageISODefault = CoreConsts.ISO_LANG_ENGLISH;
        private int m_IndentWidth;
        private bool m_SpaceSymbols;
        private bool m_ObjectLineBreak;
        private bool m_MemberLineBreak;
        private bool m_ASCIITarget;
        private bool m_ISODates = true;
        private int  m_MaxNestingLevel = 0xff;
        private bool m_RowsAsMap;
        private bool m_RowsetMetadata;
        private JsonSerializationPurpose m_Purpose;
        private bool   m_MapSkipNulls;
        private string m_RowMapTargetName;


        /// <summary>
        /// True to indicate that this instance is system and is immutable
        /// </summary>
        public bool IsSystem => m_IsSystem;


        /// <summary>
        /// Specifies language ISO code (3 chars) that is used (when set) by the NLSMap class,
        /// so only entries for that particular language are included. When NLSMap contains entries for more than 1 language,
        /// but user needs only one entry received for his/her selected language, this option can be set, then NLSMap will only inline
        /// Name:Descr pair for that language. If a map does not contain an entry for the requested lang then NLSMapLanguageISODefault
        /// will be tried
        /// </summary>
        [Config]
        public string NLSMapLanguageISO
        {
          get => m_NLSMapLanguageISO;
          set => m_NLSMapLanguageISO = nonsys(value);
        }

        /// <summary>
        /// Specified language ISO default for NLSMap lookup, "eng" is used for default
        /// </summary>
        [Config]
        public string NLSMapLanguageISODefault
        {
          get => m_NLSMapLanguageISODefault;
          set => m_NLSMapLanguageISODefault = nonsys(value);
        }

        /// <summary>
        /// Specifies character width of single indent level
        /// </summary>
        [Config]
        public int IndentWidth
        {
          get => m_IndentWidth;
          set => m_IndentWidth = nonsys(value);
        }

        /// <summary>
        /// Indicates whether a space must be placed right after the symbol, such as coma in array declaration or colon in member declaration for
        ///  better readability
        /// </summary>
        [Config]
        public bool SpaceSymbols
        {
          get => m_SpaceSymbols;
          set => m_SpaceSymbols = nonsys(value);
        }

        /// <summary>
        /// Specifies whether objects need to be separated by line brakes for better readability
        /// </summary>
        [Config]
        public bool ObjectLineBreak
        {
          get => m_ObjectLineBreak;
          set => m_ObjectLineBreak = nonsys(value);
        }

        /// <summary>
        /// Specifies whether every object member must be placed on a separate line for better readability
        /// </summary>
        [Config]
        public bool MemberLineBreak
        {
          get => m_MemberLineBreak;
          set => m_MemberLineBreak = nonsys(value);
        }

        /// <summary>
        /// Specifies whether the target of serialization only deals with ASCII characters,
        /// so any non-ASCII character with code above 127 must be escaped with unicode escape sequence
        /// </summary>
        [Config]
        public bool ASCIITarget
        {
          get => m_ASCIITarget;
          set => m_ASCIITarget = nonsys(value);
        }

        /// <summary>
        /// Specifies whether DateTime must be encoded using ISO8601 format that look like "2011-03-18T14:25:00Z",
        /// otherwise dates are encoded using "new Date(milliseconds_since_unix_epoch)" which is technically not a valid JSON, however
        ///  most JSON parsers understand it very well
        /// </summary>
        [Config]
        public bool ISODates
        {
          get => m_ISODates;
          set => m_ISODates = nonsys(value);
        }

        /// <summary>
        /// Sets a limit of object nesting, i.e. for recursive graph depth. Default is 0xff
        /// </summary>
        [Config]
        public int MaxNestingLevel
        {
          get => m_MaxNestingLevel;
          set => m_MaxNestingLevel = nonsys(value);
        }


        /// <summary>
        /// When true, writes every row as a map {FieldName: FieldValue,...} instead of array of values
        /// </summary>
        [Config]
        public bool RowsAsMap
        {
          get => m_RowsAsMap;
          set => m_RowsAsMap = nonsys(value);
        }

        /// <summary>
        /// When true, writes rowset metadata (i.e. schema, instance id etc.)
        /// </summary>
        [Config]
        public bool RowsetMetadata
        {
          get => m_RowsetMetadata;
          set => m_RowsetMetadata = nonsys(value);
        }

        /// <summary>
        /// Specifies the purpose of JSON serialization so the level of detail may be dynamically adjusted.
        /// Depending on this parameter IJSONWritable implementors may include additional details
        /// that are otherwise not needed
        /// </summary>
        [Config]
        public JsonSerializationPurpose Purpose
        {
          get => m_Purpose;
          set => m_Purpose = nonsys(value);
        }


        /// <summary>
        /// If true, then does not write map keys which are null
        /// </summary>
        [Config]
        public bool MapSkipNulls
        {
          get => m_MapSkipNulls;
          set => m_MapSkipNulls = nonsys(value);
        }


        /// <summary>
        /// When set, specifies the target name for Row's fields when they are written as map
        /// </summary>
        [Config]
        public string RowMapTargetName
        {
          get => m_RowMapTargetName;
          set => m_RowMapTargetName = nonsys(value);
        }

        public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, nonsys(node));
    }
}
