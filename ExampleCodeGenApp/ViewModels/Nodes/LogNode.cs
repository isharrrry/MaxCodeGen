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
    public class LogNode : BaseCodeGenNodeViewModel
    {
        static LogNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<LogNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel ValueEditor { get; } = new DoubleValueEditorViewModel();


        public double IValue { get => ValueEditor.Value; set => ValueEditor.Value = value; }

        public LogNode() : base()
        {
            this.Name = nameof(LogNode);
            InConfigDic["In"] = new NodeInConfig()
            {
                IsExpression = true,
                PortType = PortType.String,
                DataValue = "0",
            };
            ParamDic["Format"] = new ParamDefine()
            {
                PortType = PortType.String,
                Name = "格式",
                DataValue = "",
                Description = "打印日志输出格式",
                IgnoreWhenEmpty = true
            };
            ParamDic["Head"] = new ParamDefine()
            {
                PortType = PortType.String,
                Name = "描述头部",
                DataValue = "",
                Description = "描述头部文本",
            };
            //应该拿到GainVal的配置值替代上，然后绑定输入连接时也更新
            ScriptTempDic[ScriptLanguage.CSharp.ToString()] = $"Console.WriteLine([Head] + [In].ToString([Format]));";
            LoadPorts();
        }
    }
}
