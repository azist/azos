using System;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Represents a base for events that happen so instrumentation can calculate event counts and rates of occurence
  /// </summary>
  public abstract class Event : Datum
  {
      protected Event() : base() {}

      protected Event(string source) : base(source) {}

      protected Event(string source, DateTime utcDateTime) : base(source, utcDateTime) {}

      public override object ValueAsObject { get { return Count; } }

      public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_OCCURENCE; } }
  }
}
