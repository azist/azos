using System.Linq;

using Azos.Data.Modeling;
using Azos.Data.Modeling.DataTypes;

namespace Azos.Sky.Social.Graph.Server.Data.Schema
{
  public abstract class GSDomain : RDBMSDomain
  {
    protected GSDomain() : base() { }
  }


  public abstract class GSEnum : GSDomain
  {
    public DBCharType Type;

    public int Size;

    public string[] Values;

    protected GSEnum(DBCharType type, string values)
    {
      Type = type;
      var vlist = values.Split('|');
      Size = vlist.Max(v => v.Trim().Length);
      if (Size < 1) Size = 1;
      Values = vlist;
    }

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return Type == DBCharType.Varchar ? "VARCHAR({0})".Args(Size) : "CHAR({0})".Args(Size);
    }

    public override string GetColumnCheckScript(RDBMSCompiler compiler, RDBMSEntity column, Compiler.Outputs outputs)
    {
      var enumLine = string.Join(", ", Values.Select(v => compiler.EscapeString(v.Trim())));
      return compiler.TransformKeywordCase("check ({0} in ({1}))")
                      .Args(
                            compiler.GetQuotedIdentifierName(RDBMSEntityType.Column, column.TransformedName),
                            enumLine
                          );
    }
  }


  public abstract class GSGDID : GSDomain
  {
    public readonly bool Required;

    public GSGDID(bool required)
    {
      Required = required;
    }

    public override bool? GetColumnRequirement(RDBMSCompiler compiler)
    {
      return Required;
    }

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return Required ? "BINARY(12)" : "VARBINARY(12)";
    }
  }


  public abstract class GSGDIDRef : GSGDID
  {
    public GSGDIDRef(bool required) : base(required) { }

    public override void TransformColumnName(RDBMSCompiler compiler, RDBMSEntity column)
    {
      column.TransformedName = "G_{0}".Args(column.TransformedName);
    }
  }


  public sealed class GSRequiredGDID    : GSGDID    { public GSRequiredGDID()    : base(true)  { } }
  public sealed class GSNullableGDID    : GSGDID    { public GSNullableGDID()    : base(false) { } }
  public sealed class GSRequiredGDIDRef : GSGDIDRef { public GSRequiredGDIDRef() : base(true)  { } }
  public sealed class GSNullableGDIDRef : GSGDIDRef { public GSNullableGDIDRef() : base(false) { } }


  public sealed class GSBool : GSEnum { public const int MAX_LEN = 1;  public GSBool() : base(DBCharType.Char, "T|F") { } }

  public sealed class GSDescription : GSDomain
  {
    public const int MAX_LEN = 64;

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "varchar({0})".Args(MAX_LEN);
    }
  }


  public sealed class GSNodeName : GSDomain
  {
    public const int MAX_LEN = 48;

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "varchar({0})".Args(MAX_LEN);
    }
  }


  /// <summary>
  /// Node type such as: User, Room, Forum, Group etc.
  /// </summary>
  public sealed class GSNodeType : GSDomain
  {
    public const int MIN_LEN = 1;
    public const int MAX_LEN = 8; // So we can pack in ulong

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "char({0})".Args(MAX_LEN);
    }
  }

  /// <summary> Such as Subscription parameters </summary>
  public sealed class GSParameters : GSDomain
  {
    public const int MIN_LEN = 3;
    public const int MAX_LEN = 1024;

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "varchar({0})".Args(MAX_LEN);
    }
  }


  /// <summary>
  /// No second fraction resolution
  /// </summary>
  public sealed class GSTimestamp : GSDomain
  {
    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "datetime(0)";
    }
  }

  public sealed class GSFriendListID : GSDomain
  {
    public const int MIN_LEN = 1;
    public const int MAX_LEN = 16;

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "varchar({0})".Args(MAX_LEN);
    }
  }

  public sealed class GSFriendListIDs : GSDomain
  {
    public const int MIN_LEN = 1;
    public const int MAX_LEN = (GSFriendListID.MAX_LEN+1/*for comma*/)*12;//12 lists max

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "varchar({0})".Args(MAX_LEN);
    }
  }

  public sealed class GSFriendStatus : GSEnum
  {
    public const int MAX_LEN = 1;

    public const string PENDING  = "P";
    public const string APPROVED = "A";
    public const string DENIED  =  "D";
    public const string BANNED  =  "B";

    public const string VALUE_LIST = "P: Pending, A: Approved, D: Denied, B: Banned";

    public static string ToDomainString(FriendStatus status)
    {
      switch (status)
      {
        case FriendStatus.Approved:  return GSFriendStatus.APPROVED;
        case FriendStatus.Denied:    return GSFriendStatus.DENIED;
        case FriendStatus.Banned:    return GSFriendStatus.BANNED;
        default: return GSFriendStatus.PENDING;
      }
    }

    public static FriendStatus ToFriendStatus(string status)
    {
      if (status.EqualsOrdIgnoreCase(GSFriendStatus.APPROVED))return FriendStatus.Approved;
      if (status.EqualsOrdIgnoreCase(GSFriendStatus.DENIED))  return FriendStatus.Denied;
      if (status.EqualsOrdIgnoreCase(GSFriendStatus.BANNED))  return FriendStatus.Banned;

      return FriendStatus.Pending;
    }

    public GSFriendStatus() : base(DBCharType.Char, "P|A|D|B") { }
  }


  public sealed class GSFriendshipRequestDirection : GSEnum
  {
    public const int MAX_LEN = 1;

    public const string I      = "I";
    public const string FRIEND = "F";

    public const string VALUE_LIST = "I: I myself, F: Friend";

    public static string ToDomainString(FriendshipRequestDirection dir)
    {
      if (dir==FriendshipRequestDirection.Friend) return FRIEND;
      return I;
    }

    public static FriendshipRequestDirection ToFriendshipRequestDirection(string status)
    {
      if (status.EqualsOrdIgnoreCase(GSFriendshipRequestDirection.FRIEND))return FriendshipRequestDirection.Friend;

      return FriendshipRequestDirection.I;
    }

    public GSFriendshipRequestDirection() : base(DBCharType.Char, "I|F") { }
  }


  public sealed class GSFriendVisibility : GSEnum
  {
    public const int MAX_LEN = 1;

    public const string ANYONE  = "A";
    public const string PUBLIC  = "P";
    public const string FRIENDS = "F";
    public const string PRIVATE = "T";

    public const string VALUE_LIST = "A: Anyone, P: Public, F: Friends, T: Private";

    public static string ToDomainString(FriendVisibility vis)
    {
      switch(vis)
      {
        case FriendVisibility.Anyone:  return GSFriendVisibility.ANYONE;
        case FriendVisibility.Friends: return GSFriendVisibility.FRIENDS;
        case FriendVisibility.Public:  return GSFriendVisibility.PUBLIC;
        default: return GSFriendVisibility.PRIVATE;
      }
    }

    public static FriendVisibility ToFriendVisibility(string status)
    {
      if (status.EqualsOrdIgnoreCase(GSFriendVisibility.ANYONE)) return FriendVisibility.Anyone;
      if (status.EqualsOrdIgnoreCase(GSFriendVisibility.PUBLIC)) return FriendVisibility.Public;
      if (status.EqualsOrdIgnoreCase(GSFriendVisibility.FRIENDS))return FriendVisibility.Friends;

      return FriendVisibility.Private;
    }

    public GSFriendVisibility() : base(DBCharType.Char, "A|P|F|T") { }
  }


  public sealed class GSBSONData : GSDomain
  {
    public const int MAX_LEN = 256;

    public readonly bool Required;

    public GSBSONData()
    {
      Required = false;
    }

    public GSBSONData(bool required)
    {
      Required = required;
    }

    public override bool? GetColumnRequirement(RDBMSCompiler compiler)
    {
      return Required;
    }
    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return Required ? "BINARY({0})".Args(MAX_LEN) : "VARBINARY({0})".Args(MAX_LEN);
    }
  }

  public class GSCounter : GSDomain
  {
    public GSCounter() : base() { }

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "BIGINT(8) UNSIGNED";
    }
  }

  public sealed class GSMessageType : GSDomain
  {
    public const int MIN_LEN = 1;
    public const int MAX_LEN = 8; // So we can pack in ulong

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "char({0})".Args(MAX_LEN);
    }
  }

  public sealed class GSRating : GSDomain
  {

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "tinyint";
    }
  }

  public sealed class GSCommentMessage : GSDomain
  {
    public const int MIN_LEN = 1;
    public const int MAX_LEN = 1024;

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "varchar({0})".Args(MAX_LEN);
    }
  }

  public sealed class GSLike : GSDomain
  {

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "int";
    }
  }

  public sealed class GSPublicationState : GSEnum
  {
    public const int MAX_LEN = 1;

    public const string PRIVATE  = "R";
    public const string PUBLIC = "P";
    public const string FRIEND = "F";
    public const string DELETED = "D";

    public const string VALUE_LIST = "R: Private, P: Public, F:Friend, D:Deleted";

    public static string ToDomainString(PublicationState status)
    {
      switch (status)
      {
        case PublicationState.Private:   return GSPublicationState.PRIVATE;
        case PublicationState.Public:    return GSPublicationState.PUBLIC;
        case PublicationState.Friend:    return GSPublicationState.FRIEND;
        case PublicationState.Deleted:    return GSPublicationState.DELETED;
        default: return GSPublicationState.PUBLIC;
      }
    }

    public static PublicationState ToPublicationState(string status)
    {
      if (status.EqualsOrdIgnoreCase(GSPublicationState.PRIVATE))return PublicationState.Private;
      if (status.EqualsOrdIgnoreCase(GSPublicationState.PUBLIC))  return PublicationState.Public;
      if (status.EqualsOrdIgnoreCase(GSPublicationState.FRIEND))  return PublicationState.Friend;
      if (status.EqualsOrdIgnoreCase(GSPublicationState.DELETED))  return PublicationState.Deleted;

      return PublicationState.Public;
    }

    public GSPublicationState() : base(DBCharType.Char, "C|P") { }
  }

  public sealed class GSDimension : GSDomain
  {
    public const int MIN_LEN = 1;
    public const int MAX_LEN = 8; // So we can pack in ulong

    public override string GetTypeName(RDBMSCompiler compiler)
    {
      return "char({0})".Args(MAX_LEN);
    }
  }
}
