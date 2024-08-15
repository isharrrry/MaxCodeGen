using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.ViewModels.Nodes;

namespace ExampleCodeGenApp.Model
{
    /// <summary>
    /// Base IStatement and Name
    /// 生命带类型的变量，怎么解决不同语言类型不同问题？
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LocalVariableDefinition: IVariableDefinition//<T> : ITypedVariableDefinition<T>
    {
        public BaseExpressionLiteral VariableNameExpression { get; set; } = new BaseExpressionLiteral();
        public string DataType { get; set; } = "";
        public string VariableName { get => VariableNameExpression.Value; set => VariableNameExpression.Value=value; }
        public string Value { get; set; }
        public NodePortConfig PortConfig { get; set; }

        public delegate string CompileHandle(CompilerContext ctx);
        public CompileHandle CompileEvent;

        public string Compile(CompilerContext context)
        {
            if(VariableName == null)
                VariableName = context.FindFreeVariableName();
            context.AddVariableToCurrentScope(this);
            var DType = "";
            switch (context.ScriptLanguage)
            {
                case ScriptLanguage.C:
                    {
                        if (string.IsNullOrWhiteSpace(DataType) && PortConfig != null)
                        {
                            switch (PortConfig.PortType)
                            {
                                case ViewModels.PortType.DataType:
                                    break;
                                case ViewModels.PortType.Execution:
                                    break;
                                case ViewModels.PortType.String:
                                    DType = "char*";
                                    break;
                                case ViewModels.PortType.Double:
                                    DType = "double";
                                    break;
                                case ViewModels.PortType.Float:
                                    DType = "float";
                                    break;
                                case ViewModels.PortType.I8:
                                    DType = "char";
                                    break;
                                case ViewModels.PortType.U8:
                                    DType = "unsigned char";
                                    break;
                                case ViewModels.PortType.I16:
                                    DType = "short";
                                    break;
                                case ViewModels.PortType.U16:
                                    DType = "unsigned short";
                                    break;
                                case ViewModels.PortType.I32:
                                    DType = "int";
                                    break;
                                case ViewModels.PortType.U32:
                                    DType = "unsigned int";
                                    break;
                                case ViewModels.PortType.I64:
                                    DType = "long long";
                                    break;
                                case ViewModels.PortType.U64:
                                    DType = "unsigned long long";
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                            DType = DataType;
                        var res = $"{DType} {VariableName}";
                        if (string.IsNullOrWhiteSpace(DataType) && PortConfig != null)
                        {
                            if (PortConfig.IsArr() && PortConfig.Dim > 0)
                                DType += $"[{PortConfig.Dim}]";
                            else if (PortConfig.IsArr())
                                DType += $"[]";
                        }
                        if (string.IsNullOrWhiteSpace(Value))
                        {
                            if (PortConfig != null && PortConfig.IsArr())
                            {
                                res += " = {}";
                            }
                            else
                            {
                                //非数组不需要初值
                            }
                        }
                        else
                        {
                            if (PortConfig != null && PortConfig.IsArr())
                            {
                                res += " = { ";
                                res += Value;
                                res += " }";
                            }
                            else
                            {
                                //非数组
                                res += " = ";
                                res += Value;
                                res += "";
                            }
                        }
                        res += $";\n";
                        return res;
                    }
                case ScriptLanguage.CSharp:
                    {

                        if (string.IsNullOrWhiteSpace(DataType) && PortConfig != null)
                        {
                            switch (PortConfig.PortType)
                            {
                                case ViewModels.PortType.DataType:
                                    break;
                                case ViewModels.PortType.Execution:
                                    break;
                                case ViewModels.PortType.String:
                                    DType = "string";
                                    break;
                                case ViewModels.PortType.Double:
                                    DType = "double";
                                    break;
                                case ViewModels.PortType.Float:
                                    DType = "float";
                                    break;
                                case ViewModels.PortType.I8:
                                    DType = "char";
                                    break;
                                case ViewModels.PortType.U8:
                                    DType = "byte";
                                    break;
                                case ViewModels.PortType.I16:
                                    DType = "Int16";
                                    break;
                                case ViewModels.PortType.U16:
                                    DType = "UInt16";
                                    break;
                                case ViewModels.PortType.I32:
                                    DType = "Int32";
                                    break;
                                case ViewModels.PortType.U32:
                                    DType = "UInt32";
                                    break;
                                case ViewModels.PortType.I64:
                                    DType = "Int64";
                                    break;
                                case ViewModels.PortType.U64:
                                    DType = "UInt64";
                                    break;
                                default:
                                    break;
                            }
                            if (PortConfig.IsArr() && PortConfig.Dim > 0)
                                DType += $"[{PortConfig.Dim}]";
                            else if (PortConfig.IsArr())
                                DType += $"[]";
                        }
                        else
                            DType = DataType;
                        var res = $"{DType} {VariableName}";
                        if (string.IsNullOrWhiteSpace(Value))
                        {
                            if (PortConfig != null && PortConfig.IsArr())
                            {
                                res += " = new ";
                                res += DType;
                                res += "{}";
                            }
                            else
                            {
                                //非数组不需要初值
                            }
                        }
                        else
                        {
                            if (PortConfig != null && PortConfig.IsArr())
                            {
                                res += " = new ";
                                res += DType;
                                res += "{ ";
                                res += Value;
                                res += " }";
                            }
                            else
                            {
                                //非数组
                                res += " = ";
                                res += Value;
                                res += "";
                            }
                        }
                        res += $";\n";
                        return res;
                    }
                case ScriptLanguage.Lua:
                    return $"local {VariableName} = {Value}\n";
                default:
                    throw new NotImplementedException();
            }
            CompileEvent?.Invoke(context);//附加的代码
        }
    }
}
