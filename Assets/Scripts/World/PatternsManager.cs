using System;
using System.Collections.Generic;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem;

//GESTION DE L'INSTANCIATION DES PATTERNES D'OBSTCACLES ET ITEMS
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
    bool _hasSpawnRelique;

    float _distFromStopSpawn;
    bool _hasPlayedAlert;


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
        //Si boss en cours rien ne spawn
        if (_isBossMode) return;

        _patternSpawnTimerInMeters -= _player.GetComponent<PlayerManager>().DeltaDistance;
        _gateSpawnTimerInMeters -= _player.GetComponent<PlayerManager>().DeltaDistance;

        //A -10% du spawn des prochains portails on fait une zone de calme avec un boss qui arrive
        if (!_bossHasDefendedGate && _gateSpawnTimerInMeters <= _gateSpawnTempoInMeters * 0.1f)
        {
            //Gestion du boss en fin de biome
            DealWithEndBoss();
            return;
        }

        //A -20% Instanciation d'une relique
        if (!_hasSpawnRelique && _gateSpawnTimerInMeters <= _gateSpawnTempoInMeters * 0.2f) InstanciateRelique();


        //Instanciation de nouveaux obstacles si le timer le requiert
        if (_patternSpawnTimerInMeters <= 0) InstanciateNewPattern();

        //Destruction des vieux patternes si besoin    
        DeleteOldPattern();
    }

    //Gestion du boss en fin de biome
    void DealWithEndBoss()
    {
        //On maj un compteur pour savoir quand on arrive au niveau des derniers obstacles instanciés
        if (_distFromStopSpawn <= 0) _distFromStopSpawn = 1.5f * _spawnDistance;
        _distFromStopSpawn -= _player.GetComponent<PlayerManager>().DeltaDistance;

        //Quand on est presque à ce niveau on affiche le warning
        if (!_hasPlayedAlert && _distFromStopSpawn <= 0.1 * _spawnDistance)
        {
            GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Filters").Find("Alarm").GetComponent<BossFilter>().SetActive(true);
            _hasPlayedAlert = true;
        }
        //arrivé à niveau (un peu plus loin) on fait apparaitre le boss
        if (_distFromStopSpawn <= 0)
        {
            _bossHasDefendedGate = true;
            BiomesManager.Instance.CurrentBoss.GetComponent<Boss>().Activate();
            _hasPlayedAlert = false;
        }
    }

    //Instanciation d'une relique
    void InstanciateRelique()
    {
        GameObject vRelique = BiomesManager.Instance.CurrentRelique;
        //Si relique null (déjà récupérée) on ne fait rien
        if (vRelique == null) return;

        Pattern vCurrentPattern = _objects.Peek().GetComponent<Pattern>();
        bool _isFound = false;

        byte vLine = 0;
        byte vLane = 0;

        Action<byte, byte> vCheckPattern = (byte pLane, byte pLine) =>
        {
            bool lThereIsObject = false;
            foreach (ObjectData lObj in vCurrentPattern.Objects)
            {
                if (lObj.Lane == pLane && lObj.Line == pLine)
                {
                    lThereIsObject = true;
                    break;
                }
            }
            _isFound = !lThereIsObject;
            return;
        };

        int vLoopCounter = 0;
        while (!_isFound)
        {
            vLoopCounter++;
            if (vLoopCounter > 250)
            {
                Debug.Log("boucle infinie patterne");
                return;
            }

            for (vLane = 0; vLane < _laneManager.LaneNumber; vLane++)
            {
                vCheckPattern(vLane, vLine);
                if (_isFound) break;
            }

            vLine++;
        }

        Vector3 vReliquePosition = new Vector3((float)_laneManager.GetLaneCenter(vLane), vRelique.transform.position.y, vCurrentPattern.transform.position.z + vLine * vCurrentPattern.PatternConfig.DistanceBtwLines);

        _objects.Enqueue(Instantiate(vRelique, vReliquePosition, vRelique.transform.localRotation, transform));

        _hasSpawnRelique = true;
    }

    //Instanciation d'un nouveau patterne
    void InstanciateNewPattern()
    {
        Vector3 vPlayerPivotPosition = _player.transform.Find("Pivot").position;

        _patternSpawnTimerInMeters = _patternSpawnTempoInMeters;

        //Si il est temps d'instancier des portails alors on fait ça à la place
        if (_gateSpawnTimerInMeters <= 0)
        {
            _gateSpawnTimerInMeters = _gateSpawnTempoInMeters;
            for (byte lLane = 0; lLane < _laneManager.LaneNumber; lLane++)
                _objects.Enqueue(
                    Instantiate(_gate,
                    new Vector3((float)_laneManager.GetLaneCenter(lLane), _gate.transform.position.y, vPlayerPivotPosition.z + _spawnDistance),
                    Quaternion.identity, transform));
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
    void DeleteOldPattern()
    {
        if (_objects.Count == 0) return;

        GameObject vFirstObj = _objects.Peek();
        Vector3 vPlayerPivotPosition = _player.transform.Find("Pivot").position;

        if (vFirstObj.transform.position.z < vPlayerPivotPosition.z - _patternSpawnTempoInMeters * _numberOfPassedPatternsBfDelete)
        {
            _objects.Dequeue();
            Destroy(vFirstObj);
        }
    }

    //Initialisation des données lors du passage dans un nouveau biome
    public void SetNewBiome(List<GameObject> pPatternsList)
    {
        _patterns = pPatternsList;
        _bossHasDefendedGate = false;
        _hasSpawnRelique = false;
        DestroyObjects();
        _patternSpawnTimerInMeters = _patternSpawnTempoInMeters;
        _gateSpawnTimerInMeters = _gateSpawnTempoInMeters;
    }

    //Passage au mode boss ou non
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

    //Raccourcis pour déclencher manuellement le boss (pour tests)
    public void OnRotateInput(InputAction.CallbackContext pContext)
    {
        if (pContext.started)
        {
            if (_isBossMode) BiomesManager.Instance.CurrentBoss.GetComponent<Boss>().DeActivate();
            else BiomesManager.Instance.CurrentBoss.GetComponent<Boss>().Activate(); ;
        }
    }

}
