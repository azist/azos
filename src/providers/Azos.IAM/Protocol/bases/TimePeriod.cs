using System;


using Azos.Data;
using Azos.Data.Business;

namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Represents a period - a span of time. Both dates are required, use min/max date values for open spans
  /// </summary>
  public sealed class TimePeriod : FragmentModel
  {
    [Field(required: true,
           minLength: Sizes.NAME_MIN,
           maxLength: Sizes.NAME_MAX,
           description: "Provides a short name for this period of time, e.g. 'Christmas 2018'")]
    public string Name { get; set; }

    [Field(required: true, description: "Period start timestamp in UTC")]
    public DateTime? SD { get; set; }

    [Field(required: true, description: "Period end timestamp in UTC")]
    public DateTime? ED { get; set; }
  }
}
