using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;



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

        float vFurtherest = 0;
        for (int lCptChild = 0; lCptChild < transform.childCount; lCptChild++)
        {
            byte vCurrentLane = 0;
            int vCurrentLine = 0;
            int vCurrentSubLine = 0;

            Transform lChild = transform.GetChild(lCptChild);

            //Calcul de la lane
            float vShortestDist = math.abs(lChild.localPosition.x - (float)_LM.GetLaneCenter(0));
            for (byte lCptLane = 1; lCptLane < _LM.LaneNumber; lCptLane++)
            {
                float lDist = math.abs(lChild.localPosition.x - (float)_LM.GetLaneCenter(lCptLane));
                if (lDist <= vShortestDist)
                {
                    vShortestDist = lDist;
                    vCurrentLane = lCptLane;
                }
            }

            //Les oboles peuvent être positionnées sur des sublines, on calculs donc leur line différemment du reste
            if (lChild.CompareTag("Obole"))
            {
                //Calcul de la line
                for (int lCptLine = 0; lCptLine <= 20; lCptLine++)
                {
                    if (lChild.localPosition.z >= lCptLine * _patternConfig.DistanceBtwLines)
                        vCurrentLine = lCptLine;
                    else break;
                }
                //Calcul de la Subline
                float vLinePosZ = vCurrentLine * _patternConfig.DistanceBtwLines;
                vShortestDist = math.abs(lChild.localPosition.z - vLinePosZ);
                for (int lCptSubLine = 1; lCptSubLine <= _patternConfig.SublinesNb; lCptSubLine++)
                {
                    float lDist = math.abs(lChild.localPosition.z - (lCptSubLine * _patternConfig.DistanceBtwLines / _patternConfig.SublinesNb + vLinePosZ));
                    if (lDist <= vShortestDist)
                    {
                        vShortestDist = lDist;
                        vCurrentSubLine = lCptSubLine;
                    }
                }
            }
            //Calcul de la line pour le reste
            else
            {
                vShortestDist = lChild.localPosition.z;
                for (int lCptLine = 0; lCptLine <= 20; lCptLine++)
                {
                    float lDist = math.abs(lChild.localPosition.z - lCptLine * _patternConfig.DistanceBtwLines);
                    if (lDist <= vShortestDist)
                    {
                        vShortestDist = lDist;
                        vCurrentLine = lCptLine;
                    }
                }
            }
            if (vCurrentLine * _patternConfig.DistanceBtwLines > vFurtherest) vFurtherest = vCurrentLine * _patternConfig.DistanceBtwLines;


            //Enregistrement des donnees
            bool vIsFound = false;
            foreach (ObjectData lObjData in _objects)
                if (lObjData.Object == lChild.gameObject)
                {
                    lObjData.Lane = vCurrentLane;
                    lObjData.Line = vCurrentLine;
                    lObjData.SubLine = vCurrentSubLine;
                    vIsFound = true;
                    break;
                }
            if (!vIsFound) _objects.Add(new() { Object = lChild.gameObject, Lane = vCurrentLane, Line = vCurrentLine, SubLine = vCurrentSubLine });
        }

        Length = vFurtherest;

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
            lObjData.Object.transform.localPosition = new Vector3((float)_LM.GetLaneCenter(lObjData.Lane), vPosition.y, lObjData.Line * _patternConfig.DistanceBtwLines + lObjData.SubLine * _patternConfig.DistanceBtwLines / _patternConfig.SublinesNb);
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this); // Marque l’objet comme modifié
        PrefabUtility.RecordPrefabInstancePropertyModifications(this); // Pour forcer la sauvegarde sur le prefab
#endif
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        for (int i = 0; i < 20; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + 100 * Vector3.left + Vector3.forward * _patternConfig.DistanceBtwLines * i,
                        transform.position + 100 * Vector3.right + Vector3.forward * _patternConfig.DistanceBtwLines * i);
            Gizmos.color = Color.blue;
            for (int y = 1; y < _patternConfig.SublinesNb; y++)
                Gizmos.DrawLine(transform.position + 100 * Vector3.left + Vector3.forward * (_patternConfig.DistanceBtwLines * i + _patternConfig.DistanceBtwLines / _patternConfig.SublinesNb * y),
                            transform.position + 100 * Vector3.right + Vector3.forward * (_patternConfig.DistanceBtwLines * i + _patternConfig.DistanceBtwLines / _patternConfig.SublinesNb * y));
        }
        Gizmos.color = Color.cyan;
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
    [SerializeField] public GameObject Object;
    public byte Lane;
    public int Line;
    public int SubLine;
}
