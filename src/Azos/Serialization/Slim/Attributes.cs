/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;


namespace Azos.Serialization.Slim
{
  /// <summary>
  /// When set on a parameterless constructor, instructs the Slim serializer not to invoke
  ///  the ctor() on deserialization
  /// </summary>
  [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
  public class SlimDeserializationCtorSkipAttribute : Attribute
  {
    public SlimDeserializationCtorSkipAttribute(){ }
  }


  /// <summary>
  /// When set fails an attempt to serialize the decorated type
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
  public class SlimSerializationProhibitedAttribute : Attribute
  {
    public SlimSerializationProhibitedAttribute(){ }
  }

}
