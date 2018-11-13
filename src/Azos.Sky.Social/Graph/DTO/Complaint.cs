using System;

using Azos.Data;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Contains social comment data
  /// </summary>
  [Serializable]
  public struct Complaint
  {
    public Complaint(CommentID commentID,
                     GDID gComplaint,
                     GraphNode authorNode,
                     string kind,
                     string message,
                     DateTime createDate,
                     bool inUse)
    {
      CommentID = commentID;
      GDID = gComplaint;
      AuthorNode = authorNode;
      Kind = kind;
      Message = message;
      Create_Date = createDate;
      In_Use = inUse;
    }

    public readonly CommentID CommentID;
    public readonly GDID GDID;
    public readonly GraphNode AuthorNode;
    public readonly string Kind;
    public readonly string Message;
    public readonly DateTime Create_Date;
    public readonly bool In_Use;

    public override string ToString()
    {
      return "[{0}-{1}-{2}]: {3}-{4} by {5} ({6})".Args(CommentID.G_Volume, CommentID.G_Comment, GDID, Kind, Message, AuthorNode.OriginName, AuthorNode.GDID);
    }
  }
}
