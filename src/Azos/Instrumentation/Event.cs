/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Represents a base for events that happen. Events do not have a measurable value of their own, instead instrumentation
  /// calculates event counts and rates of their occurrence
  /// </summary>
  public abstract class Event : Datum
  {
    protected Event() : base() {}

    protected Event(string source) : base(source) {}

    protected Event(string source, DateTime utcDateTime) : base(source, utcDateTime) {}

    public override object ValueAsObject => Count;

    public override double? RefValue => Count;

    public override string ValueUnitName => CoreConsts.UNIT_NAME_OCCURENCE;
  }
}
