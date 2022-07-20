/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;

using Azos;
using Azos.Conf;
using Azos.Text;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Platform.ProcessActivation
{
  /// <summary>
  /// Activates program bodies from supplied command line args
  /// by invoking a class decorated with `[ProgramBody(name)]`
  /// </summary>
  public class ProgramBodyActivator
  {
    public class EMissingArgs : AzosException { public EMissingArgs(string msg) : base(msg) { } }
    public class ENotFound : AzosException { public ENotFound(string msg) : base(msg){ } }



    public const string CONFIG_NAME_PREFIX = "@";

    public const string CONFIG_ASSEMBLY_SECTION = "assembly";
    public const string CONFIG_INCLUDE_FILE_PATTERN_ATTR = "file";
    public const string CONFIG_EXCLUDE_FILE_PATTERN_ATTR = "exclude-file";

    public const string CONFIG_INCLUDE_TYPE_PATTERN_ATTR = "types";
    public const string CONFIG_EXCLUDE_TYPE_PATTERN_ATTR = "exclude-types";

    public ProgramBodyActivator(string[] args)
    {
      m_OriginalArgs = args.NonNull(nameof(args));
      if (m_OriginalArgs.Length == 0) throw new EMissingArgs("Missing arguments");

      if (args[0].StartsWith(CONFIG_NAME_PREFIX) && args[0].Length > CONFIG_NAME_PREFIX.Length)
      {
        var fn = args[0].Substring(CONFIG_NAME_PREFIX.Length);//get rid of prefix
        m_Args = args.Skip(1).ToArray();
        m_Manifest = Configuration.ProviderLoadFromFile(fn).Root;
      }
      else
      {
        m_Args = m_OriginalArgs;
        var fn = getDefaultConfigFileName();
        if (fn.IsNotNullOrWhiteSpace())
        {
          m_Manifest = Configuration.ProviderLoadFromFile(fn).Root;
        }
        else
        {
          m_Manifest = Configuration.NewEmptyRoot();
        }
      }

      m_AllPrograms = GetAllPrograms().DistinctBy(t => t.tbody).ToArray();

      if (m_Args.Length > 0)
      {
        m_ProcessName = m_Args[0];
        m_ProcessArgs = m_Args.Skip(1).ToArray();

        Ambient.SetProcessName(m_ProcessName);
        m_ProcessData = m_AllPrograms.FirstOrDefault(p => m_ProcessName.IsOneOf(p.bodyAttr.Names));
      }
      //otherwise object state is broken as there is no process name supplied
    }

    private string getDefaultConfigFileName()
    {
      var exeName = Assembly.GetEntryAssembly().Location;
      var exeNameWoExt = Path.Combine(Path.GetDirectoryName(exeName), Path.GetFileNameWithoutExtension(exeName));
      var extensions = Configuration.AllSupportedFormats.Select(fmt => '.' + fmt);
      foreach (var ext in extensions)
      {
        var configFile = exeName + ext;
        if (File.Exists(configFile)) return configFile;
        configFile = exeNameWoExt + ext;
        if (File.Exists(configFile)) return configFile;

      }
      return string.Empty;
    }

    private string[] m_OriginalArgs;
    private string[] m_Args;
    private string[] m_ProcessArgs;
    private string m_ProcessName;

    private (Type tbody, MethodInfo miEntryPoint, ProgramBodyAttribute bodyAttr)[] m_AllPrograms;
    private (Type tbody, MethodInfo miEntryPoint, ProgramBodyAttribute bodyAttr) m_ProcessData;

    private IConfigSectionNode m_Manifest;

    /// <summary>
    /// Original command line args supplied
    /// </summary>
    public string[] OriginalArgs => m_OriginalArgs;

    /// <summary>
    /// Command line args supplied without manifest config pragma (if any)
    /// </summary>
    public string[] Args => m_Args;

    /// <summary>
    /// The args passed to the program body
    /// </summary>
    public string[] ProcessArgs => m_ProcessArgs;

    /// <summary>
    /// Target process name
    /// </summary>
    public string ProcessName => m_ProcessName;

    /// <summary>
    /// Manifest as read from config file
    /// </summary>
    public IConfigSectionNode Manifest => m_Manifest;


    public IEnumerable<(Type tbody, MethodInfo miEntryPoint, ProgramBodyAttribute bodyAttr)> All => m_AllPrograms;

    protected virtual IEnumerable<(Type tbody, MethodInfo miEntryPoint, ProgramBodyAttribute bodyAttr)> GetAllPrograms()
    {
      var exeName = Assembly.GetEntryAssembly().Location;
      var rootDir = Path.GetDirectoryName(exeName);
      foreach (var nAssembly in Manifest.ChildrenNamed(CONFIG_ASSEMBLY_SECTION))
      {
        var asmFiles = rootDir.AllFileNames(true)
                              .Where(fn => nAssembly.AttributesNamed(CONFIG_INCLUDE_FILE_PATTERN_ATTR)
                                                    .Any(a => fn.MatchPattern(a.Value, senseCase: true)) &&
                                           (!nAssembly.AttributesNamed(CONFIG_EXCLUDE_FILE_PATTERN_ATTR)
                                                      .Any(a => fn.MatchPattern(a.Value, senseCase: true))));
        foreach (var afn in asmFiles)
        {
          Assembly asm;
          Type[] asmTypes;
          try
          {
            asm = Assembly.LoadFrom(afn);
            asmTypes = asm.GetTypes();
          }
          catch(Exception error)
          {
          #if DEBUG
            Console.WriteLine("Error loading assembly `{0}`: {1}".Args(afn, error.ToMessageWithType()));
          #endif
            continue;
          }

          var matchingTypes = asmTypes.Where(t => nAssembly.AttributesNamed(CONFIG_INCLUDE_TYPE_PATTERN_ATTR)
                                                    .Any(a => t.FullName.MatchPattern(a.Value, senseCase: true)) &&
                                                  (!nAssembly.AttributesNamed(CONFIG_EXCLUDE_TYPE_PATTERN_ATTR)
                                                             .Any(a => t.FullName.MatchPattern(a.Value, senseCase: true))));

          foreach (var type in matchingTypes)
          {
            var (miMain, bodyAttr) = GetMainEntryPoint(type);
            if (miMain == null) continue;//only classes decorated with atr and Main(string[])
            yield return (type, miMain, bodyAttr);
          }
        }//assembly file
      }//assembly node
    }

    /// <summary>
    /// Returns method info for a public static Main(string[]) method in a class decorated with [ProgramBody]
    /// </summary>
    protected virtual (MethodInfo mi, ProgramBodyAttribute atr) GetMainEntryPoint(Type type)
    {
      if (!type.IsClass) return (null, null);//only classes
      var atr = type.GetCustomAttribute<ProgramBodyAttribute>(false);
      if (atr == null) return (null, null);//only with attributes

      var main = type.GetMethod("Main", new[] { typeof(string[]) });
      if (main == null || !main.IsStatic) return (null, null);//must have public static Main(string[])
      return (main, atr);
    }

    public void Run()
    {
      var main = m_ProcessData.miEntryPoint;

      if (main == null) throw new ENotFound("Process `{0}` not found".Args(m_ProcessName));
      try
      {
        main.Invoke(null, new []{ m_ProcessArgs });
      }
      catch(TargetInvocationException tie)
      {
        throw tie.InnerException;
      }
    }

  }
}
