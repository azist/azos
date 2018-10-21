
namespace Azos.IO.FileSystem.SVN
{
  public class SVNFileSystemCapabilities : IFileSystemCapabilities
  {
    #region CONSTS

      internal static readonly char[] PATH_SEPARATORS = new char[]{'/'};

    #endregion

    #region Static

      private static SVNFileSystemCapabilities s_Instance = new SVNFileSystemCapabilities();

      public static SVNFileSystemCapabilities Instance { get { return s_Instance;} }

    #endregion

    #region .ctor

      public SVNFileSystemCapabilities() {}

    #endregion

    #region Public

      public bool SupportsVersioning { get { return true; } }

      public bool SupportsTransactions { get { return false; } }

      public int MaxFilePathLength { get { return 255; } }

      public int MaxFileNameLength { get { return 255; } }

      public int MaxDirectoryNameLength { get { return 255; } }

      public ulong MaxFileSize { get { return 2 * (2 ^ 30); } }

      public char[] PathSeparatorCharacters { get { return PATH_SEPARATORS; } }

      public bool IsReadonly { get { return true; } }

      public bool SupportsSecurity { get { return true; } }

      public bool SupportsCustomMetadata { get { return false; } }

      public bool SupportsDirectoryRenaming { get { return false; } }

      public bool SupportsFileRenaming { get { return false; } }

      public bool SupportsStreamSeek { get { return false; } }

      public bool SupportsFileModification { get { return false; } }

      public bool SupportsCreationTimestamps { get { return true; } }

      public bool SupportsModificationTimestamps { get { return true; } }

      public bool SupportsLastAccessTimestamps { get { return false; } }

      public bool SupportsReadonlyDirectories { get { return true; } }

      public bool SupportsReadonlyFiles { get { return true; } }

      public bool SupportsCreationUserNames { get { return false; } }

      public bool SupportsModificationUserNames { get { return false; } }

      public bool SupportsLastAccessUserNames { get { return false; } }

      public bool SupportsFileSizes { get { return false; } }

      public bool SupportsDirectorySizes { get { return false; } }

      public bool SupportsAsyncronousAPI { get { return false; } }

    #endregion

  } //SVNFileSystemCapabilities

}
