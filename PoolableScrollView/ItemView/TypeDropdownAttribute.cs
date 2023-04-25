using System;
using UnityEngine;

namespace IgorTime.PoolableScrollView.ItemView
{
    public class TypeDropdownAttribute : PropertyAttribute
    {
        public Type Type { get; }

        public TypeDropdownAttribute(Type type)
        {
            Type = type;
        }
    }
}