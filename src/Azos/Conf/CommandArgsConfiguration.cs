/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Conf
{
  /// <summary>
  /// Provides implementation of configuration based on arguments supplied from command line
  ///  which is "string[]". Arguments start with either "/" or "-" prefix. If any argument is not
  ///  prefixed then it is written as an auto-named attribute node of the root with its value set, otherwise a section (under root) with
  ///   argument's name is created. Any argument may have options. Any option may either consist of name
  ///    or name value pair delimited by "=".
  ///  Argument options are written as attribute nodes of their corresponding sections.
  ///  If option value specified without name (without "=") then option is auto-named
  /// </summary>
  /// <example>
  ///  Given command line:
  ///   <code>
  ///   c:\>dosomething.exe "c:\input.file" "d:\output.file" -compress level=100 method=zip -shadow fast -large
  ///   </code>
  ///  The following configuration object will be created from the supplied args:
  ///  <code>
  ///    [args ?1="c:\input.file" ?2="c:\output.file"]
  ///      [compress level="100" method="zip"]
  ///      [shadow ?1="fast"]
  ///      [large]
  ///  </code>
  ///
  ///  Use args:
  ///  <code>
  ///   var conf = new CmdArgsConfiguration(args);
  ///   var inFile = conf.Root.AttrByIndex(0).ValueAsString(DEFAULT_INPUT_FILE);
  ///   var outFile = conf.Root.AttrByIndex(1).ValueAsString(DEFAULT_OUTPUT_FILE);
  ///   .....
  ///    if (conf.Root["large"].Exists) .......
  ///   .....
  ///   var level = conf.Root["compress"].AttrByName("level").ValueAsInt(DEFAULT_COMPRESSION_LEVEL);
  ///   .....
  ///  </code>
  ///
  /// </example>
  [Serializable]
  public class CommandArgsConfiguration : Configuration
  {
    #region CONSTS

    public const string ARG_PREFIX_SLASH = "/";
    public const string ARG_PREFIX_DASH = "-";
    public const char OPTION_EQ = '=';
    public const string ROOT_NODE_NAME = "args";

    #endregion

    #region .ctor

    /// <summary>
    /// Creates an instance of the new configuration parsed from command line arguments
    /// </summary>
    public CommandArgsConfiguration(string[] args) : this(args, Platform.Computer.OSFamily != Platform.OSFamily.Windows)
    {
    }

    /// <summary>
    /// Creates an instance of the new configuration parsed from command line arguments
    /// </summary>
    public CommandArgsConfiguration(string[] args, bool inhibitSlashArg) : base()
    {
      m_Args = args;
      m_InhibitSlashArg = inhibitSlashArg;
      parseArgs();
      m_Loaded = true;
    }

    #endregion


    #region Private Fields

    private bool m_InhibitSlashArg;
    private bool m_Loaded = false;
    private string[] m_Args;

    #endregion


    #region Public Properties

    /// <summary>
    /// Indicates whether configuration is readonly or may be modified and saved
    /// </summary>
    public override bool IsReadOnly => m_Loaded;

    /// <summary>
    /// When true, disregards '/' as an argument delimiter
    /// </summary>
    public bool InhibitSlashArg => m_InhibitSlashArg;

    /// <summary>
    /// Returns arguments array that this configuration was parsed from
    /// </summary>
    public string[] Arguments => m_Args;

    #endregion


    #region .pvt .impl

    // -arg1 -arg2 -arg3 opt1 opt2 -arg4 optA=v1 optB=v2
    private void parseArgs()
    {

      m_Root = new ConfigSectionNode(this, null, ROOT_NODE_NAME, string.Empty);

      var uargcnt = 1; //unknown arg length
      for (int i = 0; i < m_Args.Length;)
      {
        var argument = m_Args[i];

        if (argument.Length > 1 && ((!m_InhibitSlashArg && argument.StartsWith(ARG_PREFIX_SLASH)) || argument.StartsWith(ARG_PREFIX_DASH)))
        {
          argument = argument.Remove(0, 1);//get rid of prefix
          var argNode = m_Root.AddChildNode(argument, null);

          var uopcnt = 1;//unknown option length
          for (i++; i < m_Args.Length;)//read args's options
          {
            var option = m_Args[i];
            if ((!m_InhibitSlashArg && option.StartsWith(ARG_PREFIX_SLASH)) || option.StartsWith(ARG_PREFIX_DASH)) break;
            i++;

            var j = option.IndexOf(OPTION_EQ);

            if (j < 0)
            {
              argNode.AddAttributeNode(string.Format("?{0}", uopcnt), option);
              uopcnt++;
            }
            else
            {
              var name = option.Substring(0, j);
              var val = (j < option.Length - 1) ? option.Substring(j + 1) : string.Empty;

              if (string.IsNullOrEmpty(name))
              {
                name = string.Format("?{0}", uopcnt);
                uopcnt++;
              }
              argNode.AddAttributeNode(name, val);
            }
          }
        }
        else
        {
          m_Root.AddAttributeNode(string.Format("?{0}", uargcnt), argument);
          uargcnt++;
          i++;
        }
      }

      m_Root.ResetModified();
    }

    #endregion
  }
}
