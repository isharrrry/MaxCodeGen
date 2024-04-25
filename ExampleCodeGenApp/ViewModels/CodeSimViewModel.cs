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
using System.IO;
using System.Reflection;
using System.Threading;
using ExampleCodeGenApp.Views;

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

        public ReactiveCommand<Unit, Unit> GenerateScript { get; }
        public ReactiveCommand<Unit, Unit> BuildScript { get; }
        public ReactiveCommand<Unit, Unit> RunScript { get; }
        public ReactiveCommand<Unit, Unit> StopScript { get; }
        public ReactiveCommand<Unit, Unit> ClearOutput { get; }
        string ScriptSource = "";

        public CodeSimViewModel()
        {
            if (!Directory.Exists("Script"))
                Directory.CreateDirectory("Script");
            GenerateScript = ReactiveCommand.Create(GenerateScriptExec);
            BuildScript = ReactiveCommand.Create(BuildScriptExec);
            StopScript = ReactiveCommand.Create(StopScriptExec);
            RunScript = ReactiveCommand.Create(RunScriptExec,
                this.WhenAnyValue(vm => vm.Code).Select(code => code != null));

            ClearOutput = ReactiveCommand.Create(() => { Output = ""; });
        }
        private void GenerateScriptExec()
        {
            try
            {
                Builded = false;
                Output = $"生成开始...";
                //Script script = new Script();
                //script.Globals["print"] = (Action<string>)Print;
                ScriptSource = Code.Compile(new CompilerContext());
                File.WriteAllText("Script/ScriptSource.yml", ScriptSource);
                Output = $"生成结束！脚本共{GetLineCount(ScriptSource)}！";
            }
            catch (Exception ex)
            {
                Output = ex.ToString();
            }
        }

        private void BuildScriptExec()
        {
            BuildCsharp(ScriptSource);
        }
        CancellationTokenSource CTS;

        private void StopScriptExec()
        {
            CTS?.Cancel();
        }

        private void RunScriptExec()
        {
            //script.DoString(source);

            // 创建一个 CancellationTokenSource 对象
            CTS = new CancellationTokenSource();
            // 创建一个 CancellationToken 对象，用于传递给 CSharpScript.RunAsync 方法
            CancellationToken token = CTS.Token;
            Task.Run(() => { RunCsharp(ScriptSource, token); });
            

            //if (Builded)
            //{
            //    //ProcessHelper.Start("dotnet", "Script/test.dll", ProcessWindowStyle.Normal, "./");
            //    //?dotnet Script/test.dll
            //    //                A fatal error was encountered.The library 'hostpolicy.dll' required to execute the application was not found in 'F:\hhl\IOMAP2023\NodeNetwork\ExampleCodeGenApp\bin\Debug\net6.0-windows\Script\'.
            //    //Failed to run as a self-contained app.
            //    //  - The application was run as a self-contained app because 'F:\hhl\IOMAP2023\NodeNetwork\ExampleCodeGenApp\bin\Debug\net6.0-windows\Script\test.runtimeconfig.json' was not found.
            //    //  - If this should be a framework-dependent app, add the 'F:\hhl\IOMAP2023\NodeNetwork\ExampleCodeGenApp\bin\Debug\net6.0-windows\Script\test.runtimeconfig.json' file and specify the appropriate framework.
            //    Console.SetOut(new ConsoleWriter(Print));
            //    try
            //    {
            //        Output = $"运行开始...";
            //        Assembly assembly = Assembly.Load("./test.dll");//加载不了
            //        var obj = assembly.CreateInstance("Test.Program");
            //        var method = obj.GetType().GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
            //        var added = method.Invoke(obj, new object[] { });
            //        Output = $"运行结束！";
            //    }
            //    catch (Exception ex)
            //    {
            //        Output = ex.ToString();
            //    }
            //}
        }

        private object GetLineCount(string scriptSource)
        {
            return scriptSource.Split('\n').Count();
        }
        Boolean Builded = false;
        private void BuildCsharp(string source)
        {
            try
            {
                Output = $"编译开始...";
                //匹配不上名字
                //var CompileLibraries = DependencyContext.Default.CompileLibraries
                //      .Where(cl =>
                //         cl.Assemblies.Contains(typeof(object).Assembly.FullName)
                //      || cl.Assemblies.Contains(typeof(Console).Assembly.FullName)
                //      );
                var AssemblyLs = new Dictionary<Assembly, Assembly> { };
                AssemblyLs[typeof(Object).Assembly] = null;
                AssemblyLs[typeof(Type).Assembly] = null;
                AssemblyLs[typeof(System.Threading.Thread).Assembly] = null;
                AssemblyLs[typeof(Console).Assembly] = null;
                AssemblyLs[typeof(decimal).Assembly] = null;
                AssemblyLs[typeof(List<>).Assembly] = null;
                var _ref = new List<MetadataReference> { };
                //_ref.AddRange(AssemblyLs.Keys.Select(x => MetadataReference.CreateFromFile(x.Location)));
                foreach (var item in DependencyContext.Default.CompileLibraries)
                {
                    var collection = item.ResolveReferencePaths();
                    foreach (var asm in collection)
                    {
                        _ref.Add(MetadataReference.CreateFromFile(asm));
                    }
                }
                string testClass = @"using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace Test{
    public class Program
    {
        //[STAThread]
        public static void Main()
        { 
            @@@
            Console.ReadKey();
        } 
    }
}";
                source = testClass.Replace("@@@", source);
                var syntaxTree = CSharpSyntaxTree.ParseText(source);

                var dll = Guid.NewGuid().ToString();
                if (File.Exists(dll))
                {
                    File.Delete(dll);
                }
                var compilation = CSharpCompilation.Create("Test")
                    .AddReferences(_ref)
                    .AddSyntaxTrees(syntaxTree)
                    .WithOptions(new CSharpCompilationOptions(
                        Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary
                        ));
                //,
                //        usings: new List<string> { "System" },
                //        optimizationLevel: OptimizationLevel.Release, // TODO
                //        checkOverflow: true,                       // TODO
                //        allowUnsafe: true,                          // TODO
                //        platform: Platform.AnyCpu,
                //        warningLevel: 4,
                //        xmlReferenceResolver: null // don't support XML file references in interactive (permissions & doc comment includes)

                dll = "test.dll";
                var eResult = compilation.Emit(dll);
                Output = $"编译结束！{(eResult.Success ? "成功" : "失败")}！\n";
                // 输出编译失败的原因
                foreach (var diagnostic in eResult.Diagnostics)
                {
                    Output += (diagnostic.ToString()) + "\n";
                }
                Builded = eResult.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Output = ex.ToString();
            }
        }

        private async void RunCsharp(string source, CancellationToken token)
        {
            try
            {
                CodeSimView.Handle.Invoke(() => {
                    Output = $"运行开始...\n";
                });
                var scriptOptions = ScriptOptions.Default.WithImports("System");
                var scriptState = await CSharpScript.RunAsync(
                    "Console.SetOut(ConsoleWriter);\n\n" + source
                    , scriptOptions
                    , new ConsoleClass() { 
                        ConsoleWriter = new ConsoleWriter(s => {
                            CodeSimView.Handle.Invoke(() => {
                                Print(s);
                            });
                        })
                    } 
                    ,cancellationToken: token);
                // 检查执行结果
                if (scriptState.Exception != null)
                {
                    Debug.WriteLine(scriptState.Exception);
                    CodeSimView.Handle.Invoke(() => {
                        Output += $"Script execution failed: {scriptState.Exception}";
                    });
                }
                else
                {
                }
                CodeSimView.Handle.Invoke(() => {
                    Output += $"运行结束！";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                CodeSimView.Handle.Invoke(() => {
                    Output = ex.ToString();
                });
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
