using System.Windows;
using ExampleCodeGenApp.ViewModels;
using ExampleCodeGenApp.ViewModels.Editors;
using ReactiveUI;

namespace ExampleCodeGenApp.Views.Editors
{
    public partial class DoubleValueEditorView : IViewFor<DoubleValueEditorViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(DoubleValueEditorViewModel), typeof(DoubleValueEditorView), new PropertyMetadata(null));

        public DoubleValueEditorViewModel ViewModel
        {
            get => (DoubleValueEditorViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (DoubleValueEditorViewModel)value;
        }
        #endregion

        public DoubleValueEditorView()
        {
            InitializeComponent();

            this.WhenActivated(d => d(
                this.Bind(ViewModel, vm => vm.Value, v => v.UpDown.Value)
            ));
        }
    }
}
