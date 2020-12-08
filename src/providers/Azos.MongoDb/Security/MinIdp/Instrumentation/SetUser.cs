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
  /// Sets user data
  /// </summary>
  public sealed class SetUser : LongIdCmdBase
  {
    public SetUser(MinIdpMongoDbStore mongo) : base(mongo) { }

    [Config] public UserStatus? Status { get; set; }
    [Config] public DateTime? StartUtc { get; set; }
    [Config] public DateTime? EndUtc { get; set; }
    [Config] public string Role { get; set; }
    [Config] public string Name { get; set; }
    [Config] public string Description { get; set; }
    [Config] public string Note { get; set; }


    public override ExternalCallResponse Describe()
    => new ExternalCallResponse(ContentType.TEXT,
@"
# Sets user data
```
  SetUser
  {
    realm='realm' //atom
    id=01234 //int64

    Status=User //UserStatus
    StartUtc=UTC //UTCDateTime
    EndUtc=UTC //UTCDateTime
    Role='role'//string
    Name='name'//string
    Description='descr'//string
    Note='note text'//string
  }
```");

    protected override void Validate()
    {
      base.Validate();

      if (!Status.HasValue) throw new SecurityException("Missing `$Status`") { Code = -2001 };
      if (Status != UserStatus.System && Role.IsNullOrWhiteSpace()) throw new SecurityException("Missing `$Role` for non system users") { Code = -2002 };
      if (Role?.Length > BsonDataModel.MAX_ID_LEN) throw new SecurityException("`$Role` is over {0}".Args(BsonDataModel.MAX_ID_LEN)) { Code = -2003 };

      if (Name.IsNullOrWhiteSpace()) throw new SecurityException("Missing `$Name`") { Code = -2004 };
      if (Name.Length > BsonDataModel.MAX_NAME_LEN) throw new SecurityException("`$Name` is over {0}".Args(BsonDataModel.MAX_NAME_LEN)) { Code = -2005 };

      if (Description.IsNotNullOrWhiteSpace())
      {
         if (Description.Length > BsonDataModel.MAX_DESCR_LEN) throw new SecurityException("`$Description` is over {0}".Args(BsonDataModel.MAX_DESCR_LEN)) { Code = -2006 };
      }
      else Description = Name;

      if (!StartUtc.HasValue) StartUtc = Context.App.TimeSource.UTCNow;
      if (!EndUtc.HasValue) EndUtc = StartUtc.Value.AddDays(1.1);

      if (Note.IsNotNullOrWhiteSpace())
      {
        if (Note.Length > BsonDataModel.MAX_NOTE_LEN) throw new SecurityException("`$Note` is over {0}".Args(BsonDataModel.MAX_NOTE_LEN)) { Code = -2007 };
      }
      else Note = string.Empty;
    }

    protected override object ExecuteBody()
    {
      var crud = Context.Access((tx) => {
        var cusr = tx.Db[BsonDataModel.GetCollectionName(this.Realm, BsonDataModel.COLLECTION_USER)];
        var user = new BSONDocument();
        user.Set(new BSONInt64Element(BsonDataModel._ID, Id));
        user.Set(new BSONInt32Element(BsonDataModel.FLD_STATUS, (int)Status.Value));
        user.Set(new BSONDateTimeElement(BsonDataModel.FLD_CREATEUTC, Context.App.TimeSource.UTCNow));
        user.Set(new BSONDateTimeElement(BsonDataModel.FLD_STARTUTC, StartUtc.Value));
        user.Set(new BSONDateTimeElement(BsonDataModel.FLD_ENDUTC, EndUtc.Value));

        user.Set(new BSONStringElement(BsonDataModel.FLD_ROLE, Role.Default(string.Empty)));

        user.Set(new BSONStringElement(BsonDataModel.FLD_NAME, Name));
        user.Set(new BSONStringElement(BsonDataModel.FLD_DESCRIPTION, Description));
        user.Set(new BSONStringElement(BsonDataModel.FLD_NOTE, Note));

        var cr = cusr.Save(user);
        Aver.IsNull(cr.WriteErrors, cr.WriteErrors?.FirstOrDefault().Message);
        return cr;
      });
      return crud;
    }

  }
}
