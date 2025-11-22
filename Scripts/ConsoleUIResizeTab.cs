using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Terasievert.Sehtara
{
    public class ConsoleUIResizeTab : MonoBehaviour, IDragHandler
    {
        [SerializeField]
        private float minSize = 50, minDistanceFromBottom = 50;
        [SerializeField]
        RectTransform window, container;
        [SerializeField] private LayoutElement windowLayoutElement;
        public void OnDrag(PointerEventData eventData)
        {
            var distance = eventData.position.y - transform.position.y;
            //var y = Mathf.Clamp(windowLayoutElement.preferredHeight - distance, minSize, container.rect.height - minDistanceFromBottom);
            var h = Mathf.Clamp(windowLayoutElement.preferredHeight, windowLayoutElement.minHeight, window.sizeDelta.y);
            windowLayoutElement.preferredHeight = h - distance;
        }
    }
}
