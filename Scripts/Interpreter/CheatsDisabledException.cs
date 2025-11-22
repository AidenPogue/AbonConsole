using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Terasievert.AbonConsole.Interpreter
{
    public class CheatsDisabledException : Exception
    {
        public CheatsDisabledException(ConsoleMember member) : base($"The cheat console member {member.Name} cannot be accessed because cheats are disabled.")
        { }
    }
}
