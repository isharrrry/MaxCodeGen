﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class FunctionCall : IStatement
    {
        public string FunctionName { get; set; }
        public List<IExpression> Parameters { get; } = new List<IExpression>();

        public string Compile(CompilerContext context)
        {
            switch (context.ScriptLanguage)
            {
                case ScriptLanguage.Lua:
                    return $"{FunctionName}({String.Join(", ", Parameters.Select(p => p.Compile(context)))})\n";
                case ScriptLanguage.CSharp:
                case ScriptLanguage.C:
                default:
                    return $"{FunctionName}({String.Join(", ", Parameters.Select(p => p.Compile(context)))});\n";
            }
        }
    }
}
