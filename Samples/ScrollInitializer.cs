using System.Collections.Generic;
using IgorTime.PoolableScrollView;
using IgorTime.Samples.Sample_1.ElementData;
using UnityEngine;
using UnityEngine.Serialization;

namespace IgorTime.Samples.Sample_1
{
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
        private string[] messages;

        public void Start()
        {
            var dataList = new List<IItemData>();
            for (var i = 0; i < itemsCount; i++)
            {
                IItemData item = Random.value > 0.5f
                    ? new SpriteData {Sprite = GetRandom(sprites)}
                    : new TextData {Message = GetRandom(messages)};

                dataList.Add(item);
            }

            scrollView.Initialize(dataList.ToArray());
        }

        private static T GetRandom<T>(IReadOnlyList<T> array) => array[Random.Range(0, array.Count)];
    }
}