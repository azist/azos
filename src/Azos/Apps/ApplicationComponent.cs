/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Instrumentation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Azos.Apps
{
  /// <summary>
  /// Provides marker contract requirement for an ApplicationComponent.
  /// This interface must be implemented only by the ApplicationComponent class.
  /// Components are the base building block of application service tree.
  /// </summary>
  public interface IApplicationComponent
  {
    /// <summary>
    /// References an application that this component services
    /// </summary>
    IApplication App { get; }

    /// <summary>
    /// Returns process/instance unique app component system id
    /// </summary>
    ulong ComponentSID { get; }

    /// <summary>
    /// Returns a reference to an object that this app component services/operates under, or null
    /// </summary>
    IApplicationComponent ComponentDirector { get;}

    /// <summary>
    /// Returns local computer time of component start (not from application container time)
    /// </summary>
    DateTime ComponentStartTime{ get; }

    /// <summary>
    /// Returns the common name used to identify the component, for example "Glue" for various IGlue implementations.
    /// This name is searched-by some management tools that allow to find component by this name that does not change between
    /// application restarts like ComponentSID does. Subordinate (non-root) components return null
    /// </summary>
    string ComponentCommonName { get; }


    /// <summary>
    /// Sets the log level for this component, if not defined then the component logger uses the director/log level
    /// via the ComponentEffectiveLogLevel property
    /// </summary>
    Log.MessageType? ComponentLogLevel { get; set; }


    /// <summary>
    /// Determines the effective log level for this component, taking it from director if it is not defined on this level
    /// </summary>
    Azos.Log.MessageType ComponentEffectiveLogLevel { get; }


    /// <summary>
    /// Returns  value for "Topic" log message field
    /// </summary>
    string ComponentLogTopic { get; }

    /// <summary>
    /// Returns a prefix used in "From" log message field
    /// </summary>
    string ComponentLogFromPrefix { get;}
  }



  /// <summary>
  /// An abstraction of a disposable application component - major implementation/functionality part of any app.
  /// Components logically subdivide application chassis so their instances may be discovered
  ///  by other parties, for example: one may iterate over all components in an application that support instrumentation and logging.
  ///  Services are sub-types of components.
  /// Use "ApplicationComponent.AllComponents" to access all components in the container
  /// </summary>
  [Serialization.Slim.SlimSerializationProhibited]
  public abstract class ApplicationComponent : DisposableObject, IApplicationComponent
  {
    #region .ctor

      protected ApplicationComponent(IApplication application) : this(application, null)
      {
      }

      protected ApplicationComponent(IApplicationComponent director) : this(director.NonNull(text: nameof(director)).App, director)
      {

      }

      private ApplicationComponent(IApplication application, IApplicationComponent director)
      {
        if (application==null)
          throw new AzosException(StringConsts.ARGUMENT_ERROR + nameof(ApplicationComponent) + ".ctor(application==null)");

        if (director is DisposableObject d && d.Disposed)
          throw new AzosException(StringConsts.ARGUMENT_ERROR+nameof(ApplicationComponent)+".ctor(director.Disposed)");

        if (director!=null && director.App!=application)//safeguard, this will never happen
          throw new AzosException(StringConsts.ARGUMENT_ERROR + nameof(ApplicationComponent) + ".ctor(application!=director.App");

        m_App = application;
        m_ComponentDirector = director;
        m_ComponentStartTime = DateTime.Now;

        lock(s_Instances)
        {
          s_SIDSeed++;
          m_ComponentSID = s_SIDSeed;
          Dictionary<ulong, ApplicationComponent> components;
          if (!s_Instances.TryGetValue(application, out components))
          {
            components = new Dictionary<ulong, ApplicationComponent>();
            s_Instances.Add(application, components);
          }
          components.Add(m_ComponentSID, this);
        }
      }

      protected override void Destructor()
      {
        lock(s_Instances)
        {
          var components = getComponentsOf(m_App);
          if (components!=null)
          {
            components.Remove(m_ComponentSID);
            if (components.Count==0)
              s_Instances.Remove(m_App);
          }
        }
      }


    #endregion

    #region Private Fields

      private static ulong s_SIDSeed;
      private static Dictionary<IApplication, Dictionary<ulong, ApplicationComponent>> s_Instances = new Dictionary<IApplication, Dictionary<ulong, ApplicationComponent>>();

      private readonly IApplication m_App;
      private readonly DateTime m_ComponentStartTime;
      private readonly ulong m_ComponentSID;
      private IApplicationComponent m_ComponentDirector;
    #endregion

    #region Properties

      /// <summary>
      /// Returns a thread-safe enumerable( a snapshot) of all known component instances
      /// </summary>
      public static IEnumerable<ApplicationComponent> AllComponents(IApplication app)
      {
        lock(s_Instances)
        {
          var dict = getComponentsOf(app);
          return dict?.Values.ToArray() ?? Enumerable.Empty<ApplicationComponent>();
        }
      }

      /// <summary>
      /// Returns an existing application component instance by its ComponentSID or null
      /// </summary>
      public static ApplicationComponent GetAppComponentBySID(IApplication app, ulong sid)
      {
        lock(s_Instances)
        {
          var dict = getComponentsOf(app);
          if (dict == null) return null;
          if (dict.TryGetValue(sid, out var result)) return result;
        }

        return null;
      }

      /// <summary>
      /// Returns an existing application component instance by its ComponentCommonName or null. The search is case-insensitive
      /// </summary>
      public static ApplicationComponent GetAppComponentByCommonName(IApplication app, string name)
      {
        if (name.IsNullOrWhiteSpace()) return null;

        name = name.Trim();

        lock(s_Instances)
        {
          var dict = getComponentsOf(app);
          if (dict == null) return null;
          var result = dict.Values.FirstOrDefault( c => {
              var ccn = c.ComponentCommonName;
              return ccn!=null && name.EqualsIgnoreCase(ccn.Trim());
          });
          return result;
        }
      }


      /// <summary>
      /// References application that this component services
      /// </summary>
      public IApplication App => m_App;

      /// <summary>
      /// Returns process/instance unique app component system id
      /// </summary>
      public ulong ComponentSID => m_ComponentSID;


      /// <summary>
      /// Returns local computer time of component start (not from application container time)
      /// </summary>
      public DateTime ComponentStartTime => m_ComponentStartTime;

      /// <summary>
      /// Returns the common name used to identify the component, for example "Glue" for various IGlue implementations.
      /// This name is searched-by some management tools that allow to find component by this name that does not change between
      /// application restarts like ComponentSID does. Subordinate (non-root) components return null
      /// </summary>
      public virtual string ComponentCommonName => null;


      /// <summary>
      /// Returns a reference to component that this app component services/operates under, or null
      /// </summary>
      public IApplicationComponent ComponentDirector => m_ComponentDirector;
  #warning What is this used for?
  internal void __setComponentDirector(IApplicationComponent director) { m_ComponentDirector = director; }



      /// <summary>
      /// Sets the log level for this component, if not defined then the component logger uses the director/log level
      /// via the ComponentEffectiveLogLevel property
      /// </summary>
      [Config("$log-level|$component-log-level|$cmp-log-level")]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOG)]
      public Log.MessageType? ComponentLogLevel { get; set; }

      /// <summary>
      /// Determines the effective log level for this component, taking it from director if it is not defined on this level
      /// </summary>
      public virtual Log.MessageType ComponentEffectiveLogLevel
      {
        get
        {
          return ComponentLogLevel ?? ComponentDirector?.ComponentEffectiveLogLevel ?? Log.MessageType.Info;
        }
      }

      /// <summary>
      /// Returns  value for "Topic" log message field.
      /// Must override in your particular component to reflect the proper logging topic
      /// </summary>
      public abstract string ComponentLogTopic {  get; }

      /// <summary>
      /// Returns a prefix used in "From" log message field. May Override in your particular component to reflect the better from,
      /// as the default uses class name concatenated with instance SID
      /// </summary>
      public virtual string ComponentLogFromPrefix => this.ToString();


    #endregion

    #region Public
      /// <summary>
      /// Writes a log message for this component; returns the new log msg GDID for correlation, or GDID.Empty if no message was logged
      /// </summary>
      public virtual Guid WriteLog(Log.MessageType type, string from, string text, Exception error = null, Guid? related = null, string pars = null)
      {
        if (type < ComponentEffectiveLogLevel) return Guid.Empty;

        var msg = new Log.Message
        {
          Topic = ComponentLogTopic,
          From = "{0}.{1}".Args(ComponentLogFromPrefix, from),
          Type = type,
          Text = text,
          Exception = error,
          Parameters = pars,
        };

        if (related.HasValue) msg.RelatedTo = related.Value;

        App.Log.Write(msg);

        return msg.Guid;
      }

      public override string ToString()
      {
        return "Component {0}(@{1}, '{2}')".Args(GetType().DisplayNameWithExpandedGenericArgs(), m_ComponentSID, ComponentCommonName ?? CoreConsts.NULL_STRING);
      }
    #endregion

    #region .pvt
      private static Dictionary<ulong, ApplicationComponent> getComponentsOf(IApplication app)
      {
        app.NonNull(text: nameof(app));
        if (s_Instances.TryGetValue(app, out var result)) return result;
        return null;
      }
    #endregion
  }
}
