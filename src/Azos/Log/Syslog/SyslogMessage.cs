/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Log.Syslog
{

  /// <summary>
  /// Represents a UNIX-standard SYSLOG message
  /// </summary>
  public sealed class SyslogMessage
  {
    public static SeverityLevel FromAzosLogMessageType(MessageType type)
    {
      if (type<MessageType.Info) return SeverityLevel.Debug;
      if (type<MessageType.Notice) return SeverityLevel.Information;
      if (type<MessageType.Warning) return SeverityLevel.Notice;
      if (type<MessageType.Error) return SeverityLevel.Warning;
      if (type<MessageType.Critical) return SeverityLevel.Error;
      if (type<MessageType.CriticalAlert) return SeverityLevel.Critical;
      if (type<MessageType.Emergency) return SeverityLevel.Alert;

      return SeverityLevel.Emergency;
    }



    public SyslogMessage()
    {

    }

    public SyslogMessage(FacilityLevel facility,
                          SeverityLevel level,
                          string text)
    {
        m_Facility = facility;
        m_Severity = level;
        m_Text = text;
    }

    public SyslogMessage(Message azoMsg)
    {
        m_Severity = FromAzosLogMessageType(azoMsg.Type);

        m_Facility = FacilityLevel.User;
        m_LocalTimeStamp = azoMsg.TimeStamp;
        m_Text = string.Format("{0} - {1} - {2}", azoMsg.Topic, azoMsg.From, azoMsg.Text);
    }

    private FacilityLevel m_Facility;
    private SeverityLevel m_Severity;
    private string m_Text;
    private DateTime m_LocalTimeStamp = App.LocalizedTime;

    public FacilityLevel Facility
    {
        get { return m_Facility;}
        set { m_Facility = value; }
    }

    public SeverityLevel Severity
    {
        get { return m_Severity;}
        set { m_Severity = value; }
    }

    public DateTime LocalTimeStamp
    {
        get { return m_LocalTimeStamp;}
        set { m_LocalTimeStamp = value; }
    }

    public string Text
    {
        get { return m_Text ?? string.Empty;}
        set { m_Text = value; }
    }

    public int Priority
    {
        get { return ((int)m_Facility * 8) + ((int)m_Severity);}
    }

  }
}
