using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class PoolableScroll : MonoBehaviour
{
    private static readonly Vector3[] viewPortWorldCorners = new Vector3[4];

    [SerializeField]
    private ScrollRect scrollRect;

    private readonly LinkedList<ElementView> activeElements = new();

    private IEnumerable<IElementData> itemsData;
    private Vector2? previousContentPosition;
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

    private static Vector2 GetElementSize(IElementData data)
    {
        var prefab = Resources.Load<ElementView>(data.PrefabPath);
        return prefab.Size;
    }

    public void Initialize(IEnumerable<IElementData> itemsData)
    {
        this.itemsData = itemsData;

        SetContentSize(itemsData);
        CreateInitialElements(itemsData);
    }

    private ElementView CreateElement(IElementData data, Vector2 position)
    {
        var prefab = Resources.Load<ElementView>(data.PrefabPath);
        var elementView = Instantiate(prefab, Content);
        elementView.Initialize(data);
        elementView.GetComponent<RectTransform>().anchoredPosition = position;
        return elementView;
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
        foreach (var elementData in elementsData)
        {
            var element = CreateNewElementDown(elementData);
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

    private void UpdateScrollItems(Vector2 contentPosition)
    {
        if (previousContentPosition == null)
        {
            previousContentPosition = contentPosition;
            return;
        }

        var contentDeltaPosition = contentPosition - previousContentPosition;
        switch (contentDeltaPosition.Value.y)
        {
            case > 0: HandleMoveDown();
                break;
            case < 0: HandleMoveUp();
                break;
        }

        previousContentPosition = contentPosition;
    }

    private void HandleMoveDown()
    {
        var firstElement = activeElements.First.Value;
        if (!firstElement.RectTransform.IsOverlappedBy(Viewport))
        {
            activeElements.RemoveFirst();
            Destroy(firstElement.gameObject);
        }
        
        var lastElement = activeElements.Last.Value;
        if (lastElement.RectTransform.IsOverlappedBy(Viewport))
        {
            var testData = new TextData(){ Text = "New Element"};
            CreateNewElementUp(testData);
        }
    }

    private void HandleMoveUp()
    {
        var lastElement = activeElements.Last.Value;
        if (!lastElement.RectTransform.IsOverlappedBy(Viewport))
        {
            activeElements.RemoveLast();
            Destroy(lastElement.gameObject);
        }
        
        var firstElement = activeElements.First.Value;
        if (firstElement.RectTransform.IsOverlappedBy(Viewport))
        {
            var testData = new TextData(){ Text = "New Element"};
            CreateNewElementDown(testData);
        }
    }

    private ElementView CreateFirstElement(IElementData elementData)
    {
        var startPosition = Content.rect.height / 2;
        var elementHeightHalf = GetElementSize(elementData).y / 2;
        var elementPositionY = startPosition - elementHeightHalf;

        var elementCenterPosition = new Vector2(0, elementPositionY);
        var element = CreateElement(elementData, elementCenterPosition);
        activeElements.AddFirst(element);
        return element;
    }

    private ElementView CreateNewElementDown(IElementData elementData)
    {
        if (activeElements.First == null)
        {
            return CreateFirstElement(elementData);
        }
        
        var firstElement = activeElements.First.Value;
        var firstElementPosition = firstElement.RectTransform.anchoredPosition.y;
        var firstElementSize = firstElement.Size.y;
        var newElementSize = GetElementSize(elementData).y;
        var newElementPosition = new Vector2(0, firstElementPosition - 
                                                firstElementSize / 2 - 
                                                newElementSize / 2);
            
        var newElement = CreateElement(elementData, newElementPosition);
        activeElements.AddFirst(newElement);
        return newElement;
    }
    
    private ElementView CreateNewElementUp(IElementData elementData)
    {
        var lastElement = activeElements.Last.Value;
        var lastElementPosition = lastElement.RectTransform.anchoredPosition.y;
        var lastElementSize = lastElement.Size.y;
        var newElementSize = GetElementSize(elementData).y;
        var newElementPosition = new Vector2(0, lastElementPosition + 
                                                lastElementSize / 2 + 
                                                newElementSize / 2);
            
        var newElement = CreateElement(elementData, newElementPosition);
        activeElements.AddLast(newElement);
        return newElement;
    }
}