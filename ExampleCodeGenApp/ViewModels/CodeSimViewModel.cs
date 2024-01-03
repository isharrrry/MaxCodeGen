using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;
using MoonSharp.Interpreter;
using ReactiveUI;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Diagnostics;
using ScriptOptions = Microsoft.CodeAnalysis.Scripting.ScriptOptions;
using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyModel;

namespace ExampleCodeGenApp.ViewModels
{
    public class CodeSimViewModel : ReactiveObject
    {
        #region Code
        public IStatement Code
        {
            get => _code;
            set => this.RaiseAndSetIfChanged(ref _code, value);
        }
        private IStatement _code;
        #endregion

        #region Output
        public string Output
        {
            get => _output;
            set => this.RaiseAndSetIfChanged(ref _output, value);
        }
        private string _output;
        #endregion

        public ReactiveCommand<Unit, Unit> RunScript { get; }
        public ReactiveCommand<Unit, Unit> ClearOutput { get; }

        public CodeSimViewModel()
        {
            RunScript = ReactiveCommand.Create(() =>
            {
                //Script script = new Script();
                //script.Globals["print"] = (Action<string>)Print;
                string source = Code.Compile(new CompilerContext());
                //script.DoString(source);
                //RunCsharp(source);
                RunCsharp(source);
            },
                this.WhenAnyValue(vm => vm.Code).Select(code => code != null));

            ClearOutput = ReactiveCommand.Create(() => { Output = ""; });
        }

        private void RunAndBuildCsharp(string source)
        {
            try
            {
                MetadataReference[] _ref =
                 DependencyContext.Default.CompileLibraries
                      .First(cl => cl.Name == "System.Runtime")
                      .ResolveReferencePaths()
                      .Select(asm => MetadataReference.CreateFromFile(asm))
                      .ToArray();
                string testClass = @"using System; 
                  namespace test{
                   public class tes
                   {
                     public string unescape(string Text)
                        { 
                          return Uri.UnescapeDataString(Text);
                        } 
                   }
                  }";

                var compilation = CSharpCompilation.Create(Guid.NewGuid().ToString() + ".dll")
                    .WithOptions(new CSharpCompilationOptions(
                        Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
                        usings: new List<string> { "System" },
                        optimizationLevel: OptimizationLevel.Debug, // TODO
                        checkOverflow: false,                       // TODO
                        allowUnsafe: true,                          // TODO
                        platform: Platform.AnyCpu,
                        warningLevel: 4,
                        xmlReferenceResolver: null // don't support XML file references in interactive (permissions & doc comment includes)
                        ))
                    .AddReferences(_ref)
                    .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source));
                var eResult = compilation.Emit("test.dll");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void RunCsharp(string source)
        {
            try
            {
                var scriptOptions = ScriptOptions.Default.WithImports("System");
                var scriptState = CSharpScript.RunAsync("Console.SetOut(ConsoleWriter);\n\n" + source, scriptOptions, new ConsoleClass() { ConsoleWriter = new ConsoleWriter(Print) }).Result;
                if (scriptState.Exception != null)
                {
                    Debug.WriteLine(scriptState.Exception);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void Print(string msg)
        {
            Output += msg + "\n";
        }
    }

    public class ConsoleClass
    {
        public ConsoleClass()
        {
        }

        public ConsoleWriter ConsoleWriter { get; set; }
    }
}
