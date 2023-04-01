using UnityEngine;

public class MeshData : IElementData
{
    public Mesh Mesh { get; set; }
    public string PrefabPath => "MeshPrefab";
}