using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Common;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.ViewModels.Editors;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;
using YamlDotNet.Serialization;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    /// <summary>
    /// 运行时的代码效果是：把所有输出节点、testpoint、尾巴节点（可跳过），依次解算更新到结构体，如果前面解算了则传递值。
    /// 输入由节点CompileEvent生成，输出由遍历生成（顺带对节点CompileEvent生成）
    /// </summary>
    public class BaseCodeGenNodeViewModel : CodeGenNodeViewModel
    {
        public Dictionary<string, NodeInConfig> InConfigDic = new Dictionary<string, NodeInConfig> { };
        public Dictionary<string, NodeOutConfig> OutConfigDic = new Dictionary<string, NodeOutConfig> { };
        public Dictionary<string, ParamDefine> ParamDic = new Dictionary<string, ParamDefine> { };
        public Dictionary<string, List<AssemblyConfig>> ScriptAssemblyDic = new Dictionary<string, List<AssemblyConfig>> { };
        public Dictionary<string, string> ScriptTempDic = new Dictionary<string, string> { };
        public PropertyList ParamPropertyList = new PropertyList();
        public string CompileEvent(CompilerContext ctx)
        {
            var ScriptLanguage = ctx.ScriptLanguage.ToString();
            var VarDefStatement = "";
            if (ScriptTempDic.ContainsKey(ScriptLanguage))
            {
                var codeTemp = ScriptTempDic[ScriptLanguage];
                foreach (var parmitem in ParamDic)
                {
                    var value = parmitem.Value.DataValue;
                    if(parmitem.Value.PortType == PortType.String)
                    {
                        if (parmitem.Value.IgnoreWhenEmpty && string.IsNullOrWhiteSpace(value))
                            value = "";
                        else
                            value = $"\"{value}\"";
                    }
                    codeTemp = codeTemp.Replace($"[{parmitem.Key}]", value);
                }
                foreach (var inport in InConfigDic)
                {
                    if (inport.Value.IsExpression == false)
                        continue;
                    if(inport.Value.Port is CodeGenInputViewModel<IExpression> cgep)
                    {
                        if(cgep.Value == null)
                            codeTemp = codeTemp.Replace($"[{inport.Key}]", inport.Value.DataValue);
                        else
                        {
                            //输入的表达式
                            var ep = cgep.Value.Compile(ctx);
                            codeTemp = codeTemp.Replace($"[{inport.Key}]", ep);
                        }
                    }
                }
                foreach (var outport in OutConfigDic)
                {
                    if (outport.Value.IsExpression == false)
                        continue;

                    //输出的定义
                    var vardef = outport.Value.VarDef;
                    //if(vardef.VariableName == null)
                    {
                        vardef.VariableName = outport.Key + "_" + ctx.FindFreeVariableName();
                        VarDefStatement += vardef.Compile(ctx);
                    }
                    var ep = vardef.VariableName;
                    codeTemp = codeTemp.Replace($"[{outport.Key}]", ep);
                }
                foreach (var var in ctx.GlobalVariables)
                    codeTemp = codeTemp.Replace($"[{var.Key}]", var.Value.VariableName);
                return VarDefStatement + codeTemp;
            }
            throw new NotImplementedException();
        }
        public BaseCodeGenNodeViewModel() : this(NodeType.Function)
        {
            //思路：
            //在node代码生成时
            //代码ScriptTempDic里面把in代为对应变量名,也就是InConfigDic["In***"]拿到对应的已经连接节点输出的表达式
            //把OutConfigDic["Out***"]的变量名表达式加入全局变量定义，并且输出给下级
            //对于参数字典，生成配置页面进行配置参数值，代入到表达式

            //TODO：
            //done 怎么写入OUT变量定义、拿到变量名 
            //done 怎么拿到IN表达式
            //done CompileEvent异常怎么解决，去掉输出初值生成表达式过程
            //怎么调用这个节点的CompileEvent生成过程，并把代码依次放到主代码，用之前推导的数据流过程依次生成
        }
        public BaseCodeGenNodeViewModel(NodeType type) : base(type)
        {
        }

        public virtual void LoadPorts()
        {
            LoadInputPorts();
            this.Outputs.Clear();
            NodeOutConfig Last = null;
            foreach (var outkv in OutConfigDic)
            {
                var cfg = outkv.Value;
                if (cfg.IsExpression)
                {
                    var vm = new CodeGenOutputViewModel<IExpression>(cfg.PortType)
                    {
                        Name = outkv.Key,
                        Value = Observable.Return(cfg.VarDef.VariableNameExpression)//this.WhenAnyValue(v => cfg.VarDef.VariableNameExpression)
                    };
                    cfg.Port = vm;
                    //cfg.CreateValueEditor(vm);//输出初值编辑生成表达式过程
                    this.Outputs.Add(cfg.Port);
                    //主代码附加
                    Last = cfg;
                }
                else
                {
                    //连接上一个EFlow
                    var vm = new CodeGenOutputViewModel<IStatement>(cfg.PortType)
                    {
                        Name = outkv.Key,
                    };
                    cfg.Port = vm;
                    //cfg.CreateValueEditor(vm);//输出初值编辑生成表达式过程
                    this.Outputs.Add(cfg.Port);
                }
            }
            //主代码附加
            //if (Last != null)
            //    Last.VarDef.CompileEvent = CompileEvent;

            //参数
            ParamPropertyList.Clear();
            foreach (var param in ParamDic)
            {
                var attr = new PropertyAttr(
                    param.Value.Name,
                    param.Value.DataValue,
                    param.Value.GetType(),
                    (v) =>
                    {
                        param.Value.DataValue = v as string;
                        if (param.Value.EnPortsUpdate)
                            LoadPorts();
                    }
                );
                attr.Category = param.Value.Category;
                attr.Description = param.Value.Description;
                attr.DisplayName = param.Value.Name;
                ParamPropertyList.Add(attr);
            }
        }

        public virtual void LoadInputPorts()
        {
            this.Inputs.Clear();
            foreach (var inkv in InConfigDic)
            {
                var cfg = inkv.Value;
                if (cfg.IsExpression)
                {
                    cfg.Port = new CodeGenInputViewModel<IExpression>(cfg.PortType)
                    {
                        Name = inkv.Key
                    };
                    this.Inputs.Add(cfg.Port);
                }
                else
                {
                    //连接下一个EFlow
                    cfg.Port = new CodeGenInputViewModel<IStatement>(cfg.PortType)
                    {
                        Name = inkv.Key
                    };
                    this.Inputs.Add(cfg.Port);
                }
            }
        }
    }

    public class ParamDefine
    {
        public PortType PortType { get; set; } = PortType.String;
        public string DataValue { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool EnPortsUpdate { get; set; }
        public bool IgnoreWhenEmpty { get; internal set; }
    }

    public class AssemblyConfig
    {
        public string Include { get; set; }
        public string IncludePath { get; set; }
        public string Lib { get; set; }
        public string LibPath { get; set; }
    }

    public class NodePortConfig
    {
        public bool IsExpression { get; set; }
        public PortType PortType { get; set; } = PortType.Integer;
    }

    public class NodeInConfig : NodePortConfig
    {
        [YamlIgnore]
        public NodeInputViewModel Port { get; set; }
        public string DataValue { get; set; }
    }

    public class NodeOutConfig : NodePortConfig
    {
        [YamlIgnore]
        public LocalVariableDefinition VarDef { get; set; } = new LocalVariableDefinition();
        [YamlIgnore]
        public NodeOutputViewModel Port { get; set; }
        public string DataType { get => VarDef.DataType; set => VarDef.DataType=value; }
        public string DataValue { get => VarDef.Value; set => VarDef.Value=value; }
        public NodeOutConfig()
        {
        }
#if false //输出初值编辑生成表达式过程
        [YamlIgnore]
        private NodeEndpointEditorViewModel Editor { get; set; }
        public string EditorValue { get; set; } = "";

        internal void CreateValueEditor<T>(CodeGenOutputViewModel<T> outVm) where T : class, IProgram
        {
            switch (PortType)
            {
                case PortType.Integer:
                    {
                        var vm = new IntegerValueEditorViewModel();
                        Editor = vm;
                        outVm.Editor = vm;
                        //在Editor改变配置内容时重新生成表达式对象（好像可以去掉），在CompileEvent时找到ScriptTemp输出
                        //printnode在输入连接时更新输出声明，tostringnode在输入连接时拿到输入的表达式生成新声明
                        //现在是IntegerValueEditor与输出端口耦合太重了，往往一个节点可能有多个配置而不是一个端口一个配置
                        outVm.Value = vm.ValueChanged.Select(v =>
                        {
                            EditorValue = v.ToString();
                            return new BaseExpressionLiteral { CompileEvent = CompileEvent } as T;
                        });
                    }
                    break;
                case PortType.String:
                    {
                        var vm = new StringValueEditorViewModel();
                        Editor = vm;
                        outVm.Editor = vm;
                        outVm.Value = vm.ValueChanged.Select(v =>
                        {
                            EditorValue = v.ToString();
                            return new BaseExpressionLiteral { CompileEvent = CompileEvent } as T;
                        });
                    }
                    break;
                case PortType.Double:
                    {
                        var vm = new DoubleValueEditorViewModel();
                        Editor = vm;
                        outVm.Editor = vm;
                        outVm.Value = vm.ValueChanged.Select(v =>
                        {
                            EditorValue = v.ToString();
                            return new BaseExpressionLiteral { CompileEvent = CompileEvent } as T;
                        });
                    }
                    break;
                case PortType.Execution:
                    {
                        //?
                    }
                    break;
                default:
                    break;
            }
        }
#endif

    }
}