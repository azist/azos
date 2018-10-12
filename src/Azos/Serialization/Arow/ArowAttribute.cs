using System;


namespace Azos.Serialization.Arow
{
  /// <summary>
  /// Denotes types that generate Arow ser/deser core
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  public sealed class ArowAttribute : Attribute
  {
  }
}
