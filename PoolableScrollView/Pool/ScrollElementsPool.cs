using UnityEngine;
using UnityEngine.Pool;

namespace IgorTime.PoolableScrollView.Pool
{
    public class ScrollElementsPool
    {
        private readonly Transform parent;
        private readonly IObjectPool<ItemView.ItemView> internalPool;
        private readonly ItemView.ItemView prefab;

        public ScrollElementsPool(ItemView.ItemView prefab, Transform parent)
        {
            this.parent = parent;
            internalPool = new ObjectPool<ItemView.ItemView>(
                CreateElement,
                GetElement,
                ReleaseElement,
                collectionCheck: false);

            this.prefab = prefab;
        }

        private static void ReleaseElement(ItemView.ItemView itemView)
        {
            itemView.SetVisibility(false);
        }

        private static void GetElement(ItemView.ItemView itemView)
        {
            itemView.SetVisibility(true);
        }

        public ItemView.ItemView Get() => internalPool.Get();

        public ItemView.ItemView Peek() => prefab;

        public void Release(ItemView.ItemView item)
        {
            internalPool.Release(item);
        }

        private ItemView.ItemView CreateElement() => Object.Instantiate(prefab, parent);
    }
}