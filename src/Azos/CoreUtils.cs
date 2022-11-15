/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.IO;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using Azos.Apps;
using System.Runtime.CompilerServices;
using Azos.Serialization.JSON;
using System.Threading.Tasks;
using Azos.Data.Idgen;

namespace Azos
{
  /// <summary>
  /// Provides core utility functions used by the majority of projects
  /// </summary>
  public static class CoreUtils
  {
    /// <summary>
    /// Returns true for DEV or LOCAL environments.
    /// This method is used to ascertain the "non-prod" nature of the either and is typically used
    /// to disclose or cloak/block sensitive information/details in things like logs and debug endpoints
    /// (e.g. config content listing, debugging etc...)
    /// </summary>
    public static bool IsDeveloperEnvironment(this IApplication app)
     => app.NonNull(nameof(app)).EnvironmentName.IsOneOf(CoreConsts.ENVIRONMENTS_DEVELOPER);

    /// <summary>
    /// Returns the name of entry point executable file optionally with its path
    /// </summary>
    public static string EntryExeName(bool withPath = true)
    {
      var file = Assembly.GetEntryAssembly().Location;

      if (!withPath) file = Path.GetFileName(file);

      return file;
    }

    /// <summary>
    /// Determines if component is being used within designer
    /// </summary>
    public static bool IsComponentDesignerHosted(Component cmp)
    {
      if (cmp != null)
      {

        if (cmp.Site != null)
          if (cmp.Site.DesignMode == true)
            return true;

      }

      return false;
    }


    /// <summary>
    /// Tests bool? for being assigned with true
    /// </summary>
    public static bool IsTrue(this Nullable<bool> value, bool dflt = false)
    {
      if (!value.HasValue) return dflt;
      return value.Value;
    }

    /// <summary>
    /// Shortcut to App.DependencyInjector.InjectInto(...)
    /// </summary>
    public static T InjectInto<T>(this IApplication app, T target) where T : class
    {
      app.NonNull(nameof(app)).DependencyInjector.InjectInto(target);
      return target;
    }


    /// <summary>
    /// Writes exception message with exception type
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static string ToMessageWithType(this Exception error)
    {
      if (error == null) return null;
      return "[{0}] {1}".Args(error.GetType().FullName, error.Message);
    }

    /// <summary>
    /// Returns full name of the nested type, for example: System.Namespace.Type+Sub -> Type.Sub
    /// </summary>
    public static string FullNestedTypeName(this Type type, bool verbatimPrefix = true)
    {
      if (type.DeclaringType == null) return verbatimPrefix ? '@' + type.Name : type.Name;

      return type.DeclaringType.FullNestedTypeName(verbatimPrefix) + '.' + (verbatimPrefix ? '@' + type.Name : type.Name);
    }

    /// <summary>
    /// Returns the full name of the type optionally prefixed with verbatim id specifier '@'.
    /// The generic arguments are expanded into their full names i.e.
    ///   List'1[System.Object]  ->  System.Collections.Generic.List&lt;System.Object&gt;
    /// </summary>
    public static string FullNameWithExpandedGenericArgs(this Type type, bool verbatimPrefix = true)
    {
      var ns = type.Namespace;

      if (verbatimPrefix)
      {
        var nss = ns.Split('.');
        ns = string.Join(".@", nss);
      }


      var gargs = type.GetGenericArguments();

      if (gargs.Length == 0)
      {
        return verbatimPrefix ? "@{0}.{1}".Args(ns, type.FullNestedTypeName(true)) : (type.Namespace + '.' + type.FullNestedTypeName(false));
      }

      var sb = new StringBuilder();

      for (int i = 0; i < gargs.Length; i++)
      {
        if (i > 0) sb.Append(", ");
        sb.Append(gargs[i].FullNameWithExpandedGenericArgs(verbatimPrefix));
      }

      var nm = type.FullNestedTypeName(verbatimPrefix);
      var idx = nm.IndexOf('`');
      if (idx >= 0)
        nm = nm.Substring(0, idx);


      return verbatimPrefix ? "@{0}.{1}<{2}>".Args(ns, nm, sb.ToString()) :
                              "{0}.{1}<{2}>".Args(ns, nm, sb.ToString());
    }


    /// <summary>
    /// Returns the name of the type with expanded generic argument names.
    /// This helper is useful for printing class names to logs/messages.
    ///   List'1[System.Object]  ->  List&lt;Object&gt;
    /// </summary>
    public static string DisplayNameWithExpandedGenericArgs(this Type type)
    {

      var gargs = type.GetGenericArguments();

      if (gargs.Length == 0)
      {
        return type.Name;
      }

      var sb = new StringBuilder();

      for (int i = 0; i < gargs.Length; i++)
      {
        if (i > 0) sb.Append(", ");
        sb.Append(gargs[i].DisplayNameWithExpandedGenericArgs());
      }

      var nm = type.Name;
      var idx = nm.IndexOf('`');
      if (idx >= 0)
        nm = nm.Substring(0, idx);


      return "{0}<{1}>".Args(nm, sb.ToString());
    }

    /// <summary>
    /// Converts expression tree to simple textual form for debugging
    /// </summary>
    public static string ToDebugView(this Expression expr, int indent = 0)
    {
      const int TS = 2;

      var pad = "".PadLeft(TS * indent, ' ');

      if (expr == null) return "";
      if (expr is BlockExpression)
      {
        var block = (BlockExpression)expr;
        var sb = new StringBuilder();
        sb.Append(pad); sb.AppendLine("{0}{{".Args(expr.GetType().Name));

        sb.Append(pad); sb.AppendLine("//variables");
        foreach (var v in block.Variables)
        {
          sb.Append(pad);
          sb.AppendLine("var {0}  {1};".Args(v.Type.FullName, v.Name));
        }


        sb.Append(pad); sb.AppendLine("//sub expressions");
        foreach (var se in block.Expressions)
        {
          sb.Append(pad);
          sb.AppendLine(se.ToDebugView(indent + 1));
        }

        sb.Append(pad); sb.AppendLine("}");
        return sb.ToString();
      }
      return pad + expr.ToString();
    }


    /// <summary>
    /// Returns true when supplied name can be used for XML node naming (node names, attribute names)
    /// </summary>
    public static bool IsValidXMLName(this string name)
    {
      if (name.IsNullOrWhiteSpace()) return false;
      for (int i = 0; i < name.Length; i++)
      {
        char c = name[i];
        if (c == '-' || c == '_') continue;
        if (!Char.IsLetterOrDigit(c) || (i == 0 && !Char.IsLetter(c))) return false;
      }

      return true;
    }

    /// <summary>
    /// Searches an Exception and its InnerException chain for the first instance of T or null.
    /// The original instance may be null itself in which case null is returned
    /// </summary>
    public static T SearchThisOrInnerExceptionOf<T>(this Exception target) where T : class
    {
      while(target != null)
      {
        var match = target as T;
        if (match != null) return match;

        target = target.InnerException;
      }

      return null;
    }

    /// <summary>
    /// Builds a default map with base exception descriptor, searching its InnerException chain for IExternalStatusProvider.
    /// This method never returns null as the very root data map is always built
    /// </summary>
    public static JsonDataMap DefaultBuildErrorStatusProviderMap(this Exception error, bool includeDump, string ns, string type = null)
    {
      var result = new JsonDataMap
      {
        { CoreConsts.EXT_STATUS_KEY_NS, ns.NonBlank(nameof(ns)) },
        { CoreConsts.EXT_STATUS_KEY_TYPE, type.Default(error.GetType().Name) }
      };

      if (error is AzosException aze)
      {
        result[CoreConsts.EXT_STATUS_KEY_CODE] = aze.Code;
      }

      var inner = error.InnerException.SearchThisOrInnerExceptionOf<IExternalStatusProvider>();

      if (inner != null)
      {
        var innerData = inner.ProvideExternalStatus(includeDump);
        if (innerData != null)
          result[CoreConsts.EXT_STATUS_KEY_CAUSE] = innerData;
      }

      return result;
    }


    /// <summary>
    /// Encloses an action in try catch and logs the error if it leaked from action. This method never leaks.
    /// Returns true if there was no error on action success, or false if error leaked from action and was logged by component.
    /// The actual logging depends on the component log level
    /// </summary>
    public static bool DontLeak(this IApplicationComponent cmp, Action action, string errorText = null, [CallerMemberName]string errorFrom = null, Log.MessageType errorLogType = Log.MessageType.Error, Guid? rel = null)
    {
      var ac = (cmp.NonNull(nameof(cmp)) as ApplicationComponent).NonNull("Internal error: not a AC");
      action.NonNull(nameof(action));
      try
      {
        action();
        return true;
      }
      catch(Exception error)
      {
        if (errorText.IsNullOrWhiteSpace()) errorText = "Error leaked: ";
        errorText += error.ToMessageWithType();

        ac.WriteLog(errorLogType, errorFrom, errorText, error, related: rel);
      }

      return false;
    }

    /// <summary>
    /// Encloses an action in try catch and logs the error if it leaked from action. This method never leaks.
    /// Returns true if there was no error on action success, or false if error leaked from action and was logged by component.
    /// The actual logging depends on the component log level
    /// </summary>
    public static async Task<bool> DontLeakAsync(this IApplicationComponent cmp, Func<Task> action, string errorText = null, [CallerMemberName]string errorFrom = null, Log.MessageType errorLogType = Log.MessageType.Error, Guid? rel = null)
    {
      var ac = (cmp.NonNull(nameof(cmp)) as ApplicationComponent).NonNull("Internal error: not a AC");
      action.NonNull(nameof(action));
      try
      {
        await action().ConfigureAwait(false);
        return true;
      }
      catch (Exception error)
      {
        if (errorText.IsNullOrWhiteSpace()) errorText = "Error leaked: ";
        errorText += error.ToMessageWithType();

        ac.WriteLog(errorLogType, errorFrom, errorText, error, related: rel);
      }

      return false;
    }


    /// <summary>
    /// Encloses an action in try catch and logs the error if it leaked from action. This method never leaks.
    /// Returns true if there was no error on action success, or false if error leaked from action and was logged by component.
    /// The actual logging depends on the component log level
    /// </summary>
    public static TResult DontLeak<TResult>(this IApplicationComponent cmp, Func<TResult> func, string errorText = null, [CallerMemberName]string errorFrom = null, Log.MessageType errorLogType = Log.MessageType.Error, Guid? rel = null)
    {
      var ac = (cmp.NonNull(nameof(cmp)) as ApplicationComponent).NonNull("Internal error: not a AC");
      func.NonNull(nameof(func));
      try
      {
        return func();
      }
      catch (Exception error)
      {
        if (errorText.IsNullOrWhiteSpace()) errorText = "Error leaked: ";
        errorText += error.ToMessageWithType();

        ac.WriteLog(errorLogType, errorFrom, errorText, error, related: rel);
      }

      return default(TResult);
    }


    /// <summary>
    /// Encloses an action in try catch and logs the error if it leaked from action. This method never leaks.
    /// Returns true if there was no error on action success, or false if error leaked from action and was logged by component.
    /// The actual logging depends on the component log level
    /// </summary>
    public static async Task<TResult> DontLeakAsync<TResult>(this IApplicationComponent cmp, Func<Task<TResult>> func, string errorText = null, [CallerMemberName]string errorFrom = null, Log.MessageType errorLogType = Log.MessageType.Error, Guid? rel = null)
    {
      var ac = (cmp.NonNull(nameof(cmp)) as ApplicationComponent).NonNull("Internal error: not a AC");
      func.NonNull(nameof(func));
      try
      {
        return await func().ConfigureAwait(false);
      }
      catch (Exception error)
      {
        if (errorText.IsNullOrWhiteSpace()) errorText = "Error leaked: ";
        errorText += error.ToMessageWithType();

        ac.WriteLog(errorLogType, errorFrom, errorText, error, related: rel);
      }

      return default(TResult);
    }

    /// <summary>
    /// Returns current UTC Now using app precision time source
    /// </summary>
    public static DateTime GetUtcNow(this IApplication app) => app.NonNull(nameof(app)).TimeSource.UTCNow;

    /// <summary>
    /// Returns app cloud origin
    /// </summary>
    public static Atom GetCloudOrigin(this IApplication app)
     => app.NonNull(nameof(app)).CloudOrigin.IsTrue(o => !o.IsZero && o.IsValid, "cloud-origin");


    /// <summary>
    /// Returns IGdidProvider
    /// </summary>
    public static IGdidProvider GetGdidProvider(this IApplication app)
    {
      //1 - search the app space
      var module = app.NonNull(nameof(app)).ModuleRoot.TryGet<IGdidProviderModule>();

      //2 - search the dynamic scope
      if (module == null)
      {
        module = Apps.Injection.DynamicModuleFlowScope.Find(typeof(IGdidProviderModule), null) as IGdidProviderModule;
      }

      module.NonNull("Required `IGdidProviderModule` module installed in app or dynamic scope");

      return module.Provider;
    }

  }
}
