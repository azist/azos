
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
    public static class JSONExtensions
    {
        /// <summary>
        ///  Deserializes JSON content into dynamic JSON object
        /// </summary>
        public static dynamic JSONToDynamic(this string json, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDynamic(json, caseSensitiveMaps);
        }

        /// <summary>
        ///  Deserializes JSON content into dynamic JSON object
        /// </summary>
        public static dynamic JSONToDynamic(this Stream json, Encoding encoding = null, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDynamic(json, encoding, caseSensitiveMaps);
        }


        /// <summary>
        ///  Deserializes JSON content into dynamic JSON object
        /// </summary>
        public static dynamic JSONToDynamic(this ISourceText json, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDynamic(json, caseSensitiveMaps);
        }


        /// <summary>
        ///  Deserializes JSON content into IJSONDataObject
        /// </summary>
        public static IJSONDataObject JSONToDataObject(this string json, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDataObject(json, caseSensitiveMaps);
        }

        /// <summary>
        ///  Deserializes JSON content into IJSONDataObject
        /// </summary>
        public static IJSONDataObject JSONToDataObject(this Stream json, Encoding encoding = null, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDataObject(json, encoding, caseSensitiveMaps);
        }


        /// <summary>
        ///  Deserializes JSON content into IJSONDataObject
        /// </summary>
        public static IJSONDataObject JSONToDataObject(this ISourceText json, bool caseSensitiveMaps = true)
        {
            return JSONReader.DeserializeDataObject(json, caseSensitiveMaps);
        }



        /// <summary>
        ///  Serializes object into JSON string
        /// </summary>
        public static string ToJSON(this object root, Serialization.JSON.JSONWritingOptions options = null)
        {
            return JSONWriter.Write(root, options);
        }

        /// <summary>
        ///  Serializes object into JSON format using provided TextWriter
        /// </summary>
        public static void ToJSON(this object root, TextWriter wri, Serialization.JSON.JSONWritingOptions options = null)
        {
            JSONWriter.Write(root, wri, options);
        }

        /// <summary>
        ///  Serializes object into JSON format using provided stream and optional encoding
        /// </summary>
        public static void ToJSON(this object root, Stream stream, Serialization.JSON.JSONWritingOptions options = null, Encoding encoding = null)
        {
            JSONWriter.Write(root, stream, options, encoding);
        }
    }
}
