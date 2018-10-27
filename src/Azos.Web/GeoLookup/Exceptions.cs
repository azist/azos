/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Web.GeoLookup
{
  /// <summary>
  /// Base exception class thrown by geo-related logic
  /// </summary>
  [Serializable]
  public class GeoException : WebException
  {
    public GeoException() { }
    public GeoException(string message) : base(message) { }
    public GeoException(string message, Exception inner) : base(message, inner) { }
    protected GeoException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
