using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LanesManager : MonoBehaviour
{
    public int LaneNumber { get; private set; } = 3;
    public float LaneWidth { get; private set; }
    public byte CurrentPlayerLane { get; private set; }
    public float GroundHeight { get; private set; }

    [SerializeField] GameObject _corridorPrefab;
    [SerializeField] int _numberOfCorridorToInit;
    [SerializeField] int _numberOfPassedorridorsBfDelete;
    Vector3 _corridorSize;
    Queue<GameObject> _corridors = new();

    Transform _playertransform;

    Lane[] _lanes;

    void Start()
    {
        _playertransform = GameObject.FindGameObjectWithTag("Player").transform.Find("Pivot");
        GroundHeight = _playertransform.position.y;
        _corridorSize = GetCorridorSize();

        InitLanes();

        AddCorridor(_playertransform.position);
        for (int lCptCorridor = 1; lCptCorridor <= _numberOfCorridorToInit; lCptCorridor++)
            AddCorridorInFront();
    }

    void Update()
    {
        GameObject vFirstCorridor = _corridors.Peek();
        if (vFirstCorridor.transform.position.z < _playertransform.position.z - _corridorSize.z * _numberOfPassedorridorsBfDelete)
        {
            _corridors.Dequeue();
            Destroy(vFirstCorridor);
            AddCorridorInFront();
        }
    }

    public void InitLanes()
    {
        LaneWidth = (_corridorSize.x==0?GetCorridorSize().x:_corridorSize.x) / LaneNumber;
        _lanes = new Lane[] { new(LaneWidth, -LaneWidth), new(LaneWidth, 0), new(LaneWidth, LaneWidth) };
    }

    public float? GetNextLaneX(int pXDirection, bool pMajPlayerLane = true)
    {
        byte vNextLane = pXDirection > 0 ? (byte)(CurrentPlayerLane + 1) : pXDirection == 0 ? CurrentPlayerLane : (byte)(CurrentPlayerLane - 1);
        float? vCenter = GetLaneCenter(vNextLane);
        if (pMajPlayerLane && vCenter != null) CurrentPlayerLane = vNextLane;
        return vCenter;
    }

    public float? GetLaneCenter(byte pLaneNb)
    {
        if (_lanes == null) return null;
        if (pLaneNb >= 0 && pLaneNb < _lanes.Length) return _lanes[pLaneNb].CenterX;
        else return null;
    }

    void AddCorridorInFront()
    {
        Vector3 vPosition = _corridors.Last().transform.position + _corridorSize.z * Vector3.forward;
        AddCorridor(vPosition);
    }

    void AddCorridor(Vector3 pPosition)
    {
        GameObject vNewCorridor = Instantiate(_corridorPrefab, pPosition, Quaternion.identity, transform);
        GameObject vLaneLimit = vNewCorridor.transform.Find("LaneDelimitation").gameObject;
        
        for (int lCptLaneLimit = 1; lCptLaneLimit<LaneNumber ; lCptLaneLimit++)
            Instantiate(vLaneLimit,pPosition+Vector3.right*(-_corridorSize.x/2+lCptLaneLimit*LaneWidth), vLaneLimit.transform.rotation,vNewCorridor.transform);
        Destroy(vLaneLimit);

        _corridors.Enqueue(vNewCorridor);
    }

    public Vector3 GetCorridorSize()
    {
        Vector3 vCorridorSize = Vector3.Cross(_corridorPrefab.transform.localScale, _corridorPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size);
        vCorridorSize = new Vector3(Mathf.Abs(vCorridorSize.x), Mathf.Abs(vCorridorSize.y), Mathf.Abs(vCorridorSize.z));
        return vCorridorSize;
    }
}

public struct Lane
{
    public float CenterX { get; private set; }
    public float Width { get; private set; }

    public Lane(float pWidth, float pCenterX)
    {
        Width = pWidth;
        CenterX = pCenterX;
    }

}
