/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Text;
using Azos.Log.Sinks;

namespace Azos.Log
{
    /// <summary>
    /// Defines an expression used for log message filtering.
    /// Important: it is not a good practice to create many different scopes as it leads to creation of many assemblies dynamically
    /// </summary>
    public class MessageFilterExpression : CompilingExpressionEvaluator<Sink, bool, Message>
    {
       public const string SCOPE = "_AZOS._Log._Filtering";

       /// <summary>
       /// Creates a new expression in a default logging filter scope.
       /// This .ctor will fail if at least one expression from this scope has already been compiled
       /// </summary>
       public MessageFilterExpression(string expr) : base(SCOPE, expr, null, new string[]{ "Azos.Log", "Azos.Log.Sinks"})
       {

       }

       /// <summary>
       /// Use this .ctor to specify a different scope name. Every unique scope name gets compiled into a new assembly,
       /// consequently it is not a good practice to create many different scopes.
       /// This .ctor will fail if at least one expression from this scope has already been compiled
       /// </summary>
       public MessageFilterExpression(string scope,
                                      string expr,
                                      IEnumerable<string> referencedAssemblies = null,
                                      IEnumerable<string> usings = null) : base(scope, expr, referencedAssemblies, usings)
       {

       }

    }
}
