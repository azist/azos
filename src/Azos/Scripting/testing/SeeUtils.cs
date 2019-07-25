using Azos.Serialization.JSON;
using System;
using System.Collections.Generic;
using System.Text;

using Azos.IO;

namespace Azos.Scripting
{
  /// <summary>
  /// Provides extension/utilities for dumping stuff into console
  /// </summary>
  public static class SeeUtils
  {
    /// <summary>
    /// Writes object into console in JSON format
    /// </summary>
    public static void See(this object obj, JsonWritingOptions options = null)
    {
      Console.WriteLine(obj.ToJson(options ?? JsonWritingOptions.PrettyPrintRowsAsMap));
    }

    /// <summary>
    /// Writes object into console in JSON format wit a header
    /// </summary>
    public static void See(this object obj, string header, JsonWritingOptions options = null)
    {
      Console.WriteLine();
      Console.Write(header);
      Console.WriteLine(obj.ToJson(options ?? JsonWritingOptions.PrettyPrintRowsAsMap));
      Console.WriteLine();
    }

    /// <summary>
    /// Writes formatted string into console
    /// </summary>
    public static void See(this string text, params object[] args)
      => Console.WriteLine(text.Args(args));


    /// <summary> Writes a formatted message with info header </summary>
    public static void Info(this string tpl, params object[] args) => ConsoleUtils.Info(tpl.Args(args));
    /// <summary> Writes a formatted message with info header </summary>
    public static void Info(this string tpl, int ln, params object[] args) => ConsoleUtils.Info(tpl.Args(args), ln);

    /// <summary> Writes a formatted message with warning header </summary>
    public static void Warning(this string tpl, params object[] args) => ConsoleUtils.Warning(tpl.Args(args));
    /// <summary> Writes a formatted message with warning header </summary>
    public static void Warning(this string tpl, int ln, params object[] args) => ConsoleUtils.Warning(tpl.Args(args), ln);

    /// <summary> Writes a formatted message with error header </summary>
    public static void Error(this string tpl, params object[] args) => ConsoleUtils.Error(tpl.Args(args));
    /// <summary> Writes a formatted message with error header </summary>
    public static void Error(this string tpl, int ln, params object[] args) => ConsoleUtils.Error(tpl.Args(args), ln);

  }
}
