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
    public class BytePackNode : BaseCodeGenNodeViewModel
    {
        static BytePackNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<BytePackNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel ValueEditor { get; } = new DoubleValueEditorViewModel();


        public double IValue { get => ValueEditor.Value; set => ValueEditor.Value = value; }

        public BytePackNode() : base()
        {
            this.Name = nameof(BytePackNode);
            InConfigDic["In"] = new NodeInConfig()
            {
                IsExpression = true,
                PortType = PortType.Double,
                DataValue = "",
            };
            OutConfigDic["Out"] = new NodeOutConfig() {
                IsExpression = true,
                PortType = PortType.U8,
                Dim = -1,
            };
            ParamDic["DataType"] = new ParamDefine()
            {
                PortType = PortType.DataType,
                Name = "数据类型",
                DataValue = "Double",
                Description = "数据类型",
            };
            //应该拿到GainVal的配置值替代上，然后绑定输入连接时也更新
            ScriptTempDic[ScriptLanguage.CSharp.ToString()] = @"[Out] = BitConverter.GetBytes(([DataType])[In]);";
            ScriptTempDic[ScriptLanguage.C.ToString()] = @"[Out] = &[In];";
            LoadPorts();
        }
    }
}
