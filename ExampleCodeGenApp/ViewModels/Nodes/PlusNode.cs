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
    public class PlusNode : BaseCodeGenNodeViewModel
    {
        static PlusNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<PlusNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel ValueEditor { get; } = new DoubleValueEditorViewModel();


        public double IValue { get => ValueEditor.Value; set => ValueEditor.Value = value; }

        public PlusNode() : base()
        {
            this.Name = nameof(PlusNode);
            InConfigDic["In1"] = new NodeInConfig()
            {
                IsExpression = true,
                PortType = PortType.Double,
                DataValue = "0",
            };
            InConfigDic["In2"] = new NodeInConfig()
            {
                IsExpression = true,
                PortType = PortType.Double,
                DataValue = "0",
            };
            OutConfigDic["Out"] = new NodeOutConfig() {
                IsExpression = true,
                PortType = PortType.Double,
                DataValue = "0",
            };
            //应该拿到GainVal的配置值替代上，然后绑定输入连接时也更新
            ScriptTempDic[ScriptLanguage.CSharp.ToString()] = $"[Out] = [In1] + [In2];";
            ScriptTempDic[ScriptLanguage.C.ToString()] = $"[Out] = [In1] + [In2];";
            LoadPorts();
        }
    }
}
