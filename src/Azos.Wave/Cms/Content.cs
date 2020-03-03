/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Security.Cryptography;
using System.Text;

using Azos.Serialization.JSON;

namespace Azos.Wave.Cms
{
  /// <summary>
  /// Represents content data returned from Content Management Systems
  /// </summary>
  [Serializable]
  public sealed class Content
  {
    private Content() { }//used by serializer to speed-up the factory lambda call

    /// <summary>
    /// Create an instance of string content
    /// </summary>
    public Content(ContentId id,
                   NLSMap name,
                   string stringContent,
                   string contentType,
                   LangInfo? lang = null,
                   string attachmentFileName = null,
                   string createUser = null,
                   string modifyUser = null,
                   DateTime? createDate = null,
                   DateTime? modifyDate = null)
    {
      if (!id.IsAssigned) throw new CmsException($"{StringConsts.ARGUMENT_ERROR} {nameof(Content)}.ctor(id.!IsAssigned)");
      m_Id = id;
      m_Name = name;
      m_StringContent = stringContent.NonBlank(nameof(stringContent));
      m_ContentType = contentType.NonBlank(nameof(contentType));
      m_Language = LangInfo.Default(lang);
      m_AttachmentFileName = attachmentFileName;
      m_ETag = getETag(m_ContentType, m_StringContent, null);
      m_CreateDate = createDate;
      m_ModifyDate = modifyDate;
      m_CreateUser = createUser;
      m_ModifyUser = modifyUser;
    }

    /// <summary>
    /// Create an instance of binary content
    /// </summary>
    public Content(ContentId id,
                   NLSMap name,
                   byte[] binaryContent,
                   string contentType,
                   LangInfo? lang = null,
                   string attachmentFileName = null,
                   string createUser = null,
                   string modifyUser = null,
                   DateTime? createDate = null,
                   DateTime? modifyDate = null)
    {
      if (!id.IsAssigned) throw new CmsException($"{StringConsts.ARGUMENT_ERROR} {nameof(Content)}.ctor(id.!IsAssigned)");
      m_Id = id;
      m_Name = name;
      m_BinaryContent = binaryContent.NonNull(nameof(binaryContent));
      m_ContentType = contentType.NonBlank(nameof(contentType));
      m_Language = LangInfo.Default(lang);
      m_AttachmentFileName = attachmentFileName;
      m_ETag = getETag(m_ContentType, null, m_BinaryContent);
      m_CreateDate = createDate;
      m_ModifyDate = modifyDate;
      m_CreateUser = createUser;
      m_ModifyUser = modifyUser;
    }

    /// <summary>
    /// Create an instance of content which contains both string and binary data representation
    /// </summary>
    public Content(ContentId id,
                   NLSMap name,
                   string stringContent,
                   byte[] binaryContent,
                   string contentType,
                   LangInfo? lang =null,
                   string attachmentFileName = null,
                   string createUser = null,
                   string modifyUser = null,
                   DateTime? createDate = null,
                   DateTime? modifyDate = null)
    {
      if (!id.IsAssigned) throw new CmsException($"{StringConsts.ARGUMENT_ERROR} {nameof(Content)}.ctor(id.!IsAssigned)");
      m_Id = id;
      m_Name = name;
      m_StringContent = stringContent;
      m_BinaryContent = binaryContent;

      if (m_StringContent == null && m_BinaryContent == null)
        throw new CmsException($"{StringConsts.ARGUMENT_ERROR} {nameof(Content)}.ctor(stringContent==null && binaryContent==null)");

      m_ContentType = contentType.NonBlank(nameof(contentType));
      m_Language = LangInfo.Default(lang);
      m_AttachmentFileName = attachmentFileName;
      m_ETag = getETag(m_ContentType, m_StringContent, m_BinaryContent);
      m_CreateDate = createDate;
      m_ModifyDate = modifyDate;
      m_CreateUser = createUser;
      m_ModifyUser = modifyUser;
    }

    private static readonly Encoding UTF8NOBOM = new UTF8Encoding(false);

    private static string getETag(string ctp, string scontent, byte[] bcontent)
    {
      using (var md5 = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
      {
        if (ctp.IsNotNullOrWhiteSpace())
          md5.AppendData(UTF8NOBOM.GetBytes(ctp));

        if (scontent.IsNotNullOrWhiteSpace())
          md5.AppendData(UTF8NOBOM.GetBytes(scontent));

        if (bcontent!=null)
          md5.AppendData(bcontent);

        return md5.GetHashAndReset().ToWebSafeBase64();
      }
    }

    private readonly ContentId m_Id;
    private NLSMap m_Name;
    private readonly LangInfo m_Language;
    private readonly string m_AttachmentFileName;

    private readonly string m_ETag;
    private string m_CreateUser;
    private string m_ModifyUser;
    private DateTime? m_CreateDate;
    private DateTime? m_ModifyDate;
    private readonly string m_ContentType;
    private readonly string m_StringContent;
    private readonly byte[] m_BinaryContent;


    /// <summary>
    /// Returns the ID which identifies this content in a CMS
    /// </summary>
    public ContentId Id => m_Id;

    /// <summary>
    /// Returns the Name:Description pairs which describe the content represented by this instance.
    /// The Name is returned in NLS (Native Language Support) format
    /// </summary>
    /// <remarks>
    /// Although this content body is localized per .Language property, the .Name
    /// is returned in a NLS format
    /// </remarks>
    public NLSMap Name => m_Name;

    /// <summary>
    /// The language info (such as ISO code) of this content, e.g. 'eng', 'deu' etc..
    /// Note: Binary content may need localization as well (e.g. an image containing rendered Spanish text)
    /// </summary>
    public LangInfo Language => m_Language;

    /// <summary>
    /// For certain content types, such as the ones that can be downloaded as an attachment, returns a suggested
    /// file name per ContentId. The specifics are up to the Cms source. Returns null if not applicable
    /// </summary>
    public string AttachmentFileName => m_AttachmentFileName;


    /// <summary>
    /// Who created the content, or null
    /// </summary>
    public string CreateUser => m_CreateUser;

    /// <summary>
    /// Who modified the content or null
    /// </summary>
    public string ModifyUser => m_ModifyUser;

    /// <summary>
    /// When content was created (UTC timestamp) or null if feature is not supported
    /// </summary>
    public DateTime? CreateDate => m_CreateDate;

    /// <summary>
    /// When content was last modified (UTC timestamp) or null if feature is not supported
    /// </summary>
    public DateTime? ModifyDate => m_ModifyDate;

    /// <summary>
    /// Returns a binary fingerprint calculated from string/binary content.
    /// The fingerprint is typically used for ETag http header value
    /// </summary>
    public string ETag => m_ETag;

    /// <summary>
    /// The standardized MIME content type, e.g. 'imgage/png','text/plain' etc.
    /// The content type is mapped by the CMS backing source
    /// </summary>
    /// <remarks>
    /// You can use GetTypeMapping() extension method on a content object to get
    /// detailed local content mapping for the content:
    /// <code>
    ///   //Azos.Web.ContentType.Mapping type is returned
    ///   var mapping = content.GetTypeMapping();
    /// </code>
    /// </remarks>
    public string ContentType => m_ContentType;

    /// <summary>
    /// Tries to retrieve content as string. If the content is binary, BASE64 encoded version is returned.
    /// You can check for presence of string content using .IsString property
    /// </summary>
    public string StringContent => m_StringContent ?? Convert.ToBase64String(m_BinaryContent);

    /// <summary>
    /// Gets the content as raw byte buffer. If content is binary it returns it as-is.
    /// If the content is string, it tries to decode it using Base64.
    /// You can check for presence of binary content using '.IsBinary' property
    /// </summary>
    public byte[] BinaryContent => m_BinaryContent ?? Convert.FromBase64String(StringContent);

    /// <summary>
    /// True if the content has a textual/string representation
    /// </summary>
    public bool IsString => m_StringContent != null;

    /// <summary>
    /// True if the content has a binary representation
    /// </summary>
    public bool IsBinary => m_BinaryContent != null;

  }
}
