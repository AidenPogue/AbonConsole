using UnityEngine;

namespace Terasievert.AbonConsole.BuiltInCommands
{
    /// <summary>
    /// Commands wrapping constructors of some common types
    /// </summary>
    public static class CommonConstructorCommands
    {
        [ConsoleMember("Creates a Vector3 with x, y, z")]
        public static Vector3 Vector3(float x, float y, float z) => new(x, y, z);

        [ConsoleMember("Creates a Vector2 with x, y")]
        public static Vector2 Vector2(float x, float y) => new(x, y);
    }
}
