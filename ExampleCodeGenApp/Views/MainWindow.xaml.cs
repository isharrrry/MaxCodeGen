using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using ExampleCodeGenApp.ViewModels;
using ReactiveUI;

namespace ExampleCodeGenApp.Views
{
    public partial class MainWindow : Window, IViewFor<MainViewModel>
    {
        #region ViewModel
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(MainViewModel), typeof(MainWindow), new PropertyMetadata(null));

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }
        #endregion

        private readonly MenuItem groupNodesButton;
        private readonly MenuItem ungroupNodesButton;
        private readonly MenuItem openGroupButton;

        public MainWindow()
        {
            InitializeComponent();
            //Config.ConfigWindow.Config(new ViewModels.Nodes.GainNode(), true, false);
            //CodeGenNodeView.PropertyGridShow(new ViewModels.Nodes.GainNode(), "");

            var nodeMenu = ((ContextMenu)Resources["nodeMenu"]).Items.OfType<MenuItem>();
            groupNodesButton = nodeMenu.First(c => c.Name == nameof(groupNodesButton));
            ungroupNodesButton = nodeMenu.First(c => c.Name == nameof(ungroupNodesButton));
            openGroupButton = nodeMenu.First(c => c.Name == nameof(openGroupButton));

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel, vm => vm.Network, v => v.network.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NodeList, v => v.nodeList.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CodePreview, v => v.codePreviewView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CodeSim, v => v.codeSimView.ViewModel).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.NetworkBreadcrumbBar, v => v.breadcrumbBar.ViewModel).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.AutoLayout, v => v.autoLayoutButton);

                this.BindCommand(ViewModel, vm => vm.GroupNodes, v => v.groupNodesButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.UngroupNodes, v => v.ungroupNodesButton).DisposeWith(d);
                this.BindCommand(ViewModel, vm => vm.OpenGroup, v => v.openGroupButton).DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.StartAutoLayoutLive, v => v.startAutoLayoutLiveButton);
                this.WhenAnyObservable(v => v.ViewModel.StartAutoLayoutLive.IsExecuting)
	                .Select((isRunning) => isRunning ? Visibility.Collapsed : Visibility.Visible)
	                .BindTo(this, v => v.startAutoLayoutLiveButton.Visibility);

                this.BindCommand(ViewModel, vm => vm.StopAutoLayoutLive, v => v.stopAutoLayoutLiveButton);
				this.WhenAnyObservable(v => v.ViewModel.StartAutoLayoutLive.IsExecuting)
					.Select((isRunning) => isRunning ? Visibility.Visible : Visibility.Collapsed)
					.BindTo(this, v => v.stopAutoLayoutLiveButton.Visibility);

                //code
                this.OneWayBind(ViewModel, vm => vm.CodeSim.GenerateScript, v => v.genButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CodeSim.BuildScript, v => v.buildButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CodeSim.RunScript, v => v.runButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CodeSim.StopScript, v => v.stopButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CodeSim.ClearOutput, v => v.clearButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.CodeSim.Output, v => v.codeSimView.outputTextBlock.Text).DisposeWith(d);

                //file
                this.OneWayBind(ViewModel, vm => vm.newButton, v => v.newButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.openButton, v => v.openButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.saveButton, v => v.saveButton.Command).DisposeWith(d);
                this.OneWayBind(ViewModel, vm => vm.saveAsButton, v => v.saveAsButton.Command).DisposeWith(d);
            });

            this.ViewModel = new MainViewModel();
            Loaded +=MainWindow_Loaded;
            Closed +=MainWindow_Closed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.New(true);
        }

        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            //ViewModel.Save();
        }
    }
}
