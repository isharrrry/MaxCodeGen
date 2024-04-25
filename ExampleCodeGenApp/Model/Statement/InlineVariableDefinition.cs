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
    /// 定义包含ITypedExpression<T> Value
    /// 输出代码为变量名=其值
    /// 能当成赋值用？
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InlineVariableDefinition<T> : ITypedVariableDefinition<T>
    {
        public string VariableName { get; private set; }
        public ITypedExpression<T> Value { get; set; }

        public string Compile(CompilerContext context)
        {
            VariableName = context.FindFreeVariableName();
            context.AddVariableToCurrentScope(this);
            return $"{VariableName} = {Value.Compile(context)}";
        }
    }
}
