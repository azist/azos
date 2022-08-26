/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Linq;
using Azos.Apps;
using Azos.Conf;

namespace Azos.Scripting.Packaging
{
  /// <summary>
  /// Facilitates installation of packages on local machine.
  /// Note: since installation is not a high-perm operation this class uses sync-only method for simplicity
  /// </summary>
  public class Installer : DisposableObject, IConfigurable
  {
    public const string CONFIG_RESOLVER_SECTION = "command-type-resolver";

    public enum RunStatus
    {
      Idle = 0,
      Starting,//while loading pkg etc
      Running,//while running
      Stopping,//after run before transitioning to stopped
      Stopped
    }


    /// <summary>
    /// Types of built-in commends declared in this namespace
    /// </summary>
    public static readonly Type[] BUILT_IN_COMMANDS =
       typeof(Installer).Assembly
                        .GetTypes()
                        .Where(t => t.Namespace.StartsWith(typeof(Installer).Namespace) && t.IsSubclassOf(typeof(Command)))
                        .ToArray();

    public delegate void ProgressHandler(string status);

    public Installer(IApplication app, GuidTypeResolver<Command, PackageCommandAttribute> commandTypeResolver = null)
    {
      m_App = app.NonNull(nameof(app));
      m_CommandTypeResolver = commandTypeResolver;
    }

    protected override void Destructor()
    {
      base.Destructor();
      DisposeAndNull(ref m_Package);
    }

    private IApplication m_App;

    private RunStatus m_Status;

    private GuidTypeResolver<Command, PackageCommandAttribute> m_CommandTypeResolver;
    private Package m_Package;
    private string m_RootPath;
    private string m_PackagePath;
    private string[] m_TargetNames;


    /// <summary>
    /// Error, if any which aborted the install run or null
    /// </summary>
    public Exception State_Error => m_State_Error;
    /// <summary>
    /// True if there is a non-null error which aborted the run
    /// </summary>
    public bool State_IsError => m_State_Error != null;
    private Exception m_State_Error;

    /// <summary>
    /// Currently executing target name. Null or empty signifies ANY target
    /// </summary>
    public string State_CurrentTarget => m_State_CurrentTarget;
    protected string m_State_CurrentTarget;

    /// <summary>
    /// Currently executing command
    /// </summary>
    public Command State_CurrentCommand => m_State_CurrentCommand;
    protected Command m_State_CurrentCommand;

    /// <summary>
    /// Currently executing command zero-based index
    /// </summary>
    public long State_CurrentCommandIndex => m_State_CurrentCommandIndex;
    protected long m_State_CurrentCommandIndex = -1;


    /// <summary>
    /// Currently open file stream or null
    /// </summary>
    public FileStream State_FileStream => m_State_FileStream;
    protected FileStream m_State_FileStream;

    /// <summary>
    /// Currently path segments
    /// </summary>
    public string[] State_PathSegments => m_State_PathSegments;
    protected string[] m_State_PathSegments;

    /// <summary>
    /// Installation application chassis
    /// </summary>
    public IApplication App => m_App;


    /// <summary>
    /// Returns configured type resolver or null if nothing was set in which case the installer will
    /// use the default package resolution strategy which returns stock commands from this assembly
    /// </summary>
    public GuidTypeResolver<Command, PackageCommandAttribute> CommandTypeResolver => m_CommandTypeResolver;

    /// <summary>
    /// Mounted package or null if not loaded yet
    /// </summary>
    public Package Package => m_Package;

    public event ProgressHandler Progress;

    /// <summary>
    /// Defines where the package is going to be installed.
    /// Package paths are relative to this root path
    /// </summary>
    [Config]
    public string RootPath
    {
      get => m_RootPath;
      set => m_RootPath = idle(value);
    }

    /// <summary>
    /// Defines where the package file path
    /// </summary>
    [Config]
    public string PackagePath
    {
      get => m_PackagePath;
      set => m_PackagePath = idle(value);
    }

    /// <summary>
    /// Defines what targets should be installed. Multiple targets get separated by commas
    /// </summary>
    [Config]
    public string TargetNames
    {
      get => m_TargetNames != null ? string.Join(',', m_TargetNames) : string.Empty;
      set
      {
        if (idle(value)==null)
        {
          m_TargetNames = null;
          return;
        }

        m_TargetNames = value.Split(",")
                             .Where(s => s.IsNotNullOrWhiteSpace())
                             .Distinct(StringComparer.InvariantCultureIgnoreCase).ToArray();
        if (m_TargetNames.Length == 0) m_TargetNames = null;
      }
    }






    public virtual void Configure(IConfigSectionNode node)
    {
      if (node == null) return;

      ConfigAttribute.Apply(this, node);
      var nresolver = node[CONFIG_RESOLVER_SECTION];
      if (nresolver.Exists)
      {
        m_CommandTypeResolver = FactoryUtils.Make<GuidTypeResolver<Command, PackageCommandAttribute>>(nresolver,
                                    typeof(GuidTypeResolver<Command, PackageCommandAttribute>), new[]{nresolver});
      }
    }


    /// <summary>
    /// Starts the installation.
    /// If `onlyIntrospect` is specified then only loads the Guid resolver and Package for introspection
    /// </summary>
    public void Run(bool onlyIntrospect = false)
    {
      (m_Status == RunStatus.Idle || m_Status == RunStatus.Stopped).IsTrue("Idle|Stopped");
      m_Status = RunStatus.Starting;
      m_State_Error = null;
      m_State_CurrentCommand = null;
      m_State_CurrentCommandIndex = -1;
      try
      {
        DoBeforeRun();

        if (!onlyIntrospect)
        {
          m_Status = RunStatus.Running;
          DoRun();
          m_Status = RunStatus.Stopping;
        }

        DoAfterRun();
      }
      catch(Exception error)
      {
        m_State_Error = error;
      }
      finally
      {
        m_Status = RunStatus.Stopped;
        CloseCurrentFile();
      }
    }

    protected virtual void DoBeforeRun()
    {
      System.IO.Directory.Exists(m_RootPath.NonBlank(nameof(RootPath))).IsTrue("Existing root path: `{0}`".Args(m_RootPath));
      System.IO.File.Exists(m_PackagePath.NonBlank(nameof(PackagePath))).IsTrue("Existing package file: `{0}`".Args(m_PackagePath));
      DisposeAndNull(ref m_Package);
      m_Package = Package.FromFile(App, m_PackagePath, m_CommandTypeResolver);
    }


    protected virtual bool DoFilterCommand(Command one)
    {
      if (m_TargetNames == null) return true;
      return m_State_CurrentTarget.IsOneOf(m_TargetNames);
    }

    protected virtual void DoRun()
    {
      foreach(var one in m_Package.Commands)//loop though this once for speed
      {
        m_State_CurrentCommand = one;
        m_State_CurrentCommandIndex++;

        if (one is not FileChunkCommand) CloseCurrentFile();

        if (one is StartTargetCommand cmdStartTarget) //special system command
        {
          m_State_CurrentTarget = cmdStartTarget.TargetName;
          continue;
        }

        if (one is StopCommand cmdStop) //special system command
        {
          break;
        }

        var ok = DoFilterCommand(one);
        if (!ok) continue;

        one.Execute(this);
      }//foreach
    }

    protected virtual void DoAfterRun()
    {
      CloseCurrentFile();
    }


    /// <summary>
    /// Returns True if condition is evaluated positively according to current installer state
    /// </summary>
    public virtual bool EvaluateCondition(string condition)
    {
      return true;
    }

    /// <summary>
    /// Executes script on host OS
    /// </summary>
    public virtual void ExecuteOsScript(string text)
    {
    //https://stackoverflow.com/questions/45132081/file-permissions-on-linux-unix-with-net-core
    }

    public virtual void ChangeDirectory(string name)
    {
      if (name == "/" || name == "\\")
      {
        m_State_PathSegments = null;
        return;
      }

      if (name == "..")
      {
        if (m_State_PathSegments != null  &&  m_State_PathSegments.Length > 0)
        {
          m_State_PathSegments = m_State_PathSegments.Take(m_State_PathSegments.Length - 1).ToArray();
        }
        return;
      }

      name = GuardOnePathSegment(name);
      var newFullPath = GetCurrentFullPathOf(name);
      Directory.Exists(newFullPath).IsTrue("Existing dir");

      m_State_PathSegments = m_State_PathSegments == null ? new string[]{ name } : m_State_PathSegments.AppendToNew(name);
    }

    public virtual void CreateDirectory(string name)
    {
      var currentDir = GetCurrentFullPathOf(null);
      Directory.Exists(currentDir).IsTrue("Existing stem dir");

      var newFullPath = GetCurrentFullPathOf(GuardOnePathSegment(name));
      Directory.CreateDirectory(newFullPath);
    }

    public virtual void CreateFile(string name)
    {
      CloseCurrentFile();
      var fullPath = GetCurrentFullPathOf(GuardOnePathSegment(name));
      m_State_FileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
    }

    public virtual void WriteFileChunk(long offset, byte[] data)
    {
      //WARNING: Never trust data arriving from wire. MUST perform all MAX size checks!!!
      (offset >= 0 && offset < Package.MAX_TOTAL_FILE_SIZE_BYTES).IsTrue("Max file sz");
      (data.NonNull(nameof(data)).Length <= Package.MAX_CHUNK_FILE_SIZE_BYTES).IsTrue("Max chunk sz");

      var fs = m_State_FileStream.NonDisposed("Open fs");
      fs.Position = offset;
      fs.Write(data, 0, data.Length);
    }

    public virtual void CloseCurrentFile()
    {
      if (m_State_FileStream != null)
      {
        m_State_FileStream.Close();
        DisposeAndNull(ref m_State_FileStream);
      }
    }

    protected string GuardOnePathSegment(string seg)
    {
      (seg.IsNotNullOrWhiteSpace() &&
       seg.IndexOf(Path.PathSeparator) < 0 &&
       seg.IndexOf(Path.DirectorySeparatorChar) < 0 &&
       seg.IndexOf(Path.AltDirectorySeparatorChar) < 0 &&
       seg.IndexOf(Path.VolumeSeparatorChar) < 0).IsTrue("Valid path segment");

      return seg.Trim();
    }

    protected string GetCurrentFullPathOf(string name)
     => Path.Combine(m_RootPath, GetCurrentRelativePathOf(name));

    protected string GetCurrentRelativePathOf(string name)
    {
      if (m_State_PathSegments == null || m_State_PathSegments.Length == 0) return name;
      return name.IsNotNullOrWhiteSpace() ? Path.Combine(m_State_PathSegments.AppendToNew(name))
                                          : Path.Combine(m_State_PathSegments);
    }

    private T idle<T>(T v)
    {
      (m_Status == RunStatus.Idle).IsTrue("Idle");
      return v;
    }
  }
}
