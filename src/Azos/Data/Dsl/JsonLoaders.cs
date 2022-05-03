﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using Azos.Conf;
using Azos.Scripting;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Data.Dsl
{
  public class JsonObjectLoader : DataLoader<JsonObjectDataSource>
  {
    public JsonObjectLoader(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config]
    public string FileName { get; set; }

    [Config]
    public string Json { get; set; }

    [Config]
    public string IncludeFilePragma { get; set; }

    protected override IDataSource MakeDataSource(JsonDataMap state)
    {
      var fn = Eval(FileName, state);
      var json = Eval(Json, state);

      if (fn.IsNotNullOrWhiteSpace())
        return JsonObjectDataSource.FromFile(App, Name, fn, IncludeFilePragma);
      else
        return JsonObjectDataSource.FromJson(App, Name, json, IncludeFilePragma);
    }
  }

  public sealed class JsonObjectDataSource : DisposableObject, IDataSource<IJsonDataObject>
  {
    public static JsonObjectDataSource FromFile(IApplication app, string name, string fileName, string includeFilePragma)
     => new JsonObjectDataSource(app.NonNull(nameof(app)), name.NonBlank(nameof(name)), JsonReader.DeserializeDataObjectFromFile(fileName), includeFilePragma);

    public static JsonObjectDataSource FromJson(IApplication app, string name, string json, string includeFilePragma)
     => new JsonObjectDataSource(app.NonNull(nameof(app)), name.NonBlank(nameof(name)), JsonReader.DeserializeDataObject(json), includeFilePragma);

    private JsonObjectDataSource(IApplication app, string name, IJsonDataObject data, string includeFilePragma)
    {
      m_App = app;
      m_Name = name;
      m_Data = data;
      m_IncludeFilePragma = includeFilePragma;
      if (includeFilePragma.IsNotNullOrWhiteSpace())
      {
        m_Data = m_Data.ProcessJsonLocalFileIncludes(App, null, IncludeFilePragma);
      }
    }

    private IApplication m_App;
    private string m_Name;
    private IJsonDataObject m_Data;
    private string m_IncludeFilePragma;

    public IApplication App => m_App;
    public string Name => m_Name;
    public IJsonDataObject Data => m_Data;
    public object ObjectData => m_Data;
    public string IncludeFilePragma => m_IncludeFilePragma;
  }

  /// <summary>
  /// Reads the whole json content supplied either as a literal text or loaded from a file directly into a
  /// local or a global state var. A file name is evaluated using a template syntax e.g. `my_file{~local.number}.json`
  /// </summary>
  public sealed class ReadJson : Step
  {
    public ReadJson(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx)
    {
    }

    [Config]
    public string FileName { get; set; }

    [Config]
    public string Json { get; set; }

    [Config]
    public string Global { get; set; }

    [Config]
    public string Local { get; set; }

    [Config]
    public string IncludeFilePragma { get; set; }


    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      if (Local.IsNullOrWhiteSpace() && Global.IsNullOrWhiteSpace())
      {
        throw new RunnerException("{0} step requires at least either global or local assignment".Args(nameof(ReadJson)));
      }

      var fn = StepRunnerVarResolver.FormatString(Eval(FileName, state), Runner, state);
      var json = Eval(Json, state);


      if (fn.IsNotNullOrWhiteSpace() && json.IsNotNullOrWhiteSpace())
      {
        throw new RunnerException("{0} step has both literal json content and file name specified".Args(nameof(ReadJson)));
      }

      if (fn.IsNotNullOrWhiteSpace())
      {
        if (!System.IO.File.Exists(fn)) throw new RunnerException("State JSON File does not exist");
        json = System.IO.File.ReadAllText(fn);
      }

      var obj = JsonReader.DeserializeDataObject(json);

      if (IncludeFilePragma.IsNotNullOrWhiteSpace())
      {
        obj = obj.ProcessJsonLocalFileIncludes(App, null, IncludeFilePragma);
      }

      if (Global.IsNotNullOrWhiteSpace()) Runner.GlobalState[Global] = obj;

      if (Local.IsNotNullOrWhiteSpace()) state[Local] = obj;

      if (Global.IsNullOrWhiteSpace() && Local.IsNullOrWhiteSpace()) Runner.SetResult(obj);

      return Task.FromResult<string>(null);
    }
  }
}
