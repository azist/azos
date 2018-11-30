using System;
using System.Collections.Generic;

using Azos.Conf;

namespace Azos.Apps.Injection
{
  /// <summary>
  /// Designates entities that process [Inject]-decorated fields and IApplicationInjection on
  /// a target objects passed into InjectInto(target) method.
  /// Access application default injector using IApplication.DependencyInjector property
  /// </summary>
  public interface IApplicationDependencyInjector : IApplicationComponent
  {
    /// <summary>
    /// Injects application context along with its derivatives (e.g. modules, logs etc.) as
    /// specified by the field-level attribute decorations on a target instance
    /// </summary>
    /// <remarks>
    /// Since this method is declared on IApplicationComponent, it injects the application
    /// context of the self
    /// </remarks>
    void InjectInto(object target);

    /// <summary>
    /// Enumerates app injectable roots (root application chassis objects).
    /// This method is usually used by [Inject]-derived attributes for defaults
    /// </summary>
    IEnumerable<object> GetApplicationRoots();
  }

  public interface IApplicationDependencyInjectorImplementation : IApplicationDependencyInjector, IConfigurable, IDisposable
  {
  }

  /// <summary>
  /// Denotes entities that implement custom application injection code, e.g.
  /// a data document may implement this interface to set its internal module
  /// dependencies by code (vs. using [Inject] attributes).
  /// </summary>
  public interface IApplicationInjection
  {
    /// <summary>
    /// Sets application context on an implementing entity.
    /// Return true to signify that app context has been set and
    /// no further processing of attributes shall be done.
    /// </summary>
    /// <param name="injector">
    /// Injector instance performing the injection, use injector.App to get the injected application context
    /// </param>
    /// <returns>True if the injection is completed</returns>
    bool InjectApplication(IApplicationDependencyInjector injector);
  }
}
