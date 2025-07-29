/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Provides IInstrumentation implementation that does nothing
  /// </summary>
  public sealed class NOPInstrumentation : ApplicationComponent, IInstrumentationImplementation
  {
    internal NOPInstrumentation(IApplication app) : base(app)
    {

    }


    public bool InstrumentationEnabled { get { return false; } set { } }

    public override string ComponentLogTopic => CoreConsts.INSTRUMENTATION_TOPIC;

    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters { get { return null; } }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups) { return null; }

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      value = null;
      return false;
    }

    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return false;
    }

    public bool Enabled
    {
      get { return false; }
    }

    public bool Overflown
    {
      get { return true; }
    }

    public int RecordCount
    {
      get { return 0; }
    }

    public int ProcessingIntervalMS
    {
      get { return 0; }
    }

    public int OSInstrumentationIntervalMS
    {
      get { return 0; }
    }

    public bool SelfInstrumented
    {
      get { return false; }
    }

    public int MaxRecordCount
    {
      get { return 0; }
      set { }
    }

    public IEnumerable<Type> DataTypes { get { return Enumerable.Empty<Type>(); } }

    public void Record(Datum datum)
    {
      //does nothing
    }

    public IEnumerable<HASKey> GetDatumTypeSources(Type datumType, out Datum defaultInstance)
    {
      defaultInstance = null;
      return Enumerable.Empty<HASKey>();
    }

    public void Configure(Conf.IConfigSectionNode node)
    {

    }

    public Time.TimeLocation TimeLocation
    {
      get { return App.TimeLocation; }
    }

    public DateTime LocalizedTime
    {
      get { return App.LocalizedTime; }
    }

    public DateTime UniversalTimeToLocalizedTime(DateTime utc)
    {
      return App.UniversalTimeToLocalizedTime(utc);
    }

    public DateTime LocalizedTimeToUniversalTime(DateTime local)
    {
      return App.LocalizedTimeToUniversalTime(local);
    }

    public int ResultBufferSize
    {
      get { return 0; }
    }

    public IEnumerable<Datum> GetBufferedResults(int count = 0)
    {
      return Enumerable.Empty<Datum>();
    }

    public IEnumerable<Datum> GetBufferedResultsSince(DateTime utcDate)
    {
      return Enumerable.Empty<Datum>();
    }
  }
}
