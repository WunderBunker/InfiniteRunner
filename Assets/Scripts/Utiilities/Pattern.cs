using UnityEngine;
using System.Collections.Generic;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(Pattern))]
public class ObstaclesPatternEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Pattern vOP = (Pattern)target;
        if (GUILayout.Button("Init  Pattern")) vOP.RecalculPattern();
        if (GUILayout.Button("Maj Pattern")) vOP.MajPattern();
    }
}
#endif

public class Pattern : MonoBehaviour
{
    [SerializeField] PatternConfig _patternConfig;

    //Key : Prefab de l'obstacle ; Value : Lane(axe x), Line (axe y)
    [SerializeField, ReadOnly] private List<ObjectData> _objects = new();

    //Les patternes qui ne prennent pas toute la largeur du couloir peuvent d'être décaler d'un certain nombre de lane,
    //cette valeur permet de controler ça (ex : partterne à 2 lane dans couloir à 3 => offset max de 1 lane)
    public int MaxLaneOffset { get; private set; }
    [SerializeField, ReadOnly] float _length;
    public float Length { get => _length; private set { _length = value; } }

    LanesManager _LM;

    //Définit en fonction de l'emplacement de chaque objet leur position en ligne et lane
    public void RecalculPattern()
    {
        _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _objects.Clear();
        _LM.InitLanes();
        float vMaxDeltaXSameLane = _LM.LaneWidth * 0.75f;

        //On tri les obstacles selon leur position sur X
        List<Transform> vOrderedTransforms = new();
        for (int lCptChild = 0; lCptChild < transform.childCount; lCptChild++)
            vOrderedTransforms.Add(transform.GetChild(lCptChild));
        vOrderedTransforms = vOrderedTransforms.OrderBy(transform => transform.position.x).ToList();

        //On détermine les lane de chaque obstacle en fonction de l'écart maximal toléré en X sur une même lane
        byte vCurrentLane = 0;
        for (int lCptTrsf = 0; lCptTrsf < vOrderedTransforms.Count; lCptTrsf++)
        {
            if (lCptTrsf > 0)
            {
                float vXDist = Mathf.Abs(vOrderedTransforms[lCptTrsf].position.x - vOrderedTransforms[lCptTrsf - 1].position.x);

                for (int lCptLane = 1; lCptLane <= _LM.LaneNumber; lCptLane++)
                    if (vXDist > lCptLane * vMaxDeltaXSameLane) vCurrentLane = (byte)(vCurrentLane + 1);
                if (vCurrentLane >= _LM.LaneNumber) vCurrentLane = (byte)(_LM.LaneNumber - 1);
            }
            _objects.Add(new() { Object = vOrderedTransforms[lCptTrsf].gameObject, Lane = vCurrentLane, Line = 0 });
        }
        MaxLaneOffset = _LM.LaneNumber - 1 - vCurrentLane;

        float vMaxDeltaZSameLine = _patternConfig.DistanceBtwLines * 0.75f;
        //On tri les obstacles selon leur position sur Z
        vOrderedTransforms = vOrderedTransforms.OrderBy(transform => transform.position.z).ToList();
        //On détermine les lignes (axe z) de chaque obstacle en fonction de l'écart maximal toléré en z sur une même ligne
        int vCurrentLine = 0;
        for (int lCptTrsf = 0; lCptTrsf < vOrderedTransforms.Count; lCptTrsf++)
        {
            if (lCptTrsf > 0)
            {
                float vZDist = Mathf.Abs(vOrderedTransforms[lCptTrsf].position.z - vOrderedTransforms[lCptTrsf - 1].position.z);

                //Le nombre de ligne max (20) est arbitraire, à modifier si un patterne a plus de 20 lignes
                for (int lCptLine = 1; lCptLine <= 20; lCptLine++)
                    if (vZDist > lCptLine * vMaxDeltaZSameLine) vCurrentLine++;
            }
            foreach (ObjectData lObjData in _objects)
                if (lObjData.Object == vOrderedTransforms[lCptTrsf].gameObject)
                {
                    lObjData.Line = vCurrentLine;
                    break;
                }
        }

        Length = vCurrentLine * _patternConfig.DistanceBtwLines;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this); // Marque l’objet comme modifié
        PrefabUtility.RecordPrefabInstancePropertyModifications(this); // Pour forcer la sauvegarde sur le prefab
#endif
    }

    //Applique à chaque objet sa position à partir de sa ligne et lane
    public void MajPattern()
    {
        if (_LM == null)
        {
            _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
            _LM.InitLanes();
        }
        foreach (ObjectData lObjData in _objects)
        {
            Vector3 vPosition = lObjData.Object.transform.localPosition;
            lObjData.Object.transform.localPosition = new Vector3((float)_LM.GetLaneCenter(lObjData.Lane), vPosition.y, lObjData.Line * _patternConfig.DistanceBtwLines);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 20; i++)
            Gizmos.DrawLine(transform.position + 100 * Vector3.left + Vector3.forward * _patternConfig.DistanceBtwLines * i,
                            transform.position + 100 * Vector3.right + Vector3.forward * _patternConfig.DistanceBtwLines * i);
        if (_LM != null && _LM.GetLaneCenter(0) != null)
            for (int i = 0; i < _LM.LaneNumber; i++)
                Gizmos.DrawLine(transform.position + 100 * Vector3.forward + Vector3.right * (float)_LM.GetLaneCenter((byte)i),
                            transform.position + 100 * Vector3.back + Vector3.right * (float)_LM.GetLaneCenter((byte)i));
    }
#endif
}

[System.Serializable]
public class ObjectData
{
    public GameObject Object;
    public byte Lane;
    public int Line;
}
