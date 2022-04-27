/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using Azos.Conf;
using Azos.Scripting;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;
using Azos.Text;

namespace Azos.Data.Dsl
{
  public class JsonObjectLoader : DataLoader<JsonObjectDataSource>
  {
    public JsonObjectLoader(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    [Config]
    public string FileName { get; set; }

    [Config]
    public string Json { get; set; }

    protected override IDataSource MakeDataSource(JsonDataMap state)
    {
      var fn = Eval(FileName, state);
      var json = Eval(Json, state);

      if (fn.IsNotNullOrWhiteSpace())
        return JsonObjectDataSource.FromFile(Name, fn);
      else
        return JsonObjectDataSource.FromJson(Name, json);
    }
  }

  public sealed class JsonObjectDataSource : DisposableObject, IDataSource<IJsonDataObject>
  {
    public static JsonObjectDataSource FromFile(string name, string fileName)
     => new JsonObjectDataSource(name.NonBlank(nameof(name)), JsonReader.DeserializeDataObjectFromFile(fileName));

    public static JsonObjectDataSource FromJson(string name, string json)
     => new JsonObjectDataSource(name.NonBlank(nameof(name)), JsonReader.DeserializeDataObject(json));

    private JsonObjectDataSource(string name, IJsonDataObject data)
    {
      m_Name = name;
      m_Data = data;
    }

    private string m_Name;
    private IJsonDataObject m_Data;

    public string Name => m_Name;
    public IJsonDataObject Data => m_Data;
    public object ObjectData => m_Data;
  }

  /// <summary>
  /// Sets a global or local value to the specified expression
  /// </summary>
  /// <example><code>
  ///  do{ type="Set" global=a to='((x * global.a) / global.b) + 23'}
  ///  do{ type="Set" local=x to='x+1' name="inc x"}
  /// </code></example>
  public sealed class JsonStateLoader : Step
  {
    public const string CONFIG_GLOBAL_ATTR = "global";
    public const string CONFIG_LOCAL_ATTR = "local";

    [Config]
    public string FileName { get; set; }

    [Config]
    public string Json { get; set; }

    public JsonStateLoader(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx)
    {
      m_Local = cfg.ValOf(CONFIG_LOCAL_ATTR);
      m_Global = cfg.ValOf(CONFIG_GLOBAL_ATTR);
      if (m_Local.IsNullOrWhiteSpace() && m_Global.IsNullOrWhiteSpace())
      {
        throw new RunnerException("JsonStateLoader step requires at least either global or local assignment");
      }
    }

    private string m_Global;
    private string m_Local;

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var fn = StepRunnerVarResolver.FormatString(Eval(FileName, state), Runner, state);
      if (fn.IsNotNullOrWhiteSpace())
      {
        if (!System.IO.File.Exists(fn)) throw new RunnerException("State JSON File does not exist");
        Json = System.IO.File.ReadAllText(fn);
      }

      var json = JsonReader.DeserializeDataObject(Json) as JsonDataMap;
      if (json == null) throw new RunnerException("JSON State does not contain a valid JSON map");
      if (m_Global.IsNotNullOrWhiteSpace()) Runner.GlobalState[m_Global] = json;
      if (m_Local.IsNotNullOrWhiteSpace()) state[m_Local] = json;

      return Task.FromResult<string>(null);
    }
  }
}
