/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Azos.Apps
{
    /// <summary>
    /// Represents a session that does nothing and returns fake user
    /// </summary>
    public sealed class NOPSession : ISession
    {
        private static NOPSession s_Instance = new NOPSession();

        private static Guid s_ID = new Guid("DA132A02-0D36-47D3-A9D7-10BC64741A6E");

        private NOPSession()
        {

        }

        /// <summary>
        /// Returns a singleton instance of the NOPSession
        /// </summary>
        public static NOPSession Instance
        {
           get { return s_Instance; }
        }


        public Guid ID
        {
            get { return s_ID; }
        }

        public ulong IDSecret
        {
          get { return 0; }
        }

        public Guid? OldID
        {
           get { return null;}
        }

        public bool IsNew
        {
           get { return false; }
        }

        public bool IsJustLoggedIn
        {
           get { return false; }
        }

        public DateTime? LastLoginUTC
        {
           get { return null; }
        }

        public SessionLoginType LastLoginType
        {
           get { return SessionLoginType.Unspecified; }
        }

        public bool IsEnded
        {
           get { return false; }
        }

        public Security.User User
        {
            get { return Security.User.Fake; }
            set {}
        }

        public string LanguageISOCode
        {
            get { return CoreConsts.ISO_LANG_ENGLISH;}
        }


        public string DataContextName { get => null; set { } }


        public IDictionary<object, object> Items
        {
            get { return new Dictionary<object, object>(); } //new instance is needed for thread safety
        }


        public object this[object key]
        {
           get { return null;}
           set {}
        }

        public void End()
        {

        }

        public void Acquire()
        {

        }

        public void Release()
        {

        }

        public void HasJustLoggedIn(SessionLoginType loginType, DateTime utcNow)
        {

        }

        public void RegenerateID()
        {

        }

        IIdentity IPrincipal.Identity => User;
        bool IPrincipal.IsInRole(string role) => User.IsInRole(role);
  }
}
