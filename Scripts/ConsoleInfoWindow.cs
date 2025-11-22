using TMPro;
using UnityEngine;

namespace Terasievert.AbonConsole.UI
{
    public class ConsoleInfoWindow : SingletonMonoBehavior<ConsoleInfoWindow>
    {
        private void Awake()
        {
            SingletonInstance = this;
            canvas = GetComponent<Canvas>();
            canvas.enabled = false;
        }

        [SerializeField]
        private TMP_InputField contentBox, stackTraceBox;

        [SerializeField]
        private LogLevelIcon logLevelIcon;

        [SerializeField]
        private TextMeshProUGUI titleText;

        private Canvas canvas;

        public static void OpenLog(ConsoleLogItem logItem)
        {
            if (!SingletonInstance)
            {
                return;
            }

            SingletonInstance.contentBox.text = logItem.CachedToString;
            SingletonInstance.stackTraceBox.text = string.IsNullOrEmpty(logItem.Trace) ? "No stack trace available." : logItem.Trace;

            SingletonInstance.logLevelIcon.SetIcon(logItem);
            SingletonInstance.titleText.text = $"{logItem.LogLevel} at {logItem.Time.ToLongTimeString()} {logItem.Time.ToShortDateString()}";

            SingletonInstance.canvas.enabled = true;
        }
    }
}
