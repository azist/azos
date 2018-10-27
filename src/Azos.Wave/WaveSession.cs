/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Azos.Apps;
using Azos.Web.GeoLookup;

namespace Azos.Wave
{
    /// <summary>
    /// Represents a session in a WAVE server application
    /// </summary>
    [Serializable]
    public class WaveSession : BaseSession
    {
        protected WaveSession():base(){} //used by serializer
        public WaveSession(Guid id) : base(id)
        {
           m_CSRFToken = App.Random.NextRandomWebSafeString();
        }

        private string m_CSRFToken;


        /// <summary>
        /// Stores user Geographical/Location information
        /// </summary>
        [NonSerialized]
        public GeoEntity GeoEntity;

        /// <summary>
        /// Unique token assigned at session start used for checking of form posts.
        /// This is for Cross Site Request Forgery protection.
        /// </summary>
        public string CSRFToken { get{ return m_CSRFToken;} }

    }
}
