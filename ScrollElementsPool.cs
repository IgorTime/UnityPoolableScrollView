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
            CreateFunc,
            GetFunc,
            ReleaseFunc);

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

    private void ReleaseFunc(ElementView elementView)
    {
        elementView.gameObject.SetActive(false);
        elementView.transform.SetParent(poolRoot);
    }

    private void GetFunc(ElementView elementView)
    {
        elementView.gameObject.SetActive(true);
    }

    private ElementView CreateFunc() => Object.Instantiate(prefab, poolRoot);

    private void CreatePoolRoot(string prefabPath, Transform scrollRoot)
    {
        var poolRootObject = new GameObject($"PoolRoot_{prefabPath}");
        poolRoot = poolRootObject.transform;
        poolRoot.SetParent(scrollRoot, false);
    }
}