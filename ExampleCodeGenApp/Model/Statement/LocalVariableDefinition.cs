using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

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
        public delegate string CompileHandle(CompilerContext ctx);
        public CompileHandle CompileEvent;

        public string Compile(CompilerContext context)
        {
            if(VariableName == null)
                VariableName = context.FindFreeVariableName();
            context.AddVariableToCurrentScope(this); 
            switch (context.ScriptLanguage)
            {
                case ScriptLanguage.CSharp:
                case ScriptLanguage.C:
                    return $"{DataType} {VariableName} = {Value};\n";
                case ScriptLanguage.Lua:
                    return $"local {VariableName} = {Value}\n";
                default:
                    throw new NotImplementedException();
            }
            CompileEvent?.Invoke(context);//附加的代码
        }
    }
}
