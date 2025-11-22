using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Terasievert.AbonConsole.UI
{
    public class AutoCompleteUIList : PooledUiList<AutoCompletionUIElement, AutoCompletion>
    {
        private readonly TMP_InputField inputField;

        public AutoCompleteUIList(TMP_InputField inputField, AutoCompletionUIElement elementPrefab, RectTransform elementParent) : base(elementPrefab, elementParent)
        {
            if (inputField == null)
            {
                throw new ArgumentNullException(nameof(inputField));
            }
            this.inputField = inputField;
        }

        protected override AutoCompletionUIElement SpawnNewEmptyElement()
        {
            var el = base.SpawnNewEmptyElement();
            el.SetFontSize(inputField.pointSize);
            return el;
        }
    }
}
