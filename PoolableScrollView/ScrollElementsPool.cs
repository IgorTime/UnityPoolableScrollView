using UnityEngine;
using UnityEngine.Pool;

namespace IgorTime.PoolableScrollView
{
    public class ScrollElementsPool
    {
        private readonly Transform parent;
        private readonly IObjectPool<ElementView> internalPool;
        private readonly ElementView prefab;

        public ScrollElementsPool(ElementView prefab, Transform parent)
        {
            this.parent = parent;
            internalPool = new ObjectPool<ElementView>(
                CreateElement,
                GetElement,
                ReleaseElement,
                collectionCheck: false);

            this.prefab = prefab;
        }

        private static void ReleaseElement(ElementView elementView)
        {
            elementView.SetVisibility(false);
        }

        private static void GetElement(ElementView elementView)
        {
            elementView.SetVisibility(true);
        }

        public ElementView Get() => internalPool.Get();

        public ElementView Peek() => prefab;

        public void Release(ElementView element)
        {
            internalPool.Release(element);
        }

        private ElementView CreateElement() => Object.Instantiate(prefab, parent);
    }
}