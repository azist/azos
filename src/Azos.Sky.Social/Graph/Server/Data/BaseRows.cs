using System;

using Azos.Data;


namespace Azos.Sky.Social.Graph.Server.Data
{
  public abstract class BaseRow : AmorphousTypedDoc
  {
    public BaseRow() : base()
    {

    }
  }

  /// <summary>
  /// Base class for all GraphRows
  /// </summary>
  public abstract class BaseRowWithGDID : BaseRow
  {
    public static UniqueSequenceAttribute GetGDIDAttribute(Type tRow)
    {
      if (!typeof(BaseRow).IsAssignableFrom(tRow))
        throw new GraphException("{0}: {1} is not BaseRow".Args("GetGDIDAttribute", tRow.FullName));

      var attr = UniqueSequenceAttribute.GetForRowType(tRow);
      if (attr == null ||
          attr.Scope.IsNullOrWhiteSpace() ||
          attr.Sequence.IsNullOrWhiteSpace())
        throw new GraphException("Both scope and sequence must be defined in UniqueSequenceAttribute decorating {0}".Args(tRow.FullName));

      return attr;
    }

    public BaseRowWithGDID() : base()
    {

    }

    public BaseRowWithGDID(bool newGdid) : base()
    {
      if (newGdid)
      {
        GDID = GenerateNewGDID();
      }
    }

    /// <summary>
    /// Generates new gdid properly scoping it and naming it for this row (see GDIDSequenceName)
    /// </summary>
    public static GDID GenerateNewGDID(Type t)
    {
      var attr = GetGDIDAttribute(t);
      return GraphOperationContext.Instance.DataStore.GdidGenerator.GenerateOneGdid(attr.Scope, attr.Sequence);
    }

    /// <summary>
    /// Generates new gdid properly scoping it and naming it for this row (see GDIDSequenceName)
    /// </summary>
    public GDID GenerateNewGDID()
    {
      return BaseRowWithGDID.GenerateNewGDID(this.GetType());
    }

    /// <summary>
    /// Primary KEY
    /// </summary>
    [Field(targetName: TargetedAttribute.ANY_TARGET, required: true, key: true, visible: false)]
    public GDID GDID { get ; set; }
  }
}
