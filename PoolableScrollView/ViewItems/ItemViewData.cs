using UnityEngine;

namespace IgorTime.PoolableScrollView
{
    public readonly struct ItemViewData
    {
        public readonly Vector2 Min;
        public readonly Vector2 Max;
        public readonly Vector2 Position;

        public ItemViewData(Vector2 position, Vector2 size)
        {
            Position = position;
            Min = position - size * 0.5f;
            Max = position + size * 0.5f;
        }
    }
}