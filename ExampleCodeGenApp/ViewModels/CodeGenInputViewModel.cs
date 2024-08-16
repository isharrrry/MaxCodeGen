using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.ViewModels.Nodes;
using ExampleCodeGenApp.Views;
using NodeNetwork;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    public class CodeGenInputViewModel<T> : ValueNodeInputViewModel<T>
    {
        static CodeGenInputViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new NodeInputView(), typeof(IViewFor<CodeGenInputViewModel<T>>));
        }

        public CodeGenInputViewModel(PortType type)
        {
            this.Port = new CodeGenPortViewModel { PortType = type };

            if (type == PortType.Execution)
            {
                this.PortPosition = PortPosition.Right;
            }

            ConnectionValidator = TypeConnectionValidator;
        }

        private ConnectionValidationResult TypeConnectionValidator(PendingConnectionViewModel pending)
        {
            var t1 = ((CodeGenPortViewModel)pending.Output.Port)?.PortType;
            var t2 = ((CodeGenPortViewModel)Port)?.PortType;
            var isValid = t1 == t2 || t1 >= PortType.Double && t2 >= PortType.Double;
            if (!isValid)
            {
                MainViewModel.Log($"类型 {t1} 与 {t2} 不匹配!");
            }
            return new ConnectionValidationResult(isValid, null);
        }

        public NodePortConfig PortConfig { get; set; }
    }
}
