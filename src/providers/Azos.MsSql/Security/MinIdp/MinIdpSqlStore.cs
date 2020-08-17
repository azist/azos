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


    public async Task<MinIdpUserData> GetByIdAsync(string id)
     => await fetch( cmd => {
       cmd.CommandText = @"
SELECT
  tusr.*, tlg.PWD, trl.RIGHTS, tlg.SUTC as LOGIN_SUTC, tlg.EUTC as LOGIN_EUTC
FROM
  MIDP_LOGIN tlg,
  MIDP_USER tusr,
  MIDP_ROLE TRL
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
       cmd.Parameters.AddWithValue("@REALM", "??????????????");
       cmd.Parameters.AddWithValue("@UTC_NOW", App.TimeSource.UTCNow);
     }, reader => new MinIdpUserData{
         SysId = reader["SYSID"].AsULong(),
         Realm = reader["REALM"].AsAtom(Atom.ZERO, ConvertErrorHandling.ReturnDefault),
         Status = mapStatus(reader["STAT"].AsInt(0)),
         CreateUtc =reader["CUTC"].AsDateTime(),
         StartUtc = reader["SUTC"].AsDateTime(),
         EndUtc = reader["EUTC"].AsDateTime(),
         //todo....
     });


    public async Task<MinIdpUserData> GetBySysAsync(SysAuthToken sysToken)
     => await fetch(cmd => {
       cmd.CommandText = "aa";
       cmd.Parameters.AddWithValue("A", "Jeremy");
     }, reader => null);

    public async Task<MinIdpUserData> GetByUriAsync(string uri)
     => await fetch(cmd => {
       cmd.CommandText = "aa";
       cmd.Parameters.AddWithValue("A", "Jeremy");
     }, reader => null);



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
            return body(reader);
        }
      }
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
    MIDP_ROLE TRL
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
*/
