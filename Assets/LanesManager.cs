using UnityEngine;

public class LanesManager : MonoBehaviour
{
    public int LaneNumber { get; private set; } = 3;
    public float LaneWidth {get; private set;}

    Lane[] _lanes;
    byte _currentPlayerLane;

    void Start()
    {
        InitLanes();
    }

    public void InitLanes()
    {
        LaneWidth = GameObject.FindGameObjectWithTag("GroundsManager").GetComponent<GroundsManager>().GetGroundSize().x / LaneNumber;
        _lanes = new Lane[] { new(LaneWidth, -LaneWidth), new(LaneWidth, 0), new(LaneWidth, LaneWidth) };
    }

    public float? GetNextLaneX(int pXDirection, bool pMajPlayerLane = true)
    {
        byte vNextLane = pXDirection > 0 ? (byte)(_currentPlayerLane + 1) : pXDirection == 0 ? _currentPlayerLane : (byte)(_currentPlayerLane - 1);
        float? vCenter = GetLaneCenter(vNextLane);
        if (pMajPlayerLane && vCenter != null) _currentPlayerLane = vNextLane;
        return vCenter;
    }

    public float? GetLaneCenter(byte pLaneNb)
    {
        if(_lanes ==null)return null;
        if (pLaneNb >= 0 && pLaneNb < _lanes.Length) return _lanes[pLaneNb].CenterX;
        else return null;
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
