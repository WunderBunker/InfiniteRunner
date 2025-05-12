using System.Collections.Generic;
using UnityEngine;

public class ObstaclesManager : MonoBehaviour
{
    [SerializeField] List<GameObject> _obstaclesPatterns = new();
    [SerializeField] float _spawnTempoInMeters;
    [SerializeField] float _spawnDistance;
    [SerializeField] int _numberOfPassedPatternsBfDelete;
    float _spawnTimerInMeters;
    System.Random _spawnRandom = new();
    GameObject _player;
    Queue<GameObject> _obstacles = new();

    bool _isBossMode = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _spawnTimerInMeters = _spawnTempoInMeters;
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (_isBossMode) return;

        _spawnTimerInMeters -= _player.GetComponent<PlayerManager>().DeltaDistance;
        Vector3 vPlayerPivotPosition = _player.transform.Find("Pivot").position;

        //instantition de nouveau obstacles si le timer le requiert
        if (_spawnTimerInMeters <= 0)
        {
            _spawnTimerInMeters = _spawnTempoInMeters;
            GameObject vPattern = _obstaclesPatterns[_spawnRandom.Next(0, _obstaclesPatterns.Count)];

            Vector3 vPatternPosition = new Vector3(vPattern.transform.position.x, vPlayerPivotPosition.y, vPlayerPivotPosition.z + _spawnDistance);
            GameObject vNewPattern = Instantiate(vPattern, vPatternPosition, Quaternion.identity, transform);
            vNewPattern.GetComponent<ObstaclesPattern>().InitPattern();
            vNewPattern.GetComponent<ObstaclesPattern>().ApplyPattern();
            _obstacles.Enqueue(vNewPattern);
        }

        //Destruction des vieux patternes si besoin    
        if (_obstacles.Count > 0)
        {
            GameObject vFirstObst = _obstacles.Peek();
            if (vFirstObst.transform.position.z < vPlayerPivotPosition.z - vFirstObst.GetComponent<ObstaclesPattern>().Length * _numberOfPassedPatternsBfDelete)
            {
                _obstacles.Dequeue();
                Destroy(vFirstObst);
            }
        }
    }

    public void SwitchBossMode(bool pIsBoss)
    {
        if (pIsBoss && !_isBossMode)
            while (_obstacles.Count > 0)
            {
                GameObject vObstacle = _obstacles.Dequeue();
                if (vObstacle != null)
                    Destroy(vObstacle);
            }

        _isBossMode = pIsBoss;
    }
}
