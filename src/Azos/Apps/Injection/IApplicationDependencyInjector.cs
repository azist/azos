using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Apps.Injection
{
  /// <summary>
  /// Declares entities that process [Inject]-decorated fields and IApplicationInjection on
  /// a target objects passed into InjectInto(target) method
  /// </summary>
  public interface IApplicationDependencyInjector : IApplicationComponent
  {
    void InjectInto(object target);
  }

  /// <summary>
  /// Denotes entities that implement custom application injection code, e.g.
  /// a data document may implement this interface to set its internal module
  /// dependencies by code (vs using [Inject] attributes).
  /// </summary>
  public interface IApplicationInjection
  {
    /// <summary>
    /// Sets application context on an implementing entity.
    /// Return true to signify that app context has been set and
    /// no further processing of attributes shall be done.
    /// </summary>
    /// <param name="application">Application instance to inject</param>
    /// <returns>True if the injection is completed</returns>
    bool InjectApplication(IApplication application);
  }
}
