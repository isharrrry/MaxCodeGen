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
    public class GainNode : BaseCodeGenNodeViewModel
    {
        static GainNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<GainNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel ValueEditor { get; } = new DoubleValueEditorViewModel();


        public double IValue { get => ValueEditor.Value; set => ValueEditor.Value = value; }

        public GainNode() : base()
        {
            this.Name = nameof(GainNode);
            InConfigDic["In"] = new NodeInConfig()
            {
                IsExpression = true,
                PortType = PortType.Double,
            };
            OutConfigDic["Out"] = new NodeOutConfig() { 
                IsExpression = true,
                PortType = PortType.Double,
            };
            OutConfigDic["Out"].ScriptTempDic[ScriptLanguage.CSharp.ToString()] = $"([In] * [Out])";//应该拿到Editor的更新值替代 * 1，然后绑定输入连接时也更新
            LoadPorts();
        }
    }
}
