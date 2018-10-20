
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using Azos.Conf;
using Azos.Templatization;
using Azos.IO;
using Azos.Platform;

namespace Azos.Tools.Ntc
{
    public static class ProgramBody
    {
        public static void Main(string[] args)
        {
          try
          {
           var w = Stopwatch.StartNew();
           run(args);
           w.Stop();
           ConsoleUtils.Info("Runtime: "+w.Elapsed);
           System.Environment.ExitCode = 0;
          }
          catch(Exception error)
          {
           ConsoleUtils.Error(error.ToMessageWithType());
           System.Environment.ExitCode = -1;
          }
        }


        private static void run(string[] args)
        {
          var config = new CommandArgsConfiguration(args);


          ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Welcome.txt") );

          if (config.Root["?"].Exists ||
              config.Root["h"].Exists ||
              config.Root["help"].Exists)
          {
             ConsoleUtils.WriteMarkupContent( typeof(ProgramBody).GetText("Help.txt") );
             return;
          }



          if (!config.Root.AttrByIndex(0).Exists)
          {
            ConsoleUtils.Error("Template source path is missing");
            return;
          }


          var files =  getFiles(config.Root);

          var ctypename = config.Root["c"].AttrByIndex(0).Value ?? config.Root["compiler"].AttrByIndex(0).Value;

          var ctype = string.IsNullOrWhiteSpace(ctypename)? typeof(Azos.Templatization.TextCSTemplateCompiler) : Type.GetType(ctypename);
          if (ctype == null)
               throw new AzosException("Can not create compiler type: " + (ctypename??"<none>"));

          var compiler = Activator.CreateInstance(ctype) as TemplateCompiler;

          var onode = config.Root["options"];
          if (!onode.Exists) onode = config.Root["o"];
          if (onode.Exists)
          {
            ConfigAttribute.Apply(compiler, onode);
            var asms = onode.AttrByName("ref").ValueAsString(string.Empty).Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            foreach(var asm in asms)
             compiler.ReferenceAssembly(asm);
          }

          showOptions(compiler);

          foreach(var f in files)
          {
             var src = new FileTemplateStringContentSource(f);
             ConsoleUtils.Info("Included: " + src.GetName(50));
             compiler.IncludeTemplateSource(src);
          }

          compiler.Compile();

          if (compiler.HasErrors)
          {
            showErrors(compiler, config.Root);
            return;
          }

          if (config.Root["src"].Exists)
            writeToDiskCompiledSourceFiles(compiler, config.Root);


        }

                                  static IEnumerable<string> getFiles(IConfigSectionNode configRoot)
                                  {
                                      var pathArg = configRoot.AttrByIndex(0).Value;
                                      if (!Path.IsPathRooted(pathArg)) pathArg = "."+Path.DirectorySeparatorChar + pathArg;
                                      var rootPath = Path.GetDirectoryName(pathArg);
                                      var mask = Path.GetFileName(pathArg);
                                      if (mask.Length == 0) mask = "*";

                                      return rootPath.AllFileNamesThatMatch(mask, configRoot["r"].Exists || configRoot["recurse"].Exists);
                                  }


                                  static void showErrors(TemplateCompiler compiler, IConfigSectionNode configRoot)
                                  {
                                    ConsoleUtils.Warning("Compilation finished with errors");
                                    Console.WriteLine();
                                    ConsoleUtils.Info("Compile unit errors:");
                                    foreach(var erru in compiler.CompileUnitsWithErrors)
                                    {
                                     ConsoleUtils.Error(string.Format("Unit '{0}'. Type '{1}' Text: '{2}'",
                                                                         erru.TemplateSource.GetName(16),
                                                                         erru.CompilationException.GetType().Name,
                                                                         erru.CompilationException.Message));
                                    }
                                    Console.WriteLine();
                                    ConsoleUtils.Info(string.Format("{0} Code compilation errors:", compiler.LanguageName));
                                    foreach(var cerr in compiler.CodeCompilerErrors)
                                    {
                                     ConsoleUtils.Error( cerr.ToString() );
                                    }
                                  }

                                  static void writeToDiskCompiledSourceFiles(TemplateCompiler compiler, IConfigSectionNode configRoot)
                                  {
                                    const string DEFAULT_SUBDIR = ".tc";

                                    var ext = configRoot["ext"].AttrByIndex(0).ValueAsString(compiler.LanguageSourceFileExtension);
                                    var re = configRoot["replace"].AttrByIndex(0).ValueAsString();
                                    var dest = configRoot["dest"].AttrByIndex(0).ValueAsString();

                                    string subdir = null;
                                    var nsub = configRoot["sub"];
                                    if (nsub.Exists)
                                      subdir = nsub.AttrByIndex(0).ValueAsString(DEFAULT_SUBDIR);


                                    foreach(var cu in compiler)
                                    {
                                      if (cu.CompilationException!=null) continue;
                                      var fs = cu.TemplateSource as FileTemplateStringContentSource;
                                      if (fs==null) continue;

                                      var fn = (re.IsNotNullOrWhiteSpace() ? fs.FileName.Replace(re, string.Empty) : fs.FileName) + ext;
                                      if (dest.IsNullOrWhiteSpace())
                                      {
                                        if (subdir.IsNotNullOrEmpty())
                                        {
                                          var fnDir = Path.GetDirectoryName(fn);
                                          var fnFile = Path.GetFileName(fn);
                                          fnDir = Path.Combine(fnDir, subdir);
                                          if (!Directory.Exists(fnDir)) Directory.CreateDirectory(fnDir);
                                          fn = Path.Combine(fnDir, fnFile);
                                        }
                                        File.WriteAllText(fn , cu.CompiledSource, Encoding.UTF8);
                                      }
                                      else
                                      {
                                        var path = Path.Combine(dest, Path.GetFileName(fn));
                                        if (!Directory.Exists(path)) Directory.CreateDirectory(dest);
                                        File.WriteAllText(path, cu.CompiledSource, Encoding.UTF8);
                                      }
                                    }
                                  }


                                  static void showOptions(TemplateCompiler compiler)
                                  {
                                     const string NONE = "<none>";
                                     Console.WriteLine();
                                     ConsoleUtils.Info("Compiler type: "+compiler.GetType().FullName);
                                     ConsoleUtils.Info("Assembly file: "+compiler.AssemblyFileName ?? NONE);
                                     ConsoleUtils.Info("Base template type name: "+compiler.BaseTypeName ?? NONE);
                                     ConsoleUtils.Info("Compile code: "+compiler.CompileCode ?? NONE);
                                     ConsoleUtils.Info("Language name: "+compiler.LanguageName ?? NONE);
                                     ConsoleUtils.Info("Namespace: "+compiler.Namespace ?? NONE);
                                     foreach(var asm in compiler.ReferencedAssemblies)
                                       ConsoleUtils.Info("Referenced assembly: "+asm);
                                     ConsoleUtils.Info("Ref assembly search path: "+compiler.ReferencedAssembliesSearchPath ?? NONE );
                                  }




    }
}
