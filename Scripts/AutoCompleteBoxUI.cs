using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using Terasievert.AbonConsole.AntlrGenerated;
using Terasievert.Pooling;
using TMPro;
using UnityEngine;

namespace Terasievert.AbonConsole.UI
{
    public class AutoCompleteBoxUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField inputField;

        [SerializeField, Min(0)]
        private int minimumChars = 3;

        [SerializeField]
        private RectTransform listParent, inputArea;

        [SerializeField]
        private AutoCompletionUIElement elementPrefab;

        private PooledUiList<AutoCompletionUIElement, AutoCompletion> uiList;
        private IList<AutoCompletion> currentCompletions;
        private int lastCaretPosition = -1;

        private Canvas rootCanvas, canvas;
        private RectTransform rectTransform;
        private RectCorners ourCorners, inputCorners;

        private bool isAboveInput;

        private int currentSelectedIndex
        {
            get => _currentSelectedIndex;
            set
            {
                var len = currentCompletions?.Count ?? 0;

                if (value < 0)
                {
                    value = len + value;
                }
                else if (len > 0)
                {
                    value %= len;
                }

                value = Mathf.Clamp(value, 0, len);

                if (currentSelectedElement)
                {
                    currentSelectedElement.SetSelected(false);
                }

                if (len != 0)
                {
                    currentSelectedElement = uiList.SpawnedElements[value];
                    currentSelectedElement.SetSelected(true);
                }

                _currentSelectedIndex = value;
            }
        }

        private int _currentSelectedIndex;
        private AutoCompletionUIElement currentSelectedElement;

        private void Awake()
        {
            AbonConsoleUI.OnInputTokenStreamUpdated += OnInputChanged;
            inputField.onSelect.AddListener(OnFocus);
            inputField.onDeselect.AddListener(OnUnfocus);
            uiList = new AutoCompleteUIList(inputField, elementPrefab, listParent);
            
            if (!AbonPooler.HasPool(elementPrefab))
            {
                AbonPooler.CreatePool(elementPrefab, 15, new AbonPooler.PoolSettings(true));
            }
            
            canvas = GetComponent<Canvas>();
            rootCanvas = canvas.rootCanvas;
            rectTransform = transform as RectTransform;
            ourCorners = new RectCorners();
            inputCorners = new RectCorners();

            isAboveInput = true;
            SetBelowInput();
        }

        private void OnInputChanged(BufferedTokenStream tokenStream)
        {
            RefreshAutoComplete(GetTokenContainingIndex(inputField.stringPosition));
        }

        private void OnFocus(string _)
        {
            canvas.enabled = true;
        }

        private void OnUnfocus(string _)
        {
            canvas.enabled = false;
        }

        private void RefreshAutoComplete(IToken caretToken)
        {
            if (caretToken is null)
            {
                currentCompletions = null;
                uiList.Clear();
                return;
            }

            currentCompletions = AutoCompleteProvider.GetAutoCompletions(caretToken.Text, 10);

            uiList.ReplaceAllElements(currentCompletions);

            var charInfo = inputField.textComponent.GetTextInfo(inputField.text).characterInfo;

            var charWorldX = inputField.textComponent.transform.TransformPoint(charInfo[caretToken.StartIndex].topLeft).x;

            transform.position = new Vector2(charWorldX, transform.position.y);

            currentSelectedIndex = 0;
        }

        private IToken GetTokenContainingIndex(int index)
        {
            var stream = AbonConsoleUI.CurrentInputTokenStream;
            
            if (stream is null)
            {
                return null;
            }

            stream.Seek(0);

            //Size - 1 to skip EOF token
            for (int i = 0; i < stream.Size - 1; i++)
            {
                var token = stream.Get(i);
                if (token.StartIndex <= index && token.StopIndex + 1 >= index)
                {
                    return token;
                }
            }

            return null;
        }

        private void ApplyAutoCompletion(IToken token, AutoCompletion autoCompletion)
        {
            var oldInput = inputField.text;
            var newInput = oldInput.Substring(0, token.StartIndex) + autoCompletion.Token + oldInput.Substring(token.StopIndex + 1);
            inputField.text = newInput;
            inputField.stringPosition = token.StartIndex + autoCompletion.Token.Length;
        }

        private void Update()
        {
            if (inputField.stringPosition != lastCaretPosition)
            {
                var curToken = GetTokenContainingIndex(inputField.stringPosition);
                var oldToken = GetTokenContainingIndex(lastCaretPosition);

                if (curToken != oldToken)
                {
                    RefreshAutoComplete(curToken);
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentSelectedIndex++;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentSelectedIndex--;
            }
            if (Input.GetKeyDown(KeyCode.Tab) && (currentCompletions?.Count ?? 0) > 0)
            {
                ApplyAutoCompletion(GetTokenContainingIndex(inputField.stringPosition), currentCompletions[currentSelectedIndex]);
            }

            lastCaretPosition = inputField.stringPosition;

            var belowInputOnScreen = CheckWouldBeOnScreen(false);

            if (isAboveInput)
            {
                if (belowInputOnScreen)
                {
                    SetBelowInput();
                }
            }
            else
            {
                if (!belowInputOnScreen)
                {
                    SetAboveInput();
                }
            }
        }

        private void SetBelowInput()
        {
            if (!isAboveInput)
            {
                return;
            }

            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchorMax = rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            isAboveInput = false;
        }

        private void SetAboveInput()
        {
            if (isAboveInput)
            {
                return;
            }

            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchorMax = rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchoredPosition = Vector2.zero;
            isAboveInput = true;
        }

        private bool CheckWouldBeOnScreen(bool aboveInput)
        {
            rectTransform.FillRectWorldCorners(ourCorners);
            inputArea.FillRectWorldCorners(inputCorners);

            var height = ourCorners.TopLeft.y - ourCorners.BottomLeft.y;

            var inputY = (aboveInput ? inputCorners.TopLeft.y : inputCorners.BottomLeft.y);

            var yMax = inputY + (aboveInput ? height : 0);
            var yMin = inputY - (aboveInput ? 0 : height);

            return yMin >= 0 && yMax <= rootCanvas.pixelRect.height;
        }
    }
}
