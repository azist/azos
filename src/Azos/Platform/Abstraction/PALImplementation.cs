/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;

namespace Azos.Platform.Abstraction
{
  /// <summary>
  /// Provides base for all platform abstraction layer implementations
  /// </summary>
  public abstract class PALImplementation : ApplicationComponent, Collections.INamed
  {
    //note: PalImplementation gets allocated before any meaningful app container, hence
    //it uses NOPApplication as its chassis
    protected PALImplementation() : base(NOPApplication.Instance) { }


    public abstract string Name { get; }
    public abstract bool                  IsNetCore    { get; }
    public abstract Graphics.IPALGraphics Graphics     { get; }
    public abstract IPALMachineInfo       MachineInfo  { get; }
    public abstract IPALFileSystem        FileSystem   { get; }
  }
}
