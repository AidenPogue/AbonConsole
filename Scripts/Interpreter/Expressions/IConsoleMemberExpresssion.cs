using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terasievert.AbonConsole.Interpreter.Expressions
{
    /// <summary>
    /// An expression that accesses a console member in any way.
    /// </summary>
    public interface IConsoleMemberExpresssion
    {
        public ConsoleMember ConsoleMember { get; }
    }
}
