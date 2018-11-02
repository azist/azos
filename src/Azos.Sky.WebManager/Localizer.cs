using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using Azos;
using Azos.Log;
using Azos.ApplicationModel;
using Azos.Environment;
using Azos.Wave;
using Azos.Serialization.JSON;
using Azos.Sky.WebManager.Controls;
using Azos.Sky.WebManager.Pages;

namespace Azos.Sky.WebManager
{
  /// <summary>
  /// Facilitates tasks working with objects of appropriate culture per user
  /// </summary>
  public static class Localizer
  {
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

    static Localizer()
    {
      Azos.Wave.Client.RecordModelGenerator.DefaultInstance.ModelLocalization += recGeneratorLocalization;
    }


    private static IConfigSectionNode s_LocalizationData;

    private static void ensureData()
    {
      if (s_LocalizationData!=null) return;

      var loc = App.ConfigRoot[CONFIG_LOCALIZATION_SECTION];
      s_LocalizationData = loc;
      var msgFile = loc.AttrByName(CONFIG_MSG_FILE_ATTR).Value;
      if (msgFile.IsNotNullOrWhiteSpace())
      try
      {
        App.Log.Write( new Message{
          Type = MessageType.Info,
          From = "enusreData()",
          Topic = SysConsts.LOG_TOPIC_LOCALIZATION,
          Text = "Configured in '/{0}/${1}' to load localization msg file '{2}'".Args(CONFIG_LOCALIZATION_SECTION, CONFIG_MSG_FILE_ATTR, msgFile),
        });
        s_LocalizationData = Configuration.ProviderLoadFromFile(msgFile).Root;
      }
      catch(Exception error)
      {
        App.Log.Write( new Message{
          Type = MessageType.CatastrophicError,
          From = "enusreData()",
          Topic = SysConsts.LOG_TOPIC_LOCALIZATION,
          Text = "Error loading localization msg file '{0}': {1}".Args(msgFile, error.ToMessageWithType()),
          Exception = error
        });
      }
    }

    private static string recGeneratorLocalization(Azos.Wave.Client.RecordModelGenerator sender, string schema, string field, string value, string isoLang)
    {
      if (value.IsNullOrWhiteSpace()) return value;

      ensureData();
      if (!s_LocalizationData.Exists) return value;//nowhere to lookup

      var session = ExecutionContext.Session as WebManagerSession;
      if (session==null) return value;

      isoLang = session.LanguageISOCode;
      if (isoLang==ISO_LANG_ENGLISH) return value;


      if (schema.IsNullOrWhiteSpace()) schema = LOC_ANY_SCHEMA_KEY;
      if (field.IsNullOrWhiteSpace())  field = LOC_ANY_FIELD_KEY;
      bool exists;
      var lv = lookupValue(isoLang, schema, field, value, out exists);

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

    private static string lookupValue(string isoLang, string schema, string field, string value, out bool exists)
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




    public static string Money(decimal amount, MoneyFormat format = MoneyFormat.WithCurrencySymbol, AWMWebSession session = null)
    {
      return amount.ToString(); //todo Implement
    }

    public static string DateTime(DateTime dt, DateTimeFormat format = DateTimeFormat.LongDateTime, AWMWebSession session = null)
    {
      return dt.ToString();//todo implement
    }

    /// <summary>
    /// Converts country code into language code
    /// </summary>
    public static string CountryISOCodeToLanguageISOCode(string countryISOCode)
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

    private static Dictionary<string, Type> s_PageTypes = new Dictionary<string,Type>();

    /// <summary>
    /// Makes localized page instance per session
    /// </summary>
    public static AWMPage MakePage<TPage>(params object[] ctorArgs) where TPage : AWMPage
    {
      return MakePage<TPage>(typeof(TPage), WorkContext.Current, ctorArgs);
    }

    /// <summary>
    /// Makes localized page instance per session
    /// </summary>
    public static AWMPage MakePage<TPage>(Type type, WorkContext work, object[] ctorArgs) where TPage : AWMPage
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
                tname = "{0}_{1}, Apex.Web".Args(type.FullName, lang);
                localizedType = Type.GetType(tname, false);
                var dict = new Dictionary<string, Type>(s_PageTypes);
                dict[key] = localizedType;
                s_PageTypes = dict;//atomic
              }
          }

          if (localizedType==null) localizedType = type;

          tname = type.FullName;
          return (AWMPage)Activator.CreateInstance(localizedType, ctorArgs);
      }
      catch(Exception error)
      {
        throw new AWMException("Error making localized page '{0}'. Error: {1}".Args(tname, error.ToMessageWithType()), error);
      }
    }

    /// <summary>
    /// Tries to determine work context lang and returns it or ENG
    /// </summary>
    public static string GetLanguage(WorkContext work = null)
    {
      if (work==null) work = WorkContext.Current;
      if (work==null) return ISO_LANG_ENGLISH;

      string lang = null;

      var session = work.Session as AWMWebSession;
      if (session!=null)
        lang = session.LanguageISOCode;
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


     private static Dictionary<string, string> s_Content = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
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
    public static string Content(string key, WorkContext work = null)
    {
      var lang = GetLanguage();
      string result;
      if (s_Content.TryGetValue(key+"_"+lang, out result)) return result;
      return string.Empty;
    }



  }
}
