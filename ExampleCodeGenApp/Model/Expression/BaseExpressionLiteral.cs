using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class BaseExpressionLiteral : IExpression, IStatement
    {
        public string Value { get; set; }

        public delegate string CompileHandle(CompilerContext ctx);

        public CompileHandle CompileEvent;

        public virtual string Compile(CompilerContext ctx)
        {
            return CompileEvent?.Invoke(ctx) ?? Value;
        }
    }
}
