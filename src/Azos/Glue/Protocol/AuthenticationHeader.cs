/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Security;

namespace Azos.Glue.Protocol
{
    /// <summary>
    /// Marshalls user authentication information
    /// </summary>
    [Serializable]
    public sealed class AuthenticationHeader : Header
    {
        private AuthenticationToken m_Token;
        private Credentials m_Credentials;

        /// <summary>
        /// Returns AuthenticationToken
        /// </summary>
        public AuthenticationToken Token { get { return m_Token;} }


        /// <summary>
        /// Returns Credentials
        /// </summary>
        public Credentials Credentials { get { return m_Credentials;} }


        public AuthenticationHeader(AuthenticationToken token)
        {
            m_Token = token;
        }

        /// <summary>
        /// Inits header with Credentials instance.
        /// Note: passing IDPasswordCredentials over public wire is not a good practice,
        /// pass AuthenticationToken instead
        /// </summary>
        public AuthenticationHeader(Credentials credentials)
        {
            m_Credentials = credentials;
        }

    }
}
