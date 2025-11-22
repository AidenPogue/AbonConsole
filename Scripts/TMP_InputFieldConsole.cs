using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Terasievert.AbonConsole.UI
{
    /// <summary>
    /// TMP input field that ignores up and down arrows.
    /// </summary>
    public class TMP_InputFieldConsole : TMP_InputField
    {
        public void OnGUI()
        {
            var e = Event.current;
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.DownArrow))
            {
                e.Use();
            }
        }
    } 
}