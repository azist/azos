/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Data.Access.MongoDb.Client;
using Azos.Glue;
using Azos.Instrumentation;
using Azos.Security;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;
using Azos.Web;


namespace Azos.Security.MinIdp.Instrumentation
{
  /// <summary>
  /// Sets role data
  /// </summary>
  [SystemAdministratorPermission(AccessLevel.ADVANCED)]
  public abstract class CmdBase : ExternalCallRequest<MinIdpMongoDbStore>
  {
    public CmdBase(MinIdpMongoDbStore mongo) : base(mongo) { }

    [Config]
    public Atom Realm {  get; set;}

    [Config]
    public string Id { get; set; }


    public sealed override ExternalCallResponse Execute()
    {
      Validate();
      var result = ExecuteBody();
      var response = ToResponse(result);
      return response;
    }

    protected virtual void Validate()
    {
      if (Realm.IsValid) throw new SecurityException("Parameter `$realm` must be a valid Atom");
      if (Id.IsNotNullOrWhiteSpace()) throw new SecurityException("Parameter `$id` is not set");
    }

    protected abstract object ExecuteBody();
    //{
    //  Context.Access((tx) => tx.Db);
    //}

    protected virtual ExternalCallResponse ToResponse(object result)
    {
      var json = result.ToJson(JsonWritingOptions.PrettyPrintASCII);
      return new ExternalCallResponse(ContentType.JSON, json);
    }

  }
}
