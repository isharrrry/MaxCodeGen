using ExampleCodeGenApp.Views.Editors;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;

namespace ExampleCodeGenApp.ViewModels.Editors
{
    public class DoubleValueEditorViewModel : ValueEditorViewModel<double>
    {
        static DoubleValueEditorViewModel()
        {
            Splat.Locator.CurrentMutable.Register(() => new DoubleValueEditorView(), typeof(IViewFor<DoubleValueEditorViewModel>));
        }

        public DoubleValueEditorViewModel()
        {
            Value = 0;
        }
    }
}
