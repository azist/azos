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

using Azos.Conf;
using Azos.Text;

namespace Azos.Platform.Process
{
  /// <summary>
  /// Activates program bodies from supplied command line args
  /// by invoking a class decorated with `[ProgramBody(name)]`
  /// </summary>
  public class ProgramBodyActivator
  {
    public const string CONFIG_NAME_PREFIX = "@";

    public const string CONFIG_ASSEMBLY_SECTION = "assembly";
    public const string CONFIG_INCLUDE_FILE_PATTERN_ATTR = "file";
    public const string CONFIG_EXCLUDE_FILE_PATTERN_ATTR = "exclude-file";

    public const string CONFIG_INCLUDE_TYPE_PATTERN_ATTR = "types";
    public const string CONFIG_EXCLUDE_TYPE_PATTERN_ATTR = "exclude-types";

    public ProgramBodyActivator(string[] args)
    {
      m_OriginalArgs = args.NonNull(nameof(args));
      (m_OriginalArgs.Length > 0).IsTrue("Present activator args");

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

      (m_Args.Length > 0).IsTrue("Present process name activator args");
      m_ProcessName = m_Args[0];
      m_ProcessArgs = m_Args.Skip(1).ToArray();

      Ambient.SetProcessName(m_ProcessName);

      m_AllPrograms = GetAllPrograms().ToArray();
      m_ProcessData = m_AllPrograms.FirstOrDefault(p => m_ProcessName.IsOneOf(p.bodyAttr.Names));
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
                                                      .Any(a => fn.MatchPattern(a.Value, senseCase: true))) );

        foreach(var afn in asmFiles)
        {
          var asm = Assembly.LoadFrom(afn);
          var asmTypes = asm.GetTypes();

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
    public (MethodInfo mi, ProgramBodyAttribute atr) GetMainEntryPoint(Type type)
    {
      if (!type.IsClass) return (null, null);//only classes
      var atr = type.GetCustomAttribute<ProgramBodyAttribute>(false);
      if (atr == null) return (null, null);//only with attributes

      var main = type.GetMethod("Main", new[] { typeof(string[]) });
      if (main == null || !main.IsStatic) return (null, null);//must have public static Main(string[])
      return (main, atr);
    }

    public bool Run()
    {
      if (m_ProcessData==null) return false;
      try
      {
        var toRun = allPrograms.FirstOrDefault(p => activator.ProcessName.IsOneOf(p.bodyAttr.Names));
        if (toRun == null)
        {
          m_ProcessData.miEntryPoint.Invoke(null, m_ProcessArgs);
        return true;
      }
      catch(TargetInvocationException tie)
      {
        throw tie.InnerException;
      }
    }

  }
}
