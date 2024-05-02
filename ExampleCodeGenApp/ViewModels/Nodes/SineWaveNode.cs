﻿using System;
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
            ParamDic["Amp"] = new ParamDefine()
            {
                PortType = PortType.Double,
                Name = "幅值",
                DataValue = "1",
                Description = "幅值",
            };
            ParamDic["Bias"] = new ParamDefine()
            {
                PortType = PortType.Double,
                Name = "偏移",
                DataValue = "0",
                Description = "偏移",
            };
            ParamDic["Frequency"] = new ParamDefine()
            {
                PortType = PortType.Double,
                Name = "频率",
                DataValue = "1",
                Description = "频率（rad/sec）",
            };
            ParamDic["Phase"] = new ParamDefine()
            {
                PortType = PortType.Double,
                Name = "相位",
                DataValue = "0",
                Description = "相位（rad）",
            };
            //应该拿到GainVal的配置值替代上，然后绑定输入连接时也更新
            ScriptTempDic[ScriptLanguage.CSharp.ToString()] = $"[Out] = [Amp] * Math.Sine([Frequency] * SysTimeSec + [Phase]) + [Bias];";
            LoadPorts();
        }
    }
}
