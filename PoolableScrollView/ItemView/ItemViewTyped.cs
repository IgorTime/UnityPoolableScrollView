using System;
using IgorTime.PoolableScrollView.DataItems;

namespace IgorTime.PoolableScrollView.ItemView
{
    public abstract class ItemViewTyped<T> : ItemView, IItemViewDataTypeConstrain
        where T : IItemData
    {
        public Type DataType => typeof(T);

        protected sealed override void UpdateContent(IItemData data)
        {
            if (data is T typedData)
            {
                UpdateContent(typedData);
            }
            else
            {
                throw new ArgumentException($"Data type {data.GetType()} is not supported by this view");
            }
        }

        protected abstract void UpdateContent(T data);
    }
}