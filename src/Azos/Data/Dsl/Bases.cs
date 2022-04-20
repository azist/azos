/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

using Azos.Collections;
using Azos.Conf;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Data.Dsl
{
  public abstract class DataLoader : Step
  {
    /// <summary>
    /// Tries to find a IDataSource of a specified name on a call stack of frames.
    /// Returns null if such data source is not found
    /// </summary>
    public static IDataSource TryGet(string name)
     => StepRunner.Frame.Current?.All.FirstOrDefault(o => (o is IDataSource d) && d.Name.EqualsOrdIgnoreCase(name.NonBlank(nameof(name)))) as IDataSource;

    /// <summary>
    /// Tries to find a IDataSource of a specified TData type with optional name on a call stack of frames.
    /// Returns null if such data source is not found
    /// </summary>
    public static IDataSource<TData> TryGet<TData>(string name = null)
     => StepRunner.Frame.Current?.All.FirstOrDefault(o => (o is IDataSource<TData> d) && (name.IsNullOrWhiteSpace() || d.Name.EqualsOrdIgnoreCase(name))) as IDataSource<TData>;

    /// <summary>
    /// Tries to find a IDataSource of a specified TData type with optional name on a call stack of frames.
    /// Throws if such data source is not found and dependency could not be satisfied
    /// </summary>
    public static IDataSource Get(string name)
     => TryGet(name).NonNull("Satisfied IDataSource dependency on `IDataSource('{0}')` loaded by `{1}` step".Args(
                             name.Default("<null>"),
                             nameof(DataLoader)));

    /// <summary>
    /// Tries to find a IDataSource of a specified TData type with optional name on a call stack of frames.
    /// Throws if such data source is not found and dependency could not be satisfied
    /// </summary>
    public static IDataSource<TData> Get<TData>(string name = null)
     => TryGet<TData>(name).NonNull("Satisfied IDataSource dependency on `{0}('{1}')` loaded by `{2}` step".Args(
                             typeof(TData).DisplayNameWithExpandedGenericArgs(),
                             name.Default("<null>"),
                             nameof(DataLoader)));


    protected DataLoader(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }
  }


  /// <summary>
  /// Facilitates the loading of TDataStream into a call frame
  /// </summary>
  public abstract class DataLoader<TDataSource> : DataLoader where TDataSource : class, IDataSource, INamed
  {
    protected DataLoader(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }

    protected abstract IDataSource MakeDataSource(JsonDataMap state);

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var stream = MakeDataSource(state);
      StepRunner.Frame.Current.Owned.Add(stream);
      return Task.FromResult<string>(null);
    }
  }

  public interface IDataSource : IDisposable, INamed
  {
    object ObjectData { get; }
  }

  public interface IDataSource<TData> : IDataSource
  {
    TData Data { get; }
  }


  /// <summary>
  /// Iterates through an enumerable object in named data source
  /// </summary>
  public sealed class ForEachData : Step
  {
    public const string CONFIG_BODY_SECTION = "body";

    public ForEachData(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx)
    {
      var nSource = cfg[CONFIG_BODY_SECTION].NonEmpty();
      m_Body = FactoryUtils.Make<StepRunner>(nSource, typeof(StepRunner), new object[] { runner.App, nSource, runner.GlobalState });
    }

    private StepRunner m_Body;

    [Config]
    public string DataSourceName { get; set;}

    protected override async Task<string> DoRunAsync(JsonDataMap state)
    {
      var dsn = Eval(DataSourceName, state);
      var source = DataLoader.Get(dsn);
      var enumerable = source.ObjectData as IEnumerable;

      enumerable.NonNull("Enumerable data");

      foreach(var o in enumerable)
      {
        m_Body.SetResult(o);
        await m_Body.RunAsync().ConfigureAwait(false);
      }
      return null;
    }
  }

}
