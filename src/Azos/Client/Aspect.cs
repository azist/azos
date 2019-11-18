using System;
using System.Collections.Generic;
using System.Text;

using Azos.Collections;
using Azos.Conf;

namespace Azos.Client
{
  /// <summary>
  /// Provides abstraction of Aspects/Features which allow for injection of cross-cutting concern handlers
  /// for service calls. For example, an endpoint may be configured with ErrorClassificationAspect which
  /// tries to classify certain remote error conditions as ServiceLogic error
  /// </summary>
  public abstract class AspectBase : IAspect
  {
    protected AspectBase(IConfigSectionNode config)
    {
      ConfigAttribute.Apply(this, config);
      if (m_Name.IsNullOrWhiteSpace()) m_Name = "{0}-{1}".Args(GetType().Name, FID.Generate());
    }

    [Config]private string m_Name;
    [Config]private int m_Order;

    public string Name => m_Name;
    public int Order => m_Order;
  }
}
