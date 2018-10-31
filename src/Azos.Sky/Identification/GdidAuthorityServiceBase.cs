using System;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.Log;

namespace Azos.Sky.Identification
{
  /// <summary>
  /// Base for GdidAuthority and GdidPersistenceLocation services
  /// </summary>
  public abstract partial class GdidAuthorityServiceBase : Service
  {
    #region CONSTS
      public const int MAX_PATH_LENGTH = 240;//dictated by Windows
      public const int MAX_NAME_LENGTH = 80;// ns[80] + seq[80] = 160  240 - 160 = 80 for disk path prefix
      public const int MAX_DISK_PATH_LENGTH = MAX_PATH_LENGTH - ( 1 + MAX_NAME_LENGTH + MAX_NAME_LENGTH + 3);// 76 = 240 - (aid[1] + ns[80] + seq[80] + separators[3])

      public const int MAX_BLOCK_SIZE = 1024;//this number effects performance greatly, however increasing it is dangerous as it may lead to
                                             // too many wasted IDs if requesting process requests too many than does not use them up

      public const string CONFIG_GDID_AUTHORITY_SECTION = "gdid-authority";

      public const string CONFIG_AUTHORITY_IDS_ATTR  = "authority-ids";

      public const string CONFIG_PERSISTENCE_SECTION = "persistence";
      public const string CONFIG_LOCATION_SECTION = "location";
    #endregion

    #region Inner Classes
      public struct _id
      {
        private const string SEPARATOR = "::";

        public _id(uint era, ulong value) { Era = era; Value = value;}
        public _id(string content)
        {
          try
          {
            if (content.IsNullOrWhiteSpace())
             throw new GdidException("<null|empty>");

            var i = content.IndexOf(SEPARATOR);
            if (i<=0) throw new GdidException("no "+SEPARATOR);

            var sera = content.Substring(0, i);
            var sid = content.Substring(i+SEPARATOR.Length);

            Era = uint.Parse(sera);
            Value = ulong.Parse(sid);
          }
          catch(Exception error)
          {
            throw new GdidException(StringConsts.GDIDAUTH_ID_DATA_PARSING_ERROR.Args(content, error.ToMessageWithType()), error);
          }
        }

        public readonly uint Era;
        public readonly ulong Value;

        public override string ToString()
        {
          return Era.ToString()+SEPARATOR+Value.ToString();
        }

        public static bool operator >(_id left, _id right)
        {
          return left.Era > right.Era || (left.Era == right.Era && left.Value > right.Value);
        }

        public static bool operator <(_id left, _id right)
        {
          return left.Era < right.Era || (left.Era == right.Era && left.Value < right.Value);
        }
      }
    #endregion


    #region Static

      /// <summary>
      /// Checks the name for validity. Throws if name does not contain valid chars or over the max length
      /// </summary>
      public static void CheckNameValidity(string name)
      {
        if (name.IsNullOrWhiteSpace() ||
            name.Length>MAX_NAME_LENGTH)
            throw new GdidException(StringConsts.GDIDAUTH_NAME_INVALID_LEN_ERROR.Args(name, MAX_NAME_LENGTH));

        for(var i=0; i<name.Length; i++)
        {
          char c = name[i];
          if (c>='0' && c<='9') continue;
          if (c>='A' && c<='Z') continue;
          if (c>='a' && c<='z') continue;
          if (i!=0 && (c=='_'||c=='.'||c=='-') && i!=name.Length-1) continue;
          throw new GdidException(StringConsts.GDIDAUTH_NAME_INVALID_CHARS_ERROR.Args(name));
        }
      }

      public static string AuthorityPathSeg(byte authority)
      {
        return ((char)(authority<=9 ? '0'+authority : 'a'+authority-10)).ToString();
      }

    #endregion

    #region .ctor/.dctor
      protected GdidAuthorityServiceBase() : base(null)
      {
      }
    #endregion

    #region Fields

      private OrderedRegistry<PersistenceLocation> m_Locations = new OrderedRegistry<PersistenceLocation>();

    #endregion

    #region Properties

      /// <summary>
      /// Specifies where and how the service persists data on disk
      /// </summary>
      public IRegistry<PersistenceLocation> PersistenceLocations { get {return m_Locations;} }

    #endregion


    #region Protected

      protected override void DoConfigure(IConfigSectionNode node)
      {
        if (node==null)
          node = App.ConfigRoot[CONFIG_GDID_AUTHORITY_SECTION];

        base.DoConfigure(node);
        ConfigAttribute.Apply( this, node );

        foreach(var lnode in node[CONFIG_PERSISTENCE_SECTION].Children.Where(n=>n.IsSameName(CONFIG_LOCATION_SECTION)))
        {
          var location = FactoryUtils.Make<PersistenceLocation>(lnode, typeof(DiskPersistenceLocation), new object[] {lnode});
          m_Locations.Register(location);
        }
      }

      protected override void DoStart()
      {
        base.DoStart();

        string error = null;
        if (m_Locations.Count==0) error = "<no locations>";
        else
         foreach(var location in m_Locations)
         {
           var ve = location.Validate();
           if (ve.IsNotNullOrWhiteSpace())
            error += "Location '{0}'. Config error: {1} \n".Args(location.Name, ve);
         }

        if (error!=null)
          throw new GdidException(StringConsts.GDIDAUTH_LOCATIONS_CONFIG_ERROR + error);
      }

      //blocking call and throws if can not write to any devices
      protected void WriteToLocations(byte authority, string scope, string seq, _id id)
      {
        var guid = Guid.NewGuid();

        StringBuilder errors = null;
        var totalFailure = true;

        foreach(var location in m_Locations.OrderedValues)
          try
          {
            location.Write(authority, scope, seq, id);
            totalFailure = false;
          }
          catch(Exception error)
          {
            if (errors==null) errors = new StringBuilder();
            errors.AppendLine( "Path '{0}'. Exception: {1}".Args(location, error.ToMessageWithType()) );
            Log(MessageType.CriticalAlert, "WriteToLocations()", location.ToString(), error, guid);
            Instrumentation.AuthLocationWriteFailureEvent.Happened(location.ToString());//Location-level
          }

        if (totalFailure)
        {
          var txt = StringConsts.GDIDAUTH_LOCATION_PERSISTENCE_FAILURE_ERROR + ( errors!=null ? errors.ToString() : "no locations");

          Log(MessageType.CatastrophicError, "WriteToLocations()", txt, null, guid);

          Instrumentation.AuthLocationWriteTotalFailureEvent.Happened();//TOTAL-LEVEL(for all locations)

          throw new GdidException(txt);
        }
      }


      //blocking call
      protected _id ReadFromLocations(byte authority, string scope, string seq)
      {
        var guid = Guid.NewGuid();

        StringBuilder errors = null;
        _id? result = null;

        var onlyErrors = true;
        var first = true;

        foreach(var location in m_Locations.OrderedValues)
          try
          {
            var got = location.Read(authority, scope, seq);
            onlyErrors = false;
            if (!got.HasValue) continue;

            if (!result.HasValue || got.Value > result.Value)
            {
              result = got.Value;//take the maximum
              if (!first)
              {
                if (errors==null) errors = new StringBuilder();
                var txt = "Location '{0}' had a later sequence value '{1}' than prior location".Args(location, got.Value);
                errors.AppendLine(txt);
                Log(MessageType.CriticalAlert, "ReadFromLocations()", txt, null, guid);
              }
            }
            first = false;
          }
          catch(Exception error)
          {
            if (errors==null) errors = new StringBuilder();
            var txt = "Error at location '{0}': {1}".Args(location, error.ToMessageWithType());
            errors.AppendLine(txt);
            Log(MessageType.CriticalAlert, "ReadFromLocations()", txt, null, guid);
            Instrumentation.AuthLocationReadFailureEvent.Happened(location.ToString());//LOCATION-LEVEL
            throw;
          }


        if (!result.HasValue && onlyErrors)
        {
          var txt = StringConsts.GDIDAUTH_LOCATIONS_READ_FAILURE_ERROR + ( errors!=null ? errors.ToString() : "no locations");

          Log(MessageType.CatastrophicError, "ReadFromLocations()", txt, null, guid);

          Instrumentation.AuthLocationReadTotalFailureEvent.Happened();//TOTAL-LEVEL

          throw new GdidException(txt);
        }

        return result ?? new _id(0, 0);
      }

      protected void Log(MessageType type, string from, string msg, Exception error = null, Guid? batch = null)
      {
        var lm = new Log.Message{
               Type = type,
               Topic = SysConsts.LOG_TOPIC_ID_GEN,
                From = "{0}.{1}".Args(GetType().Name, from),
                Text = msg,
                Exception = error
            };

        if (batch.HasValue)
          lm.RelatedTo = batch.Value;

        App.Log.Write( lm );
      }

    #endregion

  }

}
