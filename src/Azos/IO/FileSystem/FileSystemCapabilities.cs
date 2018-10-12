

namespace Azos.IO.FileSystem
{
    /// <summary>
    /// Supplies capabilities for the file system. The implementation must be thread safe
    /// </summary>
    public interface IFileSystemCapabilities
    {
        /// <summary>
        /// Indicates whether a file system supports versioning
        /// </summary>
        bool SupportsVersioning { get;}

        /// <summary>
        /// Indicates whether a file system supports transactions
        /// </summary>
        bool SupportsTransactions { get;}

        /// <summary>
        /// Returns maximum allowed length of the whole path that includes directory name/s and/or separator chars and/or file name
        /// </summary>
        int MaxFilePathLength { get;}

        /// <summary>
        /// Returns maximum allowed length of a file name
        /// </summary>
        int MaxFileNameLength { get;}

        /// <summary>
        /// Returns maximum allowed length of a directory name
        /// </summary>
        int MaxDirectoryNameLength { get;}


        /// <summary>
        /// Returns the maximum size of a file
        /// </summary>
        ulong MaxFileSize { get;}

        /// <summary>
        /// Returns understood path separator characters
        /// </summary>
        char[] PathSeparatorCharacters { get;}

        /// <summary>
        /// Indicates whether file system supports modification of its files and structure
        /// </summary>
        bool IsReadonly { get;}

        /// <summary>
        /// Indicates whether the file system supports security permissions
        /// </summary>
        bool SupportsSecurity { get;}

        /// <summary>
        /// Indicates whether the file system supports custom metadata for files and folders
        /// </summary>
        bool SupportsCustomMetadata { get;}

        bool SupportsDirectoryRenaming { get;}
        bool SupportsFileRenaming { get; }

        bool SupportsStreamSeek { get;}

        bool SupportsFileModification { get;}

        bool SupportsCreationTimestamps     { get;}
        bool SupportsModificationTimestamps { get;}
        bool SupportsLastAccessTimestamps   { get;}

        bool SupportsReadonlyDirectories   { get;}
        bool SupportsReadonlyFiles         { get;}


        bool SupportsCreationUserNames     { get;}
        bool SupportsModificationUserNames { get;}
        bool SupportsLastAccessUserNames   { get;}

        bool SupportsFileSizes      { get;}
        bool SupportsDirectorySizes { get;}

        /// <summary>
        /// Defines if this FileSystem implements Async methods in real asynchronous manner.
        /// By default asynchronous methods are actually executed syncronously and return Task with execution result or exception
        /// </summary>
        bool SupportsAsyncronousAPI { get;}
    }

}
