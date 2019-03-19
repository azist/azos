/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.CodeAnalysis.Source;


namespace Azos.Serialization.JSON
{
    /// <summary>
    /// Provides JSON extension methods
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        ///  Deserializes JSON content into dynamic JSON object
        /// </summary>
        public static dynamic JsonToDynamic(this string json, bool caseSensitiveMaps = true)
        {
            return JsonReader.DeserializeDynamic(json, caseSensitiveMaps);
        }

        /// <summary>
        ///  Deserializes JSON content into dynamic JSON object
        /// </summary>
        public static dynamic JsonToDynamic(this Stream json, Encoding encoding = null, bool caseSensitiveMaps = true)
        {
            return JsonReader.DeserializeDynamic(json, encoding, caseSensitiveMaps);
        }


        /// <summary>
        ///  Deserializes JSON content into dynamic JSON object
        /// </summary>
        public static dynamic JsonToDynamic(this ISourceText json, bool caseSensitiveMaps = true)
        {
            return JsonReader.DeserializeDynamic(json, caseSensitiveMaps);
        }


        /// <summary>
        ///  Deserializes JSON content into IJSONDataObject
        /// </summary>
        public static IJsonDataObject JsonToDataObject(this string json, bool caseSensitiveMaps = true)
        {
            return JsonReader.DeserializeDataObject(json, caseSensitiveMaps);
        }

        /// <summary>
        ///  Deserializes JSON content into IJSONDataObject
        /// </summary>
        public static IJsonDataObject JsonToDataObject(this Stream json, Encoding encoding = null, bool caseSensitiveMaps = true)
        {
            return JsonReader.DeserializeDataObject(json, encoding, caseSensitiveMaps);
        }


        /// <summary>
        ///  Deserializes JSON content into IJSONDataObject
        /// </summary>
        public static IJsonDataObject JsonToDataObject(this ISourceText json, bool caseSensitiveMaps = true)
        {
            return JsonReader.DeserializeDataObject(json, caseSensitiveMaps);
        }



        /// <summary>
        ///  Serializes object into JSON string
        /// </summary>
        public static string ToJson(this object root, JsonWritingOptions options = null)
        {
            return JsonWriter.Write(root, options);
        }

        /// <summary>
        ///  Serializes object into JSON format using provided TextWriter
        /// </summary>
        public static void ToJson(this object root, TextWriter wri, JsonWritingOptions options = null)
        {
            JsonWriter.Write(root, wri, options);
        }

        /// <summary>
        ///  Serializes object into JSON format using provided stream and optional encoding
        /// </summary>
        public static void ToJson(this object root, Stream stream, JsonWritingOptions options = null, Encoding encoding = null)
        {
            JsonWriter.Write(root, stream, options, encoding);
        }
    }
}
