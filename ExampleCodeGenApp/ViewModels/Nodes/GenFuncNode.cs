using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class GenFuncNode : BaseCodeGenNodeViewModel
    {
        static GenFuncNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<GenFuncNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel ValueEditor { get; } = new DoubleValueEditorViewModel();


        public double IValue { get => ValueEditor.Value; set => ValueEditor.Value = value; }

        [DisplayName("算子名称")]
        public string FuncName { get; set; } = "GenFuncNode";

        public GenFuncNode() : base()
        {
        }

        public override void LoadPorts()
        {
            this.Name = FuncName;
            InConfigDic["In"] = new NodeInConfig()
            {
                IsExpression = true,
                PortType = PortType.Double,
                DataValue = "0",
            };
            OutConfigDic["Out"] = new NodeOutConfig()
            {
                IsExpression = true,
                PortType = PortType.Double,
                DataValue = "0",
            };
            ParamDic["GainVal"] = new ParamDefine()
            {
                PortType = PortType.Double,
                Name = "倍数",
                DataValue = "1",
                Description = "放大倍数",
            };
            //应该拿到GainVal的配置值替代上，然后绑定输入连接时也更新
            ScriptTempDic[ScriptLanguage.CSharp.ToString()] = $"[Out] = ([In] * [GainVal]);";
            base.LoadPorts();
        }
    }
}
