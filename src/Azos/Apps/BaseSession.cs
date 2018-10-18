/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Security.Principal;

using Azos.Security;

namespace Azos.Apps
{
    /// <summary>
    /// Implements base ISession  functionality
    /// </summary>
    [Serializable]
    public class BaseSession : ISession
    {
        #region .ctor

            protected BaseSession() //used by serializer
            {
            }

            public BaseSession(Guid id)
            {
              m_ID = id;
              m_IDSecret = App.Random.NextRandomUnsignedLong;
              m_IsNew = true;
            }
        #endregion


        #region Fields

            private Guid m_ID;
            private ulong m_IDSecret;

            [NonSerialized]
            private Guid? m_OldID;

            [NonSerialized]
            internal bool m_IsNew;

            [NonSerialized]
            internal bool m_IsEnded;

            [NonSerialized]
            internal bool m_IsJustLoggedIn;


            private DateTime? m_LastLoginUTC;
            private SessionLoginType m_LastLoginType;
            private User m_User;


            [NonSerialized]
            private object m_ItemsLock = new object();

            private volatile ConcurrentDictionary<object, object> m_Items;

        #endregion

        #region Properties

            /// <summary>
            /// Returns Session ID
            /// </summary>
            public Guid ID
            {
                get { return m_ID; }
            }

            /// <summary>
            /// Returns Session ID secret - the ulong that identifies this session.
            /// This property is needed for cross-check upon GUID id lookup, so that
            /// Session ID GUID can not be forged by the client - a form of a "password"
            /// </summary>
            public ulong IDSecret
            {
                get { return m_IDSecret; }
            }

            /// <summary>
            /// When this parameter is not null then RegenerateID() was called and session provider may need to re-stow session object under a new ID
            /// </summary>
            public Guid? OldID
            {
               get { return m_OldID;}
            }

            /// <summary>
            /// Indicates that session object was just created with current request processing cycle
            /// </summary>
            public bool IsNew
            {
               get { return m_IsNew; }
            }

            /// <summary>
            /// Indicates that user login happened in current request processing cycle. This flag is
            /// useful for long term token assignment on release
            /// </summary>
            public bool IsJustLoggedIn
            {
               get { return m_IsJustLoggedIn; }
            }


            /// <summary>
            /// Returns the UTC DateTime of the last login/when HasJustLoggedIn() was called for the last time within the lifetime of this session object or NULL
            /// </summary>
            public DateTime? LastLoginUTC
            {
              get { return m_LastLoginUTC;}
            }

            /// <summary>
            /// Returns the last login type
            /// </summary>
            public SessionLoginType LastLoginType
            {
              get { return m_LastLoginType;}
            }

            /// <summary>
            /// Indicates whether this session was ended and will be destroyed after current request processing cycle
            /// </summary>
            public bool IsEnded
            {
               get { return m_IsEnded; }
            }

            /// <summary>
            /// Returns session user
            /// </summary>
            public User User
            {
                get { return m_User ?? User.Fake; }
                set { m_User = value;}
            }


            /// <summary>
            /// Returns user language preference or null
            /// </summary>
            public virtual string LanguageISOCode
            {
                get { return CoreConsts.ISO_LANG_ENGLISH;}
            }

            /// <summary>
            /// Provides a thread-safe dictionary of items
            /// </summary>
            public IDictionary<object, object> Items
            {
                get
                {
                  if (m_Items==null)
                      lock(m_ItemsLock)
                      {
                       if (m_Items==null)
                          m_Items = new ConcurrentDictionary<object,object>(4, 16);
                      }
                  return m_Items;
                }
            }

            /// <summary>
            /// Shortcut to .Items. Getter return null instead of throwing if key is not found
            /// </summary>
            public object this[object key]
            {
              get
              {
                object result;
                if (Items.TryGetValue(key, out result)) return result;
                return null;
              }
              set { Items[key] = value;}
            }


            public virtual void Acquire()
            {

            }

            public virtual void Release()
            {
              m_IsNew = false;
              m_IsJustLoggedIn = false;
              m_OldID = null;
            }

            /// <summary>
            /// Called from business code when user supplies login credentals and/or performs another action
            /// that can be qualified as a reliable user identity proof
            /// </summary>
            public virtual void HasJustLoggedIn(SessionLoginType loginType)
            {
              m_IsJustLoggedIn = true;
              m_LastLoginUTC = App.TimeSource.UTCNow;
              m_LastLoginType = loginType;
            }

            /// <summary>
            /// Generates new GUID and stores it in ID storing old ID value in OldID property which is not serializable.
            /// The implementations may elect to re-stow the existing session under the new ID.
            /// This method is useful for security, i.e. when user logs-in we may want to re-generate ID
            /// </summary>
            public void RegenerateID()
            {
              if (m_OldID.HasValue) return;
              m_OldID = m_ID;
              m_ID = Guid.NewGuid();
              m_IDSecret = App.Random.NextRandomUnsignedLong;
            }


        #endregion


        #region Public

           /// <summary>
           /// Ends session
           /// </summary>
           public virtual void End()
           {
             m_IsEnded = true;
           }

           IIdentity IPrincipal.Identity => User;
           bool IPrincipal.IsInRole(string role) => User.IsInRole(role);

        #endregion
    }
}
