
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;

namespace Azos.Log
{

  /// <summary>
  /// Represents log that does not do anything
  /// </summary>
  public sealed class NOPLog : ApplicationComponent, ILog
  {
      private static NOPLog s_Instance = new NOPLog();


      private NOPLog(): base()
      {

      }

      /// <summary>
      /// Returns a singlelton instance of the log that does not do anything
      /// </summary>
      public static NOPLog Instance
      {
        get { return s_Instance; }
      }

    #region ILog Members


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


    #endregion

  }
}
