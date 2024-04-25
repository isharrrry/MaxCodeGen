using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleCodeGenApp.Model.Compiler
{
    public interface IProgram
    {
        string Compile(CompilerContext context);
    }
    public interface IExpression : IProgram
    {
    }
    /// <summary>
    /// Base IExpression
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITypedExpression<T> : IExpression
    {
    }
}
