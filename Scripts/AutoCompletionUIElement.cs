using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terasievert.AbonConsole.UI
{
    public class AutoCompletionUIElement : SerializedMonoBehaviour, IUiObjectDisplay<AutoCompletion>
    {

        [SerializeField]
        private TextMeshProUGUI text;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Dictionary<AutoCompleteType, Sprite> typeSprites;

        [SerializeField]
        private Color normalColor, selectedColor;

        [SerializeField]
        private Graphic background;

        private void OnEnable()
        {
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            background.color = selected ? selectedColor : normalColor;
        }

        public void SetFontSize(float fontSize) => text.fontSize = fontSize;

        public void UpdateDisplayObject(AutoCompletion displayObject)
        {
            text.text = displayObject.Token;

            if (typeSprites.TryGetValue(displayObject.Type, out Sprite sprite))
            {
                icon.sprite = sprite;
            }
            else
            {
                Debug.LogError("No icon set for auto completion type " +  displayObject.Type);
            }
        }
    }
}
