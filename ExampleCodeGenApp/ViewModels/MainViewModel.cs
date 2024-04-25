using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Common;
using DynamicData;
using ExampleCodeGenApp.Model;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.ViewModels.Nodes;
using NodeNetwork.Toolkit.BreadcrumbBar;
using NodeNetwork.Toolkit.Group;
using NodeNetwork.Toolkit.Layout;
using NodeNetwork.Toolkit.Layout.ForceDirected;
using NodeNetwork.Toolkit.NodeList;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.Utilities;
using NodeNetwork.ViewModels;
using ReactiveUI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeInspectors;

namespace ExampleCodeGenApp.ViewModels
{
    class NetworkBreadcrumb : BreadcrumbViewModel
    {
        #region Network
        private NetworkViewModel _network;
        public NetworkViewModel Network
        {
            get => _network;
            set => this.RaiseAndSetIfChanged(ref _network, value);
        }
        #endregion
    }

    public class MainViewModel : ReactiveObject
    {
        #region Network
        private readonly ObservableAsPropertyHelper<NetworkViewModel> _network;
        public NetworkViewModel Network => _network.Value;
        #endregion

        public BreadcrumbBarViewModel NetworkBreadcrumbBar { get; } = new BreadcrumbBarViewModel();
        public NodeListViewModel NodeList { get; } = new NodeListViewModel();
        public CodePreviewViewModel CodePreview { get; } = new CodePreviewViewModel();
        public CodeSimViewModel CodeSim { get; } = new CodeSimViewModel();

        public ReactiveCommand<Unit, Unit> AutoLayout { get; }
        public ReactiveCommand<Unit, Unit> StartAutoLayoutLive { get; }
        public ReactiveCommand<Unit, Unit> StopAutoLayoutLive { get; }

        public ReactiveCommand<Unit, Unit> GroupNodes { get; }
        public ReactiveCommand<Unit, Unit> UngroupNodes { get; }
        public ReactiveCommand<Unit, Unit> OpenGroup { get; }

        public MainViewModel()
        {
            CodeSim.CodePreview = CodePreview;
            this.WhenAnyValue(vm => vm.NetworkBreadcrumbBar.ActiveItem).Cast<NetworkBreadcrumb>()
                .Select(b => b?.Network)
                .ToProperty(this, vm => vm.Network, out _network);
            NetworkBreadcrumbBar.ActivePath.Add(new NetworkBreadcrumb
            {
                Name = "Main",
                Network = new NetworkViewModel()
            });

            LoadProgramNode();

            ForceDirectedLayouter layouter = new ForceDirectedLayouter();
            AutoLayout = ReactiveCommand.Create(() => layouter.Layout(new Configuration { Network = Network }, 10000));
            StartAutoLayoutLive = ReactiveCommand.CreateFromObservable(() =>
                Observable.StartAsync(ct => layouter.LayoutAsync(new Configuration { Network = Network }, ct)).TakeUntil(StopAutoLayoutLive)
            );
            StopAutoLayoutLive = ReactiveCommand.Create(() => { }, StartAutoLayoutLive.IsExecuting);

            var grouper = new NodeGrouper
            {
                GroupNodeFactory = subnet => new GroupNodeViewModel(subnet),
                EntranceNodeFactory = () => new GroupSubnetIONodeViewModel(Network, true, false) { Name = "Group Input" },
                ExitNodeFactory = () => new GroupSubnetIONodeViewModel(Network, false, true) { Name = "Group Output" },
                SubNetworkFactory = () => new NetworkViewModel(),
                IOBindingFactory = (groupNode, entranceNode, exitNode) =>
                    new CodeNodeGroupIOBinding(groupNode, entranceNode, exitNode)
            };
            GroupNodes = ReactiveCommand.Create(() =>
            {
                var groupBinding = (CodeNodeGroupIOBinding)grouper.MergeIntoGroup(Network, Network.SelectedNodes.Items);
                ((GroupNodeViewModel)groupBinding.GroupNode).IOBinding = groupBinding;
                ((GroupSubnetIONodeViewModel)groupBinding.EntranceNode).IOBinding = groupBinding;
                ((GroupSubnetIONodeViewModel)groupBinding.ExitNode).IOBinding = groupBinding;
            }, this.WhenAnyObservable(vm => vm.Network.SelectedNodes.CountChanged).Select(c => c > 1));

            var isGroupNodeSelected = this.WhenAnyValue(vm => vm.Network)
                .Select(net => net.SelectedNodes.Connect())
                .Switch()
                .Select(_ => Network.SelectedNodes.Count == 1 && Network.SelectedNodes.Items.First() is GroupNodeViewModel);

            UngroupNodes = ReactiveCommand.Create(() =>
            {
                var selectedGroupNode = (GroupNodeViewModel)Network.SelectedNodes.Items.First();
                grouper.Ungroup(selectedGroupNode.IOBinding);
            }, isGroupNodeSelected);

            OpenGroup = ReactiveCommand.Create(() =>
            {
                var selectedGroupNode = (GroupNodeViewModel)Network.SelectedNodes.Items.First();
                NetworkBreadcrumbBar.ActivePath.Add(new NetworkBreadcrumb
                {
                    Network = selectedGroupNode.Subnet,
                    Name = selectedGroupNode.Name
                });
            }, isGroupNodeSelected);
        }

        private void LoadProgramNode()
        {
            ButtonEventNode eventNode = new ButtonEventNode { CanBeRemovedByUser = false };
            Network.Nodes.Add(eventNode);
            ButtonEventNodeInit(eventNode);

            //NodeList.AddNodeType(() => new ButtonEventNode());
            NodeList.AddNodeType(() => new IntLiteralNode());
            NodeList.AddNodeType(() => new TextLiteralNode());
            NodeList.AddNodeType(() => new RealtimeNode());
            NodeList.AddNodeType(() => new ForLoopNode());
            NodeList.AddNodeType(() => new PrintNode());


        }

        private void ButtonEventNodeInit(ButtonEventNode eventNode)
        {
            var codeObservable = eventNode.OnClickFlow.Values.Connect().Select(_ => new StatementSequence(eventNode.OnClickFlow.Values.Items));
            //codeObservable.BindTo(this, vm => vm.CodePreview.Code);
            codeObservable.BindTo(this, vm => vm.CodeSim.Code);
        }

        static List<Type> WithTypeMappingSet;
        static MainViewModel()
        {
            WithTypeMappingSet = GetWithTypeMappingSet();
        }
        private static List<Type> GetWithTypeMappingSet()
        {
            Dictionary<Type, Type> tDic = new Dictionary<Type, Type>();
            var tList = Assembly.GetExecutingAssembly().GetSubClassTypesByType(typeof(IStatement));
            tList.AddRange(Assembly.GetExecutingAssembly().GetSubClassTypesByType(typeof(IExpression)));
            tList.AddRange(Assembly.GetExecutingAssembly().GetSubClassTypesByType(typeof(Endpoint)));
            tList.AddRange(Assembly.GetExecutingAssembly().GetSubClassTypesByType(typeof(NodeViewModel)));
            tList.AddRange(Assembly.GetExecutingAssembly().GetSubClassTypesByType(typeof(NodeEndpointEditorViewModel)));
            tList.AddRange(Assembly.GetExecutingAssembly().GetSubClassTypesByType(typeof(NetworkViewModel)));
            tList.AddRange(Assembly.GetExecutingAssembly().GetSubClassTypesByType(typeof(PortViewModel)));
            //tList.AddRange(Assembly.GetExecutingAssembly().GetSubClassTypesByType(typeof(NodeInputViewModel)));
            tList.Add((typeof(ConnectionViewModel)));
            tList.Add((typeof(CodeGenNodeViewModel)));
            tList.Add((typeof(NodeViewModel)));
            tList.Add((typeof(NodeEndpointEditorViewModel)));
            tList.Add((typeof(NetworkViewModel)));
            tList.Add((typeof(PortViewModel)));
            tList.Add((typeof(CodeGenOutputViewModel<ITypedExpression<int>>)));
            tList.Add((typeof(CodeGenOutputViewModel<ITypedExpression<string>>)));
            tList.Add((typeof(CodeGenOutputViewModel<ITypedExpression<double>>)));
            tList.Add((typeof(CodeGenInputViewModel<ITypedExpression<int>>)));
            tList.Add((typeof(CodeGenInputViewModel<ITypedExpression<string>>)));
            tList.Add((typeof(CodeGenInputViewModel<ITypedExpression<double>>)));
            //tList.Add((typeof(ValueNodeInputViewModel<ITypedExpression<string>>))); 
            //tList.Add((typeof(ObservableCollection<ISwitchModule>)));
            foreach (var t in tList)
            {
                tDic[t] = t;
            }
            return tDic.Values.ToList();
        }
        public T Deserialize<T>(string SerializeText)
        {
            return YmlConfigHelper.ConfigDeserializeWithTypeMappingSet<T>(SerializeText, WithTypeMappingSet);
        }

        internal void Load()
        {
            try
            {
                var obj = Deserialize<NetworkConfig>(File.ReadAllText("All.yml"));
                Network.Nodes.Clear();
                foreach (var item in obj.Nodes)
                {
                    if (item is ButtonEventNode bt)
                    {
                        bt.CanBeRemovedByUser = false;
                        ButtonEventNodeInit(bt);
                    }
                    Network.Nodes.Add(item);
                }
                obj.GetConnectionConfigs(Network);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public string Serialize()
        {
            var cfg = new NetworkConfig();
            cfg.Nodes = Network.Nodes.Items.ToList();
            cfg.SetConnectionConfigs(Network.Connections.Items.ToList());
            return YmlConfigHelper.ConfigSerializeWithTypeMappingSet(cfg, WithTypeMappingSet, SerializerBuilderWith);
        }

        internal void Save()
        {
            // 创建一个Serializer实例
            //var serializer = new SerializerBuilder()
            //    //.WithNamingConvention(CamelCaseNamingConvention.Instance)
            //    .Build();

            //把列表中所有节点序列化，把节点的关系序列化、组关系
            //找列表：创建节点过程NetworkViewModel的Nodes和Connections、创建组过程

            //// 将对象序列化为YAML字符串
            //string yaml = serializer.Serialize(Network);

            if(MessageBox.Show("是否保存？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var yaml = Serialize();
                File.WriteAllText("All.yml", yaml);
            }
        }

        private void SerializerBuilderWith(SerializerBuilder serializer)
        {
            var ignore = new YamlIgnoreAttribute();
            serializer = serializer.WithAttributeOverride<ReactiveObject>(d => d.Changed, ignore);
            serializer = serializer.WithAttributeOverride<ReactiveObject>(d => d.Changing, ignore);
            serializer = serializer.WithAttributeOverride<ReactiveObject>(d => d.ThrownExceptions, ignore);
            //serializer = serializer.WithAttributeOverride<IObservableList<Object>>(d => d.CountChanged, ignore);
            //NetworkViewModel
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.OnPendingConnectionDropped, ignore);
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.Validator, ignore);
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.SelectionRectangle, ignore);
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.ConnectionFactory, ignore);
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.SelectedNodes, ignore);
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.DeleteSelectedNodes, ignore);
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.UpdateValidation, ignore);
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.Validation, ignore);
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.ConnectionsUpdated, ignore);
            serializer = serializer.WithAttributeOverride<NetworkViewModel>(d => d.NetworkChanged, ignore);

            //NodeViewModel
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.Parent, ignore);//比导致ButtonEventNode 具有Parent
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.EndpointGroupViewModelFactory, ignore);
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.HeaderIcon, ignore);
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.Resizable, ignore);
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.CanBeRemovedByUser, ignore);
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.Inputs, ignore);
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.Outputs, ignore);
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.VisibleInputs, ignore);
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.VisibleOutputs, ignore);
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.VisibleEndpointGroups, ignore);
            serializer = serializer.WithAttributeOverride<NodeViewModel>(d => d.IsSelected, ignore);
            serializer = serializer.WithAttributeOverride<IntLiteralNode>(d => d.ValueEditor, ignore);
            serializer = serializer.WithAttributeOverride<IntLiteralNode>(d => d.Output, ignore);
            serializer = serializer.WithAttributeOverride<GroupSubnetIONodeViewModel>(d => d.IOBinding, ignore);
            serializer = serializer.WithAttributeOverride<GroupSubnetIONodeViewModel>(d => d.AddEndpointDropPanelVM, ignore);

            //PortViewModel
            //serializer = serializer.WithAttributeOverride<PortViewModel>(d => d.Parent, ignore);
            serializer = serializer.WithAttributeOverride<PortViewModel>(d => d.IsVisible, ignore);
            serializer = serializer.WithAttributeOverride<PortViewModel>(d => d.IsHighlighted, ignore);

            //Endpoint
            //serializer = serializer.WithAttributeOverride<Endpoint>(d => d.Parent, ignore);
            serializer = serializer.WithAttributeOverride<Endpoint>(d => d.Port, ignore);
            serializer = serializer.WithAttributeOverride<Endpoint>(d => d.Visibility, ignore);
            serializer = serializer.WithAttributeOverride<Endpoint>(d => d.Connections, ignore);
            serializer = serializer.WithAttributeOverride<NodeInputViewModel>(d => d.ConnectionValidator, ignore);
            serializer = serializer.WithAttributeOverride<NodeInputViewModel>(d => d.IsEditorVisible, ignore);
            serializer = serializer.WithAttributeOverride<NodeInputViewModel>(d => d.HideEditorIfConnected, ignore);
            serializer = serializer.WithAttributeOverride<NodeInputViewModel>(d => d.MaxConnections, ignore);
            serializer = serializer.WithAttributeOverride<NodeInputViewModel>(d => d.PortPosition, ignore);
            serializer = serializer.WithAttributeOverride<NodeOutputViewModel>(d => d.MaxConnections, ignore);
            serializer = serializer.WithAttributeOverride<NodeOutputViewModel>(d => d.PortPosition, ignore);
            serializer = serializer.WithAttributeOverride<ValueListNodeInputViewModel<object>>(d => d.Values, ignore);

            //NodeEndpointEditorViewModel
            //serializer = serializer.WithAttributeOverride<NodeEndpointEditorViewModel>(d => d.Parent, ignore);
            //serializer = serializer.WithAttributeOverride<ValueEditorViewModel<Object>>(d => d.ValueChanged, ignore);

            serializer.WithTypeInspector(inner => new CustomTypeInspector(inner));


            //....
            serializer = serializer.WithAttributeOverride<ButtonEventNode>(d => d.OnClickFlow, ignore);
            serializer = serializer.WithAttributeOverride<CodeGenNodeViewModel>(d => d.NodeType, ignore);
            serializer = serializer.WithAttributeOverride<Size>(d => d.IsEmpty, ignore);
            serializer = serializer.WithAttributeOverride<PrintNode>(d => d.Text, ignore);
            serializer = serializer.WithAttributeOverride<PrintNode>(d => d.Flow, ignore);
            serializer = serializer.WithAttributeOverride<TextLiteralNode>(d => d.ValueEditor, ignore);
            serializer = serializer.WithAttributeOverride<TextLiteralNode>(d => d.Output, ignore);
            serializer = serializer.WithAttributeOverride<ConnectionViewModel>(d => d.Parent, ignore);
        }

    }

    internal class CustomTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeInspector;

        public CustomTypeInspector(ITypeInspector innerTypeInspector)
        {
            this.innerTypeInspector = innerTypeInspector;
        }

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            var properties = innerTypeInspector.GetProperties(type, container);

            // 忽略带有泛型类型的属性
            properties = properties.Where(p =>
            {
                if (p.Name == "CountChanged")
                {
                    var s = type;
                    return !(
                        IsImportGenInterface(container, typeof(IObservableList<>))
                    );
                }
                else if (p.Name == "ValueChanged")
                {
                    return !(
                       (typeof(NodeEndpointEditorViewModel)).IsAssignableFrom(container.GetType())//ValueEditorViewModel<> 不好判断，用IsAssignableFrom
                    || (typeof(NodeInputViewModel)).IsAssignableFrom(container.GetType())
                    );
                }
                return true;
            });
            return properties;
        }

        public static bool IsImportGenInterface(object container, Type type)
        {
            return container.GetType().GetInterfaces().Where(x => x.FullName.StartsWith(type.FullName)).Count() > 0;
        }
    }
}
