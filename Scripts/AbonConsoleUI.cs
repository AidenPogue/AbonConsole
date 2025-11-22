using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using Terasievert.AbonConsole.AntlrGenerated;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.Rendering.DebugUI;

namespace Terasievert.AbonConsole.UI
{
    public class AbonConsoleUI : MonoBehaviour
    {
        [ConsoleMember("For testing", cheat: true)]
        public static string TestCheat()
        {
            return "Those who know";
        }

        /// <summary>
        /// The lexer token stream for the current input.
        /// </summary>
        public static BufferedTokenStream CurrentInputTokenStream { get; private set; }

        public static event Action<BufferedTokenStream> OnInputTokenStreamUpdated;
        public static event Action<string> OnInputUpdated;
        /// <summary>
        /// Fired when the input is changed by the user, making it dirty.
        /// </summary>
        public static event Action<string> OnInputDirtied;
        /// <summary>
        /// Fired when the console is opened or closed. Bool is true if opened.
        /// </summary>
        public static event Action<bool> OnOpenStateChanged;

        public static bool InputDirty { get; private set; }

        [SerializeField] private GameObject container;
        [SerializeField] private TMP_InputFieldConsole input;
        [SerializeField]
        private SyntaxHighlighter syntaxHighlighter;

        private List<string> inputHistory;
        private int _currentHistoryIndex = -1;
        private bool historyUpdated;

        private int currentHistoryIndex
        {
            get => _currentHistoryIndex;
            set
            {
                if (inputHistory.Count < 1)
                {
                    _currentHistoryIndex = -1;
                    return;
                }

                if (value < 0)
                {
                    value += inputHistory.Count;
                }

                value %= inputHistory.Count;

                _currentHistoryIndex = value;
            }
        }

        private GameObject selectOnClose;

        private void Awake()
        {
            input.onSubmit.AddListener(OnInputSubmit);
            input.onValueChanged.AddListener(OnInputChanged);
            inputHistory = new List<string>(50);
        }

        private void Start()
        {
            container.SetActive(false);
        }

        private void Update()
        {
            var currentSelected = EventSystem.current.currentSelectedGameObject;

            bool buttonDown = GameManager.RewiredPlayer.GetButtonDown(RewiredConsts.Action.UI.ToggleConsole), buttonUp = GameManager.RewiredPlayer.GetButtonUp(RewiredConsts.Action.UI.ToggleConsole);

            if (buttonDown)
            {
                input.DeactivateInputField();

                container.SetActive(!container.activeInHierarchy);

                if (container.activeInHierarchy)
                {
                    selectOnClose = currentSelected;
                    input.text = input.text.Trim('`');
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(selectOnClose);
                }

                OnOpenStateChanged?.Invoke(container.activeInHierarchy);

            }

            if (buttonUp && container.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(input.gameObject);
            }

            if (container.activeInHierarchy && !InputDirty && inputHistory.Count > 0)
            {
                bool update = false;

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (!historyUpdated)
                    {
                        currentHistoryIndex--;
                    }
                    update = true;
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    currentHistoryIndex++;
                    update = true;
                }

                if (update)
                {
                    input.SetTextWithoutNotify(inputHistory[currentHistoryIndex]);
                    input.MoveTextEnd(false);
                    historyUpdated = false;
                    CreateTokenStream();
                }     
            }
        }

        public void OnInputSubmit(string text)
        {
            CommandExecutor.ExecuteCommand(text);
            input.text = "";
            input.ActivateInputField();

            if (inputHistory.Count == 0 || inputHistory[^1] != text)
            {
                inputHistory.Add(text);
            }

            currentHistoryIndex = -1;
            historyUpdated = true;
        }

        private void OnInputChanged(string text)
        {
            CreateTokenStream();

            InputDirty = text != "";
        }

        private void CreateTokenStream()
        {
            var charStream = CharStreams.fromString(input.text);
            var lex = new ConsoleLexer(charStream);
            CurrentInputTokenStream = new CommonTokenStream(lex);

            CurrentInputTokenStream.Fill();

            syntaxHighlighter.UpdateText(input.text);

            OnInputTokenStreamUpdated?.Invoke(CurrentInputTokenStream);
        }

        //If we get a keydown event and the input field isn't focused, we want to immediately focus it and send the event there.
        public void OnGUI()
        {
            var e = Event.current;
            //We check for control only or control+c to allow copying from info panel, all other inputs go to the input box.
            if (container.activeInHierarchy && e.type == EventType.KeyDown && !input.isFocused && e.keyCode is not KeyCode.LeftControl or KeyCode.RightControl && !(e.control && e.keyCode == KeyCode.C))
            {
                input.ActivateInputField();
                input.ProcessEvent(e);
            }
        }
    }
}