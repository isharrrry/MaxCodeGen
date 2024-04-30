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
    public class SineWareNode : BaseCodeGenNodeViewModel
    {
        static SineWareNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<SineWareNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel ValueEditor { get; } = new DoubleValueEditorViewModel();

        [YamlIgnore]
        public ValueNodeOutputViewModel<IProgram> Output { get; }

        public double IValue { get => ValueEditor.Value; set => ValueEditor.Value = value; }

        public SineWareNode() : base(NodeType.Literal)
        {
            this.Name = "SineWare";

            Output = new CodeGenOutputViewModel<IProgram>(PortType.Double)
            {
                Editor = ValueEditor,
                Value = ValueEditor.ValueChanged.Select(v => new BaseExpressionLiteral
                {
                    CompileEvent = CompileEvent
                })
            };
            this.Outputs.Add(Output);
        }
    }
}
