/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps;
using Azos.Collections;

namespace Azos.Time
{
  /// <summary>
  /// Normally this class should never be used as the dafult EventTimer is always present instead of nop
  /// </summary>
  public sealed class NOPEventTimer : ApplicationComponent, IEventTimerImplementation
  {
    private static NOPEventTimer s_Instance = new NOPEventTimer();

    public NOPEventTimer() {}

    public static NOPEventTimer Instance { get { return s_Instance;}}



    public int ResolutionMs{ get { return 1000;} set {}}

    public void __InternalRegisterEvent(Event evt)
    {

    }

    public void __InternalUnRegisterEvent(Event evt)
    {

    }


    public IRegistry<Event> Events
    {
      get { return new Registry<Event>(); }
    }

    public bool InstrumentationEnabled { get {return false; } set {}}


    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters
    {
      get { return Enumerable.Empty<KeyValuePair<string, Type>>(); }
    }

    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return Enumerable.Empty<KeyValuePair<string, Type>>();
    }

    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      value = null;
      return false;
    }

    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return false;
    }

    public void Configure(Conf.IConfigSectionNode node)
    {

    }
  }
}
