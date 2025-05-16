using System.Collections.Generic;
using UnityEngine;

public class PatternsManager : MonoBehaviour
{
    [SerializeField] List<Biome> _biomes = new();
    [SerializeField] float _spawnTempoInMeters;
    [SerializeField] float _spawnDistance;
    [SerializeField] int _numberOfPassedPatternsBfDelete;
    float _spawnTimerInMeters;
    System.Random _spawnRandom = new();
    GameObject _player;
    Queue<GameObject> _objects = new();
    BiomesManager _biomesManager;

    bool _isBossMode = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _spawnTimerInMeters = 0;
        _player = GameObject.FindGameObjectWithTag("Player");
        _biomesManager = GameObject.FindGameObjectWithTag("BiomesManager").GetComponent<BiomesManager>();
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

            //On sélectionne un nouveau patterne parmis ceux du biome actuel
            GameObject vPattern = null;
            foreach (Biome lBiome in _biomes)
                if (lBiome.BiomeId == _biomesManager.CurrentBiomeId)
                {
                    vPattern = lBiome.Patterns[_spawnRandom.Next(0, lBiome.Patterns.Count)];
                    break;
                }

            Vector3 vPatternPosition = new Vector3(vPattern.transform.position.x, vPlayerPivotPosition.y, vPlayerPivotPosition.z + _spawnDistance);
            GameObject vNewPattern = Instantiate(vPattern, vPatternPosition, Quaternion.identity, transform);
            vNewPattern.GetComponent<Pattern>().MajPattern();
            _objects.Enqueue(vNewPattern);
        }

        //Destruction des vieux patternes si besoin    
        if (_objects.Count > 0)
        {
            GameObject vFirstObj = _objects.Peek();
            if (vFirstObj.transform.position.z < vPlayerPivotPosition.z - vFirstObj.GetComponent<Pattern>().Length * _numberOfPassedPatternsBfDelete)
            {
                _objects.Dequeue();
                Destroy(vFirstObj);
            }
        }
    }

    public void SwitchBossMode(bool pIsBoss)
    {
        if (pIsBoss && !_isBossMode)
            while (_objects.Count > 0)
            {
                GameObject vObstacle = _objects.Dequeue();
                if (vObstacle != null)
                    Destroy(vObstacle);
            }

        _isBossMode = pIsBoss;
    }
}
