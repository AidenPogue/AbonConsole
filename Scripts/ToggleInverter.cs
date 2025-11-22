using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Terasievert.AbonConsole.UI
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleInverter : MonoBehaviour
    {
        public UnityEvent<bool> onValueChanged;
        private void Awake()
        {
            GetComponent<Toggle>().onValueChanged.AddListener(b => onValueChanged.Invoke(!b));
        }
    }
}
