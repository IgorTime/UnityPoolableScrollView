using System;
using System.Collections.Generic;
using IgorTime.PoolableScrollView.Helpers;
using UnityEngine;

namespace IgorTime.PoolableScrollView
{
    [AddComponentMenu(MenuConstants.ADD_COMPONENT_MENU_PATH + nameof(DefaultViewItemProvider))]
    public class DefaultViewItemProvider : ViewItemProvider, ISerializationCallbackReceiver
    {
        [Serializable]
        public class TypeNameToView
        {
            [TypeDropdown(typeof(IElementData))]
            public string typeName;
            
            public ElementView item;
        }

        [SerializeField]
        private TypeNameToView[] typeToPrefabMap;

        private Dictionary<string, ElementView> internalMap;

        private static bool IsValid(TypeNameToView typeNameToView)
        {
            if (typeNameToView == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(typeNameToView.typeName))
            {
                return false;
            }

            return typeNameToView.item != null;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            internalMap = new Dictionary<string, ElementView>();
            foreach (var typeNameToView in typeToPrefabMap)
            {
                if (!IsValid(typeNameToView))
                {
                    continue;
                }

                internalMap[typeNameToView.typeName] = typeNameToView.item;
            }
        }

        protected override ElementView GetPrefab(IElementData dataItem)
        {
            var typeName = dataItem.GetType().Name;
            return internalMap[typeName];
        }
    }
}