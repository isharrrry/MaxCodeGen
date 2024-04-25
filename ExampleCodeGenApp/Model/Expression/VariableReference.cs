using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;
using ExampleCodeGenApp.Model.Compiler.Error;

namespace ExampleCodeGenApp.Model
{
    /// <summary>
    /// 这基于表达式，ITypedVariableDefinition<T>基于声明且含有名称
    /// 引用直接输出变量名（必须是在该作用域的变量）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VariableReference<T> : ITypedExpression<T>
    {
        public ITypedVariableDefinition<T> LocalVariable { get; set; }

        public string Compile(CompilerContext context)
        {
            if (!context.IsInScope(LocalVariable))
            {
                throw new VariableOutOfScopeException(LocalVariable.VariableName);
            }
            return LocalVariable.VariableName;
        }
    }
}
