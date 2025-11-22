using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Terasievert.AbonConsole.UI
{
    [RequireComponent(typeof(Toggle))]
    public class LogLevelToggle : MonoBehaviour
    {
        [SerializeField] private AbonLogType logType;

        public UnityEvent<AbonLogType> onEnabled, onDisabled;

        private Toggle toggle;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnToggle);
        }

        private void OnToggle(bool value)
        {
            if (!value)
            {
                onEnabled.Invoke(logType);
            }
            else
            {
                onDisabled.Invoke(logType);
            }
        }
    }
}
