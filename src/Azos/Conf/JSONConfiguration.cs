
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Serialization.JSON;

namespace Azos.Conf
{
  /// <summary>
  /// Provides implementation of configuration based on a classic JSON content
  /// </summary>
  [Serializable]
  public class JSONConfiguration : FileConfiguration
  {
    #region CONSTS

      public const string SECTION_VALUE_ATTR = "-section-value";


    #endregion


    #region .ctor / static

      /// <summary>
      /// Creates an instance of a new configuration not bound to any JSON file
      /// </summary>
      public JSONConfiguration() : base()
      {

      }

      /// <summary>
      /// Creates an isntance of the new configuration and reads contents from a JSON file
      /// </summary>
      public JSONConfiguration(string filename) : base(filename)
      {
        readFromFile();
      }

      /// <summary>
      /// Creates an instance of configuration initialized from JSON content passed as string
      /// </summary>
      public static JSONConfiguration CreateFromJSON(string content)
      {
        var result = new JSONConfiguration();
        result.readFromString(content);

        return result;
      }


    #endregion


    #region Public Properties

    #endregion


    #region Public

      /// <summary>
      /// Saves configuration into a JSON file
      /// </summary>
      public override void SaveAs(string filename)
      {
        SaveAs(filename, null, null);

        base.SaveAs(filename);
      }

      /// <summary>
      /// Saves configuration into a JSON file
      /// </summary>
      public void SaveAs(string filename, JSONWritingOptions options = null, Encoding encoding = null)
      {
        var data = ToConfigurationJSONDataMap();
        if (options==null) options = JSONWritingOptions.PrettyPrint;
        JSONWriter.WriteToFile(data, filename, options, encoding);

        base.SaveAs(filename);
      }

      /// <summary>
      /// Saves JSON configuration to string
      /// </summary>
      public string SaveToString(JSONWritingOptions options = null)
      {
        var data = ToConfigurationJSONDataMap();
        return JSONWriter.Write(data, options);
      }


      public override void Refresh()
      {
        readFromFile();
      }


      public override void Save()
      {
        SaveAs(m_FileName);
      }

      public override string ToString()
      {
        return SaveToString();
      }

    #endregion

    #region Private Utils

        private void readFromFile()
        {
          var data = JSONReader.DeserializeDataObjectFromFile(m_FileName, caseSensitiveMaps: false) as JSONDataMap;
          read(data);
        }

        private void readFromString(string content)
        {
          var data = JSONReader.DeserializeDataObject(content, caseSensitiveMaps: false) as JSONDataMap;
          read(data);
        }

        private void read(JSONDataMap data)
        {
          if (data==null || data.Count==0 || data.Count>1)
            throw new ConfigException(StringConsts.CONFIG_JSON_MAP_ERROR);

          var root = data.First();
          var sect = root.Value as JSONDataMap;
          if (sect==null)
            throw new ConfigException(StringConsts.CONFIG_JSON_MAP_ERROR);

          m_Root = buildSection(root.Key, sect, null);
          m_Root.ResetModified();
        }


        private ConfigSectionNode buildSection(string name, JSONDataMap sectData, ConfigSectionNode parent)
        {
          var value = sectData[SECTION_VALUE_ATTR].AsString();
          ConfigSectionNode result = parent==null ? new ConfigSectionNode(this, null, name, value)
                                                  : parent.AddChildNode(name, value);

          foreach(var kvp in sectData)
          {
            if (kvp.Value is JSONDataMap)
              buildSection(kvp.Key, (JSONDataMap)kvp.Value, result);
            else if (kvp.Value is JSONDataArray)
            {
              var lst = (JSONDataArray)kvp.Value;
              foreach(var lnode in lst)
              {
                var lmap = lnode as JSONDataMap;
                if (lmap==null)
                  throw new ConfigException(StringConsts.CONFIG_JSON_STRUCTURE_ERROR, new ConfigException("Bad structure: "+sectData.ToJSON()));
                buildSection(kvp.Key, lmap, result);
              }
            }
            else
             result.AddAttributeNode(kvp.Key, kvp.Value);
          }

          return result;
        }

    #endregion
  }
}
