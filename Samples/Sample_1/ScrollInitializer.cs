using System.Collections.Generic;
using IgorTime.PoolableScrollView;
using IgorTime.Samples.Sample_1.ElementData;
using UnityEngine;

namespace IgorTime.Samples.Sample_1
{
    [RequireComponent(typeof(PoolableScroll))]
    public class ScrollInitializer : MonoBehaviour
    {
        [SerializeField]
        private PoolableScroll verticalScroll;

        [SerializeField]
        private int itemsCount;

        [SerializeField]
        private Sprite[] sprites;

        [SerializeField]
        private int itemIndex;

        [SerializeField]
        private float animationDuration = 0.5f;
    
        [SerializeField]
        private AnimationCurve easeInOut = AnimationCurve.EaseInOut(0, 0, 1, 1);

    
        public void Start()
        {
            var dataList = new List<IElementData>();
            for (var i = 0; i < itemsCount; i++)
            {
                IElementData item = Random.value > 0.5f
                    ? new SpriteData {Sprite = GetRandom(sprites)}
                    : new TextData {Text = $"Item {i}"};

                dataList.Add(item);
            }

            verticalScroll.Initialize(dataList.ToArray());
        }

        [ContextMenu(nameof(ScrollTo))]
        public void ScrollTo()
        {
            verticalScroll.ScrollToItem(itemIndex);
        }
    
        [ContextMenu(nameof(ScrollToAnimated))]
        public void ScrollToAnimated()
        {
            verticalScroll.ScrollToItem(itemIndex, animationDuration, easeInOut);
        }

        [ContextMenu(nameof(Next))]
        public void Next()
        {
            verticalScroll.ScrollToNext(animationDuration, easeInOut);
        }
    
        [ContextMenu(nameof(Previous))]

        public void Previous()
        {
            verticalScroll.ScrollToPrevious(animationDuration, easeInOut);
        }
    
        private T GetRandom<T>(T[] array) => array[Random.Range(0, array.Length)];
    }
}