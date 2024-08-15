using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.ViewModels.Editors;
using ExampleCodeGenApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;
using YamlDotNet.Serialization;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    public class MuxNode : BaseCodeGenNodeViewModel
    {
        static MuxNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<MuxNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel ValueEditor { get; } = new DoubleValueEditorViewModel();


        public double IValue { get => ValueEditor.Value; set => ValueEditor.Value = value; }

        public MuxNode() : base()
        {
            this.Name = nameof(MuxNode);
            InConfigDic["In0"] = new NodeInConfig()
            {
                IsExpression = true,
                PortType = PortType.U8,
                Dim = -1,
                DataValue = "new byte[]{}",
            };
            OutConfigDic["Out"] = new NodeOutConfig() {
                IsExpression = true,
                PortType = PortType.U8,
                Dim = -1,
            };
            ParamDic["ElemCount"] = new ParamDefine()
            {
                PortType = PortType.I32,
                Name = "输入元素数量",
                DataValue = "1",
                Description = "输入元素数量",
                EnPortsUpdate = true
            };
            LoadPorts();
        }
        public override void LoadInputPorts()
        {
            this.Inputs.Clear();
            if (int.TryParse(ParamDic["ElemCount"].DataValue, out var ElemCount))
            {
                for (int i = 0; i < ElemCount; i++)
                {
                    var cfg = new NodeInConfig()
                    {
                        IsExpression = true,
                        PortType = PortType.U8,
                        Dim = -1,
                        DataValue = "new byte[]{}",
                    };
                    var inkvKey = $"In{i}";
                    InConfigDic[inkvKey] = cfg;
                    if (cfg.IsExpression)
                    {
                        cfg.Port = new CodeGenInputViewModel<IExpression>(cfg.PortType)
                        {
                            Name = inkvKey
                        };
                        this.Inputs.Add(cfg.Port);
                    }
                    else
                    {
                        //连接下一个EFlow
                        cfg.Port = new CodeGenInputViewModel<IStatement>(cfg.PortType)
                        {
                            Name = inkvKey
                        };
                        this.Inputs.Add(cfg.Port);
                    }
                }
            }
        }
        public override void LoadPorts()
        {
            base.LoadPorts();
            //应该拿到GainVal的配置值替代上，然后绑定输入连接时也更新
            ScriptTempDic[ScriptLanguage.CSharp.ToString()] = @"[Out] = [In0];";
            if (int.TryParse(ParamDic["ElemCount"].DataValue, out var ElemCount))
            {
                ScriptTempDic[ScriptLanguage.CSharp.ToString()] = $"[Out] = {GetInPortExp(0, ElemCount)}.ToArray();";
            }
        }

        private string GetInPortExp(int idx, int elemCount)
        {
            //byte[] a = (new byte[0]).Concat(null).ToArray();
            if (elemCount - idx == 1)
            {
                return $"[In{idx}]";
            }
            return $"[In{idx}].Concat(" + GetInPortExp(idx + 1, elemCount) + ")";
        }
    }
}
