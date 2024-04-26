using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.ViewModels.Editors;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using YamlDotNet.Serialization;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    public class BaseCodeGenNodeViewModel : CodeGenNodeViewModel
    {
        public Dictionary<string, NodeInConfig> InConfigDic = new Dictionary<string, NodeInConfig> { };
        public Dictionary<string, NodeOutConfig> OutConfigDic = new Dictionary<string, NodeOutConfig> { };

        public BaseCodeGenNodeViewModel() : this(NodeType.Function)
        {
        }
        public BaseCodeGenNodeViewModel(NodeType type) : base(type)
        {
        }

        public void LoadPorts()
        {
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
            foreach (var outkv in OutConfigDic)
            {
                var cfg = outkv.Value;
                if (cfg.IsExpression)
                {
                    var vm = new CodeGenOutputViewModel<IExpression>(cfg.PortType)
                    {
                        Name = outkv.Key,
                    };
                    cfg.Port = vm;
                    cfg.CreateValueEditor(vm);
                    this.Outputs.Add(cfg.Port);
                }
                else
                {
                    //连接上一个EFlow
                    var vm = new CodeGenOutputViewModel<IStatement>(cfg.PortType)
                    {
                        Name = outkv.Key,
                    };
                    cfg.Port = vm;
                    cfg.CreateValueEditor(vm);
                    this.Outputs.Add(cfg.Port);
                }
            }
        }
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
    }

    public class NodeOutConfig : NodePortConfig
    {
        public Dictionary<string, string> ScriptTempDic = new Dictionary<string, string> { };
        [YamlIgnore]
        public NodeOutputViewModel Port { get; set; }
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

        public string CompileEvent(CompilerContext ctx)
        {
            if (ScriptTempDic.ContainsKey(ctx.ScriptLanguage.ToString()))
            {
                return ScriptTempDic[ctx.ScriptLanguage.ToString()];
            }
            throw new NotImplementedException();
        }
    }
}