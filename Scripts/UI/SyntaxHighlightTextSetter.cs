using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Terasievert.Sehtara
{
    /// <summary>
    /// Copies the text from the input field into a separate text componenent every time it changes. The separate text contains a preprocessor that does syntax highlighting.
    /// </summary>
    /// <remarks>Preprocessing the text controlled by the input field breaks everything.</remarks>
    public class SyntaxHighlightTextSetter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI syntaxHighlightText;

        private void Awake()
        {
            GetComponent<TMP_InputField>().onValueChanged.AddListener(s => syntaxHighlightText.text = s);
        }
    }
}
