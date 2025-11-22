using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terasievert.AbonConsole.AntlrGenerated;
using TMPro;
using UnityEngine;

namespace Terasievert.AbonConsole.UI
{
    public class SyntaxHighlighter : MonoBehaviour
    {

        [SerializeField]
        private TokenColor[] tokenColors;

        private Dictionary<int, string> tokenColorsDict;

        private TextMeshProUGUI ourText;

        private StringBuilder stringBuilder;

        public void UpdateText(string text)
        {
            var stream = AbonConsoleUI.CurrentInputTokenStream;
            if (stream is null)
            {
                return;
            }

            stringBuilder.Clear();
            stringBuilder.Append(text);

            stream.Seek(0);

            for (int i = stream.Size - 2; i >= 0; i--)
            {
                var t = stream.Get(i);

                if (tokenColorsDict.TryGetValue(t.Type, out var color))
                {
                    stringBuilder.Insert(t.StopIndex + 1, "</color>");
                    stringBuilder.Insert(t.StartIndex, $"<color={color}>");

                }
            }

            ourText.text = stringBuilder.ToString();
        }

        private void Awake()
        {
            ourText = GetComponent<TextMeshProUGUI>();

            tokenColorsDict = new Dictionary<int, string>(tokenColors.Sum(tC => tC.TokenIds.Length));

            foreach (var tokenColor in tokenColors)
            {
                foreach (var id in tokenColor.TokenIds)
                {
                    if (!tokenColorsDict.TryAdd(id, "#" + ColorUtility.ToHtmlStringRGB(tokenColor.Color)))
                    {
                        Debug.LogWarning($"Token {ConsoleLexer.DefaultVocabulary.GetSymbolicName(id)} already has a color assigned ({tokenColorsDict[id]}). This one will be ignored");
                    }
                    
                }
            }

            stringBuilder = new StringBuilder(100);
        }
    }
}
