/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Conf;
using Azos.Instrumentation;
using Azos.Serialization.BSON;
using Azos.Web;

namespace Azos.Security.MinIdp.Instrumentation
{
  /// <summary>
  /// Sets login data
  /// </summary>
  public sealed class SetLogin : IdCmdBase
  {
    public SetLogin(MinIdpMongoDbStore mongo) : base(mongo) { }

    [Config] public long SysId { get; set; }
    [Config] public string Password { get; set; }
    [Config] public DateTime? StartUtc { get; set; }
    [Config] public DateTime? EndUtc { get; set; }

    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Sets user data
```
  SetLogin
  {
    realm='realm' //atom
    id='login' //string

    SysId=123 //User system id: long
    Password='{phash JSON vector}' //string: vector produced by phash
    StartUtc=UTC //UTCDateTime
    EndUtc=UTC //UTCDateTime
  }
```");

    protected override void Validate()
    {
      base.Validate();

      if (SysId==0) throw new SecurityException("Missing `$SysId`") { Code = -2001 };
      if (Password.IsNullOrWhiteSpace()) throw new SecurityException("Missing `$Password`") { Code = -2002 };
      if (Password.Length > BsonDataModel.MAX_PWD_LEN) throw new SecurityException("`$Password` vector len is over {0}".Args(BsonDataModel.MAX_PWD_LEN)) { Code = -2003 };

      if (!StartUtc.HasValue) StartUtc = Context.App.TimeSource.UTCNow;
      if (!EndUtc.HasValue) EndUtc = StartUtc.Value.AddDays(1.1);
    }

    protected override object ExecuteBody()
    {
      var crud = Context.Access((tx) => {
        var clin = tx.Db[BsonDataModel.GetCollectionName(this.Realm, BsonDataModel.COLLECTION_LOGIN)];
        var login = new BSONDocument();
        login.Set(new BSONStringElement(BsonDataModel._ID, Id.ToLowerInvariant()));
        login.Set(new BSONInt64Element(BsonDataModel.FLD_SYSID, SysId));
        login.Set(new BSONDateTimeElement(BsonDataModel.FLD_CREATEUTC, Context.App.TimeSource.UTCNow));
        login.Set(new BSONDateTimeElement(BsonDataModel.FLD_STARTUTC, StartUtc.Value));
        login.Set(new BSONDateTimeElement(BsonDataModel.FLD_ENDUTC, EndUtc.Value));

        login.Set(new BSONStringElement(BsonDataModel.FLD_PASSWORD, Password));

        var cr = clin.Save(login);
        Aver.IsNull(cr.WriteErrors, cr.WriteErrors.First().Message);
        return cr;
      });
      return crud;
    }

  }
}
