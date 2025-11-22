using Antlr4.Runtime;
using Sirenix.Reflection.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Terasievert.AbonConsole.AntlrGenerated;
using Terasievert.AbonConsole.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Terasievert.AbonConsole.Editor
{
    //WARNING: This will completely break if we ever have more than 31 token types in the lexer.
    [CustomPropertyDrawer(typeof(TokenColor))]
    public class TokenColorDrawer : PropertyDrawer
    {
        private SerializedProperty tokenIds, color;
        private static readonly string[] tokenNames;

        static TokenColorDrawer()
        {
            tokenNames = new string[((Vocabulary)ConsoleLexer.DefaultVocabulary).getMaxTokenType() + 1];
            for (int i = 0; i < tokenNames.Length; i++)
            {
                tokenNames[i] = ConsoleLexer.DefaultVocabulary.GetSymbolicName(i);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            tokenIds = property.FindPropertyRelative(nameof(TokenColor.TokenIds));
            color = property.FindPropertyRelative(nameof(TokenColor.Color));

            var realLabel = EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, realLabel);
            var propRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            var currentMask = CreateMask(tokenIds);
            var newMask = EditorGUI.MaskField(propRect, new GUIContent("Token Types"), currentMask, tokenNames);

            if (currentMask != newMask)
            {    
                FillSetFromMask(tokenIds, newMask);
            }

            propRect.y += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(propRect, color);

            EditorGUI.EndProperty();
        }

        private int CreateMask(SerializedProperty idList)
        {
            var mask = 0;
            for(int i = 0; i < idList.arraySize; i++)
            {
                mask |= 1 << idList.GetArrayElementAtIndex(i).intValue;
            }

            return mask;
        }

        private void FillSetFromMask(SerializedProperty idList, int mask)
        {
            idList.ClearArray();
            for (int i = 0; i < sizeof(int) * 8; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    idList.InsertArrayElementAtIndex(idList.arraySize);
                    idList.GetArrayElementAtIndex(idList.arraySize - 1).intValue = i;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 2 * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
