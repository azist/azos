/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Data;

using Azos.Sky;
using Azos.Sky.Workers;

namespace WinFormsTestSky.Workers
{
  [Process("53339D4F-961A-4C19-80E3-54018E917AD5", Description = "Tezt Process")]
  public class TeztProcess : Process
  {
    protected override ResultSignal DoAccept(IProcessHost host, Signal signal) { return null; }


    protected internal override void Merge(IProcessHost host, DateTime utcNow, Process another)
    {
    }
  }
}
