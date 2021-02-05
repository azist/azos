/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;

namespace Azos.IO.FileSystem
{
    /// <summary>
    /// Stipulates contract for various file system abstractions.
    /// FileSystem abstractions are mostly useful for working with components/classes that may need to inter-operate not only with local file system
    ///  but also with distributed systems like ApacheHDFS, SVN, GIT, or Sky Cluster File System.
    ///  Azos library provides compatibility wrapper 'Azos.IO.FileSystem.Local.LocalFileSystem' for access to local machine file system
    ///  (which is based on this class and is implemented using a traditional System.IO.* set of classes).
    /// The FileSystem abstraction supports the following concepts: versioning, transactions, metadata, security; however it does not guarantee that
    ///  every implementation is capable of providing all of these functions. Query "GeneralCapabilities" and "InstanceCapabilities" to see what functions
    ///   are supported by a particular instance.
    /// </summary>
    public interface IFileSystem : IApplicationComponent, Collections.INamed, IDisposable
    {
        /// <summary>
        /// Returns a list of sessions
        /// </summary>
        IEnumerable<FileSystemSession> Sessions { get; }

        /// <summary>
        /// Returns capabilities for this file system in general
        /// </summary>
        IFileSystemCapabilities GeneralCapabilities { get;}

        /// <summary>
        /// Returns capabilities for this file system instance, that may or may not be the same as GeneralCapabilities
        /// </summary>
        IFileSystemCapabilities InstanceCapabilities { get;}


        /// <summary>
        /// Creates a new session for the specified user and version
        /// </summary>
        FileSystemSession StartSession(FileSystemSessionConnectParams cParams);

        /// <summary>
        /// Combines two or more path segments joining them using primary file system path separator
        /// </summary>
        string CombinePaths(string first, params string[] others);

        /// <summary>
        /// Returns a file name from full path or null if the specified path does not have a file path.
        /// Keep in mind that a file system does not necessarily have to support hierarchical directory structure
        /// </summary>
        /// <param name="fullPath">Full path</param>
        /// <returns>File name or null</returns>
        string ExtractFileName(string fullPath);

        /// <summary>
        /// Returns an array of path segments - directory names on the path. The last name may be directory of a file name.
        /// The specifics depend on particular file system support
        /// </summary>
        /// <param name="fullPath">Full path string like '/a/b/c'</param>
        /// <returns>And array of segments</returns>
        string[] SplitPathSegments(string fullPath);
    }

    public interface IFileSystemImplementation : IFileSystem, IConfigurable, IDisposable
    {

    }

    /// <summary>
    /// Decorates entities that represent version for file systems that support versioning
    /// </summary>
    public interface IFileSystemVersion : Collections.INamed
    {
    }

    /// <summary>
    /// Denotes a handle for item in a file system.
    /// For example: in a distributed system this may be some form of unique file/directory id (i.e. a GUID) or
    ///  object instance internal to implementation
    /// </summary>
    public interface IFileSystemHandle
    {

    }


    /// <summary>
    /// Denotes a handle for transaction in a file system.
    /// For example: in a distributed system this may be an object instance holding information about transaction which is internal to implementation
    /// </summary>
    public interface IFileSystemTransactionHandle
    {

    }


}
