/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos
{
  /// <summary>
  /// Call guard exceptions thrown by the framework to indicate violations of value constraints.
  /// Guards are typically applied to method parameters in a fluent manner
  /// </summary>
  [Serializable]
  public class CallGuardException : AzosException, IHttpStatusProvider, IExternalStatusProvider
  {
    public const string DETAILS_FLD_NAME = "GUARD-DETAILS";
    public const string SITE_FLD_NAME = "GUARD-SITE";
    public const string PARAM_FLD_NAME = "GUARD-PARAM";

    public CallGuardException(string callSite, string parameterName, string message) : base(message)
    {
      CallSite = callSite;
      ParamName = parameterName;
    }

    public CallGuardException(string callSite, string parameterName, string message, Exception inner) : base(message, inner)
    {
      CallSite = callSite;
      ParamName = parameterName;
    }

    protected CallGuardException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      PutDetailsInHttpStatus = info.GetBoolean(DETAILS_FLD_NAME);
      CallSite = info.GetString(SITE_FLD_NAME);
      ParamName = info.GetString(PARAM_FLD_NAME);
    }

    /// <summary>When set to true, will provide details in HttpStatusDescription</summary>
    public bool PutDetailsInHttpStatus { get; set; }

    /// <summary>When set to true, will put error details via IExternalStatusProvider interface</summary>
    public bool PutDetailsInExternalStatus { get; set; }

    /// <summary>Name of member/caller which employs the guard</summary>
    public string CallSite { get; set; }

    /// <summary>Name of parameter such as method parameter</summary>
    public string ParamName { get; set; }

    public virtual int HttpStatusCode => WebConsts.STATUS_400;

    public virtual string HttpStatusDescription => WebConsts.STATUS_400_DESCRIPTION + (PutDetailsInHttpStatus ? ": "+this.ToMessageWithType() : string.Empty);

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException(nameof(info), GetType().Name + ".GetObjectData(info=null)");

      info.AddValue(DETAILS_FLD_NAME, PutDetailsInHttpStatus);
      info.AddValue(SITE_FLD_NAME, CallSite);
      info.AddValue(PARAM_FLD_NAME, ParamName);
      base.GetObjectData(info, context);
    }

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      var result = this.DefaultBuildErrorStatusProviderMap(includeDump, "guard");

      if (PutDetailsInExternalStatus)
      {
        result[CoreConsts.EXT_STATUS_KEY_SITE] = CallSite;
        result[CoreConsts.EXT_STATUS_KEY_PARAM] = ParamName;
      }

      return result;
    }

    /// <summary>
    /// Surrounds an action by protected scope: any exception thrown by this action gets wrapped in a CallGuardException.
    /// If action is unassigned, nothing is done
    /// </summary>
    /// <remarks>
    /// You can use another version of this function with action argument in order not to create unneeded closures
    /// </remarks>
    public static void Protect(Action action,
                              [CallerFilePath]   string callFile = null,
                              [CallerLineNumber] int callLine = 0,
                              [CallerMemberName] string callMember = null,
                              bool putHttpDetails = false,
                              bool putExternalDetails = false)
    {
      if (action==null) return;

      try
      {
        action();
      }
      catch(Exception error)
      {
        var callSite = GuardUtils.callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                     nameof(action),
                                     StringConsts.GUARDED_ACTION_SCOPE_ERROR
                                                 .Args(callSite ?? CoreConsts.UNKNOWN, error.ToMessageWithType()),
                                     error){ PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
    }

    /// <summary>
    /// Surrounds an action by protected scope: any exception thrown by this action gets wrapped in a CallGuardException.
    /// If action is unassigned, nothing is done
    /// </summary>
    /// <remarks>
    /// You can use another version of this function with action argument in order not to create unneeded closures
    /// </remarks>
    public static async Task ProtectAsync(Func<Task> action,
                              [CallerFilePath]   string callFile = null,
                              [CallerLineNumber] int callLine = 0,
                              [CallerMemberName] string callMember = null,
                              bool putHttpDetails = false,
                              bool putExternalDetails = false)
    {
      if (action == null) return;

      try
      {
        await action().ConfigureAwait(false);
      }
      catch (Exception error)
      {
        var callSite = GuardUtils.callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                     nameof(action),
                                     StringConsts.GUARDED_ACTION_SCOPE_ERROR
                                                 .Args(callSite ?? CoreConsts.UNKNOWN, error.ToMessageWithType()),
                                     error){ PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
    }

    /// <summary>
    /// Surrounds an action by protected scope: any exception thrown by this action gets wrapped in a CallGuardException.
    /// If action is unassigned, nothing is done
    /// </summary>
    /// <remarks>
    /// Use this function with action argument not to create unneeded closures
    /// </remarks>
    public static void Protect<TArg>(TArg arg, Action<TArg> action,
                              [CallerFilePath]   string callFile = null,
                              [CallerLineNumber] int callLine = 0,
                              [CallerMemberName] string callMember = null,
                              bool putHttpDetails = false,
                              bool putExternalDetails = false)
    {
      if (action == null) return;

      try
      {
        action(arg);
      }
      catch (Exception error)
      {
        var callSite = GuardUtils.callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                     nameof(action),
                                     StringConsts.GUARDED_ACTION_SCOPE_ERROR
                                                 .Args(callSite ?? CoreConsts.UNKNOWN, error.ToMessageWithType()),
                                     error){ PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
    }

    /// <summary>
    /// Surrounds an action by protected scope: any exception thrown by this action gets wrapped in a CallGuardException.
    /// If action is unassigned, nothing is done
    /// </summary>
    /// <remarks>
    /// Use this function with action argument not to create unneeded closures
    /// </remarks>
    public static async Task ProtectAsync<TArg>(TArg arg, Func<TArg, Task> action,
                              [CallerFilePath]   string callFile = null,
                              [CallerLineNumber] int callLine = 0,
                              [CallerMemberName] string callMember = null,
                              bool putHttpDetails = false,
                              bool putExternalDetails = false)
    {
      if (action == null) return;

      try
      {
        await action(arg).ConfigureAwait(false);
      }
      catch (Exception error)
      {
        var callSite = GuardUtils.callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                     nameof(action),
                                     StringConsts.GUARDED_ACTION_SCOPE_ERROR
                                                 .Args(callSite ?? CoreConsts.UNKNOWN, error.ToMessageWithType()),
                                     error){ PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
    }
  }


  /// <summary>
  /// Provides guard utility functions such as the ones used for param value guard checking
  /// </summary>
  public static class GuardUtils
  {
    internal static string callSiteOf(string file, int line, string member)
    => "{2}@{0}:{1}".Args(file.IsNotNullOrWhiteSpace() ? System.IO.Path.GetFileName(file) : CoreConsts.UNKNOWN, line, member);


    /// <summary>
    /// Create a CallGuard exception instance for a not found clause. You may throw it on your own (e.g. in a ternary conditional expression)
    /// </summary>
    public static CallGuardException IsNotFound(this string name,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false)
    {
      var callSite = callSiteOf(callFile, callLine, callMember);
      return new CallGuardException(callSite,
                                    name,
                                    StringConsts.GUARDED_CLAUSE_NOT_FOUND_ERROR
                                                .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
      { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
    }


    /// <summary>
    /// Ensures that a value is not null
    /// </summary>
    public static T NonNull<T>(this T obj,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int    callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false) where T : class
    {
      if (obj == null)
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_MAY_NOT_BE_NULL_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return obj;
    }


    /// <summary>
    /// Ensures that a nullable value-typed value is not null
    /// </summary>
    public static Nullable<T> NonNull<T>(this Nullable<T> value,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false) where T : struct
    {
      if (!value.HasValue)
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_MAY_NOT_BE_NULL_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return value;
    }


    /// <summary>
    /// Ensures that a value is not null and that it has not been already disposed if it is a DisposableObject-derivative.
    /// This method is useful to conditionally check various interfaces which do not have `IDisposable` lineage, however
    /// their implementors may derive from `DisposableObject`
    /// </summary>
    public static T NonDisposed<T>(this T obj,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false) where T : class
    {
      if (obj != null)
      {
        var dsp = obj as DisposableObject;
        if (dsp == null) return obj;//not a DisposableObject derivative
        if (!dsp.Disposed) return obj;//still active
      }

      var callSite = callSiteOf(callFile, callLine, callMember);
      throw new CallGuardException(callSite,
                                name,
                                (obj == null ? StringConsts.GUARDED_CLAUSE_MAY_NOT_BE_NULL_ERROR
                                             : StringConsts.GUARDED_CLAUSE_MAY_NOT_BE_DISPOSED_ERROR)
                                  .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
      { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
    }

    /// <summary>
    /// Ensures that a type value is not null and is of the specified type or one of its subtypes
    /// </summary>
    public static Type IsOfType<T>(this Type type,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false)
    {
      if (type == null || !typeof(T).IsAssignableFrom(type))
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_OFTYPE_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN, typeof(T).Name))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return type;
    }

    /// <summary>
    /// Ensures that a type value is not null and is of the specified type or one of its subtypes
    /// </summary>
    public static Type IsOfType(this Type type,
                               Type expectedType,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false)
    {
      if (type == null || !expectedType.NonNull(nameof(expectedType)).IsAssignableFrom(type))
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_OFTYPE_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN, expectedType.Name))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return type;
    }


    /// <summary>
    /// Ensures that a value is not null and is of the specified type or one of its subtypes
    /// </summary>
    public static TValue ValueIsOfType<T, TValue>(this TValue value,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false)
    {
      if (value == null || !typeof(T).IsAssignableFrom(value.GetType()))
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_VALUEOFTYPE_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN, typeof(T).Name))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return value;
    }

    /// <summary>
    /// Ensures that a value is not null and is of the specified type or one of its subtypes
    /// </summary>
    public static TValue ValueIsOfType<TValue>(this TValue value,
                               Type expectedType,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false)
    {
      if (value == null || !expectedType.NonNull(nameof(expectedType)).IsAssignableFrom(value.GetType()))
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_VALUEOFTYPE_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN, expectedType.Name))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return value;
    }

    /// <summary>
    /// Ensures that the value is not null and can be type-casted to TResult, then performs type cast
    /// throwing CallGuardException otherwise
    /// </summary>
    public static TResult CastTo<TResult>(this object value,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false)
    {
      if (value is TResult result) return result;

      var callSite = callSiteOf(callFile, callLine, callMember);
      throw new CallGuardException(callSite,
                                  name,
                                  StringConsts.GUARDED_CLAUSE_TYPECAST_ERROR
                                              .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN, typeof(TResult).DisplayNameWithExpandedGenericArgs()))
      { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
    }

    /// <summary>
    /// Ensures that a config node value is non-null existing node
    /// </summary>
    public static T NonEmpty<T>(this T node,
                                string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false) where T : Conf.IConfigNode
    {
      if (node==null || !node.Exists)
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CONFIG_NODE_CLAUSE_MAY_NOT_BE_EMPTY_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return node;
    }

    /// <summary>
    /// Ensures that a string value is non-null/blank/whitespace
    /// </summary>
    public static string NonBlank(this string str,
                                  string name = null,
                                  [CallerFilePath]   string callFile = null,
                                  [CallerLineNumber] int callLine = 0,
                                  [CallerMemberName] string callMember = null,
                                  bool putHttpDetails = false,
                                  bool putExternalDetails = false)
    {
      if (str.IsNullOrWhiteSpace())
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_STRING_CLAUSE_MAY_NOT_BE_BLANK_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return str;
    }

    /// <summary>
    /// Ensures that a string value is non-null/blank having at most the specified length
    /// </summary>
    public static string NonBlankMax(this string str,
                                     int maxLen,
                                     string name = null,
                                     [CallerFilePath]   string callFile = null,
                                     [CallerLineNumber] int callLine = 0,
                                     [CallerMemberName] string callMember = null,
                                     bool putHttpDetails = false,
                                     bool putExternalDetails = false)
    {
      var len = str.NonBlank(name, callFile, callLine, callMember).Length;
      if (len > maxLen)
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_STRING_CLAUSE_MAY_NOT_EXCEED_MAX_LEN_ERROR
                                            .Args(callSite ?? CoreConsts.UNKNOWN,
                                                  name ?? CoreConsts.UNKNOWN,
                                                  str.TakeFirstChars(15, ".."),
                                                  len,
                                                  maxLen))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return str;
    }

    /// <summary>
    /// Ensures that a string value is non-null/blank having at least the specified length
    /// </summary>
    public static string NonBlankMin(this string str,
                                     int minLen,
                                     string name = null,
                                     [CallerFilePath]   string callFile = null,
                                     [CallerLineNumber] int callLine = 0,
                                     [CallerMemberName] string callMember = null,
                                     bool putHttpDetails = false,
                                     bool putExternalDetails = false)
    {
      var len = str.NonBlank(name, callFile, callLine, callMember).Length;
      if (len < minLen)
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                     name,
                                     StringConsts.GUARDED_STRING_CLAUSE_MAY_NOT_BE_LESS_MIN_LEN_ERROR
                                                 .Args(callSite ?? CoreConsts.UNKNOWN,
                                                  name ?? CoreConsts.UNKNOWN,
                                                  str.TakeFirstChars(15, ".."),
                                                  len,
                                                  minLen))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return str;
    }

    /// <summary>
    /// Ensures that a string value is non-null/blank having its length between the specified min/max bounds
    /// </summary>
    public static string NonBlankMinMax(this string str,
                                        int minLen,
                                        int maxLen,
                                        string name = null,
                                        [CallerFilePath]   string callFile = null,
                                        [CallerLineNumber] int callLine = 0,
                                        [CallerMemberName] string callMember = null,
                                        bool putHttpDetails = false,
                                        bool putExternalDetails = false)
    {
      var len = str.NonBlank(name, callFile, callLine, callMember).Length;
      if (len < minLen || len > maxLen)
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                     name,
                                     StringConsts.GUARDED_STRING_CLAUSE_MUST_BE_BETWEEN_MIN_MAX_LEN_ERROR
                                                 .Args(callSite ?? CoreConsts.UNKNOWN,
                                                  name ?? CoreConsts.UNKNOWN,
                                                  str.TakeFirstChars(15, ".."),
                                                  len,
                                                  minLen,
                                                  maxLen))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return str;
    }


    /// <summary>
    /// Ensures that the required value is present as defined by <see cref="Data.IRequiredCheck"/>
    /// implementation on the instance
    /// </summary>
    public static T HasRequiredValue<T>(this T obj,
                               string name = null,
                               string targetName = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false) where T : Data.IRequiredCheck
    {
      if (obj == null || !obj.CheckRequired(targetName))
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_NO_REQUIRED_VALUE_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return obj;
    }


    /// <summary>
    /// Ensures that the condition is true
    /// </summary>
    public static T IsTrue<T>(this T value,
                               Func<T, bool>  f,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false)
    {
      if (f!=null && !f(value))
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_CONDITION_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
      return value;
    }

    /// <summary>
    /// Ensures that the condition is true
    /// </summary>
    public static void IsTrue(this bool condition,
                               string name = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false)
    {
      if (!condition)
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_CONDITION_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }
    }

    /// <summary>
    /// Ensures that the Validation is passing.
    /// Note: in case of use on complex models, you need to perform DI before this call
    /// </summary>
    public static T AsValid<T>(this T value,
                               string name = null,
                               ValidState state = new ValidState(),
                               string scope = null,
                               [CallerFilePath]   string callFile = null,
                               [CallerLineNumber] int callLine = 0,
                               [CallerMemberName] string callMember = null,
                               bool putHttpDetails = false,
                               bool putExternalDetails = false) where T : IValidatable
    {
      if (!state.IsAssigned) state = new ValidState(TargetedAttribute.ANY_TARGET, ValidErrorMode.FastBatch);

      state = value.Validate(state, scope);

      if (state.HasErrors)
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_VALIDATION_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN,
                                                   name ?? CoreConsts.UNKNOWN,
                                                   state.Error.Message), state.Error)
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }

      return value;
    }

    /// <summary>
    /// Ensures that the Validation is passing.
    /// Performs DI.
    /// </summary>
    public static T AsValidIn<T>(this T value,
                                 IApplication app,
                                 string name = null,
                                 ValidState state = new ValidState(),
                                 string scope = null,
                                 [CallerFilePath]   string callFile = null,
                                 [CallerLineNumber] int callLine = 0,
                                 [CallerMemberName] string callMember = null,
                                 bool putHttpDetails = false,
                                 bool putExternalDetails = false) where T : class, IValidatable
    {
      app.NonNull(nameof(app), callFile, callLine, callMember)
         .InjectInto(value.NonNull(name, callFile, callLine, callMember));

      return value.AsValid(name, state, scope, callFile, callLine, callMember, putHttpDetails, putExternalDetails);
    }


    /// <summary>
    /// Ensures that an atom value is non-zero and valid
    /// </summary>
    public static Atom IsValidNonZero(this Atom value,
                                      string name = null,
                                      [CallerFilePath] string callFile = null,
                                      [CallerLineNumber] int callLine = 0,
                                      [CallerMemberName] string callMember = null,
                                      bool putHttpDetails = false,
                                      bool putExternalDetails = false)
    {
      if (value.IsZero || !value.IsValid)
      {
        var callSite = callSiteOf(callFile, callLine, callMember);
        throw new CallGuardException(callSite,
                                 name,
                                 StringConsts.GUARDED_CLAUSE_ATOM_VALUE_ZERO_INVALID_ERROR
                                             .Args(callSite ?? CoreConsts.UNKNOWN, name ?? CoreConsts.UNKNOWN))
        { PutDetailsInHttpStatus = putHttpDetails, PutDetailsInExternalStatus = putExternalDetails };
      }

      return value;
    }


  }
}
