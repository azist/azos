using System;
using System.IO;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Represents a read-only tuple of { gVolume: GDID, gComment: GDID}.
  /// The gRating is a globally-unique ID however graph system prepends it with
  /// gVolume which allows for instant location of a concrete data store which holds gRating.
  /// </summary>
  [Serializable]
  public struct CommentID : IEquatable<CommentID>, IJSONWritable
  {
    public CommentID(GDID gVolume, GDID gComment) { G_Volume = gVolume; G_Comment = gComment; }

    /// <summary>
    /// Sharding GDID used to instantly find the data store shard where data is kept
    /// </summary>
    public readonly GDID G_Volume;

    /// <summary>
    ///The global unique id of a comment
    /// </summary>
    public readonly GDID G_Comment;

    /// <summary>
    /// True if struct is unassigned
    /// </summary>
    public bool Unassigned { get{return G_Volume.IsZero;} }

    /// <summary>
    /// True if G_Volume | G_Comment isZero
    /// </summary>
    public bool IsZero
    {
      get { return Unassigned || G_Comment.IsZero; }
    }

    public bool Equals(CommentID other)
    {
      return this.G_Volume == other.G_Volume &&
             this.G_Comment == other.G_Comment;
    }

    public override bool Equals(object obj)
    {
      if (!(obj is CommentID)) return false;
      return this.Equals((CommentID)obj);
    }

    public override int GetHashCode()
    {
      return G_Volume.GetHashCode() ^ G_Comment.GetHashCode();
    }

    public string Stringify()
    {
      var eLink = new ELink(G_Volume, G_Comment.Bytes);
      return eLink.Link;
    }

    public override string ToString()
    {
      return "Comment [{0}@{1}]".Args(G_Volume, G_Comment);
    }

    public void WriteAsJSON(TextWriter wri, int nestingLevel, JSONWritingOptions options = null)
    {
      wri.Write('"');
      wri.Write(Stringify());
      wri.Write('"');
    }

    public static CommentID Parse(string str)
    {
      CommentID result;
      if (!TryParse(str, out result))
        throw new SocialException("CommentID.Parse({0})".Args(str));

      return result;
    }

    public static bool TryParse(string str, out CommentID commentId)
    {
      try
      {
        var eLink = new ELink(str);
        var gVolume = eLink.GDID;
        var gComment = new GDID(eLink.Metadata);
        commentId = new CommentID(gVolume, gComment);
        return true;
      }
      catch
      {
        commentId = default(CommentID);
        return false;
      }
    }
  }
}