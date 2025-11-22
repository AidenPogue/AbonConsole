using System;

namespace Terasievert.AbonConsole
{
    public enum AbonLogType
    {
        //
        // Summary:
        //     LogType used for Errors.
        Error,
        //
        // Summary:
        //     LogType used for Asserts. (These could also indicate an error inside Unity itself.)
        Assert,
        //
        // Summary:
        //     LogType used for Warnings.
        Warning,
        //
        // Summary:
        //     LogType used for regular log messages.
        Log,
        //
        // Summary:
        //     LogType used for Exceptions.
        Exception,

        //Special

        UserEntry,
        CommandResult,
    }
}
