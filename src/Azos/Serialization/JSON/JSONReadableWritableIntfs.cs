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

namespace Azos.Serialization.JSON
{
    /// <summary>
    /// Denotes a CLR type-safe entity (class or struct) that can directly write itself as JSON content string.
    /// This mechanism bypasses all of the reflection/dynamic code.
    /// This approach may be far more performant for some classes that need to serialize their state/data in JSON format,
    /// than relying on general-purpose JSON serializer that can serialize any type but is slower
    /// </summary>
    public interface IJSONWritable
    {
        /// <summary>
        /// Writes entitie's data/state as JSON string
        /// </summary>
        ///<param name="wri">
        ///TextWriter to write JSON content into
        ///</param>
        /// <param name="nestingLevel">
        /// A level of nesting that this instance is at, relative to the graph root.
        /// Implementations may elect to use this parameter to control indenting or ignore it
        /// </param>
        /// <param name="options">
        /// Writing options, such as indenting.
        /// Implementations may elect to use this parameter to control text output or ignore it
        /// </param>
        void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null);
    }


    /// <summary>
    /// Denotes a CLR type-safe entity (class or struct) that can directly read itself from IJSONDataObject which is supplied by JSON parser.
    /// This mechanism bypasses all of the reflection/dynamic code.
    /// This approach may be far more performant for some classes that need to de-serialize their state/data from JSON format,
    /// than relying on general-purpose JSON serializer that can deserialize any type but is slower.
    /// The particular type has to be allocated first, then it's instance can be hydrated with data/state using this method
    /// </summary>
    public interface IJSONReadable
    {
        /// <summary>
        /// Reads entitie's data/state from low-level IJSONDataObject which is supplied right by JSONParser.
        /// An implementer may elect to throw various types of exceptions to signal such conditions as:
        ///  unknown key map, or too many fields not supplied etc.
        /// </summary>
        /// <param name="data">JSONParser-supplied object</param>
        void ReadAsJSON(IJSONDataObject data);
    }
}
