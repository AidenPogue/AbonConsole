using System;
using System.Collections.Generic;
using System.Linq;
using Terasievert.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Terasievert.AbonConsole.UI
{
    public class AbonConsoleUILogList : SingletonMonoBehavior<AbonConsoleUILogList>
    {
        private const int logsInitialCapacityPerType = 75;

        private static Dictionary<AbonLogType, List<ConsoleLogItem>> groupedLogs;

        private static int currentSortIndex = 0;

        [SerializeField]
        private UIConsoleLogElement elementPrefab;

        [SerializeField]
        private ScrollRect scrollRect;

        private float currentHeight;
        public float CurrentContentHeight
        {
            get => currentHeight;
            set
            {
                currentHeight = value;
                scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, value);
            }
        }

        private bool refreshQueued;

        [ConsoleMember("Clears the console.")]
        public static void Clear()
        {
            foreach (var list in groupedLogs.Values)
            {
                list.Clear();
            }

            currentSortIndex = 0;

            SingletonInstance.RebuildVisibleLogs();
            SingletonInstance.QueueRefresh();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            groupedLogs = new(7);
            Application.logMessageReceived += LogMessageReceived;
        }

        private static void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            var item = new ConsoleLogItem(condition, type, stackTrace, DateTime.Now, currentSortIndex++);

            if (!groupedLogs.ContainsKey(item.LogLevel))
            {
                groupedLogs[item.LogLevel] = new List<ConsoleLogItem>(logsInitialCapacityPerType);
            }

            groupedLogs[item.LogLevel].Add(item);

            if (SingletonInstance)
            {
                SingletonInstance.AfterLogMessageRecieved(item);
            }
        }

        private HashSet<AbonLogType> visibleLogTypes;

        private List<ConsoleLogItem> visibleLogs;
        private Dictionary<int, UIConsoleLogElement> indexToSpawnedElement;

        private Vector3[] viewportWorldCorners;

        private void Awake()
        {
            SingletonInstance = this;
            viewportWorldCorners = new Vector3[4];
            visibleLogs = new List<ConsoleLogItem>(logsInitialCapacityPerType);
            visibleLogTypes = new HashSet<AbonLogType>(5);

            foreach (var val in Enum.GetValues(typeof(AbonLogType)))
            {
                visibleLogTypes.Add((AbonLogType)val);
            }

            indexToSpawnedElement = new Dictionary<int, UIConsoleLogElement>(50);

            if (!AbonPooler.HasPool(elementPrefab))
            {
                AbonPooler.CreatePool(elementPrefab, 64, new AbonPooler.PoolSettings(persistAfterCleanup: true));
            }

            scrollRect.onValueChanged.AddListener(OnScrollPositionChanged);

            RebuildVisibleLogs();
        }

        private void OnEnable()
        {
            QueueRefresh();
        }

        private void QueueRefresh()
        {
            refreshQueued = true;
        }

        public void SetLogTypeVisible(AbonLogType logType)
        {
            SetLogTypeVisibility(logType, true);
        }

        public void SetLogTypeHidden(AbonLogType logType) 
        {
            SetLogTypeVisibility(logType, false);
        }

        public void SetLogTypeVisibility(AbonLogType logType, bool visible)
        {
            var alreadyVisible = visibleLogTypes.Contains(logType);
            var updated = false;


            if (visible && !alreadyVisible)
            {
                visibleLogTypes.Add(logType);
                updated = true;
            }
            else if (!visible && alreadyVisible)
            {
                visibleLogTypes.Remove(logType);
                updated = true;
            }

            if (updated)
            {
                RebuildVisibleLogs();
            }
        }

        private void AfterLogMessageRecieved(ConsoleLogItem item)
        {
            if (visibleLogTypes.Contains(item.LogLevel))
            {
                visibleLogs.Add(item);
                CurrentContentHeight += LogHeight;

                if (IsScrolledToBottom)
                {
                    ScrollToBottom();
                }
            }

            QueueRefresh();
        }

        #region Scroll Handling

        private void OnScrollPositionChanged(Vector2 vec)
        {
            QueueRefresh();
        }

        private void RefreshSpawnedLogsNow()
        {
            scrollRect.viewport.GetWorldCorners(viewportWorldCorners);

            float startY = scrollRect.content.InverseTransformPoint(viewportWorldCorners[1]).y, endY = scrollRect.content.InverseTransformPoint(viewportWorldCorners[0]).y;

            int startIndex = Mathf.FloorToInt((scrollRect.content.sizeDelta.y - startY) / LogHeight), endIndex = Mathf.FloorToInt((scrollRect.content.sizeDelta.y - endY) / LogHeight);

            SetVisibleLogRange(startIndex, endIndex);
        }


        #endregion
        private void LateUpdate()
        {
            if (refreshQueued)
            {
                RefreshSpawnedLogsNow();
                refreshQueued = false;
            }
        }


        #region Helpers
        public bool IsScrolledToBottom => Mathf.Approximately(scrollRect.verticalNormalizedPosition, 0);

        private float LogHeight => (elementPrefab.transform as RectTransform).rect.height;

        public void ScrollToBottom()
        {
            scrollRect.verticalNormalizedPosition = 0;
        }

        private void SetVisibleLogRange(int startIndex, int endIndex)
        {
            startIndex = Mathf.Max(startIndex, 0);
            endIndex = Mathf.Min(endIndex, visibleLogs.Count - 1);

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (indexToSpawnedElement.ContainsKey(i))
                {
                    continue;
                }

                var e = GetUILogElement(i);
                var rT = e.transform as RectTransform;
                rT.anchoredPosition = new Vector2(0, -i * LogHeight);
            }

            foreach (var index in indexToSpawnedElement.Keys.ToArray())
            {
                if (index > endIndex || index < startIndex)
                {
                    RemoveSpawnedLog(index);
                }
            }
        }

        private void RebuildVisibleLogs()
        {
            visibleLogs.Clear();
            foreach (var type in groupedLogs.Keys)
            {
                if (visibleLogTypes.Contains(type))
                {
                    visibleLogs.AddRange(groupedLogs[type]);
                }
            }
            visibleLogs.Sort((a, b) => a.SortIndex.CompareTo(b.SortIndex));
            CurrentContentHeight = visibleLogs.Count * LogHeight;
            RemoveAllSpawnedLogs();
            QueueRefresh();
        }

        private UIConsoleLogElement GetUILogElement(int indexInVisibleLogs)
        {
            var log = visibleLogs[indexInVisibleLogs];
            var element = AbonPooler.GetPooledObject(elementPrefab, scrollRect.content);
            element.SetItem(log);
            (element.transform as RectTransform).sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, LogHeight);
            element.transform.localScale = Vector3.one;
            indexToSpawnedElement.Add(indexInVisibleLogs, element);
            return element;
        }

        /// <summary>
        /// Returns a spawned log to the pool and removes it from the list of spawned logs.
        /// </summary>
        private void RemoveSpawnedLog(int indexInVisibleLogs)
        {
            if (!indexToSpawnedElement.TryGetValue(indexInVisibleLogs, out var element))
            {
                return;
            }

            AbonPooler.ReturnPooledObject(element);
            indexToSpawnedElement.Remove(indexInVisibleLogs);
        }

        private void RemoveAllSpawnedLogs()
        {
            foreach (var key in indexToSpawnedElement.Keys.ToArray())
            {
                RemoveSpawnedLog(key);
            }
        }
        #endregion

    }
}
