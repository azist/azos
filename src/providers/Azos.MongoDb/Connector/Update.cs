/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.IO;

using Azos.Serialization.BSON;

namespace Azos.Data.Access.MongoDb.Connector
{
  /// <summary>
  /// Represents an update document sent to MongoDB
  /// </summary>
  public class Update : BSONDocument
  {
    public Update() : base() { }
    public Update(Stream stream) : base(stream) { }

    /// <summary>
    /// Creates an instance of the update from JSON template with parameters populated from args optionally caching the template internal
    /// representation. Do not cache templates that change often
    /// </summary>
    public Update(string template, bool cacheTemplate, params TemplateArg[] args):base(template, cacheTemplate, args)
    {
    }
  }

}
