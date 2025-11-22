using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terasievert.AbonConsole.UI
{
    /// <summary>
    /// Stores the 4 corners of a rect and provides utility methods for converting to and from the arrays provided by RectTransform.GetWorldCorners
    /// </summary>
    public class RectCorners
    {
        public RectCorners() 
        {
            BottomLeft = TopLeft = TopRight = BottomRight = Vector3.zero;
        }

        public RectCorners(IList<Vector3> array)
        {
            SetFromArray(array);
        }

        public Vector3 BottomLeft, TopLeft, TopRight, BottomRight;

        public void FillArray(IList<Vector3> array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (array.Count != 4)
            {
                throw new ArgumentException("Array must have exactly 4 elements.");
            }

            array[0] = BottomLeft;
            array[1] = TopLeft;
            array[2] = TopRight;
            array[3] = BottomRight;
        }

        public void SetFromArray(IList<Vector3> array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (array.Count != 4)
            {
                throw new ArgumentException("Array must have exactly 4 elements.");
            }

            BottomLeft = array[0];
            TopLeft = array[1];
            TopRight = array[2];
            BottomRight = array[3];
        }
    }
}
