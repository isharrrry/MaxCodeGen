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
    public class DoubleLiteralNode : CodeGenNodeViewModel
    {
        static DoubleLiteralNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<DoubleLiteralNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel ValueEditor { get; } = new DoubleValueEditorViewModel();

        [YamlIgnore]
        public ValueNodeOutputViewModel<ITypedExpression<double>> Output { get; }

        public double IValue { get => ValueEditor.Value; set => ValueEditor.Value = value; }

        public DoubleLiteralNode() : base(NodeType.Literal)
        {
            this.Name = "Double";

            Output = new CodeGenOutputViewModel<ITypedExpression<double>>(PortType.Double)
            {
                Editor = ValueEditor,
                Value = ValueEditor.ValueChanged.Select(v => new DoubleLiteral{ Value = v})
            };
            this.Outputs.Add(Output);
        }
    }
}
