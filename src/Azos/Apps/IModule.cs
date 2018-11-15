/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

namespace Azos.Apps
{
  /// <summary>
  /// Describes application modules - entities that contain business domain logic of the application or
  /// general system logic (e.g. financial logic, complex image rendering service, social network mix-in etc.)
  /// </summary>
  public interface IModule : IApplicationComponent, Collections.INamed, Collections.IOrdered
  {
    /// <summary>
    /// References a parent logic module, or null if this is a root module injected in the application container
    /// </summary>
    IModule ParentModule { get; }

    /// <summary>
    /// Returns true when the module is injected in the parent context by the code, not configuration script
    /// </summary>
    bool IsHardcodedModule { get; }

    /// <summary>
    /// Enumerates an ordered collection of child modules and provides access by name
    /// </summary>
    Collections.IOrderedRegistry<IModule> ChildModules { get; }


    /// <summary>
    /// Gets a child module of the specified TModule type optionally applying a filter.
    /// If module is not found then exception is thrown. Contrast with TryGet()
    /// </summary>
    TModule Get<TModule>(Func<TModule, bool> filter = null) where TModule : class, IModule;

    /// <summary>
    /// Tries to get a child module of the specified TModule type optionally applying a filter.
    /// If module is not found then returns null. Contrast with Get()
    /// </summary>
    TModule TryGet<TModule>(Func<TModule, bool> filter = null) where TModule : class, IModule;

    /// <summary>
    /// Gets a child module of the specified TModule type with the specified name.
    /// If module is not found then exception is thrown. Contrast with TryGet()
    /// </summary>
    TModule Get<TModule>(string name) where TModule : class, IModule;

    /// <summary>
    /// Tries to get a child module of the specified TModule type with the specified name.
    /// If module is not found then returns null. Contrast with Get()
    /// </summary>
    TModule TryGet<TModule>(string name) where TModule : class, IModule;
  }

  /// <summary>
  /// Describes module implementation
  /// </summary>
  public interface IModuleImplementation : IModule, IDisposable, Conf.IConfigurable, Instrumentation.IInstrumentable
  {
    /// <summary>
    /// Called by the application container after all services have initialized.
    /// An implementation is expected to notify all subordinate (child) modules.
    /// The call is used to perform initialization tasks such as inter-service dependency fixups,
    /// initial data loads (e.g. initial cache fetch etc..) after everything has loaded in the application container.
    /// The implementation is expected to handle internal exceptions gracefully (i.e. use log etc.)
    /// </summary>
    void ApplicationAfterInit(IApplication application);

    /// <summary>
    /// Called by the application container before services shutdown.
    /// An implementation is expected to notify all subordinate (child) modules.
    /// The call is used to perform finalization tasks such as inter-service dependency cleanups,
    /// buffer flushes etc. before the application container starts to shutdown.
    /// The implementation is expected to handle internal exceptions gracefully (i.e. use log etc.)
    /// </summary>
    void ApplicationBeforeCleanup(IApplication application);
  }
}
