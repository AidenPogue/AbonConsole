using TMPro;
using UnityEngine;

namespace Terasievert.AbonConsole.UI
{
    public class UIClipboardHelper : MonoBehaviour
    {
        public void CopyToClipboard(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        public void CopyToClipboard(TMP_InputField inputField) => CopyToClipboard(inputField.text);
    }
}
