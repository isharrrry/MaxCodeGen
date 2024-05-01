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
    public class SineWaveNode : BaseCodeGenNodeViewModel
    {
        static SineWaveNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<SineWaveNode>));
        }


        public SineWaveNode() : base(NodeType.Literal)
        {
            this.Name = "SineWare";

            OutConfigDic["Out"] = new NodeOutConfig()
            {
                IsExpression = true,
                PortType = PortType.Double,
                DataType = "double",
                DataValue = "0",
            };
            //应该拿到GainVal的配置值替代上，然后绑定输入连接时也更新
            ScriptTempDic[ScriptLanguage.CSharp.ToString()] = $"[Out] = Math.Sine(SysTimeSec);";
            LoadPorts();
        }
    }
}
