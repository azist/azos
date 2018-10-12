using System;


namespace Azos.Serialization.BSON
{
  /// <summary>
  /// Denotes types that support BSON serialization by GUID
  /// </summary>
  [AttributeUsage( AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  public class BSONSerializableAttribute : Apps.GuidTypeAttribute
  {
    public BSONSerializableAttribute(string typeGuid) : base(typeGuid){ }
  }
}
