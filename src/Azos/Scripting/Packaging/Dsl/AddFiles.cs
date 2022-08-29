/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Scripting.Dsl;
using Azos.Serialization.JSON;

namespace Azos.Scripting.Packaging.Dsl
{
  /// <summary>
  /// Sets target name
  /// </summary>
  public sealed class AddFiles : PackageStepBase
  {
    public AddFiles(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }
    [Config] public string FromPath { get; set; }

    [Config] public string Pattern { get; set; }

    [Config] public int ChunkSize { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var fromPath = Eval(FromPath, state);
      var pat = Eval(Pattern, state);

      var fNames = fromPath.AllFileNamesThatMatch(pat, false);

      if (Verbosity > 0) Conout.WriteLine("Adding `{0}` files from `{1}`".Args(pat, fromPath));
      foreach (var one in fNames)
      {
        if (Verbosity > 1) Conout.WriteLine("  Add: " + one);
        addOneFile(one);
      }
      if (Verbosity > 0) Conout.WriteLine("..done");

      return Task.FromResult<string>(null);
    }

    private void addOneFile(string path)
    {
      using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        var fn = Path.GetFileName(path);
        var cmd = new CreateFileCommand
        {
          Description = Description,
          FileName = fn
        };
        Builder.Appender.Append(cmd);

        var chunkSize = ChunkSize;

        if (chunkSize <= 0) chunkSize = Package.DEFAULT_CHUNK_FILE_SIZE_BYTES;
        chunkSize = chunkSize.KeepBetween(Package.MIN_CHUNK_FILE_SIZE_BYTES, Package.MAX_CHUNK_FILE_SIZE_BYTES);
        var buffer = new byte[chunkSize];

        long offset = 0;
        for(var i = 0; App.Active && Runner.IsRunning; i++)
        {
          var got = fs.Read(buffer, 0, buffer.Length);
          if (got < 1) break;//eof


          var chunk = new FileChunkCommand
          {
            Description = "{0} chunk #{1} at {2:x8}".Args(fn, i, offset),
            Offset = offset,
            Data = buffer.Take(got).ToArray()
          };

          if (Verbosity > 2 && (i % 5) == 0) Conout.WriteLine("  ...." + chunk.Description);

          Builder.Appender.Append(chunk);
          offset += got;
        }//for
        if (Verbosity > 2) Conout.WriteLine("  ....done at {0:x8}".Args(offset));
      }
    }
  }
}
