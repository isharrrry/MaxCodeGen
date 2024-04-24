using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using NodeNetwork.ViewModels;

namespace ExampleCodeGenApp.ViewModels
{
    public class NetworkConfig
    {
        private List<NodeViewModel> nodes;
        private List<ConnectionConfig> connectionConfigs = new List<ConnectionConfig>();

        public List<NodeViewModel> Nodes { get => nodes; set => nodes=value; }
        public List<ConnectionConfig> ConnectionConfigs { get => connectionConfigs; set => connectionConfigs=value; }
        public NetworkConfig() { }

        public void SetConnectionConfigs(List<ConnectionViewModel> connectionViewModels)
        {
            foreach (var conn in connectionViewModels)
            {
                ConnectionConfigs.Add(new ConnectionConfig()
                {
                    OutputNode = conn.Output.Parent,
                    OutputIdx = ConnectionConfig.GetPortIdx(conn.Output),
                    InputNode = conn.Input.Parent,
                    InputIdx = ConnectionConfig.GetPortIdx(conn.Input)
                });
            }
        }

        public void GetConnectionConfigs(NetworkViewModel Network)
        {
            Network.Connections.Clear();
            foreach (var item in ConnectionConfigs)
            {
                Network.Connections.Add(Network.ConnectionFactory(item.GetInput(), item.GetOutput()));
            }
        }
    }

    public class ConnectionConfig
    {
        public NodeViewModel InputNode { get; set; }
        public NodeViewModel OutputNode { get; set; }
        //public NodeInputViewModel Input { get; set; }
        //public NodeOutputViewModel Output { get; set; }
        public int InputIdx { get; set; }
        public int OutputIdx { get; set; }

        public static int GetPortIdx(NodeInputViewModel input)
        {
            int i = 0;
            foreach (var item in input.Parent.Inputs.Items)
            {
                if (item == input)
                    return i;
                i++;
            }
            return -1;
        }
        public static int GetPortIdx(NodeOutputViewModel output)
        {
            int i = 0;
            foreach (var item in output.Parent.Outputs.Items)
            {
                if (item == output)
                    return i;
                i++;
            }
            return -1;
        }

        internal NodeInputViewModel GetInput()
        {
            int i = 0;
            foreach (var item in InputNode.Inputs.Items)
            {
                if (i == InputIdx)
                    return item;
                i++;
            }
            return null;
        }

        internal NodeOutputViewModel GetOutput()
        {
            int i = 0;
            foreach (var item in OutputNode.Outputs.Items)
            {
                if (i == OutputIdx)
                    return item;
                i++;
            }
            return null;
        }
    }
}
