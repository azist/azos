/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Apps.ZoneGovernor
{
  /// <summary>
  /// Thrown to indicate AZGOV related problems
  /// </summary>
  [Serializable]
  public class AZGOVException : Sky.SkyException
  {
    public AZGOVException() : base() { }
    public AZGOVException(string message) : base(message) { }
    public AZGOVException(string message, Exception inner) : base(message, inner) { }
    protected AZGOVException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
