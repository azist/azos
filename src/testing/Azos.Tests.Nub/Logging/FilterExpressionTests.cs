using System;
using System.Collections.Generic;
using System.Text;

using Azos.Conf;
using Azos.Log;
using Azos.Log.Filters;
using Azos.Scripting;

namespace Azos.Tests.Nub.Logging
{
  [Runnable]
  public class FilterExpressionTests
  {
    [Run(@"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.Or'
        left{ type='Azos.Log.Filters.ByTopic' include='apple'}
        right{ type='Azos.Log.Filters.ByTopic' include='doesnt-exist'}
      }
    }")]

    [Run(@"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.Or'
        left{ type='Azos.Log.Filters.ByTopic' include='shake;bake;cake'}
        right{ type='Azos.Log.Filters.ByTopic' include='grapes;  apple ;tea  '}
      }
    }")]

    [Run(@"
    expect=true
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left{ type='Azos.Log.Filters.ByTopic' include='apple'}
        right{ type='Azos.Log.Filters.ByTopic' exclude='grapes'}
      }
    }")]

    [Run(@"
    expect=false
    def
    {
      tree
      {
        type='Azos.Log.Filters.And'
        left{ type='Azos.Log.Filters.ByTopic' include='apple'}
        right{ type='Azos.Log.Filters.ByTopic' exclude='apple'}
      }
    }")]
    public void FromConf(IConfigSectionNode def, bool expect)
    {
      def.See();

      var filter = FactoryUtils.MakeAndConfigure<LogMessageFilter>(def, typeof(LogMessageFilter));
      var msg = new Message
      {
        Topic = "apple",
        From = "Hydra.Records",
        Text = "Lenin loved mushrooms",
        Host = "cleveland.com",
        Channel = Atom.Encode("meduza"),
        App = Atom.Encode("dud")
      };
      var got = filter.Evaluate(msg);
      Aver.AreEqual(expect, got);
    }
  }
}
