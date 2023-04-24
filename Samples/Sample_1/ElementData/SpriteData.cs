using UnityEngine;

public class SpriteData : IElementData
{
    public Sprite Sprite { get; set; }
    public string PrefabPath => "ImageElement";
}