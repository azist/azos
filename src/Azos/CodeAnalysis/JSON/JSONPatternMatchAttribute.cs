
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Azos.CodeAnalysis.JSON
{
    /// <summary>
    /// Base class for JSON pattern matching
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited=true)]
    public abstract class JSONPatternMatchAttribute : Attribute
    {

        /// <summary>
        /// Checks all pattern match attributes against specified member info until first match found
        /// </summary>
        public static bool Check(MemberInfo info, JSONLexer content)
        {
            var attrs = info.GetCustomAttributes(typeof(JSONPatternMatchAttribute), true).Cast<JSONPatternMatchAttribute>();
            foreach(var attr in attrs)
                if (attr.Match(content)) return true;

            return false;
        }


        /// <summary>
        /// Override to perform actual pattern matching, i.e. the one that uses FSM
        /// </summary>
        public abstract bool Match(JSONLexer content);
    }
}
