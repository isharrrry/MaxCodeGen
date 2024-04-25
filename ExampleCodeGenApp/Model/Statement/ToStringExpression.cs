using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class ToStringExpression : ITypedExpression<string>//IExpression
    {
        public IExpression Expression { get; set; }

        public string Compile(CompilerContext context)
        {
            switch (context.ScriptLanguage)
            {
                case ScriptLanguage.CSharp:
                    return $"{Expression.Compile(context)}.ToString()";
                case ScriptLanguage.C:
                    return $"\"\" + {Expression.Compile(context)}";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
