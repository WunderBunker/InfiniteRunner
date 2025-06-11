using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialList", menuName = "Scriptable Objects/MaterialList")]
public class MaterialList : ScriptableObject
{
    public string ListId;
    public IdentifiedMaterial[] MatArray;
}

[Serializable]
public struct IdentifiedMaterial
{
    public string Id;
    public Material Material;
}