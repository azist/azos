using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

using Azos;
using Azos.Conf;

namespace Agnivo
{
  public class SettingsManager
  {
    private const string WINDOW_CONFIG_NAME = "{0}.win.laconf";
    private const string CONFIG_WINDOWS_SECTION = "windows";
    private const string CONFIG_WINDOW_SECTION = "window";
    private const string CONFIG_WINDOW_NAME_ATTR = "name";

    private static readonly SettingsManager m_Instance = new SettingsManager();
    public static SettingsManager Instance { get { return m_Instance; } }

    private SettingsManager()
    {
    }

    private string m_SettingsConfigPath;
    private Configuration m_Config;

    public string SettingsConfigPath
    {
      get
      {
        if (m_SettingsConfigPath == null)
        {
          var configName = WINDOW_CONFIG_NAME.Args(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
          m_SettingsConfigPath = Path.GetFullPath(configName);
        }

        return m_SettingsConfigPath;
      }
    }

    public Configuration Config
    {
      get
      {
        if (m_Config == null)
        {
          if (!File.Exists(SettingsConfigPath))
            File.Create(SettingsConfigPath).Dispose();

          m_Config = Configuration.ProviderLoadFromFile(SettingsConfigPath);
        }

        return m_Config;
      }
    }

    public ConfigSectionNode WindowsSection
    {
      get
      {
        var result = Config.Root[CONFIG_WINDOWS_SECTION];
        if (result == null || !result.Exists)
          Config.Root.AddChildNode(CONFIG_WINDOWS_SECTION);

        return result;
      }
    }

    public ConfigSectionNode GetFormSettings(string name, bool autoCreate = true)
    {
      var node = WindowsSection.Children
                               .Where(c => c.IsSameName(CONFIG_WINDOW_SECTION) && c.IsSameNameAttr(name))
                               .FirstOrDefault();
      if (node == null && autoCreate)
      {
        node = WindowsSection.AddChildNode(CONFIG_WINDOW_SECTION);
        node.AttrByName(CONFIG_WINDOW_NAME_ATTR, true).Value = name;
      }

      return node;
    }

    public virtual void SaveToConfig(AgnivoFormBase form)
    {
      var wnode = GetFormSettings(form.Name);
      if (wnode == null || !wnode.Exists)
        wnode = WindowsSection.AddChildNode(form.Name);

      form.SaveConfiguration(wnode);
      Config.Save();
    }
  }
}
