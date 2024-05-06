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
using NodeNetwork.ViewModels;
using ExampleCodeGenApp.ViewModels.Nodes;
using NodeNetwork.Toolkit.ValueNode;
using ExampleCodeGenApp.Model;

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
        public static List<ScriptLanguage> ScriptLanguages { get; set; } = Enum.GetValues(typeof(ScriptLanguage)).Cast<ScriptLanguage>().ToList();
        public ScriptLanguage ScriptLanguage { get; set; } = ScriptLanguage.CSharp;
        public static List<ScriptMode> ScriptModes { get; set; } = Enum.GetValues(typeof(ScriptMode)).Cast<ScriptMode>().ToList();
        public ScriptMode ScriptMode { get; set; } = ScriptMode.数据流;
        public CodePreviewViewModel CodePreview { get; internal set; }
        public NetworkViewModel Network { get; internal set; }

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
                if(ScriptMode == ScriptMode.数据流)
                {
                    //生成,没有做对没连接完整的报错
                    //拿到dflow节点：如果不是main节点且连接表有一端是这个节点的IStatement端口
                    var eflowNodes = new List<NodeViewModel>();
                    foreach (var node in Network.Nodes.Items)
                    {
                        if (node is not ButtonEventNode)
                        {
                            bool add = true;
                            foreach (var conn in Network.Connections.Items)
                            {
                                //找到节点的连线，判断是否是eflow连线
                                //eflow连线的特点为里面的Value类型为声明
                                if (conn.Input.Parent == node)
                                {
                                    if (conn.Input.ObjValue is IStatement || conn.Input is ValueNodeInputViewModel<IStatement>)
                                    {
                                        add = false;
                                        break;
                                    }
                                }
                                else if (conn.Output.Parent == node)
                                {
                                    if (conn.Output is ValueNodeOutputViewModel<IStatement>)//(conn.Output.ObjValue is IStatement)
                                    {
                                        add = false;
                                        break;
                                    }
                                }
                            }
                            if (add)
                            {
                                eflowNodes.Add(node);
                            }
                        }
                    }
                    //生成代码顺序算法处理
                    var codeEflowNodes = GetOrderEflowNodes(eflowNodes);
                    var ctx = new CompilerContext()
                    {
                        ScriptLanguage = this.ScriptLanguage,
                        ScriptMode = this.ScriptMode
                    };
                    var codes = "";
                    if (eflowNodes.Count != 0)
                    {
                        codes += $"//{eflowNodes.Count}个节点没有被排序到代码生成\n";
                    }
                    Includes?.Clear();
                    ctx.EnterNewScope("Main");
                    InitFlowGlobal(ctx);
                    codes += (ctx.GlobalVariables["SysTimeSec"].Compile(ctx) + "");
                    codes += (ctx.GlobalVariables["CurrentTick"].Compile(ctx) + "");
                    codes += (ctx.GlobalVariables["StepSize"].Compile(ctx) + "");
                    codes += "while (SysTimeSec < (4)) {\n\n";
                    foreach (var node in codeEflowNodes)
                    {
                        if (node is BaseCodeGenNodeViewModel bNode)
                        {
                            //include字典
                            if (bNode.ScriptAssemblyDic.ContainsKey(ctx.ScriptLanguage.ToString()))
                            {
                                foreach (var cfg in bNode.ScriptAssemblyDic[ctx.ScriptLanguage.ToString()])
                                {
                                    Includes[cfg.Include] = (cfg);
                                }
                            }
                            codes += (bNode.CompileEvent(ctx) + "\n");
                            if (codeEflowNodes.Last() == node && bNode.OutConfigDic.Count > 0)
                                codes += $"Console.WriteLine({bNode.OutConfigDic.Last().Value.VarDef.VariableName}.ToString(\"F2\"));\n";
                        }
                        //foreach (var output in node.Outputs.Items)
                        //{
                        //    if (output is ValueNodeOutputViewModel<IExpression> epVm)
                        //    {
                        //        epVm.Value.ForEach(x =>
                        //            codes += (x.Compile(ctx) + "\n"));
                        //    }
                        //}
                    }
                    //codes += $"Console.WriteLine(SysTimeSec.ToString(\"F2\"));\n";
                    codes += "\nCurrentTick++;\nSysTimeSec += StepSize;\n}\n";

                    ctx.LeaveScope();
                    ScriptSource = codes;
                }
                else
                {
                    ScriptSource = Code.Compile(new CompilerContext()
                    {
                        ScriptLanguage = this.ScriptLanguage,
                        ScriptMode = this.ScriptMode
                    });
                }
                CodePreview.PreViewCode = ScriptSource;
                File.WriteAllText("Script/ScriptSource.yml", ScriptSource);
                Output = $"生成结束！脚本共{GetLineCount(ScriptSource)}行！";
            }
            catch (Exception ex)
            {
                Output = ex.ToString();
            }
        }

        public Dictionary<string, AssemblyConfig> Includes { get; set; } = new Dictionary<string, AssemblyConfig> { };
        private string GetIncludeCode(CompilerContext ctx)
        {
            var code = "";
            foreach (var include in Includes)
            {
                if (ctx.ScriptLanguage == ScriptLanguage.CSharp)
                    code += $"using {include.Key};\n";
            }
            return code;
        }

        private void InitFlowGlobal(CompilerContext ctx)
        {
            var SysTimeSec = new LocalVariableDefinition();
            SysTimeSec.VariableName = "SysTimeSec";
            SysTimeSec.DataType = "double";
            SysTimeSec.Value = "0";
            ctx.GlobalVariables["SysTimeSec"] = SysTimeSec;
            var CurrentTick = new LocalVariableDefinition();
            CurrentTick.VariableName = "CurrentTick";
            CurrentTick.DataType = "int";
            CurrentTick.Value = "10";
            ctx.GlobalVariables["CurrentTick"] = CurrentTick;
            var StepSize = new LocalVariableDefinition();
            StepSize.VariableName = "StepSize";
            StepSize.DataType = "double";
            StepSize.Value = "0.1";
            ctx.GlobalVariables["StepSize"] = StepSize;
        }
        private List<NodeViewModel> GetOrderEflowNodes(List<NodeViewModel> eflowNodes)
        {
            List<NodeViewModel> orderNodes = new List<NodeViewModel> { };
            //空输入
            var eflowNodesls = eflowNodes.ToList();
            foreach (var node in eflowNodesls)
            {
                if (node.Inputs.Items.Count() == 0)
                    ItemFromMoveTo(node, eflowNodes, orderNodes);
                else
                {
                    //这个节点的输入没有被连接
                    if (IsFreeInputNode(node))
                        ItemFromMoveTo(node, eflowNodes, orderNodes);
                }
            }
            //所有输入数据源节点来自orderNodes
            bool freeMove = false;
            while (!freeMove)
            {
                freeMove = true;
                var ls = eflowNodes.ToList();
                foreach (var node in ls)
                {
                    var FreeOrInputFormOrder = node.Inputs.Items.Select(x => {
                        return (IsInputConnOrderSet(x, orderNodes, true));
                    });
                    if (FreeOrInputFormOrder.All(x => x == true))
                    {
                        ItemFromMoveTo(node, eflowNodes, orderNodes);
                        freeMove = false;
                    }
                }
            }
            return orderNodes;
        }
        //输入连接到有序节点或者未连接
        private bool IsInputConnOrderSet(NodeInputViewModel x, List<NodeViewModel> orderNodes, bool OrFreeConn = true)
        {
            foreach (var conn in Network.Connections.Items)
            {
                if (conn.Input == x)
                {
                    return orderNodes.IndexOf(conn.Output.Parent) != -1;
                }
            }
            return OrFreeConn;
        }

        //这个节点的输入没有被连接
        private bool IsFreeInputNode(NodeViewModel node)
        {
            bool FreeInput = true;
            foreach (var conn in Network.Connections.Items)
            {
                if (conn.Input.Parent == node)
                {
                    FreeInput = false;
                    break;
                }
            }
            return FreeInput;
        }

        private void ItemFromMoveTo(NodeViewModel item, List<NodeViewModel> oldSet, List<NodeViewModel> newSet)
        {
            oldSet.Remove(item);
            newSet.Add(item);
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
                //include加入代码
                var scriptOptions = ScriptOptions.Default.WithImports("System");
                if (Includes.Count > 0)
                {
                    var gusing = new List<string> { "System", "System.Linq" };
                    gusing.AddRange(Includes.Keys);
                    scriptOptions = ScriptOptions.Default.WithImports(gusing)
                        .AddReferences("System.Linq")
                        .AddReferences("System.Net.Sockets");
                }
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
            Output += msg;
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
