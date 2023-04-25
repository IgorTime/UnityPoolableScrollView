using System;
using System.Collections.Generic;
using IgorTime.PoolableScrollView.Helpers;
using UnityEngine;

namespace IgorTime.PoolableScrollView
{
    [AddComponentMenu(MenuConstants.ADD_COMPONENT_MENU_PATH + nameof(DefaultItemViewProvider))]
    public class DefaultItemViewProvider : ItemViewProvider, ISerializationCallbackReceiver
    {
        [Serializable]
        public class TypeNameToView
        {
            [TypeDropdown(typeof(IItemData))]
            public string typeName;
            
            public ItemView item;
        }

        [SerializeField]
        private TypeNameToView[] typeToPrefabMap;

        private Dictionary<string, ItemView> internalMap;

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
            internalMap = new Dictionary<string, ItemView>();
            foreach (var typeNameToView in typeToPrefabMap)
            {
                if (!IsValid(typeNameToView))
                {
                    continue;
                }

                internalMap[typeNameToView.typeName] = typeNameToView.item;
            }
        }

        protected override ItemView GetPrefab(IItemData dataItem)
        {
            var typeName = dataItem.GetType().Name;
            return internalMap[typeName];
        }
    }
}