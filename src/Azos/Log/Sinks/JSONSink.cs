/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Text;

using Azos.Conf;
using Azos.Serialization.BSON;
using Azos.Serialization.JSON;

namespace Azos.Log.Sinks
{
  /// <summary>
  /// Provides detailed JSON-based logging format
  /// </summary>
  public sealed class JSONSink : TextFileSink
  {
    private const string CONFIG_JSON_OPT_SECTION = "json-options";

    public JSONSink(ISinkOwner owner) : this(owner, null , 0)
    {
    }

    public JSONSink(ISinkOwner owner, string name, int order) : base(owner, name, order)
    {
       m_Options = JsonWritingOptions.CompactASCII;
    }

    private JsonWritingOptions m_Options;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      if (node==null) return;

      var on = node[CONFIG_JSON_OPT_SECTION];
      if (on.Exists)
        m_Options = FactoryUtils.MakeAndConfigure<JsonWritingOptions>(on, typeof(JsonWritingOptions));
    }

    protected override string DoFormatMessage(Message msg)
    {
      return msg.ToJson(m_Options)+"\n\n";
    }

  }
}
