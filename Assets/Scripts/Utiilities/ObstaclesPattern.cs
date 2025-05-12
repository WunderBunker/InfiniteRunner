using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(ObstaclesPattern))]
public class ObstaclesPatternEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ObstaclesPattern vOP = (ObstaclesPattern)target;
        if (GUILayout.Button("Init  Pattern")) vOP.InitPattern();
        if (GUILayout.Button("Apply Pattern")) vOP.ApplyPattern();
    }
}
#endif

public class ObstaclesPattern : MonoBehaviour
{
    //On rentre la distance en dur car impossible de sériliser des champs statiques. Todo : passer les patternes en ScriptableObject pour bénéficier de champs communs
    static public float _distanceBtwnObstacles = 7;
    //Key : Prefab de l'obstacle ; Value : Lane(axe x), Line (axe y)
    public Dictionary<GameObject, (byte, int)> Obstacles { get; private set; } = new();
    //Les patternes qui ne prennent pas toute la largeur du couloir peuvent d'être décaler d'un certain nombre de lane,
    //cette valeur permet de controler ça (ex : partterne à 2 lane dans couloir à 3 => offset max de 1 lane)
    public int MaxLaneOffset { get; private set; }
    [SerializeField] public float Length { get; private set; }

    LanesManager _LM;

    public void InitPattern()
    {
        _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        Obstacles.Clear();
        _LM.InitLanes();
        float vMaxDeltaXSameLane = _LM.LaneWidth * 0.75f;

        //On tri les obstacles selon loeur position sur X
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
            Obstacles.Add(vOrderedTransforms[lCptTrsf].gameObject, (vCurrentLane, 0));
        }
        MaxLaneOffset = _LM.LaneNumber - 1 - vCurrentLane;

        float vMaxDeltaZSameLine = _distanceBtwnObstacles * 0.75f;
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
                    if (vZDist > lCptLine * vMaxDeltaZSameLine) vCurrentLine ++;
            }
            Obstacles[vOrderedTransforms[lCptTrsf].gameObject] = (Obstacles[vOrderedTransforms[lCptTrsf].gameObject].Item1, vCurrentLine);
        }

        Length = vCurrentLine * _distanceBtwnObstacles;
    }

    public void ApplyPattern()
    {
        foreach (KeyValuePair<GameObject, (byte, int)> lObstacle in Obstacles)
        {
            Vector3 vPosition = lObstacle.Key.transform.localPosition;
            lObstacle.Key.transform.localPosition = new Vector3((float)_LM.GetLaneCenter(lObstacle.Value.Item1), vPosition.y, lObstacle.Value.Item2 * _distanceBtwnObstacles);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 20; i++)
            Gizmos.DrawLine(transform.position + 100 * Vector3.left + Vector3.forward * _distanceBtwnObstacles * i,
                            transform.position + 100 * Vector3.right + Vector3.forward * _distanceBtwnObstacles * i);
        if (_LM != null && _LM.GetLaneCenter(0)!=null)
            for (int i = 0; i < _LM.LaneNumber; i++)
                Gizmos.DrawLine(transform.position + 100 * Vector3.forward + Vector3.right * (float)_LM.GetLaneCenter((byte)i),
                            transform.position + 100 * Vector3.back + Vector3.right * (float)_LM.GetLaneCenter((byte)i));
    }
#endif
}
