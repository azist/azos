/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;


namespace Azos.Apps
{
  /// <summary>
  /// Designates service-derivative classes that should NOT be auto-started by the app container
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
  public class ApplicationDontAutoStartServiceAttribute : Attribute
  {
    public ApplicationDontAutoStartServiceAttribute(){}
  }
}
