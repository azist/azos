/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Text;
using System.IO;

using Azos.Conf;
using Azos.Collections;

namespace Azos.Sky.Identification.Server{ public partial class GdidAuthorityServiceBase {


  public abstract class PersistenceLocation : INamed, IOrdered
  {
    protected PersistenceLocation(IConfigSectionNode node)
    {
      if (node==null)
        throw new GdidException(StringConsts.ARGUMENT_ERROR + "PersistenceLocation(node=null)");

      Name  = node.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
      Order = node.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt();
      if (Name.IsNullOrWhiteSpace())
        throw new GdidException(StringConsts.ARGUMENT_ERROR + "PersistenceLocation(name=null|empty)");
    }

    public string Name  { get; private set; }
    public int    Order { get; private set; }

    public abstract string Validate();

    public abstract void Write(byte authority, string scopeName, string sequenceName, _id data);
    public abstract _id? Read(byte authority, string scopeName, string sequenceName);

    public override string ToString()
    {
      return "{0}(`{1}`,#{2})".Args(GetType().Name, Name, Order);
    }
  }



  public sealed class DiskPersistenceLocation : PersistenceLocation
  {
    public const string CONFIG_PATH_ATTR = "path";

    public DiskPersistenceLocation(IConfigSectionNode node) : base(node)
    {
      DiskPath  = node.AttrByName(CONFIG_PATH_ATTR).Value;
      if (DiskPath.IsNullOrWhiteSpace())
        throw new GdidException(StringConsts.ARGUMENT_ERROR + "DiskPersistenceLocation(path=null|empty)");
    }

    public string DiskPath  { get; private set; }

    public override string ToString()
    {
      return "{0} -> '{1}'".Args(base.ToString(), DiskPath);
    }

    public override string Validate()
    {
      if (DiskPath.IsNullOrWhiteSpace())
        return "Path is null";

      if (DiskPath.Length > MAX_DISK_PATH_LENGTH)
        return "`{0}` is too long".Args(DiskPath.TakeFirstChars(30));

      if (!Directory.Exists(DiskPath))
      return "Path `{0}` does not exist".Args(DiskPath);

      return null;
    }

    public override void Write(byte authority, string scopeName, string sequenceName, _id data)
    {
      var fname = getFileName(DiskPath, authority, scopeName, sequenceName);

      if (fname.Length>MAX_PATH_LENGTH)
        throw new GdidException(StringConsts.GDIDAUTH_DISK_PATH_TOO_LONG_ERROR.Args(fname));

      var authDir = Path.Combine(DiskPath, AuthorityPathSeg(authority));
      if (!Directory.Exists(authDir))
        Directory.CreateDirectory(authDir);

      var scopeDir = Path.Combine(authDir, scopeName);
      if (!Directory.Exists(scopeDir))
        Directory.CreateDirectory(scopeDir);

      using(var fs = new FileStream(fname, FileMode.Create, FileAccess.Write, FileShare.None, 0xff, FileOptions.WriteThrough))
      {
        var buf = Encoding.ASCII.GetBytes( data.ToString() );
        fs.Write(buf, 0, buf.Length);
        fs.Flush(true);
      }
    }

    public override _id? Read(byte authority, string scopeName, string sequenceName)
    {
      var fname = getFileName(DiskPath, authority, scopeName, sequenceName);

      if (fname.Length>MAX_PATH_LENGTH) return null;
      if (!File.Exists(fname)) return null;


      using(var fs = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.None))
        using(var reader = new StreamReader(fs, Encoding.ASCII))
        {
          var sid = reader.ReadToEnd();
          _id id = new _id(sid); //this throws
          return id;
        }
    }

    private static string getFileName(string dpath, byte authority, string scope, string seq)
    {
      return Path.Combine(dpath, AuthorityPathSeg(authority), scope, seq);
    }

  }//disk



  public sealed class RemotePersistenceLocation : PersistenceLocation
  {
    public const string CONFIG_HOST_ATTR = "host";

    public RemotePersistenceLocation(IConfigSectionNode node) : base(node)
    {
      Host  = node.AttrByName(CONFIG_HOST_ATTR).Value;
      if (Host.IsNullOrWhiteSpace())
        throw new GdidException(StringConsts.ARGUMENT_ERROR + "RemotePersistenceLocation(host=null|empty)");
    }

    public string Host  { get; private set; }

    public override string ToString()
    {
      return "{0} -> '{1}'".Args(base.ToString(), Host);
    }

    public override string Validate()
    {
      throw new NotImplementedException();
    }

    public override _id? Read(byte authority, string scopeName, string sequenceName)
    {
      throw new NotImplementedException();
    }

    public override void Write(byte authority, string scopeName, string sequenceName, _id data)
    {
      throw new NotImplementedException();
    }
  } //Remote



}}
