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
using Azos.Text;

namespace Azos.Scripting.Packaging.Dsl
{
  /// <summary>
  /// Sets target name
  /// </summary>
  public sealed class AddFiles : PackageStepBase
  {
    public AddFiles(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx) { }
    [Config] public string FromPath { get; set; }

    [Config] public string IncludePatterns { get; set; }
    [Config] public string ExcludePatterns { get; set; }

    [Config] public int ChunkSize { get; set; }

    private static readonly char[] PATTERN_DELIMS = new [] {',',';'};

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var fromPath = Eval(FromPath, state);
      if (fromPath.IsNotNullOrWhiteSpace() || fromPath == ".") fromPath = Directory.GetCurrentDirectory();

      var pat = Eval(IncludePatterns, state);
      if (pat.IsNullOrWhiteSpace()) pat = "*";
      var includes = pat.Split(PATTERN_DELIMS).Where(p => p.IsNotNullOrWhiteSpace());

      pat = Eval(ExcludePatterns, state);
      if (pat.IsNullOrWhiteSpace()) pat = null;
      var excludes = pat?.Split(PATTERN_DELIMS).Where(p => p.IsNotNullOrWhiteSpace());


      var fNames = fromPath.AllFileNames(false);
      if (Verbosity > 0)
      {
        Conout.WriteLine();
        Conout.WriteLine("Adding `{0}` files from `{1}`".Args(includes.Aggregate("", (all,e) => $"{all}+{e}") +
                                                              excludes?.Aggregate(" excluding ", (all, e) => $"{all}~{e}"),
                                                              fromPath));
      }
      foreach (var one in fNames)
      {
        var fn = Path.GetFileName(one);

        //Filter out files which do not match
        if (!includes.Any(ione => fn.MatchPattern(ione, senseCase: true))) continue;
        if (excludes != null && excludes.Any(eone => fn.MatchPattern(eone, senseCase: true))) continue;


        if (Verbosity > 1) Conout.WriteLine("  ┌─ Add: " + fn);
        addOneFile(one);
      }
      if (Verbosity > 0)
      {
        Conout.WriteLine("..done adding files; Total {0} commands, archive size: 0x{1:x8} / {2}".Args(Builder.Appender.TotalEntriesAppended,
                                                                                                      Builder.Appender.TotalBytesAppended,
                                                                                                      IOUtils.FormatByteSizeWithPrefix(Builder.Appender.TotalBytesAppended)));
      }

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
        int i;
        for(i = 0; App.Active && Runner.IsRunning; i++)
        {
          var got = fs.Read(buffer, 0, buffer.Length);
          if (got < 1) break;//eof


          var chunk = new FileChunkCommand
          {
            Description = "chunk #{0} at 0x{1:x8} / {2}".Args(i, offset, IOUtils.FormatByteSizeWithPrefix(offset)),
            Offset = offset,
            Data = buffer.Take(got).ToArray()
          };

          if (Verbosity > 2 && i > 0 && (i % 5) == 0) Conout.WriteLine("  ├──» " + chunk.Description);

          Builder.Appender.Append(chunk);
          offset += got;
        }//for
        if (Verbosity > 2) Conout.WriteLine("  └─ done {0} chunks at 0x{1:x8} / {2}".Args(i, offset, IOUtils.FormatByteSizeWithPrefix(offset)));
      }
    }
  }
}
