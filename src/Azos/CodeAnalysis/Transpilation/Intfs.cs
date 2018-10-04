
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.CodeAnalysis.Source;

namespace Azos.CodeAnalysis.Transpilation
{
    /// <summary>
    /// Describes general transpiler interface
    /// </summary>
    public interface ITranspiler : ICodeProcessor
    {
       /// <summary>
       /// Lists source parser that supply parse tree for transpilation
       /// </summary>
       IParser SourceParser { get; }

       /// <summary>
       /// Indicates whether Transpile() already happened
       /// </summary>
       bool HasTranspiled { get; }


       /// <summary>
       /// Performs transpilation and sets HasTranspiled to true if it has not been performed yet
       /// </summary>
       void Transpile();
    }

}
