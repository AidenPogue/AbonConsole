using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Terasievert.AbonConsole.UI
{
    [RequireComponent(typeof(Image))]
    public class LogLevelIcon : SerializedMonoBehaviour
    {
        [SerializeField]
        private Dictionary<AbonLogType, Sprite> logLevelSprites;

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void Reset()
        {
            logLevelSprites = new Dictionary<AbonLogType, Sprite>();
            foreach (var level in Enum.GetValues(typeof(AbonLogType)))
            {
                logLevelSprites.Add((AbonLogType)level, null);
            }
        }

        public void SetIcon(ConsoleLogItem item)
        {
            if (logLevelSprites.TryGetValue(item.LogLevel, out var sprite))
            {
                image.sprite = sprite;
            }
            else
            {
                image.sprite = null;
                Debug.LogError("No sprite was assigned for log type " + item.LogLevel);
            }
        }
    }
}
