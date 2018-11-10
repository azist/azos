/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;


namespace Azos.Serialization.BSON
{
  /// <summary>
  /// Denotes types that support BSON serialization identified by GUID.
  /// The Guid is used to rehydrate an actual CLR type from BSON stream
  /// </summary>
  [AttributeUsage( AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  public class BSONSerializableAttribute : Apps.GuidTypeAttribute
  {
    public BSONSerializableAttribute(string typeGuid) : base(typeGuid){ }
  }
}
