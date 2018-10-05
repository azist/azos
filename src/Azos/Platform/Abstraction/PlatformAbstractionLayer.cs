/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Platform.Abstraction
{
  /// <summary>
  /// Internal framework class, business app developers should not use this class directly as it provides a lower-level services
  /// to the higher level code like Azos.Graphics.Canvas, Image etc.
  /// PAL provides process-global injection point for PAL.
  /// The injection of a concrete runtime is done at process entry point which is
  /// statically linked against a concrete runtime.
  /// </summary>
  public static class PlatformAbstractionLayer
  {
    private static object s_Lock = new object();
    private static PALImplementation s_Implementation;

    /// <summary>
    /// Returns true when platform is NET Core
    /// </summary>
    public static bool IsNetCore
    {
      get
      {
        var result = s_Implementation;
        if (result==null) throw new PALException(StringConsts.PAL_ABSTRACTION_IS_NOT_PROVIDED_ERROR.Args("Implementation"));
        return result.IsNetCore;
      }
    }

    /// <summary>
    /// Returns the name of the platform implementation, for example ".NET 4.7.1"
    /// </summary>
    public static string PlatformName { get => s_Implementation?.Name; }

    /// <summary>
    /// Abstracts functions related to working with graphical images
    /// </summary>
    public static Graphics.IPALGraphics Graphics
    {
      get
      {
        var result = s_Implementation?.Graphics;
        if (result==null) throw new PALException(StringConsts.PAL_ABSTRACTION_IS_NOT_PROVIDED_ERROR.Args("Graphics"));
        return result;
      }
    }

    /// <summary>
    /// Abstracts functions related to obtaining the machine parameters such as CPU and RAM
    /// </summary>
    public static IPALMachineInfo MachineInfo
    {
      get
      {
        var result = s_Implementation?.MachineInfo;
        if (result==null) throw new PALException(StringConsts.PAL_ABSTRACTION_IS_NOT_PROVIDED_ERROR.Args("MachineInfo"));
        return result;
      }
    }

    /// <summary>
    /// Abstracts functions related to working with file system
    /// </summary>
    public static IPALFileSystem FileSystem
    {
      get
      {
        var result = s_Implementation?.FileSystem;
        if (result==null) throw new PALException(StringConsts.PAL_ABSTRACTION_IS_NOT_PROVIDED_ERROR.Args("FileSystem"));
        return result;
      }
    }


    /// <summary>
    /// Sets invariant culture for the whole process regardless of machine's culture
    /// </summary>
    public static void SetProcessInvariantCulture()
    {
      //For now it looks like this behavior does not need to be abstracted away into implementation layer,
      //this may change in future, t4 this is in PAL
      System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    }


    /// <summary>
    /// System method. Developers do not call. Entry point invoked by runtime implementation modules.
    /// </summary>
    public static void ____SetImplementation(PALImplementation implementation)
    {
      if (implementation==null)
        throw new PALException(StringConsts.ARGUMENT_ERROR+"{0}.{1}(implementation=null)".Args(nameof(PlatformAbstractionLayer), nameof(____SetImplementation)));

      lock(s_Lock)
      {
        if (s_Implementation!=null)
          throw new PALException(StringConsts.PAL_ALREADY_SET_ERROR);

        s_Implementation = implementation;
      }
    }
  }
}
