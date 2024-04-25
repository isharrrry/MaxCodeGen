using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.Views;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Nodes
{
    /// <summary>
    /// ValueNodeInputViewModel 、 ValueNodeOutputViewModel 是IProgram作为Value的容器
    /// 输入值换了ITypedExpression后，IStatement值会重新生成FunctionCall对象（基于ITypedExpression的引用的对象）
    /// </summary>
    public class PrintNode : CodeGenNodeViewModel
    {
        static PrintNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<PrintNode>));
        }

        public ValueNodeInputViewModel<ITypedExpression<string>> Text { get; }

        public ValueNodeOutputViewModel<IStatement> Flow { get; }
        
        public PrintNode() : base(NodeType.Function)
        {
            this.Name = "Print";

            Text = new CodeGenInputViewModel<ITypedExpression<string>>(PortType.String)
            {
                Name = "Text"
            };
            this.Inputs.Add(Text);

            Flow = new CodeGenOutputViewModel<IStatement>(PortType.Execution)
            {
                Name = "",
                Value = this.Text.ValueChanged.Select(stringExpr => new FunctionCall
                {
                    //FunctionName = "print",
                    FunctionName = "Console.Write",
                    Parameters =
                    {
                        stringExpr ?? new StringLiteral{Value = ""}
                    }
                })
            };
            this.Outputs.Add(Flow);
        }
    }
}
