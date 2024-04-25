using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public class Realtime : IStatement
    {
        public IStatement LoopStep { get; set; }
        public IStatement Setup { get; set; }

        public ITypedExpression<double> StepSize { get; set; }

        public InlineVariableDefinition<int> CurrentTick { get; } = new InlineVariableDefinition<int>();

        public string Compile(CompilerContext context)
        {
            context.EnterNewScope("Realtime Task");
            
            CurrentTick.Value = new IntLiteral { Value = 0 };
            string code = "";
            switch (context.ScriptLanguage)
            {
                case ScriptLanguage.CSharp:
                    code += Setup.Compile(context) + "\n"; 
                    code += $"var {CurrentTick.Compile(context)};\n";
                    code += $"while (true)" + "{\n" +
                           LoopStep.Compile(context) + "\n";
                    code += $"{CurrentTick.VariableName} ++;\n";
                    code += $"if({CurrentTick.VariableName} > 10) break;\n";//debug
                    code += $"System.Threading.Thread.Sleep((int)({StepSize.Compile(context)} * 1000));\n";
                    code += "}\n";
                    break;
                case ScriptLanguage.C:
                    code += Setup.Compile(context) + "\n";
                    code += $"int {CurrentTick.Compile(context)};\n";
                    code += $"while (true)" + "{\n" +
                           LoopStep.Compile(context) + "\n";
                    code += $"{CurrentTick.VariableName} ++;\n";
                    code += $"//Sleep((int)({StepSize.Compile(context)} * 1000));\n";
                    code += "}\n";
                    break;
                case ScriptLanguage.Lua:
                default:
                    throw new NotImplementedException();
            }

            context.LeaveScope();
            return code;
        }
    }
}
