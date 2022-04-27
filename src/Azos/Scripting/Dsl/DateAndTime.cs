/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Threading.Tasks;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Scripting.Dsl
{
  /// <summary>
  ///Returns APP container TimeSource.NOW/UTC NOW
  /// </summary>
  public sealed class Now : Step
  {
    public Now(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order)
    {
    }

    [Config] public string Utc { get; set; }
    [Config] public string Into { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var utc = Eval(Utc, state).AsBool();
      var into = Eval(Into, state);

      var result = utc ? App.TimeSource.UTCNow : App.TimeSource.Now;

      if (into.IsNotNullOrWhiteSpace())
        state[into] = result;
      else
        Runner.SetResult(result);

      return Task.FromResult<string>(null);
    }
  }


  /// <summary>
  ///Adds timepsan to datetime
  /// </summary>
  public sealed class AddTimespan : Step
  {
    public AddTimespan(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order)
    {
    }

    [Config] public string From { get; set; }
    [Config] public string Span { get; set; }
    [Config] public string Into { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var from = Eval(From, state).AsDateTime();
      var into = Eval(Into, state);
      var span = Eval(Span, state).AsTimeSpanOrThrow();

      var result = from.Add(span);

      if (into.IsNotNullOrWhiteSpace())
        state[into] = result;
      else
        Runner.SetResult(result);

      return Task.FromResult<string>(null);
    }
  }

  /// <summary>
  /// Converts a DateTime value represented in canonical form into a parsable tuple of (kind, ticks,....).
  /// If an input is a tuple then converts it back to DateTime.
  /// </summary>
  public sealed class ConvertDateTime : Step
  {
    public ConvertDateTime(StepRunner runner, IConfigSectionNode cfg, int order) : base(runner, cfg, order)
    {
    }

    [Config] public string From { get; set; }
    [Config] public string Into { get; set; }

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var from = Eval(From, state);
      var into = Eval(Into, state);

      object val = null;
      if (from.IsNotNullOrWhiteSpace())
      {
        if (DateTime.TryParse(from, out var dt))
        {
          val = new { kind = dt.Kind,
                      ticks = dt.Ticks,
                      time_of_day = dt.TimeOfDay,
                      date = dt.Date,
                      year = dt.Year,
                      month = dt.Month,
                      hour = dt.Hour,
                      day = dt.Day,
                      day_of_year = dt.DayOfYear,
                      day_of_week = dt.DayOfWeek,
                      minute = dt.Minute,
                      second = dt.Second,
                      millisecond =  dt.Millisecond }.ToJson(JsonWritingOptions.CompactRowsAsMap);
        }
        else
        {
          var map = JsonReader.DeserializeDataObject(from) as JsonDataMap;
          map.NonNull("Json representation of DateTime (ticks, kind)");
          val = new DateTime(map["ticks"].AsLong(), map["kind"].AsString().AsEnum<DateTimeKind>());
        }
      }

      if (into.IsNotNullOrWhiteSpace())
      {
        state[into] = val;
      }

      return Task.FromResult<string>(null);
    }
  }

}
