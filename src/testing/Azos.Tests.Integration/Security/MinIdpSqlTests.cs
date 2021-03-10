/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Scripting;
using Azos.Security;

namespace Azos.Tests.Integration.Security
{
  [Runnable]
  public class MinIdpSqlTests  : IRunnableHook
  {

    private static string confR1 =
      @"
app
{
  security
  {
    type='Azos.Security.MinIdp.MinIdpSecurityManager, Azos'
    realm = '1r'
    store
    {
       type='Azos.Security.MinIdp.CacheLayer, Azos'
       //max-cache-age-sec=0
       store
       {
         type='Azos.Security.MinIdp.MinIdpSqlStore, Azos.MsSql'
         connect-string='Data Source=$(~App.HOST);Initial Catalog=MINIDP;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False'
       }
    }
  }
}
";
    private AzosApplication m_App;


    public void Prologue(Runner runner, FID id)
    {
      m_App = new AzosApplication(null, confR1.AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw));
    }

    public bool Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }


    [Run]
    public void Authenticate_BadUserPassword()
    {
      var credentials = new IDPasswordCredentials("user1", "wqerwqerwqer");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status== UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_BadUser_SysToken1()
    {
      var tok = new SysAuthToken("1r", "23423423423423");
      var user = m_App.SecurityManager.Authenticate(tok);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_BadUser_SysToken2()
    {
      var tok = new SysAuthToken("4535r1", "5001");
      var user = m_App.SecurityManager.Authenticate(tok);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }

    [Run]
    public void Authenticate_BadUser_UriCredentials()
    {
      var credentials = new EntityUriCredentials("usr12222222");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.Invalid);
    }


    [Run]
    public void Authenticate_IDPasswordCredentials()
    {
      var credentials = new IDPasswordCredentials("user1", "awsedr");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("User1", user.Name);
    }

    [Run]
    public void Authenticate_SysToken()
    {
      var tok = new SysAuthToken("1r", "5001");
      var user = m_App.SecurityManager.Authenticate(tok);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("User1", user.Name);
    }

    [Run]
    public void Authenticate_UriCredentials()
    {
      var credentials = new EntityUriCredentials("usr1");
      var user = m_App.SecurityManager.Authenticate(credentials);
      Aver.IsTrue(user.Status == UserStatus.User);
      Aver.AreEqual("User1", user.Name);
    }


  }
}



//////-- --------------------------------------------------------
//////-- Host:                         OCTOD
//////-- Server version:               Microsoft SQL Server 2017 (RTM) - 14.0.1000.169
//////-- Server OS:                    Windows 10 Pro 10.0 <X64> (Build 18362: )
//////-- HeidiSQL Version:             9.4.0.5125
//////-- --------------------------------------------------------

///////*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
///////*!40101 SET NAMES  */;
///////*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
///////*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

//////-- Dumping data for table MINIDP.MIDP_LOGIN: 2 rows
///////*!40000 ALTER TABLE "MIDP_LOGIN" DISABLE KEYS */;
//////INSERT INTO "MIDP_LOGIN" ("ID", "REALM", "SYSID", "PWD", "SUTC", "EUTC") VALUES
//////  ('user1', 29233, 5001, '{alg:''KDF'', fam:''Text'', h:''3xg0BzA4wCZ9CXsfBZUKIbtPEylWoAXV1ecMJgY98Hs'', s:''k2P0NzALo4eIOmpHwNTrh-0iaEaab6dOSiniyNEDej8''}', '2001-01-01 00:00:00.000', '3000-01-01 00:00:00.000'),
//////	('xyzclient', 97, 5000, '{"p": "4235234" }', '2003-03-03 00:00:00.000', '3000-01-01 00:00:00.000');
///////*!40000 ALTER TABLE "MIDP_LOGIN" ENABLE KEYS */;

//////-- Dumping data for table MINIDP.MIDP_ROLE: 2 rows
///////*!40000 ALTER TABLE "MIDP_ROLE" DISABLE KEYS */;
//////INSERT INTO "MIDP_ROLE" ("ID", "REALM", "DESCR", "RIGHTS", "SUTC", "EUTC", "NOTE") VALUES
//////  ('Base', 97, 'Base role', '
//////r{ }

//////  ', '2001-01-01 00:00:00.000', '3000-01-01 00:00:00.000', 'B vs d'),
//////	('Role1', 29233, 'Role One', '{r:{}}', '2001-01-01 00:00:00.000', '3000-01-01 00:00:00.000', NULL);
///////*!40000 ALTER TABLE "MIDP_ROLE" ENABLE KEYS */;

//////-- Dumping data for table MINIDP.MIDP_USER: 2 rows
///////*!40000 ALTER TABLE "MIDP_USER" DISABLE KEYS */;
//////INSERT INTO "MIDP_USER" ("SYSID", "REALM", "ROLE", "STAT", "CUTC", "SUTC", "EUTC", "NAME", "DESCR", "SNAME", "NOTE") VALUES
//////  (5000, 97, 'Base', 1, '2001-01-01 00:00:00.000', '2001-01-01 00:00:00.000', '3000-01-01 00:00:00.000', 'Moike Dmith', 'Demon in exile', 'msmith97', 'I dont agree with that!'),
//////	(5001, 29233, 'Role1', 1, '2001-01-01 00:00:00.000', '2001-01-01 00:00:00.000', '3000-01-01 00:00:00.000', 'User1', 'User One', 'usr1', 'note1');
///////*!40000 ALTER TABLE "MIDP_USER" ENABLE KEYS */;

///////*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
///////*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
///////*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;




