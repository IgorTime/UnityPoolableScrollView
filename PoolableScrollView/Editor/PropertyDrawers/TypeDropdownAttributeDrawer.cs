using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace IgorTime.PoolableScrollView.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(TypeDropdownAttribute))]
    public class TypeDropdownAttributeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var targetType = ((TypeDropdownAttribute) attribute).Type;
            var dropdown = new PopupField<string>
            {
                choices = FindAllTypesImplementingInterface(targetType),
                value = property.stringValue,
            };

            dropdown.RegisterValueChangedCallback(evt =>
            {
                property.stringValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            return dropdown;
        }

        private static List<string> FindAllTypesImplementingInterface(Type interfaceType) =>
            TypeCache.GetTypesDerivedFrom(interfaceType)
                     .Select(x => x.Name)
                     .ToList();
    }
}