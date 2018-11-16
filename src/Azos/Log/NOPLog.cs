/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using Azos.Apps;

namespace Azos.Log
{

  /// <summary>
  /// Represents log that does not do anything
  /// </summary>
  public sealed class NOPLog : ApplicationComponent, ILogImplementation
  {

    internal NOPLog(IApplication app): base(app)
    {

    }

    #region ILog Members

        public override string ComponentLogTopic => CoreConsts.LOG_TOPIC;

        public Message LastWarning     { get {return null;}}

        public Message LastError       { get {return null;}}

        public Message LastCatastrophe { get {return null;}}


        public void Write(Message msg)
        {

        }

        public void Write(Message msg, bool urgent)
        {

        }


        public void Write(MessageType type, string text, string topic = null, string from = null)
        {

        }

        public void Write(MessageType type, string text, bool urgent, string topic = null, string from = null)
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

        public bool InstrumentationEnabled { get { return false; } set { } }
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

        public void Configure(Conf.IConfigSectionNode node)
        {

        }

    #endregion

  }
}
