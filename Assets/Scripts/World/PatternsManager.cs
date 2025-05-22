using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PatternsManager : MonoBehaviour
{
    [SerializeField] float _patternSpawnTempoInMeters;
    [SerializeField] float _gateSpawnTempoInMeters;
    [SerializeField] float _spawnDistance;
    [SerializeField] int _numberOfPassedPatternsBfDelete;
    [SerializeField] GameObject _gate;

    LanesManager _laneManager;

    List<GameObject> _patterns = new();
    float _patternSpawnTimerInMeters;
    float _gateSpawnTimerInMeters;

    System.Random _spawnRandom = new();
    GameObject _player;
    Queue<GameObject> _objects = new();

    bool _isBossMode = false;
    bool _bossHasDefendedGate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _patternSpawnTimerInMeters = 0;
        _gateSpawnTimerInMeters = _gateSpawnTempoInMeters;
        _player = GameObject.FindGameObjectWithTag("Player");
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //A -10% du spawn des prochain portail on fait apparaitre le boss
        if (!_bossHasDefendedGate && _gateSpawnTimerInMeters <= _gateSpawnTempoInMeters * 0.1f)
        {
            _bossHasDefendedGate = true;
            GameObject.FindGameObjectWithTag("BiomesManager").GetComponent<BiomesManager>().CurrentBoss.GetComponent<IBoss>().Activate();
        }


        //Si boss en cours rien ne spawn
        if (_isBossMode) return;

        _patternSpawnTimerInMeters -= _player.GetComponent<PlayerManager>().DeltaDistance;
        _gateSpawnTimerInMeters -= _player.GetComponent<PlayerManager>().DeltaDistance;

        Vector3 vPlayerPivotPosition = _player.transform.Find("Pivot").position;

        //instantition de nouveau obstacles si le timer le requiert
        if (_patternSpawnTimerInMeters <= 0)
        {
            _patternSpawnTimerInMeters = _patternSpawnTempoInMeters;

            //Si il est tant d'instacier des portails alors on fait ça à la place
            if (_gateSpawnTimerInMeters <= 0)
            {
                _gateSpawnTimerInMeters = _gateSpawnTempoInMeters;
                for (byte lLane = 0; lLane < _laneManager.LaneNumber; lLane++)
                    _objects.Enqueue(Instantiate(_gate, new Vector3((float)_laneManager.GetLaneCenter(lLane), _gate.transform.position.y, vPlayerPivotPosition.z + _spawnDistance), Quaternion.identity, transform));
            }
            //Sinon on instancie les objets d'un nouveau patterne
            else
            {
                GameObject vPattern = _patterns[_spawnRandom.Next(0, _patterns.Count)];

                Vector3 vPatternPosition = new Vector3(vPattern.transform.position.x, vPlayerPivotPosition.y, vPlayerPivotPosition.z + _spawnDistance);
                GameObject vNewPattern = Instantiate(vPattern, vPatternPosition, Quaternion.identity, transform);
                vNewPattern.GetComponent<Pattern>().MajPattern();
                _objects.Enqueue(vNewPattern);
            }
        }

        //Destruction des vieux patternes si besoin    
        if (_objects.Count > 0)
        {
            GameObject vFirstObj = _objects.Peek();
            if (vFirstObj.transform.position.z < vPlayerPivotPosition.z - _patternSpawnTempoInMeters * _numberOfPassedPatternsBfDelete)
            {
                _objects.Dequeue();
                Destroy(vFirstObj);
            }
        }
    }

    public void SetNewBiome(List<GameObject> pPatternsList)
    {
        _patterns = pPatternsList;
        _bossHasDefendedGate = false;
        DestroyObjects();
        _patternSpawnTimerInMeters = _patternSpawnTempoInMeters;
        _gateSpawnTimerInMeters = _gateSpawnTempoInMeters;
    }

    public void SwitchBossMode(bool pIsBoss)
    {
        if (pIsBoss && !_isBossMode)
            DestroyObjects();

        _isBossMode = pIsBoss;
    }

    void DestroyObjects()
    {
        while (_objects.Count > 0)
        {
            GameObject vObstacle = _objects.Dequeue();
            if (vObstacle != null)
                Destroy(vObstacle);
        }
    }

    public void OnRotateInput(InputAction.CallbackContext pContext)
    {
        if (pContext.started)
        {
            if (_isBossMode) GameObject.FindGameObjectWithTag("BiomesManager").GetComponent<BiomesManager>().CurrentBoss.GetComponent<IBoss>().DeActivate();
            else GameObject.FindGameObjectWithTag("BiomesManager").GetComponent<BiomesManager>().CurrentBoss.GetComponent<IBoss>().Activate();;
        }
    }
}
