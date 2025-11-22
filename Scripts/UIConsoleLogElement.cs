using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Terasievert.AbonConsole.UI
{
    public class UIConsoleLogElement : SerializedMonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private GameObject timestampPopup;
        [SerializeField] private TextMeshProUGUI contentText, timestampText;
        [SerializeField] private LogLevelIcon logLevelIcon;
        [SerializeField]
        private Color normalBgColor, hoverBgColor;
        [SerializeField]
        private Dictionary<AbonLogType, Color> logColors;
        [SerializeField]
        private Graphic background;

        private ConsoleLogItem currentItem;
        private Color currentLogColor;

        private void Reset()
        {
            logColors = new Dictionary<AbonLogType, Color>();

            foreach (var level in Enum.GetValues(typeof(AbonLogType)))
            {
                logColors.Add((AbonLogType)level, Color.white);
            }
        }

        public void SetItem(ConsoleLogItem item)
        {
            currentItem = item;
            contentText.text = item.CachedToString.Replace('\n', ' ');
            timestampText.text = item.Time.ToLongTimeString();
            logLevelIcon.SetIcon(item);
            timestampPopup.SetActive(false);

            if (logColors.TryGetValue(item.LogLevel, out var color))
            {
                currentLogColor = color;
            }
            else
            {
                currentLogColor = Color.white;
                Debug.LogError("No background color was found for log level " + item.LogLevel);
            }

            SetBgTint(normalBgColor);
        }

        public void CopyContent()
        {
            GUIUtility.systemCopyBuffer = currentItem.CachedToString;
        }


        private void SetBgTint(Color color)
        {
            background.color = color * currentLogColor;
        }

        private void Awake()
        {
            if (!Application.isPlaying) { return; }
            SetTimestampPopupActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SetTimestampPopupActive(true);
            SetBgTint(hoverBgColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetTimestampPopupActive(false);
            SetBgTint(normalBgColor);
        }

        private void SetTimestampPopupActive(bool active)
        {
            timestampPopup.SetActive(active);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ConsoleInfoWindow.OpenLog(currentItem);
        }
    }
}