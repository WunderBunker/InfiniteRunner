using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Biome))]
public class BiomePatternEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Biome vBiome = (Biome)target;
        if (GUILayout.Button("Maj All Pattern")) vBiome.MajAllPattern();
    }
}
#endif

[CreateAssetMenu(fileName = "Biome", menuName = "Scriptable Objects/Biome")]
public class Biome : ScriptableObject
{
    public string BiomeId;
    [SerializeField] public List<GameObject> Patterns;

    public void MajAllPattern()
    {
        foreach (GameObject lPattern in Patterns)
            lPattern.GetComponent<Pattern>().MajPattern();
    }
}
