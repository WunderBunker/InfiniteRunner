using System.Numerics;
using UnityEngine;

public class WaterPlanes : MonoBehaviour
{
    Transform _playerTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform.Find("Pivot");
        transform.position = new UnityEngine.Vector3(transform.position.x, _playerTransform.position.y, _playerTransform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y, _playerTransform.position.z);
    }
}
