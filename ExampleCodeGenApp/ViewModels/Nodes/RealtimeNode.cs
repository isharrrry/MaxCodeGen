using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
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
    public class RealtimeNode : CodeGenNodeViewModel
    {
        static RealtimeNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<RealtimeNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel StepSizeValueEditor { get; } = new DoubleValueEditorViewModel();

        [YamlIgnore]
        public ValueNodeOutputViewModel<ITypedExpression<double>> StepSizeOutput { get; }

        public double StepSize { get => StepSizeValueEditor.Value; set => StepSizeValueEditor.Value = value; }


        [YamlIgnore]
        public ValueNodeOutputViewModel<IStatement> FlowIn { get; }

        [YamlIgnore]
        public ValueListNodeInputViewModel<IStatement> SetupFlow { get; }

        [YamlIgnore]
        public ValueListNodeInputViewModel<IStatement> LoopStepFlow { get; }

        [YamlIgnore]
        public ValueNodeOutputViewModel<ITypedExpression<int>> CurrentTick { get; }

        public RealtimeNode() : base(NodeType.FlowControl)
        {
            var boundsGroup = new EndpointGroup("Bounds");

            var controlFlowGroup = new EndpointGroup("Control Flow");

            var controlFlowInputsGroup = new EndpointGroup(controlFlowGroup);

            this.Name = "Realtime Task";


            SetupFlow = new CodeGenListInputViewModel<IStatement>(PortType.Execution)
            {
                Name = "Setup",
                Group = controlFlowInputsGroup
            };
            this.Inputs.Add(SetupFlow);

            LoopStepFlow = new CodeGenListInputViewModel<IStatement>(PortType.Execution)
            {
                Name = "Loop Step",
                Group = controlFlowInputsGroup
            };
            this.Inputs.Add(LoopStepFlow);




            Realtime task = new Realtime();

            var loopStepChanged = LoopStepFlow.Values.Connect().Select(_ => Unit.Default).StartWith(Unit.Default);
            var setupChanged = SetupFlow.Values.Connect().Select(_ => Unit.Default).StartWith(Unit.Default);
            FlowIn = new CodeGenOutputViewModel<IStatement>(PortType.Execution)
            {
                Name = "",
                Value = Observable.CombineLatest(loopStepChanged, setupChanged, StepSizeValueEditor.ValueChanged,
                        (loopStepChange, setupChange, setpSize) => (loopStepChange, setupChange, setpSize))
                            .Select(v => {
                                task.LoopStep = new StatementSequence(LoopStepFlow.Values.Items);
                                task.Setup = new StatementSequence(SetupFlow.Values.Items);
                                task.StepSize = new DoubleLiteral { Value = v.setpSize };
                                return task; 
                            }),
                Group = controlFlowGroup
            };
            this.Outputs.Add(FlowIn);


            StepSizeOutput = new CodeGenOutputViewModel<ITypedExpression<double>>(PortType.Double)
            {
                Editor = StepSizeValueEditor,
                Value = StepSizeValueEditor.ValueChanged.Select(v => new DoubleLiteral { Value = v })
            };
            this.Outputs.Add(StepSizeOutput);

            StepSizeValueEditor.Value = 1;

            CurrentTick = new CodeGenOutputViewModel<ITypedExpression<int>>(PortType.Integer)
            {
                Name = "Current Tick",
                Value = Observable.Return(new VariableReference<int>{ LocalVariable = task.CurrentTick })
            };
            this.Outputs.Add(CurrentTick);
        }
    }
}
