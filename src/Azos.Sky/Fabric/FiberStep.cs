/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Describes the execution step of the Fiber, such is the next step
  /// </summary>
  public struct FiberStep
  {
    public const string CONVENTION_STEP_METHOD_NAME_PREFIX = "Step_";

    public static FiberStep ContinueImmediately(Atom step) => ContinueAfter(step, TimeSpan.Zero);
    public static FiberStep ContinueAfter(Atom step, TimeSpan interval)
     => new FiberStep(step.HasRequiredValue(nameof(step)).AsValid(nameof(step)), interval, null);

    public static FiberStep ContinueImmediately(Func<Task<FiberStep>> stepBody) => ContinueAfter(stepBody, TimeSpan.Zero);
    public static FiberStep ContinueAfter(Func<Task<FiberStep>> stepBody, TimeSpan interval)
    {
      var fiber = stepBody.NonNull(nameof(stepBody)).Target.CastTo<Fiber>("Step body Method of `Fiber` class");

      var mn = stepBody.Method.Name;
      var i = mn.IndexOf(CONVENTION_STEP_METHOD_NAME_PREFIX);

      (i==0 && i < mn.Length-1).IsTrue($"Step body method name must start with `{CONVENTION_STEP_METHOD_NAME_PREFIX}`");

      var stepName = mn.Substring(CONVENTION_STEP_METHOD_NAME_PREFIX.Length);

      Atom.TryEncode(stepName, out var step).IsTrue("Atom step name");

      return ContinueAfter(step, interval);
    }


    /// <summary>
    /// Indicates successful execution completion without any <see cref="FiberResult"/>
    /// </summary>
    public static FiberStep Finish() => new FiberStep(Atom.ZERO, TimeSpan.Zero, null);

    /// <summary>
    /// Indicates successful execution completion with the specified <see cref="FiberResult"/>
    /// </summary>
    public static FiberStep FinishWithResult(FiberResult result)
     => new FiberStep(Atom.ZERO, TimeSpan.Zero, result.NonNull(nameof(result)));



    private FiberStep(Atom step, TimeSpan span, FiberResult result)
    {
      NextStep = step;
      NextSliceInterval = span;
      Result = result;
    }

    /// <summary>
    /// What is next. The Zero == Finish
    /// </summary>
    public readonly Atom NextStep;

    /// <summary>
    /// When
    /// </summary>
    public readonly TimeSpan NextSliceInterval;

    /// <summary>
    /// For final steps NextStep is Zero and this set to represent final step result
    /// or null for interim (non-final) steps
    /// </summary>
    public readonly FiberResult Result;


    /// <summary>
    /// True if this step is final - the last one.
    /// You may want to check Result for value (if any)
    /// </summary>
    public bool IsFinal => NextStep.IsZero;
  }
}
