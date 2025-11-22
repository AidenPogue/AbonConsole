using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terasievert.AbonConsole.UI;
using UnityEngine;

namespace Terasievert.AbonConsole
{
    public static class UITools
    {
        private static Vector3[] corners = new Vector3[4];

        public static void FillRectWorldCorners(this RectTransform rectTransform, RectCorners corners)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (corners is null)
            {
                throw new ArgumentNullException(nameof(corners));
            }

            rectTransform.GetWorldCorners(UITools.corners);
            corners.SetFromArray(UITools.corners);
        }

        public static void FillRectLocalCorners(this RectTransform rectTransform, RectCorners corners)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            if (corners is null)
            {
                throw new ArgumentNullException(nameof(corners));
            }

            rectTransform.GetLocalCorners(UITools.corners);
            corners.SetFromArray(UITools.corners);
        }
    }
}
