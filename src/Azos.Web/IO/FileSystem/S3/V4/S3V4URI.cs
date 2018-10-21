
using System;
using System.Collections.Generic;

using Azos.Conf;
using Azos.IO.FileSystem.S3.V4.S3V4Sign;

namespace Azos.IO.FileSystem.S3.V4
{
  public class S3V4URI
  {
    #region Static

      public static S3V4URI CreateFolder(string path)
      {
        S3V4URI uri = new S3V4URI(path.ToDirectoryPath());
        return uri;
      }

      public static S3V4URI CreateFile(string path)
      {
        S3V4URI uri = new S3V4URI(path);
        return uri;
      }

    #endregion

    #region .ctor

      public S3V4URI(string path)
      {
        m_Path = path;//.TrimEnd('/');
        m_Uri = new Uri(path);

        S3V4URLHelpers.Parse(m_Uri, out m_Bucket, out m_Region, out m_LocalPath, out m_QueryParams);

        m_LocalName = m_Uri.GetLocalName();
        m_ParentPath = m_Uri.GetParentURL();
      }

    #endregion

    #region Pvt/Prot/Int Fields

      private string m_Path;
      private Uri m_Uri;

      private string m_Bucket;
      private string m_Region;
      private string m_LocalPath;
      private string m_LocalName;
      private IDictionary<string, string> m_QueryParams;

      private string m_ParentPath;

    #endregion

    #region Properties

      public string Path { get { return m_Path; } }

      public string Bucket { get { return m_Bucket; } }

      public string Region { get { return m_Region; } }

      public string LocalPath { get { return m_LocalPath; } }

      public string LocalName { get { return m_LocalName; } }

      public IDictionary<string, string> QueryParams { get { return m_QueryParams; } }

      public string ParentPath { get { return m_ParentPath; } }

    #endregion

  } //S3V4URI

}
