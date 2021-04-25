/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Globalization;
using Azos.Data;

namespace Azos.Conf
{
  /// <summary>
  /// Retrieves build information encapsulated into a module in the form of an embedded resource
  /// </summary>
  [Serializable]
  public sealed class BuildInformation
  {
    #region  CONSTS
    public const string FRAMEWORK_BUILD_INFO_PATH = "Azos." + BUILD_INFO_RESOURCE;
    public const string BUILD_INFO_RESOURCE = "BUILD_INFO.txt";
    #endregion


    #region .ctor

    /// <summary>
    /// Creates an instance of BuildInformation class for framework
    /// </summary>
    private BuildInformation() : this(null) { }


    /// <summary>
    /// Creates and instance of BuildInformation class from the specified resource path in particular assembly.
    /// If assembly is null then BuildInformation for the whole framework is returned.
    /// If Path is null then the first found BUILD info resource is used from the specified assembly
    /// </summary>
    public BuildInformation(Assembly assembly, string path = null, bool throwError = true)
    {
      if (assembly == null)
      {
        assembly = Assembly.GetExecutingAssembly();
        path = FRAMEWORK_BUILD_INFO_PATH;
      }

      if (path.IsNullOrWhiteSpace())
      {
        path = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(BUILD_INFO_RESOURCE));
        if (path.IsNullOrWhiteSpace())
          path = FRAMEWORK_BUILD_INFO_PATH;
      }

      try
      {
        load(assembly, path);
      }
      catch (Exception error)
      {
        if (throwError)
          throw new ConfigException(StringConsts.BUILD_INFO_READ_ERROR + error.Message, error);
      }
    }

    /// <summary>
    /// Allocates object from textual content
    /// </summary>
    public BuildInformation(string content)
    {
      m_Content = content.NonBlank(nameof(content));
      loadContent();
    }

    #endregion


    #region Fields

    private static BuildInformation m_ForFramework;

    private string m_Content;
    private string m_AssemblyName;
    private int m_BuildSeed;
    private string m_Computer;
    private string m_User;
    private string m_OS;
    private string m_OSVer;
    private DateTime m_DateStampUTC;

    #endregion


    #region Properties

    /// <summary>
    /// Return framework build information
    /// </summary>
    public static BuildInformation ForFramework
    {
      get
      {
        if (m_ForFramework == null)
          m_ForFramework = new BuildInformation();

        return m_ForFramework;
      }
    }

    /// <summary>
    /// Returns full content of the build information object as string
    /// </summary>
    public string Content => m_Content;

    /// <summary>
    /// Returns assembly name that this information was obtained from
    /// </summary>
    public string AssemblyName => m_AssemblyName;

    /// <summary>
    /// Returns random number assigned to a build. It is NOT guaranteed to be unique
    /// </summary>
    public int BuildSeed => m_BuildSeed;

    /// <summary>
    /// A name of the computer that performed build
    /// </summary>
    public string Computer => m_Computer ?? string.Empty;

    /// <summary>
    /// a name of user that build session was logged under
    /// </summary>
    public string User => m_User ?? string.Empty;

    /// <summary>
    /// OS name
    /// </summary>
    public string OS => m_OS ?? string.Empty;

    /// <summary>
    /// OS version name
    /// </summary>
    public string OSVer => m_OSVer ?? string.Empty;

    /// <summary>
    /// Date and time stamp when build was performed
    /// </summary>
    public DateTime DateStampUTC => m_DateStampUTC;
    #endregion


    #region Public

    /// <inheritdoc/>
    public override string ToString()
    {
      return "[{0}] by {1}@{2}:{3} on {4} UTC".Args(BuildSeed, User, Computer, OS, DateStampUTC);
    }

    #endregion


    #region .pvt .impl

    private void load(Assembly asmb, string path)
    {
      m_AssemblyName = asmb.FullName;

      using(var stream = asmb.GetManifestResourceStream(path))
        using(var reader = new StreamReader(stream))
          m_Content = reader.ReadToEnd();

      loadContent();
    }

    private void loadContent()
    {
      try
      {
        var lines = m_Content.SplitLines();
        var i = -1;
        if (++i == lines.Length) return; m_BuildSeed    =  val(lines[i]).AsInt(0);
        if (++i == lines.Length) return; m_Computer     =  val(lines[i]);
        if (++i == lines.Length) return; m_User         =  val(lines[i]);
        if (++i == lines.Length) return; m_OS           =  val(lines[i]);
        if (++i == lines.Length) return; m_OSVer        =  val(lines[i]);
        if (++i == lines.Length) return; m_DateStampUTC =  val(lines[i]).AsDateTime(DateTime.MinValue, DateTimeStyles.AssumeUniversal);
      }
      catch(Exception error)
      {
        throw new ConfigException("Bad build info content: {0}".Args(error.ToMessageWithType()), error);
      }
    }

    private string val(string line)
    {
      if (string.IsNullOrEmpty(line)) return string.Empty;
      int i = line.IndexOf('=');
      if (i < 0) return line.Trim();
      return line.Substring(i + 1).Trim();
    }
    #endregion
  }
}
