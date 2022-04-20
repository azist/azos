/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading.Tasks;

using Azos.Collections;
using Azos.Conf;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Data.Dsl
{
  /// <summary>
  /// Facilitates the loading of TDataStream into a call frame
  /// </summary>
  public abstract class DataLoader<TData> : Step where TData : class, IDataSource, INamed
  {
    /// <summary>
    /// Tries to find a data stream of a specified type with optional name on a call stack of frames.
    /// Returns null if such data stream is not found
    /// </summary>
    public static TData TryGet(string name = null)
     => StepRunner.Frame.Current?.All.FirstOrDefault(o => (o is TData d) && (name.IsNullOrWhiteSpace() || d.Name.EqualsOrdIgnoreCase(name))) as TData;


    /// <summary>
    /// Tries to find a data stream of a specified type with optional name on a call stack of frames.
    /// Throws if such data stream is not found and dependency could not be satisfied
    /// </summary>
    public static TData Get(string name = null)
     => TryGet(name).NonNull("Satisfied data stream dependency on `{0}('{1}')` loaded by `{2}` step".Args(
                             typeof(TData).DisplayNameWithExpandedGenericArgs(),
                             name.Default("<null>"),
                             nameof(DataLoader<TData>)));


    protected DataLoader(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }


    protected abstract IDataSource MakeDataSource(JsonDataMap state);

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var stream = MakeDataSource(state);
      StepRunner.Frame.Current.Owned.Add(stream);
      return Task.FromResult<string>(null);
    }
  }

  public interface IDataSource : IDisposable, INamed { }

}
