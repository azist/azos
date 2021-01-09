/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Instrumentation;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Provides in-memory caching wrapper for target IMinIdpStore
  /// </summary>
  public sealed class MinIdpSqlStore : DaemonWithInstrumentation<IApplicationComponent>, IMinIdpStoreImplementation
  {
    public MinIdpSqlStore(IApplicationComponent dir) : base(dir)
    {
    }

    protected override void Destructor()
    {
      base.Destructor();
    }


    private string m_CString;


    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set; }

    [Config("$connection-string;$cstring;$connect-string")]
    public string ConnectString
    {
      get => m_CString;
      set
      {
        CheckDaemonInactive();
        m_CString = value;
      }
    }

    public ICryptoMessageAlgorithm MessageProtectionAlgorithm => null;

    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id)
     => await fetch( cmd => {
       cmd.CommandText = @"
SELECT
  tusr.*, tlg.PWD, trl.RIGHTS, tlg.SUTC as LOGIN_SUTC, tlg.EUTC as LOGIN_EUTC
FROM
  MIDP_LOGIN tlg,
  MIDP_USER tusr,
  MIDP_ROLE trl
WHERE
  (tlg.ID = @USER_ID) and
  (tlg.REALM = @REALM) and

  (tlg.SYSID = tusr.SYSID) and
  (tusr.REALM = @REALM) and
  (tusr.ROLE = trl.ID) and
  (trl.REALM = @REALM) and

  (tusr.SUTC < @UTC_NOW) and (tusr.EUTC > @UTC_NOW) and
  (tlg.SUTC < @UTC_NOW)  and (tlg.EUTC > @UTC_NOW)  and
  (trl.SUTC < @UTC_NOW)  and (trl.EUTC > @UTC_NOW)
";
       cmd.Parameters.AddWithValue("@USER_ID", id);
       cmd.Parameters.AddWithValue("@REALM", (long)realm.ID);
       cmd.Parameters.AddWithValue("@UTC_NOW", App.TimeSource.UTCNow);
     }, reader => new MinIdpUserData{
         SysId = reader["SYSID"].AsULong(),
         Realm = reader["REALM"].AsAtom(Atom.ZERO, ConvertErrorHandling.ReturnDefault),
         Status = mapStatus(reader["STAT"].AsInt(0)),
         CreateUtc =reader["CUTC"].AsDateTime(),
         StartUtc = reader["SUTC"].AsDateTime(),
         EndUtc = reader["EUTC"].AsDateTime(),
         LoginId = id,
         LoginPassword = reader["PWD"].AsString(),
         LoginStartUtc = reader["LOGIN_SUTC"].AsDateTime(),
         LoginEndUtc = reader["LOGIN_EUTC"].AsDateTime(),
         ScreenName = reader["SNAME"].AsString(),
         Name = reader["NAME"].AsString(),
         Description = reader["DESCR"].AsString(),
         Role = reader["ROLE"].AsString(),
         Rights = reader["RIGHTS"].AsString(),
         Note = reader["NOTE"] is DBNull ? null : reader["NOTE"].AsString()
     });


    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken)
     => await fetch(cmd => {
       cmd.CommandText = @"
SELECT
  tusr.*, trl.RIGHTS
FROM
  MIDP_USER tusr,
  MIDP_ROLE trl
WHERE
  (tusr.SYSID = @SYSID) and
  (tusr.REALM = @REALM) and
  (tusr.ROLE = trl.ID) and
  (trl.REALM = @REALM) and
  (tusr.SUTC < @UTC_NOW) and (tusr.EUTC > @UTC_NOW) and
  (trl.SUTC < @UTC_NOW)  and (trl.EUTC > @UTC_NOW)
";
       cmd.Parameters.AddWithValue("@SYSID", sysToken.AsLong());
       cmd.Parameters.AddWithValue("@REALM", (long)realm.ID);
       cmd.Parameters.AddWithValue("@UTC_NOW", App.TimeSource.UTCNow);
     }, reader => new MinIdpUserData
     {
       SysId = reader["SYSID"].AsULong(),
       Realm = reader["REALM"].AsAtom(Atom.ZERO, ConvertErrorHandling.ReturnDefault),
       Status = mapStatus(reader["STAT"].AsInt(0)),
       CreateUtc = reader["CUTC"].AsDateTime(),
       StartUtc = reader["SUTC"].AsDateTime(),
       EndUtc = reader["EUTC"].AsDateTime(),
       LoginId = null,
       LoginPassword = null,
       LoginStartUtc = null,
       LoginEndUtc = null,
       ScreenName = reader["SNAME"].AsString(),
       Name = reader["NAME"].AsString(),
       Description = reader["DESCR"].AsString(),
       Role = reader["ROLE"].AsString(),
       Rights = reader["RIGHTS"].AsString(),
       Note = reader["NOTE"] is DBNull ? null : reader["NOTE"].AsString()
     });


    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri)
     => await fetch(cmd => {
       cmd.CommandText = @"
SELECT
  tusr.*, trl.RIGHTS
FROM
  MIDP_USER tusr,
  MIDP_ROLE trl
WHERE
  (tusr.SNAME = @SNAME) and
  (tusr.REALM = @REALM) and
  (tusr.ROLE = trl.ID) and
  (trl.REALM = @REALM) and
  (tusr.SUTC < @UTC_NOW) and (tusr.EUTC > @UTC_NOW) and
  (trl.SUTC < @UTC_NOW)  and (trl.EUTC > @UTC_NOW)
";
       cmd.Parameters.AddWithValue("@SNAME", uri);
       cmd.Parameters.AddWithValue("@REALM", (long)realm.ID);
       cmd.Parameters.AddWithValue("@UTC_NOW", App.TimeSource.UTCNow);
     }, reader => new MinIdpUserData
     {
       SysId = reader["SYSID"].AsULong(),
       Realm = realm,
       Status = mapStatus(reader["STAT"].AsInt(0)),
       CreateUtc = reader["CUTC"].AsDateTime(),
       StartUtc = reader["SUTC"].AsDateTime(),
       EndUtc = reader["EUTC"].AsDateTime(),
       LoginId = null,
       LoginPassword = null,
       LoginStartUtc = null,
       LoginEndUtc = null,
       ScreenName = reader["SNAME"].AsString(),
       Name = reader["NAME"].AsString(),
       Description = reader["DESCR"].AsString(),
       Role = reader["ROLE"].AsString(),
       Rights = reader["RIGHTS"].AsString(),
       Note = reader["NOTE"] is DBNull ? null : reader["NOTE"].AsString()
     });



    protected override void DoStart()
    {
      m_CString.NonBlank("Connect string");
    }

    protected override void DoSignalStop() { }

    protected override void DoWaitForCompleteStop(){ }

    private async Task<MinIdpUserData> fetch(Action<SqlCommand> builder, Func<SqlDataReader, MinIdpUserData> body)
    {
      using (var cnn = new SqlConnection(m_CString))
      {
        await cnn.OpenAsync();
        using (var cmd = cnn.CreateCommand())
        {
          cmd.CommandType = CommandType.Text;
          builder(cmd);
          using (var reader = await cmd.ExecuteReaderAsync())
            if (await reader.ReadAsync()) return body(reader);
        }
      }

      return null;
    }

    private UserStatus mapStatus(int status)
    {
      if (status<1) return UserStatus.Invalid;
      if (status<2) return UserStatus.User;
      if (status<3) return UserStatus.Admin;
      return UserStatus.System;
    }


  }
}


/*
BEGIN
  DECLARE @USER_ID nvarchar(100)
  SET @USER_ID = 'xyzclient'

  DECLARE @REALM bigint
  SET @REALM = 97

  DECLARE @UTC_NOW datetime
  SET @UTC_NOW = '08/17/2020'

  SELECT
    tusr.*, tlg.PWD, trl.RIGHTS, tlg.SUTC as LOGIN_SUTC, tlg.EUTC as LOGIN_EUTC
  FROM
    MIDP_LOGIN tlg,
    MIDP_USER tusr,
    MIDP_ROLE trl
  WHERE
    (tlg.ID = @USER_ID) and
    (tlg.REALM = @REALM) and

    (tlg.SYSID = tusr.SYSID) and
    (tusr.REALM = @REALM) and
    (tusr.ROLE = trl.ID) and
    (trl.REALM = @REALM) and

    (tusr.SUTC < @UTC_NOW) and (tusr.EUTC > @UTC_NOW) and
    (tlg.SUTC < @UTC_NOW)  and (tlg.EUTC > @UTC_NOW)  and
    (trl.SUTC < @UTC_NOW)  and (trl.EUTC > @UTC_NOW)
END;

-- ----------------------------------------------------------------------

BEGIN
  DECLARE @SYSID bigint
  SET @SYSID = 5000

  DECLARE @REALM bigint
  SET @REALM = 97

  DECLARE @UTC_NOW datetime
  SET @UTC_NOW = '08/17/2020'

  SELECT
    tusr.*, trl.RIGHTS
  FROM
    MIDP_USER tusr,
    MIDP_ROLE trl
  WHERE
    (tusr.SYSID = @SYSID) and
    (tusr.REALM = @REALM) and
    (tusr.ROLE = trl.ID) and
    (trl.REALM = @REALM) and
    (tusr.SUTC < @UTC_NOW) and (tusr.EUTC > @UTC_NOW) and
    (trl.SUTC < @UTC_NOW)  and (trl.EUTC > @UTC_NOW)
END;

------------------------------------------------------------


BEGIN
  DECLARE @SNAME varchar(128)
  SET @SNAME = 'msmith97'

  DECLARE @REALM bigint
  SET @REALM = 97

  DECLARE @UTC_NOW datetime
  SET @UTC_NOW = '08/17/2020'

  SELECT
    tusr.*, trl.RIGHTS
  FROM
    MIDP_USER tusr,
    MIDP_ROLE trl
  WHERE
    (tusr.SNAME = @SNAME) and
    (tusr.REALM = @REALM) and
    (tusr.ROLE = trl.ID) and
    (trl.REALM = @REALM) and
    (tusr.SUTC < @UTC_NOW) and (tusr.EUTC > @UTC_NOW) and
    (trl.SUTC < @UTC_NOW)  and (trl.EUTC > @UTC_NOW)
END;


*/
