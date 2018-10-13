
using System;

using Azos.Apps;

namespace Azos.Time
{
    /// <summary>
    /// Provides default time source implementation which is build on DateTime local class
    /// </summary>
    public class DefaultTimeSource : ApplicationComponent, ITimeSourceImplementation
    {
        private static DefaultTimeSource s_Instance = new DefaultTimeSource();


        /// <summary>
        /// Returns a singleton DefaultTimeSource instance which is always present
        /// </summary>
        public static DefaultTimeSource Instance
        {
          get { return s_Instance; }
        }

        private DefaultTimeSource() : base()
        {
        }


        public TimeLocation TimeLocation
        {
          get { return new TimeLocation(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now), "Local Computer", true); }
        }


        public DateTime Now { get { return DateTime.Now; } }

        /// <summary>
        /// Returns current time localized per TimeLocation
        /// </summary>
        public DateTime LocalizedTime { get { return Now;} }

        public DateTime UTCNow { get { return DateTime.UtcNow; } }


        /// <summary>
        /// Converts universal time to local time as of TimeLocation property
        /// </summary>
        public DateTime UniversalTimeToLocalizedTime(DateTime utc)
        {
            if (utc.Kind!=DateTimeKind.Utc)
              throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".UniversalTimeToLocalizedTime(utc.Kind!=UTC)");

            var loc = TimeLocation;
            if (!loc.UseParentSetting)
            {
                return DateTime.SpecifyKind(utc + loc.UTCOffset, DateTimeKind.Local);
            }
            else
            {
                return utc.ToLocalTime();
            }
        }

        /// <summary>
        /// Converts localized time to UTC time as of TimeLocation property
        /// </summary>
        public DateTime LocalizedTimeToUniversalTime(DateTime local)
        {
            if (local.Kind!=DateTimeKind.Local)
              throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".LocalizedTimeToUniversalTime(utc.Kind!=Local)");

            var loc = TimeLocation;
            if (!loc.UseParentSetting)
            {
                return DateTime.SpecifyKind(local - loc.UTCOffset, DateTimeKind.Utc);
            }
            else
            {
                return local.ToUniversalTime();
            }
        }

        public void Configure(Conf.IConfigSectionNode node)
        {

        }

    }
}
