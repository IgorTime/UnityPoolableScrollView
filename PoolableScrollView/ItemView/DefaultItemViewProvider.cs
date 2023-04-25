using System;
using System.Collections.Generic;
using IgorTime.PoolableScrollView.DataItems;
using IgorTime.PoolableScrollView.Helpers;
using UnityEngine;

namespace IgorTime.PoolableScrollView.ItemView
{
    [AddComponentMenu(MenuConstants.ADD_COMPONENT_MENU_PATH + nameof(DefaultItemViewProvider))]
    public class DefaultItemViewProvider : ItemViewProvider, ISerializationCallbackReceiver
    {
        [Serializable]
        public class TypeNameToView
        {
            [TypeDropdown(typeof(IItemData))]
            public string typeName;

            public ItemView prefab;
        }

        [SerializeField]
        private TypeNameToView[] typeToPrefabMap;

        private Dictionary<string, ItemView> internalMap;

        private static bool ValidateItemView(TypeNameToView typeNameToView) =>
            BasicValidationIfItemView(typeNameToView) &&
            DataTypeAndViewTypeValidationOfItemView(typeNameToView);

        private static bool BasicValidationIfItemView(TypeNameToView typeNameToView)
        {
            if (typeNameToView == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(typeNameToView.typeName))
            {
                return false;
            }

            return typeNameToView.prefab != null;
        }

        private static bool DataTypeAndViewTypeValidationOfItemView(TypeNameToView typeNameToView)
        {
            if (typeNameToView.prefab is not IItemViewDataTypeConstrain constrain)
            {
                Debug.LogError("Support only ItemViewTyped items");
                typeNameToView.prefab = null;
                return false;
            }

            if (typeNameToView.typeName == constrain.DataType.Name)
            {
                return true;
            }

            Debug.LogError($"Item {typeNameToView.prefab.name} has wrong type {typeNameToView.typeName}");
            typeNameToView.prefab = null;
            return false;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            internalMap = new Dictionary<string, ItemView>();
            foreach (var typeNameToView in typeToPrefabMap)
            {
                if (!BasicValidationIfItemView(typeNameToView))
                {
                    continue;
                }

                internalMap[typeNameToView.typeName] = typeNameToView.prefab;
            }
        }

        protected override ItemView GetPrefab(IItemData dataItem)
        {
            var typeName = dataItem.GetType().Name;
            return internalMap[typeName];
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            ValidateItemViews();
        }

        private void ValidateItemViews()
        {
            foreach (var typeNameToView in typeToPrefabMap)
            {
                ValidateItemView(typeNameToView);
            }
        }
    }
}