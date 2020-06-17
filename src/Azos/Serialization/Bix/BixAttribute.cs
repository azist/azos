/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Decorates data document types that support Bix serialization -
  /// types that generate Bix serialization/deserialization method cores.
  /// This class can also be cross-used for type discriminator function in JSON serialization
  /// of TypedDocs, since the Bix attribute is already decorating most types, it can be re-used
  /// for Json polymorphism
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  public sealed class BixAttribute : GuidTypeAttribute
  {
    public BixAttribute(string typeGuid) : base(typeGuid){ }
  }
}
