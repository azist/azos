/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Conf;

namespace Azos.Serialization.JSON
{
    /// <summary>
    /// Represents a data transfer object (DTO) abstraction used to read/write JSON data
    /// </summary>
    public interface IJSONDataObject
    {
    }


    /// <summary>
    /// Represents a data transfer object (DTO) JSON map, that associates keys with values
    /// </summary>
    [Serializable]
    public class JSONDataMap : Dictionary<string, object>, IJSONDataObject
    {
        /// <summary>
        /// Turns URL encoded content into JSONDataMap
        /// </summary>
        public static JSONDataMap FromURLEncodedStream(Stream stream, Encoding encoding = null, bool caseSensitive = false)
        {
          using(var reader = encoding==null ? new StreamReader(stream) : new StreamReader(stream, encoding))
          {
            return FromURLEncodedString(reader.ReadToEnd(), caseSensitive);
          }
        }

        /// <summary>
        /// Turns URL encoded content into JSONDataMap
        /// </summary>
        public static JSONDataMap FromURLEncodedString(string content, bool caseSensitive = false)
        {
          var result = new JSONDataMap(caseSensitive);

          if (content.IsNullOrWhiteSpace()) return result;

          int queryLen = content.Length;
          int idx = 0;

          while (idx < queryLen)
          {
            int ampIdx = content.IndexOf('&', idx);
            int kvLen = (ampIdx != -1) ? ampIdx - idx : queryLen - idx;

            if (kvLen < 1)
            {
              idx = ampIdx + 1;
              continue;
            }

            int eqIdx = content.IndexOf('=', idx, kvLen);
            if (eqIdx == -1)
            {
              var key = Uri.UnescapeDataString(content.Substring(idx, kvLen).Replace('+',' '));
              result.Add(key, null);
            }
            else
            {
              int keyLen = eqIdx - idx;
              if (keyLen > 0)
              {
                var key = Uri.UnescapeDataString(content.Substring(idx, keyLen).Replace('+',' '));
                var val = Uri.UnescapeDataString(content.Substring(eqIdx + 1, kvLen - keyLen - 1).Replace('+',' '));

                result.Add(key, val);
              }
            }

            idx += kvLen + 1;
          }

          return result;
        }


        public JSONDataMap(): base(StringComparer.InvariantCulture)
        {
          CaseSensitive = true;
        }

        public JSONDataMap(bool caseSensitive): base(caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase)
        {
          CaseSensitive = caseSensitive;
        }

        protected JSONDataMap(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {

        }


        public readonly bool CaseSensitive;

        public new object this[string key]
        {
          get
          {
            object result;
            if (base.TryGetValue(key, out result)) return result;
            return null;
          }
          set
          {
            base[key] = value;
          }
        }


        /// <summary>
        /// Appends contents of another JSONDataMap for keys that do not exist in this one or null.
        /// Only appends references, does not provide deep reference copy
        /// </summary>
        public JSONDataMap Append(JSONDataMap other, bool deep = false)
        {
          if (other==null) return this;

          foreach(var kvp in other)
          {
            var here = this[kvp.Key];
            if (here==null)
              this[kvp.Key] = kvp.Value;
            else
              if (deep && here is JSONDataMap) ((JSONDataMap)here).Append(kvp.Value as JSONDataMap, deep);
          }

          return this;
        }

        public override string ToString()
        {
          return this.ToJSON(JSONWritingOptions.Compact);
        }

        /// <summary>
        /// Returns this object as a config tree
        /// </summary>
        public ConfigSectionNode ToConfigNode(string rootName = null)
        {
          var mc = new LaconicConfiguration();
          mc.Create(rootName ?? GetType().Name);

          buildNode(mc.Root, this);

          return mc.Root;
        }

        private void buildNode(ConfigSectionNode node, JSONDataMap map)
        {
          foreach(var kvp in map)
          {
           var cmap = kvp.Value as JSONDataMap;
           if (cmap!=null)
            buildNode( node.AddChildNode(kvp.Key), cmap);
           else
            node.AddAttributeNode(kvp.Key, kvp.Value);
          }
        }

    }

    /// <summary>
    /// Represents a data transfer object (DTO) JSON array, that holds a list of values
    /// </summary>
    public class JSONDataArray : List<object>, IJSONDataObject
    {
      public JSONDataArray() {}
      public JSONDataArray(IEnumerable<object> other) : base(other) {}
      public JSONDataArray(int capacity) : base(capacity) {}

      public override string ToString()
      {
        return this.ToJSON(JSONWritingOptions.Compact);
      }
    }

}
