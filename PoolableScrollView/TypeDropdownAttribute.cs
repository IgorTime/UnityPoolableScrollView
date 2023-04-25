using System;
using UnityEngine;

public class TypeDropdownAttribute : PropertyAttribute
{
    public Type Type { get; }

    public TypeDropdownAttribute(Type type)
    {
        Type = type;
    }
}