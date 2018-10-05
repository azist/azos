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

namespace Azos
{
  /// <summary>
  /// Provides core utility functions used by the majority of projects
  /// </summary>
  public static class CoreUtils
  {
    /// <summary>
    /// Returns the name of entry point executable file optionaly with its path
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
    /// Checks the value for null and throws exception if it is.
    /// The method is useful for .ctor call chaining to preclude otherwise anonymous NullReferenceException
    /// </summary>
    public static T NonNull<T>(this T obj,
                               Func<Exception> error = null,
                               string text = null) where T : class
    {
      if (obj == null)
      {
        if (error != null)
          throw error();
        else
          throw new AzosException(StringConsts.PARAMETER_MAY_NOT_BE_NULL_ERROR
                                             .Args(text ?? CoreConsts.UNKNOWN,
                                                   new StackTrace(1, false).ToString()));
      }
      return obj;
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
    /// If there is error, converts its details to JSOnDataMap
    /// </summary>
    public static NFX.Serialization.JSON.JSONDataMap ToJSONDataMap(this Exception error, bool recurse = true, bool stackTrace = false)
    {
      if (error == null) return null;
      var result = new NFX.Serialization.JSON.JSONDataMap(false);

      result["Type"] = error.GetType().FullName;
      result["Msg"] = error.Message;
      result["Data"] = error.Data;
      result["Src"] = error.Source;
      if (stackTrace) result["STrace"] = error.StackTrace;

      if (recurse)
        result["Inner"] = error.InnerException.ToJSONDataMap(recurse);
      else
      {
        result["Inner.Type"] = error.InnerException != null ? error.InnerException.GetType().FullName : null;
        result["Inner.Msg"] = error.InnerException != null ? error.InnerException.Message : null;
        if (stackTrace) result["Inner.STrace"] = error.InnerException != null ? error.InnerException.StackTrace : null;
      }

      return result;
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
    /// The generic arguments ar expanded into their full names i.e.
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
    /// Returns the the name of the type with expanded generic argument names.
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
      for (int i = 0; i < name.Length; i++)
      {
        char c = name[i];
        if (c == '-' || c == '_') continue;
        if (!Char.IsLetterOrDigit(c) || (i == 0 && !Char.IsLetter(c))) return false;
      }

      return true;
    }


  }
}
