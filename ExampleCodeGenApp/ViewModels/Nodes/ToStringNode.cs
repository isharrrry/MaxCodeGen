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
using ReactiveUI;
using YamlDotNet.Serialization;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    public class ToStringNode : CodeGenNodeViewModel
    {
        static ToStringNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<ToStringNode>));
        }

        [YamlIgnore]
        public ValueNodeOutputViewModel<ITypedExpression<string>> Output { get; }

        [YamlIgnore]
        public ValueNodeInputViewModel<ITypedExpression<int>> Input { get; }

        public ToStringNode() : base(NodeType.Literal)
        {
            this.Name = "ToString";

            Input = new CodeGenInputViewModel<ITypedExpression<int>>(PortType.Integer)
            {
                Name = "Input"
            };
            this.Inputs.Add(Input);

            Output = new CodeGenOutputViewModel<ITypedExpression<string>>(PortType.String)
            {
                Name = "Output",
                Value = Input.ValueChanged.Select(v => new ToStringExpression() { Expression = v })
            };
            this.Outputs.Add(Output);
        }
    }
}
