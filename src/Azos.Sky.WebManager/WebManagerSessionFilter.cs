using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;


using Azos.Conf;
using Azos.Web;
using Azos.Wave;
using Azos.Wave.Filters;
using Azos.Serialization.JSON;


namespace Azos.Sky.WebManager
{
  /// <summary>
  /// Provides session management for AWM-specific sessions
  /// </summary>
  public sealed class WebManagerSessionFilter : SessionFilter
  {
    #region .ctor
      public AWMSessionFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) {}
      public AWMSessionFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) {ctor(confNode);}
      public AWMSessionFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
      public AWMSessionFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode) {ctor(confNode);}

      private void ctor(IConfigSectionNode confNode)
      {
        ConfigAttribute.Apply(this, confNode);
      }

    #endregion


      protected override WaveSession MakeNewSessionInstance(WorkContext work)
      {
        return new WebManagerSession(Guid.NewGuid());
      }

  }
}
