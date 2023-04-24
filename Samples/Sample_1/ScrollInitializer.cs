using System.Collections.Generic;
using IgorTime.PoolableScrollView;
using IgorTime.Samples.Sample_1.ElementData;
using UnityEngine;
using UnityEngine.Serialization;

namespace IgorTime.Samples.Sample_1
{
    [RequireComponent(typeof(BasePoolableScrollView))]
    public class ScrollInitializer : MonoBehaviour
    {
        [FormerlySerializedAs("verticalScrollView")]
        [SerializeField]
        private BasePoolableScrollView scrollView;

        [SerializeField]
        private int itemsCount;

        [SerializeField]
        private Sprite[] sprites;

        [SerializeField]
        private int itemIndex;

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

            scrollView.Initialize(dataList.ToArray());
        }

        [ContextMenu(nameof(ScrollTo))]
        public void ScrollTo()
        {
            scrollView.ScrollToItem(itemIndex);
        }

        private T GetRandom<T>(T[] array) => array[Random.Range(0, array.Length)];
    }
}