using UnityEngine;
using UnityEngine.Pool;

public class ScrollElementsPool
{
    private readonly IObjectPool<ElementView> internalPool;
    private readonly ElementView prefab;
    private Transform poolRoot;

    public ScrollElementsPool(string prefabPath, Transform scrollRoot)
    {
        internalPool = new ObjectPool<ElementView>(
            CreateElement,
            GetElement,
            ReleaseElement,
            collectionCheck: false);

        prefab = Resources.Load<ElementView>(prefabPath);
        CreatePoolRoot(prefabPath, scrollRoot);
    }

    public ElementView Get(Transform parent)
    {
        var elementView = internalPool.Get();
        elementView.transform.SetParent(parent);
        return elementView;
    }

    public ElementView Peek() => prefab;

    public void Release(ElementView element)
    {
        internalPool.Release(element);
    }

    private void ReleaseElement(ElementView elementView)
    {
        elementView.gameObject.SetActive(false);
        elementView.transform.SetParent(poolRoot);
    }

    private void GetElement(ElementView elementView)
    {
        elementView.gameObject.SetActive(true);
    }

    private ElementView CreateElement() => Object.Instantiate(prefab, poolRoot);

    private void CreatePoolRoot(string prefabPath, Transform scrollRoot)
    {
        var poolRootObject = new GameObject($"PoolRoot_{prefabPath}");
        poolRoot = poolRootObject.transform;
        poolRoot.SetParent(scrollRoot, false);
    }
}