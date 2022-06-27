/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;


namespace Azos.Wave.Mvc
{
  /// <summary>
  /// When defined on a controller or method, excludes it from documentation
  /// </summary>
  [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public sealed class NoApiDoc : Attribute{ }

  /// <summary>
  /// Base class for ApiDoc* attributes that provide additional metadata for API documentation
  /// </summary>
  public abstract class ApiDocAttribute : Attribute
  {
    /// <summary>
    /// Provides a short (typically under 64 chars) plain-text title of the decorated controller or endpoint
    /// </summary>
    public string Title {  get; set; }

    /// <summary>
    /// Provides a short (typically under 256 chars) plain-text description stating the purpose of the decorated controller or endpoint
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Specifies the list of additional type schemas, such as DataDoc and Permissions  to include as a part of documentation. The system includes all DataDoc-derived
    /// parameters automatically so extra types may be included here such as the ones used in polymorphic results
    /// </summary>
    public Type[] TypeSchemas { get; set; }

    /// <summary>
    /// Optionally specifies request headers. Use ':' to delimit header name/value
    /// </summary>
    public string[] RequestHeaders { get; set; }

    /// <summary>
    /// Optionally describes request body that this entity can process
    /// </summary>
    public string RequestBody { get; set; }

    /// <summary>
    /// Optionally describes request query parameters. Use "=" to delimit name=values
    /// </summary>
    public string[] RequestQueryParameters { get; set; }

    /// <summary>
    /// Optionally specifies response headers. Use ':' to delimit header name/value
    /// </summary>
    public string[] ResponseHeaders { get; set; }

    /// <summary>
    /// Optionally describes response content that this entity produces
    /// </summary>
    public string ResponseContent { get; set; }

    /// <summary>
    /// Optionally describes connection handling for the entity, e.g. keep alive, long poll, web socket etc.
    /// </summary>
    public string Connection { get; set; }
  }

  /// <summary>
  /// Provides documentation-related metadata for API controllers
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class ApiControllerDocAttribute : ApiDocAttribute
  {
    private string m_BaseUri;

    /// <summary>
    /// Base Uri for this controller, it must start with '/'
    /// </summary>
    public string BaseUri { get => m_BaseUri.IsNullOrWhiteSpace() ? "/" : m_BaseUri; set => m_BaseUri = value; }

    /// <summary>
    /// Specifies the name of the markdown resource file containing documentation for this controller, denotes as (#Title/H1),
    /// if left blank then the system tries to find markdown resource embedded at the same level with controller class.
    /// The endpoint/method level doc is searched if DocAnchor property is set
    /// </summary>
    public string DocFile { get; set; }

    /// <summary>
    /// Provides a short line (expected to be under 128) describing auth required
    /// </summary>
    public string Authentication { get; set; }
  }

  /// <summary>
  /// Provides documentation-related metadata for API endpoints such as action methods
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  public class ApiEndpointDocAttribute : ApiDocAttribute
  {
    /// <summary>
    /// Uri for this endpoint, if the URI starts with '/' then it is an absolute URI and not appended to controller base URI,
    /// otherwise method URIs get appended to controller URIs. If this is not set, on the method level, URI is inferred from Action attribute
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    /// Optionally specifies handled method names. Use ':' to delimit method name/description
    /// </summary>
    public string[] Methods { get; set; }

    /// <summary>
    /// Specifies the anchor/id used as a topic in the doc markdown file. Endpoint anchors start with "###" (html H3 level) like "### list".
    /// The system reads content starting from that anchor up to the beginning of the next adjacent anchor of the same level.
    /// If this property is not set, the system takes the name from action attribute and prepends "### " at the front
    /// </summary>
    public string DocAnchor { get; set; }
  }


}
