using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IgorTime.PoolableScrollView
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class ViewItemProvider : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect scrollRect;

        private readonly Dictionary<Type, ScrollElementsPool> elementPools = new();

        public ElementView Provide(IItemData dataItem)
        {
            var pool = GetElementPool(dataItem);
            return pool.Get();
        }

        public void Release(ElementView element)
        {
            var pool = GetElementPool(element.Data);
            pool.Release(element);
        }

        public ElementView Peek(IItemData data)
        {
            var pool = GetElementPool(data);
            return pool.Peek();
        }

        protected abstract ElementView GetPrefab(IItemData dataItem);

        private void OnValidate()
        {
            if (!scrollRect)
            {
                scrollRect = GetComponent<ScrollRect>();
            }
        }

        private ScrollElementsPool GetElementPool(IItemData itemData)
        {
            var dataType = itemData.GetType();
            if (!elementPools.TryGetValue(dataType, out var pool))
            {
                var prefab = GetPrefab(itemData);
                elementPools[dataType] = pool = new ScrollElementsPool(prefab, scrollRect.content);
            }

            return pool;
        }
    }
}