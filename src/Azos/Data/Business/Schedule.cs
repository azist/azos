/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Azos.Time;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Conf;

namespace Azos.Data.Business
{
  /// <summary>
  /// Represents a list of named date spans with week hours of operation
  /// </summary>
  [Bix("3086149f-ea42-415f-a688-a6c9bb249daa")]
  [Schema(Description = "A schedule is a list of date spans, each defining work hours and overrides (such as holidays)")]
  public class Schedule : TransientModel
  {
    [Bix("3d399e8b-6cb4-4384-b204-44c951d829a6")]
    [Schema(Description = "Defines a part of schedule - a named span of time")]
    public sealed class Span : FragmentModel
    {
      [Config, Field(description: "NLS name for the date span")]
      public NLSMap Name { get; set; }

      [Config, Field(description: "The date range of this span")]
      public DateRange Range { get; set; }

      [Config, Field] public HourList Monday    { get; set; }
      [Config, Field] public HourList Tuesday   { get; set; }
      [Config, Field] public HourList Wednesday { get; set; }
      [Config, Field] public HourList Thursday  { get; set; }
      [Config, Field] public HourList Friday    { get; set; }
      [Config, Field] public HourList Saturday  { get; set; }
      [Config, Field] public HourList Sunday    { get; set; }

      public HourList this[DayOfWeek day]
      {
        get => (HourList)this[day.ToString()];
        set => this[day.ToString()] = value;
      }
    }

    [Bix("342963e7-ad57-4e62-ab21-4267c6d1bc4a")]
    [Schema(Description = "Provides an overridden schedule for a specific day")]
    public sealed class DayOverride : FragmentModel
    {
      [Config, Field(description: "Nls name of the day")]
      public NLSMap Name { get; set; }

      [Config, Field(description: "The date of the day")]
      public DateTime Date { get; set; }

      [Config, Field(description: "Hours during the day")]
      public HourList Hours { get; set; }
    }

    [Field(description: "List of schedules spans")]
    public List<Span> Spans { get; set; }

    [Field(description: "Override days, such as holidays")]
    public List<DayOverride> Overrides { get; set; }

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);
      Spans = new List<Span>(node.MakeAndConfigureChildrenOfSpecific<Span>());
      Overrides = new List<DayOverride>(node.MakeAndConfigureChildrenOfSpecific<DayOverride>("override"));
    }

    public override ConfigSectionNode PersistConfiguration(ConfigSectionNode parentNode, string name)
    {
      var result = base.PersistConfiguration(parentNode, name);
      Spans?.ForEach(span => span.PersistConfiguration(result, "span"));
      Overrides?.ForEach(over => over.PersistConfiguration(result, "override"));
      return result;
    }
  }
}
