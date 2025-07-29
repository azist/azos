/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using Azos.Platform;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Designates supported content types for ArchiveAppender and ArchiveReader derivatives
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class ContentTypeSupportAttribute : Attribute
  {
    public ContentTypeSupportAttribute(params string[] contentTypePatterns)
    {
      SupportedContentTypePatterns = contentTypePatterns.NonNull(nameof(contentTypePatterns))
                                                        .IsTrue(v => v.Length > 0 && v.All( s => s.IsNotNullOrWhiteSpace()), "not empty array of not empty patterns");
    }

    public readonly IEnumerable<string> SupportedContentTypePatterns;

    private static FiniteSetLookup<Type, IEnumerable<string>> s_Lookup = new FiniteSetLookup<Type, IEnumerable<string>>( t =>
    {
      var a = t.GetCustomAttribute<ContentTypeSupportAttribute>(false);
      a.NonNull("Type `{0}` missing {1} attribute definition".Args(t.DisplayNameWithExpandedGenericArgs(), nameof(ContentTypeSupportAttribute)));
      return a.SupportedContentTypePatterns;
    });

    public static IEnumerable<string> GetSupportedContentTypePatternsFor(Type t) => s_Lookup[t.NonNull()];
  }
}
