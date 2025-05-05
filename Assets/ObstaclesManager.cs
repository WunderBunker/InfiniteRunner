using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstaclesManager : MonoBehaviour
{
    [SerializeField] List<GameObject> _obstaclesPatterns = new();
    [SerializeField] float _spawnTempo;
    [SerializeField] float _spawnDistance;
    [SerializeField] int _numberOfPassedPatternsBfDelete;
    float _spawnTimer;
    System.Random _spawnRandom = new();
    Transform _playerTransform;
    Queue<GameObject> _obstacles = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _spawnTimer = _spawnTempo;
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform.Find("Pivot");
    }

    // Update is called once per frame
    void Update()
    {
        _spawnTimer -= Time.deltaTime;

        //instantition de nouveau obstacles si le timer le requiert
        if (_spawnTimer <= 0)
        {
            _spawnTimer = _spawnTempo;
            GameObject vPattern = _obstaclesPatterns[_spawnRandom.Next(0, _obstaclesPatterns.Count)];
            GameObject vNewPattern = Instantiate(vPattern, new Vector3(vPattern.transform.position.x, _playerTransform.position.y, _playerTransform.position.z + _spawnDistance), Quaternion.identity, transform);
            vNewPattern.GetComponent<ObstaclesPattern>().InitPattern();
            vNewPattern.GetComponent<ObstaclesPattern>().ApplyPattern();
            _obstacles.Enqueue(vNewPattern);
        }

        //Destruction des veiux patternes si besoin    
        if (_obstacles.Count > 0)
        {
            GameObject vFirstObst = _obstacles.Peek();
            if (vFirstObst.transform.position.z < _playerTransform.position.z - vFirstObst.GetComponent<ObstaclesPattern>().Length * _numberOfPassedPatternsBfDelete)
            {
                _obstacles.Dequeue();
                Destroy(vFirstObst);
            }
        }
    }
}
