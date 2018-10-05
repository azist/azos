/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//todo: Replace with modules, no need for this anymore
namespace Azos.Apps
{
    /// <summary>
    /// Represents an entity that performs work on application start.
    /// This entity must be either invoked directly or declared in config file under "starters" section
    /// </summary>
    public interface IApplicationStarter : Conf.IConfigurable, Collections.INamed
    {
        /// <summary>
        /// Indicates whether an exception that leaks from starter method invocation should break the application start,
        ///  or just get logged
        /// </summary>
        bool ApplicationStartBreakOnException { get; }

        void ApplicationStartBeforeInit(IApplication application);

        void ApplicationStartAfterInit(IApplication application);
    }


    /// <summary>
    /// Represents an entity that can get notified about application finish
    /// </summary>
    public interface IApplicationFinishNotifiable : Collections.INamed
    {
        void ApplicationFinishBeforeCleanup(IApplication application);
        void ApplicationFinishAfterCleanup(IApplication application);
    }

}
