using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels
{
    public enum PortType
    {
        Integer = 8, DataType = -1, Execution, String, Double, Float, I8, U8, I16, U16, I32, U32, I64, U64
    }

    public class CodeGenPortViewModel : PortViewModel
    {
        static CodeGenPortViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new Views.CodeGenPortView(), typeof(IViewFor<CodeGenPortViewModel>));
        }

        #region PortType
        public PortType PortType
        {
            get => _portType;
            set => this.RaiseAndSetIfChanged(ref _portType, value);
        }
        private PortType _portType;
        #endregion
    }
}
