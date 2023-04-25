///*<FILE_LICENSE>
// * Azos (A to Z Application Operating System) Framework
// * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
// * See the LICENSE file in the project root for more information.
//</FILE_LICENSE>*/

//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//using Azos.Data;
//using Azos.Data.Business;

//namespace Azos.Sky.Blob
//{
//  /// <summary>
//  /// Fully qualified file name
//  /// </summary>
//  public struct FQN
//  {
//    public readonly Atom Volume;
//    public readonly string Name;
//  }


//  /// <summary>
//  /// Contract for working with SKY BLOB file system.
//  /// Blob file systems stores named files with tags which can be used to query for files.
//  /// Tags are used for paths.
//  /// The file itself does not support path and files are written out by volumes:atom.
//  /// </summary>
//  public interface IBlob
//  {
//    /// <summary>
//    /// Returns a sequence of volume ids known to the system
//    /// </summary>
//    Task<IEnumerable<Atom>> GetVolumeNamesAsync();

//    Task<IEnumerable<FileInfo>> FindFilesAsync(FileFilter filter);

//    Task<BlobFileStream> OpenFileAsync(Fqn fn);

//    Task<ChangeResult> DeleteAsync(Fqn fn);
//  }

//  public interface IBlobLogic : IBlob, IBusinessLogic
//  {
//  }
//}
