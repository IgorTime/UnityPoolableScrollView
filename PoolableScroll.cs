using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class PoolableScroll : MonoBehaviour
{
    private static readonly Vector3[] viewPortWorldCorners = new Vector3[4];

    [SerializeField]
    private ScrollRect scrollRect;

    private IEnumerable<IElementData> itemsData;

    private RectTransform Content => scrollRect.content;
    private RectTransform Viewport => scrollRect.viewport;

    private void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(UpdateScrollItems);
    }

    private void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(UpdateScrollItems);
    }

    public void Initialize(IEnumerable<IElementData> itemsData)
    {
        this.itemsData = itemsData;

        SetContentSize(itemsData);
        CreateInitialElements(itemsData);
    }

    public ElementView CreateElement(IElementData data, Vector2 position)
    {
        var prefab = Resources.Load<ElementView>(data.PrefabPath);
        var elementView = Instantiate(prefab, Content);
        elementView.Initialize(data);
        elementView.GetComponent<RectTransform>().anchoredPosition = position;
        return elementView;
    }

    public Vector2 GetElementSize(IElementData data)
    {
        var prefab = Resources.Load<ElementView>(data.PrefabPath);
        return prefab.Size;
    }

    public bool IsVisible(Vector2 elementPosition, float elementHeightHalf)
    {
        var viewPortRect = Viewport.rect;
        var elementRect = new Rect(elementPosition, new Vector2(viewPortRect.width, elementHeightHalf * 2));
        return elementRect.Overlaps(viewPortRect);

        Viewport.GetWorldCorners(viewPortWorldCorners);
        var viewRect = scrollRect.viewport;
        var localPosition = viewRect.localPosition;

        var lowerBound = localPosition.y - viewRect.rect.height * 0.5f;
        var upperBound = localPosition.y + viewRect.rect.height * 0.5f;

        var yMax = elementPosition.y + elementHeightHalf;
        var yMin = elementPosition.y - elementHeightHalf;

        return yMin < upperBound && yMax > lowerBound;
    }

    private void OnValidate()
    {
        if (!scrollRect)
        {
            scrollRect = GetComponent<ScrollRect>();
        }
    }

    private void CreateInitialElements(IEnumerable<IElementData> elementsData)
    {
        var startPosition = Content.rect.height / 2;
        foreach (var elementData in elementsData)
        {
            var elementHeightHalf = GetElementSize(elementData).y / 2;
            var elementPositionY = startPosition - elementHeightHalf;

            var elementCenterPosition = new Vector2(0, elementPositionY);
            var elementTopPosition = new Vector2(0, elementPositionY + elementHeightHalf);
            var elementDownPosition = new Vector2(0, elementPositionY - elementHeightHalf);
            
            var element = CreateElement(elementData, elementCenterPosition);
            startPosition = elementPositionY - elementHeightHalf;

            if (!element.RectTransform.IsOverlappedBy(Viewport))
            {
                break;
            }
        }
    }

    private void SetContentSize(IEnumerable<IElementData> itemsData)
    {
        var height = CalculateFullContentHeight(itemsData);
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, height);
    }

    private float CalculateFullContentHeight(IEnumerable<IElementData> elementsData)
    {
        var height = 0f;
        foreach (var elementData in elementsData)
        {
            height += GetElementSize(elementData).y;
        }

        return height;
    }

    private void UpdateScrollItems(Vector2 arg0)
    {
    }
}