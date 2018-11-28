/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Wave
{
  /// <summary>
  /// Used for matches to bypass AuthorizationExceptions, requires EXCEPTION passed in Make(context)
  /// </summary>
  public sealed class NotAuthorizationExceptionMatch : WorkMatch
  {
    public NotAuthorizationExceptionMatch(string name, int order) : base(name, order) { }
    public NotAuthorizationExceptionMatch(IConfigSectionNode confNode) : base(confNode) { }

    public override JSONDataMap Make(WorkContext work, object context = null)
    {
      if (Azos.Security.AuthorizationException.IsDenotedBy(context as Exception)) return null;
      return base.Make(work, context);
    }
  }


  /// <summary>
  /// Matches by Work.Response.StatusCode. The match makes sense in the After processing - when status is already set
  /// </summary>
  public sealed class HttpResponseStatusCodeMatch : WorkMatch
  {
    public HttpResponseStatusCodeMatch(string name, int order) : base(name, order) { }
    public HttpResponseStatusCodeMatch(IConfigSectionNode confNode) : base(confNode) { }

    [Config] public int Code {  get; set; }
    [Config] public bool IsNot {  get; set; }

    public override JSONDataMap Make(WorkContext work, object context = null)
    {
      var eq = work.Response.StatusCode == Code;
      if (IsNot == eq) return null;

      return base.Make(work, context);
    }
  }

  /// <summary>
  /// Used for matching of exceptions passed to Make(context)
  /// </summary>
  public sealed class ExceptionMatch : WorkMatch
  {
    public ExceptionMatch(string name, int order) : base(name, order) { }
    public ExceptionMatch(IConfigSectionNode confNode) : base(confNode) { }

    private Type m_ExceptionType;
    private bool m_IsNot;

    [Config]
    public string ExceptionType
    {
      get { return m_ExceptionType==null ? string.Empty : m_ExceptionType.AssemblyQualifiedName; }
      set { m_ExceptionType = value.IsNullOrWhiteSpace() ? null : Type.GetType(value, true, true); }
    }

    [Config]
    public bool IsNot
    {
      get { return m_IsNot; }
      set { m_IsNot = value; }
    }

    public override JSONDataMap Make(WorkContext work, object context = null)
    {
      if (m_ExceptionType!=null)
      {
        var error = context as Exception;

        var eq = false;

        while (error != null)
        {
          var got = error.GetType();

          if (got == m_ExceptionType)
          {
            eq = true;
            break;
          }

          error = error.InnerException;
        }

        if (m_IsNot == eq) return null;
      }

      return base.Make(work, context);
    }
  }


}
