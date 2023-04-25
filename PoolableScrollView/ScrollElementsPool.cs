using UnityEngine;
using UnityEngine.Pool;

namespace IgorTime.PoolableScrollView
{
    public class ScrollElementsPool
    {
        private readonly Transform parent;
        private readonly IObjectPool<ItemView> internalPool;
        private readonly ItemView prefab;

        public ScrollElementsPool(ItemView prefab, Transform parent)
        {
            this.parent = parent;
            internalPool = new ObjectPool<ItemView>(
                CreateElement,
                GetElement,
                ReleaseElement,
                collectionCheck: false);

            this.prefab = prefab;
        }

        private static void ReleaseElement(ItemView itemView)
        {
            itemView.SetVisibility(false);
        }

        private static void GetElement(ItemView itemView)
        {
            itemView.SetVisibility(true);
        }

        public ItemView Get() => internalPool.Get();

        public ItemView Peek() => prefab;

        public void Release(ItemView item)
        {
            internalPool.Release(item);
        }

        private ItemView CreateElement() => Object.Instantiate(prefab, parent);
    }
}