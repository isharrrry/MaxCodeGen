using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleCodeGenApp.Model.Compiler;

namespace ExampleCodeGenApp.Model
{
    public interface IVariableDefinition : IStatement
    {
        string VariableName { get; }
    }
    /// <summary>
    /// Base IStatement and Name
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITypedVariableDefinition<T> : IVariableDefinition
    {
    }
}
