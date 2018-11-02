using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Apps;
using Azos.Security;
using Azos.Wave;

namespace Azos.Sky.WebManager
{
  /// <summary>
    /// Represents AWM-specific web session on WAVE server
    /// </summary>
    [Serializable]
    public class WebManagerSession : WaveSession
    {
        protected WebManagerSession() : base(){} //used by serializer
        public WebManagerSession(Guid id) : base(id) {}


        /// <summary>
        /// Returns language code for session - defaulted from geo-location
        /// </summary>
        public override string LanguageISOCode
        {
          get
          {
            string lang = null;

            if (GeoEntity!=null && GeoEntity.Location.HasValue)
            {
                var country = GeoEntity.CountryISOName;
                if (country.IsNotNullOrWhiteSpace())
                 lang = Localizer.CountryISOCodeToLanguageISOCode(country);
            }

            if (lang.IsNullOrWhiteSpace())
             lang = Localizer.ISO_LANG_ENGLISH;

            return lang;
          }
        }
    }
}
