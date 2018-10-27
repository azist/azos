/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using Azos.Templatization;
using Azos.Scripting;

namespace Azos.Tests.Unit.Templatization
{
  #warning PAL is required to run the tests
  [Runnable]
  public class TextCSTemplateCompilerTests
  {
    public static readonly string NFX_UTEST_DLL = "Azos.Tests.Unit.dll";


    #region Public

      [Run]
      public void InitialState()
      {
        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler();

        Aver.IsFalse(compiler.Compiled);
        Aver.IsFalse(compiler.CompileCode);
        Aver.IsTrue(string.IsNullOrEmpty(compiler.ReferencedAssembliesSearchPath));
        Aver.AreEqual(0, compiler.Count());
        Aver.AreEqual(0, compiler.CompileUnitsWithErrors.Count());
        Aver.AreEqual(0, compiler.CodeCompilerErrors.Count());
        Aver.AreEqual(6, compiler.ReferencedAssemblies.Count());
      }

      [Run]
      public void CompilationProperties()
      {
        const string srcStr = @"#<conf>
          <compiler base-class-name=""Azos.Tests.Unit.Templatization.TeztTemplate""
                    namespace=""Azos.Tests.Unit.Templatization""
                    abstract=""true""
                    summary=""Test master page""
           />
        #</conf>
        #[class]

            public string Title { get {return ""aaaaa""; } }


            protected abstract void renderHeader();
            protected abstract void renderBody(bool showDetails);
            protected abstract void renderFooter();


        #[render]
        <html>
         <head>
           <title>?[Title]</title>
         </head>
         <body>

          <h1>This is Header</h1>
           @[renderHeader();]

          <h1>This is Body</h1>
           @[renderBody(true);]
          <p>This is in master page</p>

          <h1>This is Footer</h1>
           @[renderFooter();]

         </body>
        </html> ";

        TemplateStringContentSource src = new TemplateStringContentSource(srcStr);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src);

        compiler.Compile();

        Aver.AreEqual(1, compiler.Count());

        CompileUnit unit = compiler.First();

        Aver.IsNull(unit.CompilationException);
        Aver.IsNull(unit.CompiledTemplateType);
        Aver.IsTrue(unit.CompiledTemplateTypeName.IsNotNullOrEmpty());
        Aver.AreSameRef(src, unit.TemplateSource);
        Aver.AreEqual(srcStr, src.Content);
      }

  //    [Run]
  //    public void GeneralClassAttributes()
  //    {
  //      const string CLASS_NAMESPACE = "Azos.Tests.Unit.Templatization";
  //      const string BASE_CLASS_FULLNAME = "Azos.Tests.Unit.Templatization.TeztTemplate";

  //      string templateStr = @"
  //#<conf><compiler 
  //base-class-name=""{0}""
  //namespace=""{1}""
  //abstract=""true""
  //summary=""Test master page""/>
  //#</conf>".Args(BASE_CLASS_FULLNAME, CLASS_NAMESPACE);

  //      TemplateStringContentSource src = new TemplateStringContentSource(templateStr);

  //      TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src);

  //      compiler.Compile();

  //      CompileUnit unit = compiler.First();

  //      CSharpCodeProvider provider = new CSharpCodeProvider();//new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });

  //      CompilerParameters compilerParams = new CompilerParameters() { GenerateInMemory = true, GenerateExecutable = false };

  //      foreach (var referencedAssembly in compiler.ReferencedAssemblies)
  //        compilerParams.ReferencedAssemblies.Add(referencedAssembly);

  //      compilerParams.ReferencedAssemblies.Add(NFX_UTEST_DLL);

  //      CompilerResults compilerResults = provider.CompileAssemblyFromSource(compilerParams, unit.CompiledSource);

  //      Assembly asm = compilerResults.CompiledAssembly;
  //      Aver.AreEqual(1, asm.GetExportedTypes().Count());

  //      Type generatedType = asm.GetExportedTypes().First();

  //      Aver.AreEqual(CLASS_NAMESPACE, generatedType.Namespace);
  //      Aver.AreEqual(BASE_CLASS_FULLNAME, generatedType.BaseType.FullName);
  //      Aver.IsTrue(generatedType.IsAbstract);
  //    }

      [Run]
      public void NotAbstract()
      {
        const string CLASS_NAMESPACE = "Azos.Tests.Unit.Templatization";
        const string BASE_CLASS_FULLNAME = "Azos.Tests.Unit.Templatization.TeztTemplate";

        string templateStr = @"
  #<conf><compiler 
  base-class-name=""{0}""
  namespace=""{1}""
  abstract=""false""
  summary=""Test master page""/>
  #</conf>".Args(BASE_CLASS_FULLNAME, CLASS_NAMESPACE);

        TemplateStringContentSource src = new TemplateStringContentSource(templateStr);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) {CompileCode = true};
        compiler.ReferenceAssembly(NFX_UTEST_DLL);

        compiler.Compile();

        CompileUnit cu = compiler.First();

        Aver.IsFalse(cu.CompiledTemplateType.IsAbstract);
      }

      [Run]
      public void AutoGeneratedNamespace()
      {
        const string CLASS_NAMESPACE = "";
        const string BASE_CLASS_FULLNAME = "Azos.Tests.Unit.Templatization.TeztTemplate";

        string templateSrc = @"
#<conf><compiler 
base-class-name=""{0}""
namespace=""{1}""
abstract=""true""
summary=""Test master page""/>
#</conf>".Args(BASE_CLASS_FULLNAME, CLASS_NAMESPACE);

        TemplateStringContentSource src = new TemplateStringContentSource(templateSrc);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) {CompileCode = true};
        compiler.ReferenceAssembly(NFX_UTEST_DLL);

        compiler.Compile();

        CompileUnit cu = compiler.First();

        compiler.CodeCompilerErrors.ForEach(e => Console.WriteLine(e.ToMessageWithType()));

        Aver.IsTrue( cu.CompiledTemplateType.Namespace.IsNotNullOrEmpty());
      }

      [Run]
      public void DefaultIfEmptyBaseClass()
      {
        const string CLASS_NAMESPACE = "Azos.Tests.Unit.Templatization";
        const string BASE_CLASS_FULLNAME = "";

        TextCSTemplateCompiler compiler = GetCompilerForSimpleTemplateSrc(CLASS_NAMESPACE, BASE_CLASS_FULLNAME);//, NFX_WEB_DLL);

        CompileUnit cu = compiler.First();

        foreach (var err in compiler.CodeCompilerErrors)
        {
          Console.WriteLine(err.Message);
        }

        Aver.AreEqual("Template<Object, IRenderingTarget, Object>",
          cu.CompiledTemplateType.BaseType.DisplayNameWithExpandedGenericArgs());
      }

      [Run]
      public void MethodNames()
      {
        const string RENDER_HEADER = "renderHeader";
        const string RENDER_FOOTER = "renderFooter";
        const string RENDER_BODY = "renderBody";
        const string TITLE = "Title";

        string templateSrc = @"
#<conf>
  <compiler base-class-name=""Azos.Tests.Unit.Templatization.TeztTemplate""
            namespace=""Azos.Tests.Unit.Templatization""
            abstract=""true""
            summary=""Test master page""
   />
#</conf>
#[class]

    public string " + TITLE + @" { get {return ""aaaaa""; } }


    protected abstract void " + RENDER_HEADER + @"();
    protected abstract void " + RENDER_BODY + @"(bool showDetails);
    protected abstract void " + RENDER_FOOTER + @"();


#[render]
<html>
 <head>
   <title>?[Title]</title>
 </head>
 <body>

  <h1>This is Header</h1>
   @[renderHeader();]

  <h1>This is Body</h1>
   @[renderBody(true);]
  <p>This is in master page</p>

  <h1>This is Footer</h1>
   @[renderFooter();]

 </body>
</html>
";

        TemplateStringContentSource src = new TemplateStringContentSource(templateSrc);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) {CompileCode = true};
        compiler.ReferenceAssembly(NFX_UTEST_DLL);

        compiler.Compile();

        CompileUnit cu = compiler.First();
        Type compiledType = cu.CompiledTemplateType;

        Aver.IsNotNull(compiledType.GetMethod(RENDER_HEADER, BindingFlags.NonPublic | BindingFlags.Instance));
        Aver.IsNotNull(compiledType.GetMethod(RENDER_FOOTER, BindingFlags.NonPublic | BindingFlags.Instance));

        MethodInfo methodBody = compiledType.GetMethod(RENDER_BODY, BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new[] {typeof (bool)}, null);

        Aver.IsNotNull(compiledType.GetProperty(TITLE, BindingFlags.Public | BindingFlags.Instance ));
      }

      [Run]
      public void La()
      {
        string templateStr = @"
#<laconf>
  compiler
  {
     base-class-name=""Azos.Tests.Unit.Templatization.TeztTemplate""
     namespace=""TestWebApp.Templates""
     summary=""Test master page""

    using {ns=""Azos.Web"" }
    using {ns=""Azos.RecordModel"" }
    using {ns=""BusinessLogic"" }

    attribute {decl=""BusinessLogic.SultanPermission(4)"" }

   }
#</laconf>";

        TemplateStringContentSource templateSrc = new TemplateStringContentSource(templateStr);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(templateSrc);

        compiler.Compile();

        Aver.AreEqual(1, compiler.Count());

        CompileUnit unit = compiler.First();

        Aver.IsNull(unit.CompilationException);
        Aver.IsNull(unit.CompiledTemplateType);
        Aver.IsTrue(unit.CompiledTemplateTypeName.IsNotNullOrEmpty());
        Aver.AreSameRef(templateSrc, unit.TemplateSource);
        Aver.AreEqual(templateStr, templateSrc.Content);


      }

      [Run]
      public void Social()
      {
        string templateStr =@"
#<laconf>
  compiler {
    base-class-name=""Azos.Tests.Unit.Templatization.TeztTemplate""
    namespace=""Azos.Tests.Unit.Templatization""
    summary=""Social Master Page""
  }
#</laconf>

#[class]
  public string Title { get { return ""Social"";}}
#[render]
<html>
  <head>
    <title>?[Title]</title>
  </head>

  <body>

  </body>
</html>";

        TemplateStringContentSource templateSrc = new TemplateStringContentSource(templateStr);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(templateSrc);

        compiler.Compile();

        Aver.AreEqual(1, compiler.Count());

        CompileUnit unit = compiler.First();

        Console.WriteLine(unit.CompiledSource);
      }

    #endregion

    #region .pvt. impl.

      private TextCSTemplateCompiler GetCompilerForSimpleTemplateSrc(string classNamespace, string baseClassFullName, params string[] additionalReferences)
      {
        string templateSrc = @"
  #<conf><compiler 
  base-class-name=""{0}""
  namespace=""{1}""
  abstract=""true""
  summary=""Test master page""/>
  #</conf>".Args(baseClassFullName, classNamespace);

        TemplateStringContentSource src = new TemplateStringContentSource(templateSrc);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) { CompileCode = true};
        additionalReferences.ForEach(a => compiler.ReferenceAssembly(a));

        compiler.Compile();

        return compiler;
      }

      private TextCSTemplateCompiler GetCompiler(string templateSrc, bool compileCode = false)
      {
        TemplateStringContentSource src = new TemplateStringContentSource(templateSrc);

        TextCSTemplateCompiler compiler = new TextCSTemplateCompiler(src) { CompileCode = compileCode};

        compiler.Compile();

        return compiler;
      }

    #endregion
  }
}
