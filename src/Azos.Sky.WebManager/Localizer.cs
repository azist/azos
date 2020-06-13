/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using Azos;
using Azos.Log;
using Azos.Apps;
using Azos.Conf;
using Azos.Wave;
using Azos.Sky.WebManager.Controls;

namespace Azos.Sky.WebManager
{
  /// <summary>
  /// Facilitates tasks working with objects of appropriate culture per user
  /// </summary>
  public sealed class Localizer : ApplicationComponent
  {

    public static Localizer Of(IApplication app)
      => app.NonNull(nameof(app)).Singletons.GetOrCreate(()=> new Localizer(app)).instance;


    public const string CONFIG_LOCALIZATION_SECTION = "localization";
    public const string CONFIG_MSG_FILE_ATTR = "msg-file";
    public const string LOC_ANY_SCHEMA_KEY = "--ANY-SCHEMA--";
    public const string LOC_ANY_FIELD_KEY = "--ANY-FIELD--";

    public const string ISO_LANG_ENGLISH = "eng";
    public const string ISO_LANG_RUSSIAN = "rus";
    public const string ISO_LANG_GERMAN  = "deu";
    public const string ISO_LANG_FRENCH  = "fre";


    public enum MoneyFormat{WithCurrencySymbol, WithoutCurrencySymbol}
    public enum DateTimeFormat{ShortDate, LongDate, ShortDateTime, LongDateTime}

    public Localizer(IApplication app) : base(app)
    {
#warning Vidya refactoring candidate
      Azos.Wave.Client.RecordModelGenerator.DefaultInstance.ModelLocalization += recGeneratorLocalization;
    }

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_LOCALIZATION;

    private  IConfigSectionNode s_LocalizationData;

    private  void ensureData()
    {
      if (s_LocalizationData!=null) return;

      var loc = App.ConfigRoot[CONFIG_LOCALIZATION_SECTION];
      s_LocalizationData = loc;
      var msgFile = loc.AttrByName(CONFIG_MSG_FILE_ATTR).Value;
      if (msgFile.IsNotNullOrWhiteSpace())
      try
      {
        WriteLog(
          MessageType.Info,
          "ensureData()",
          "Configured in '/{0}/${1}' to load localization msg file '{2}'".Args(CONFIG_LOCALIZATION_SECTION, CONFIG_MSG_FILE_ATTR, msgFile)
        );
        s_LocalizationData = Configuration.ProviderLoadFromFile(msgFile).Root;
      }
      catch(Exception error)
      {
        WriteLog(
          MessageType.CatastrophicError,
          "enusreData()",
          "Error loading localization msg file '{0}': {1}".Args(msgFile, error.ToMessageWithType()),
          error
        );
      }
    }

    private  string recGeneratorLocalization(Azos.Wave.Client.RecordModelGenerator sender, string schema, string field, string value, Atom isoLang)
    {
      if (value.IsNullOrWhiteSpace()) return value;

      ensureData();
      if (!s_LocalizationData.Exists) return value;//nowhere to lookup

      var session = ExecutionContext.Session as WebManagerSession;
      if (session==null) return value;

      isoLang = session.LanguageISOCode;
      if (isoLang.Value==ISO_LANG_ENGLISH) return value;


      if (schema.IsNullOrWhiteSpace()) schema = LOC_ANY_SCHEMA_KEY;
      if (field.IsNullOrWhiteSpace())  field = LOC_ANY_FIELD_KEY;
      bool exists;
      var lv = lookupValue(isoLang.Value, schema, field, value, out exists);

      #if DEVELOPMENT
      if (!exists)
      {
          App.Log.Write( new Message{
            Type = MessageType.InfoZ,
            From = "lookup",
            Topic = SysConsts.LOCALIZATION_TOPIC,
            Text = "Need localization",
            Parameters = (new {iso = isoLang, sch = schema, fld = field, val = value }).ToJSON()
          });
      }
      #endif

      return lv;
    }

    private  string lookupValue(string isoLang, string schema, string field, string value, out bool exists)
    {
      exists = false;

      var nlang = s_LocalizationData[isoLang];
      if (!nlang.Exists) return value;
      var nschema = nlang[schema, LOC_ANY_SCHEMA_KEY];
      if (!nschema.Exists) return value;
      var nfield = nschema[field, LOC_ANY_FIELD_KEY];
      if (!nfield.Exists) return value;

      var nvalue = nfield.Attributes.FirstOrDefault(a=>a.Name == value);//case SENSITIVE search
      if (nvalue==null) return value;
      var lv = nvalue.Value;

      if (lv.IsNotNullOrWhiteSpace())
      {
         exists = true;
         return lv;
      }

      return value;
    }




    public  string Money(decimal amount, MoneyFormat format = MoneyFormat.WithCurrencySymbol, WebManagerSession session = null)
    {
      return amount.ToString(); //todo Implement
    }

    public  string DateTime(DateTime dt, DateTimeFormat format = DateTimeFormat.LongDateTime, WebManagerSession session = null)
    {
      return dt.ToString();//todo implement
    }

    /// <summary>
    /// Converts country code into language code
    /// </summary>
    public  string CountryISOCodeToLanguageISOCode(string countryISOCode)
    {
      if (countryISOCode.IsNullOrWhiteSpace()) return ISO_LANG_ENGLISH;
      countryISOCode = countryISOCode.ToLowerInvariant();
      switch(countryISOCode)
      {
        case "ru":
        case "ua":
        case "by":  return ISO_LANG_RUSSIAN;

        case "de":  return ISO_LANG_GERMAN;
        case "fr":  return ISO_LANG_FRENCH;

        default: return ISO_LANG_ENGLISH;
      }

    }

    private volatile Dictionary<string, Type> s_PageTypes = new Dictionary<string,Type>();

    /// <summary>
    /// Makes localized page instance per session
    /// </summary>
    public  WebManagerPage MakePage<TPage>(params object[] ctorArgs) where TPage : WebManagerPage
    {
      return MakePage<TPage>(typeof(TPage), WorkContext.Current, ctorArgs);
    }

    /// <summary>
    /// Makes localized page instance per session
    /// </summary>
    public  WebManagerPage MakePage<TPage>(Type type, WorkContext work, object[] ctorArgs) where TPage : WebManagerPage
    {
      string tname = string.Empty;
      try
      {
          Type localizedType = null;

          var lang = GetLanguage(work);

          if (lang!=ISO_LANG_ENGLISH)
          {
              var key = type.FullName+"_"+lang;

              if (!s_PageTypes.TryGetValue(key, out localizedType))
              {
                tname = "{0}_{1}, Azos.Sky.WebManager".Args(type.FullName, lang);
                localizedType = Type.GetType(tname, false);
                var dict = new Dictionary<string, Type>(s_PageTypes);
                dict[key] = localizedType;
                System.Threading.Thread.MemoryBarrier();
                s_PageTypes = dict;//atomic
              }
          }

          if (localizedType==null) localizedType = type;

          tname = type.FullName;
          return (WebManagerPage)Activator.CreateInstance(localizedType, ctorArgs);
      }
      catch(Exception error)
      {
        throw new WebManagerException("Error making localized page '{0}'. Error: {1}".Args(tname, error.ToMessageWithType()), error);
      }
    }

    /// <summary>
    /// Tries to determine work context lang and returns it or ENG
    /// </summary>
    public string GetLanguage(WorkContext work = null)
    {
      if (work==null) work = WorkContext.Current;
      if (work==null) return ISO_LANG_ENGLISH;

      string lang = null;

      var session = work.Session as WebManagerSession;
      if (session!=null)
        lang = session.LanguageISOCode.Value;
      else
        if (work.GeoEntity!=null)
        {
          var country = work.GeoEntity.CountryISOName;
          if (country.IsNotNullOrWhiteSpace())
            lang =  CountryISOCodeToLanguageISOCode(country);
        }

      if (lang.IsNullOrWhiteSpace()) lang = ISO_LANG_ENGLISH;

      return lang;
    }


     private  Dictionary<string, string> m_Content = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
     {
        {"mnuHome_eng", "Process"},                      {"mnuHome_rus", "Процесс"},
        {"mnuConsole_eng", "Console"},                   {"mnuSellers_rus", "Консоль"},
        {"mnuInstrumentation_eng", "Instrumentation"},   {"mnuConsumers_rus", "Инструментарий"},
        {"mnuTheSystem_eng", "The System"},              {"mnuDevelopers_rus", "Система"},
        {"mnuProcessManager_eng", "Process manager"},    {"mnuProcessManager_rus", "Управление процессами"}
     };


    /// <summary>
    /// Gets content by name
    /// </summary>
    public  string Content(string key, WorkContext work = null)
    {
      var lang = GetLanguage();
      string result;
      if (m_Content.TryGetValue(key+"_"+lang, out result)) return result;
      return string.Empty;
    }

  }
}
