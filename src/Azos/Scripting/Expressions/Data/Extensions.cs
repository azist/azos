/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using Azos.Conf;
using Azos.Data;

namespace Azos.Scripting.Expressions.Data
{
  /// <summary>
  /// Provides extensions to run scripts
  /// </summary>
  public static class ScriptingExtensions
  {
    public const string METADATA_SCRIPTS_SECTION = "scripts";

    /// <summary>
    /// Runs script in the specified context yielding object result
    /// </summary>
    /// <param name="ctx">Instance of `ScriptCtx` (or derivative)</param>
    /// <param name="attr">TargetedAttribute to get metadata from. If there is no metadata then `(false, null)` is returned</param>
    /// <param name="scriptName">The name of script to call</param>
    /// <returns>A tuple of `(bool: if script was found and ran, object: result)`</returns>
    public static (bool found, object result) RunScript(this ScriptCtx ctx, TargetedAttribute attr, string scriptName)
    {
      var metadata = attr.NonNull(nameof(attr)).Metadata;
      if (metadata == null) return (false, null);
      return ctx.RunScript(metadata, scriptName);
    }

    /// <summary>
    /// Runs script in the specified context yielding object result
    /// </summary>
    /// <param name="ctx">Instance of `ScriptCtx` (or derivative)</param>
    /// <param name="metadata">Metadata content which you can obtain from `TargetedAttribute`</param>
    /// <param name="scriptName">The name of script to call</param>
    /// <returns>A tuple of `(bool: if script was found and ran, object: result)`</returns>
    public static (bool found, object result) RunScript(this ScriptCtx ctx, IConfigSectionNode metadata, string scriptName)
      => ctx.RunScript(metadata.NonNull(nameof(metadata))[METADATA_SCRIPTS_SECTION][scriptName.NonBlank(nameof(scriptName))]);

    /// <summary>
    /// Runs script in the specified context yielding object result
    /// </summary>
    /// <param name="ctx">Instance of `ScriptCtx` (or derivative)</param>
    /// <param name="script">Script content</param>
    /// <exception cref="ScriptingException">Thrown when script execution fails</exception>
    /// <returns>A tuple of `(bool: if script was found and ran, object: result)`</returns>
    public static (bool found, object result) RunScript(this ScriptCtx ctx, IConfigSectionNode script)
    {
      ctx.NonNull(nameof(ctx));
      if (script == null || !script.Exists) return (false, null);//no script

      var tspOriginal = script.TypeSearchPaths;
      try
      {
        ((ConfigSectionNode)script).TypeSearchPaths = tspOriginal == null ? ctx.TypeSearchPaths : tspOriginal.Concat(ctx.TypeSearchPaths);
        var expr = FactoryUtils.MakeAndConfigure<Expression>(script);
        var result = expr.EvaluateObject(ctx);
        return (true, result);
      }
      catch(Exception cause)
      {
        throw new ScriptingException("RunScript failed on `{0}`. Error: {1}".Args(script.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact),
                                                                                  cause.ToMessageWithType()), cause);
      }
      finally
      {
        ((ConfigSectionNode)script).TypeSearchPaths = tspOriginal;
      }
    }
  }
}
