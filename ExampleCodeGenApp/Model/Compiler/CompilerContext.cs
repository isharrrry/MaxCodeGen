using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.ViewModels.Nodes;

namespace ExampleCodeGenApp.Model.Compiler
{
    public class ScopeDefinition
    {
        public string Identifier { get; }

        public List<IVariableDefinition> Variables { get; } = new List<IVariableDefinition>();

        public ScopeDefinition(string identifier)
        {
            Identifier = identifier;
        }
    }
    public enum ScriptLanguage { CSharp, C, Lua }
    public enum ScriptMode {  数据流, 过程 }
    public class CompilerContext
    {
        public ScriptLanguage ScriptLanguage { get; set; }
        public bool UseGlobalVar { get; set; } = true;
        public List<string> GlobalVar = new List<string>();
        public List<string> GlobalVarValue = new List<string>();
        public ScopeDefinition GlobalScopeDefinition { get; set; } = new ScopeDefinition("Root");

        public Stack<ScopeDefinition> VariablesScopesStack { get; } = new Stack<ScopeDefinition>();
        public ScriptMode ScriptMode { get; set; } = ScriptMode.数据流;

        public Dictionary<string, LocalVariableDefinition> GlobalVariables = new Dictionary<string, LocalVariableDefinition> { };


        /// <summary>
        /// 分配变量名/生成变量名
        /// </summary>
        /// <returns></returns>
        public string FindFreeVariableName()
        {
            if (UseGlobalVar)
                return "v" + GlobalScopeDefinition.Variables.Count();
            return "v" + VariablesScopesStack.SelectMany(s => s.Variables).Count();
        }

        public void AddVariableToCurrentScope(IVariableDefinition variable)
        {
            if (UseGlobalVar)
                GlobalScopeDefinition.Variables.Add(variable);
            else
                VariablesScopesStack.Peek().Variables.Add(variable);
        }

        public void EnterNewScope(string scopeIdentifier)
        {
            VariablesScopesStack.Push(new ScopeDefinition(scopeIdentifier));
        }

        public void LeaveScope()
        {
            VariablesScopesStack.Pop();
        }

        public bool IsInScope(IVariableDefinition variable)
        {
            if (variable == null)
            {
                return false;
            }
            if (UseGlobalVar)
                return GlobalScopeDefinition.Variables.Contains(variable);
            else
                return VariablesScopesStack.Any(s => s.Variables.Contains(variable));
        }
    }
}
