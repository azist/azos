/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.CodeAnalysis.Source;
using Azos.CodeAnalysis.Laconfig;

namespace Azos.Conf
{
  /// <summary>
  /// Provides implementation of configuration based on Laconic content format
  /// </summary>
  /// <example>
  /// Example Laconic Configuration Content:
  ///
  /// azos //comments are allowed
  /// {
  ///   log-root=$"c:\azos\"
  ///   log-csv="Azos.Log.Destinations.CSVFileDestination, Azos"
  ///   debug-default-action="Log,Throw"
  ///
  ///   log
  ///   {
  ///     name="Logger"
  ///
  ///     destination
  ///     {
  ///       type=$(/$log-csv)
  ///       name="WinFormsTest Log"//strings in dblquotes
  ///       path=$(/$log-root)
  ///       name-time-format='yyyyMMdd'//strings in snglquotes
  ///       generate-failover-msg=false
  ///     }
  ///   }
  ///   /* multiline comments
  ///    data-store {type="Azos.RecordModel.DataAccess.MongoDB.MongoDBModelDataStore, Azos.MongoDB"
  ///                connect-string="mongodb://localhost"
  ///                db-name="test"}
  ///   */
  /// }
  /// </example>
  [Serializable]
  public class LaconicConfiguration : FileConfiguration
  {
    #region .ctor / static

    /// <summary>
    /// Creates an instance of a new configuration not bound to any laconfig file
    /// </summary>
    public LaconicConfiguration() : base() {  }

    /// <summary>
    /// Creates an isntance of the new configuration and reads contents from a laconfig file
    /// </summary>
    public LaconicConfiguration(string filename) : base(filename)
    {
      readFromFile();
    }

    /// <summary>
    /// Creates an instance of configuration initialized from laconfig passed as string
    /// </summary>
    public static LaconicConfiguration CreateFromString(string content)
    {
      var result = new LaconicConfiguration();
      result.readFromString(content);

      return result;
    }

    #endregion


    #region Public

    /// <summary>
    /// Saves configuration into a file
    /// </summary>
    public override void SaveAs(string filename)
    {
      SaveAs(filename, null);
    }

    /// <summary>
    /// Saves configuration into a file
    /// </summary>
    public void SaveAs(string filename, LaconfigWritingOptions options = null)
    {
      using (var fs = new FileStream(filename, FileMode.Create))
        LaconfigWriter.Write(this, fs, options);
      base.SaveAs(filename);
    }

    /// <summary>
    /// Saves laconic configuration into string in Laconfig format and returns it
    /// </summary>
    public string SaveToString(LaconfigWritingOptions options = null)
    {
      return LaconfigWriter.Write(this, options);
    }

    /// <inheritdoc/>
    public override void Refresh()
    {
      readFromFile();
    }

    /// <inheritdoc/>
    public override void Save()
    {
      SaveAs(m_FileName);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return SaveToString();
    }

    #endregion


    #region Protected

    //for Laconfig we dont need to adjust names as the are properly escaped
    //protected override string AdjustNodeName(string name)
    //{

    //}

    #endregion


    #region .pvt

    private void readFromFile()
    {
      //20230324 DKh+Jpk wip on #845
      using (var fsrc = new FileSource(m_FileName, encoding: null, useBom: true))//auto detect encoding
      {
        read(fsrc);
      }
////      var content = File.ReadAllText(m_FileName);
////      readFromString(content);
    }

    private void readFromString(string content)
    {
      read(new StringSource(content));
    }

    private void read(ISourceText source)
    {
      var context = new LaconfigData(this);

      var lexer = new LaconfigLexer(source, throwErrors: true);
      var parser = new LaconfigParser(context, lexer, throwErrors: true);

      parser.Parse();
    }

    #endregion
  }
}
