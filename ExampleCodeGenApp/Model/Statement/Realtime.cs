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
            code += Setup.Compile(context) + "\n";
            code += $"while (true)" + "{\n" +
                   LoopStep.Compile(context) + "\n";
            code += $"System.Threading.Thread.Sleep((int)({StepSize.Compile(context)} * 1000));\n";
            code += "}\n";

            context.LeaveScope();
            return code;
        }
    }
}
