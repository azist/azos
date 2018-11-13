/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;


namespace Azos.Sky.Apps.Terminal
{
    /// <summary>
    /// Provides generalization for commandlet - terminal command handler
    /// </summary>
    public abstract class Cmdlet : DisposableObject
    {
        protected Cmdlet(AppRemoteTerminal terminal, IConfigSectionNode args)
        {
            m_Terminal = terminal;
            m_Args = args;
        }

        protected AppRemoteTerminal m_Terminal;
        protected IConfigSectionNode m_Args;

        public abstract string Execute();

        public abstract string GetHelp();
    }
}
