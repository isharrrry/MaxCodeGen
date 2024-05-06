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
    public class UDPSendNode : BaseCodeGenNodeViewModel
    {
        static UDPSendNode()
        {
            Splat.Locator.CurrentMutable.Register(() => new CodeGenNodeView(), typeof(IViewFor<UDPSendNode>));
        }

        [YamlIgnore]
        public DoubleValueEditorViewModel ValueEditor { get; } = new DoubleValueEditorViewModel();


        public double IValue { get => ValueEditor.Value; set => ValueEditor.Value = value; }

        public UDPSendNode() : base()
        {
            this.Name = nameof(UDPSendNode);
            InConfigDic["In"] = new NodeInConfig()
            {
                IsExpression = true,
                PortType = PortType.DataPack,
                DataValue = "0",
            };
            ParamDic["IP"] = new ParamDefine()
            {
                PortType = PortType.String,
                Name = "IP",
                DataValue = "127.0.0.1",
                Description = "目标IP地址",
            };
            ParamDic["Port"] = new ParamDefine()
            {
                PortType = PortType.I32,
                Name = "端口",
                DataValue = "5555",
                Description = "目标端口",
            };
            ScriptAssemblyDic[ScriptLanguage.CSharp.ToString()] = new List<AssemblyConfig>
            {
                new AssemblyConfig()
                {
                    Include = "System.Net"
                },
                new AssemblyConfig()
                {
                    Include = "System.Net.Sockets"
                }
            };
            ScriptTempDic[ScriptLanguage.CSharp.ToString()] = @"using (UdpClient udpClient = new UdpClient()){
    IPAddress ipAddress = IPAddress.Parse([IP]);
    byte[] data = [In];
    udpClient.Send(data, data.Length, new IPEndPoint(ipAddress, [Port]));
}";
            LoadPorts();
        }
    }
}
