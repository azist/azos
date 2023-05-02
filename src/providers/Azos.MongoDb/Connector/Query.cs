/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.Serialization.BSON;

namespace Azos.Data.Access.MongoDb.Connector
{
  /// <summary>
  /// Represents a query document sent to MongoDB
  /// </summary>
  public class Query : BSONDocument
  {
    public const string _ID = Protocol._ID;

    public const string PROJECTION_ROOT = "$AZOS-QUERY-PROJECTION";


    public static Query ID_EQ_Int32(Int32 id)
    {
      var result = new Query();
      result.Set( new BSONInt32Element(_ID, id) );
      return result;
    }

    public static Query ID_EQ_Int64(Int64 id)
    {
      var result = new Query();
      result.Set(new BSONInt64Element(_ID, id));
      return result;
    }

    public static Query ID_EQ_UInt64(UInt64 id)
    {
      var result = new Query();
      result.Set(new BSONInt64Element(_ID, (long)id));
      return result;
    }

    public static Query ID_EQ_String(string id)
    {
      if (id==null)
       throw new MongoDbConnectorException(StringConsts.ARGUMENT_ERROR+"ID_EQ_String(id==null)");

      var result = new Query();
      result.Set( new BSONStringElement(_ID, id) );
      return result;
    }

    public static Query ID_EQ_GDID(GDID id)
    {
      var result = new Query();
      result.Set( DataDocConverter.GDID_CLRtoBSON(_ID, id) );
      return result;
    }

    public static Query ID_EQ_BYTE_ARRAY(byte[] id)
    {
      var result = new Query();
      result.Set( DataDocConverter.ByteBufferID_CLRtoBSON(_ID, id) );
      return result;
    }


    public Query() : base() { }
    public Query(Stream stream) : base(stream) { }

    /// <summary>
    /// Creates an instance of the query from JSON template with parameters populated from args optionally caching the template internal
    /// representation. Do not cache templates that change often
    /// </summary>
    public Query(string template, bool cacheTemplate, params TemplateArg[] args) : base(template, cacheTemplate, args)
    {
      var projNode = this[PROJECTION_ROOT] as BSONDocumentElement;
      if (projNode!=null)
      {
        this.ProjectionSelector = projNode.Value;
        this.Delete(PROJECTION_ROOT);
      }
    }

    /// <summary>
    /// Gets/sets projection document which should be embedded in query with '$Azos-QUERY-PROJECTION' see the PROJECTION_ROOT constant
    /// https://docs.mongodb.com/manual/tutorial/project-fields-from-query-results
    /// </summary>
    public BSONDocument ProjectionSelector{ get; set;}
  }

}
