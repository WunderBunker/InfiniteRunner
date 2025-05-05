using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundsManager : MonoBehaviour
{
    [SerializeField] GameObject _groundPrefab;
    [SerializeField] int _numberOfPassedGroundBfDelete;
    [SerializeField] int _numberOfGroundToInit;

    Queue<GameObject> _grounds = new();
    Vector3 _groundSize;

    Transform _playertransform;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _playertransform = GameObject.FindGameObjectWithTag("Player").transform.Find("Pivot");

        _groundSize = GetGroundSize();


        GameObject vNewGround = Instantiate(_groundPrefab, _playertransform.position, Quaternion.identity, transform);
        _grounds.Enqueue(vNewGround);
        for (int lCptGround = 1; lCptGround <= _numberOfGroundToInit; lCptGround++)
            AddGroundInFront();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject vFirstGround = _grounds.Peek();
        if (vFirstGround.transform.position.z < _playertransform.position.z - _groundSize.z * _numberOfPassedGroundBfDelete)
        {
            _grounds.Dequeue();
            Destroy(vFirstGround);
            AddGroundInFront();
        }
    }

    void AddGroundInFront()
    {
        Vector3 vPosition = _grounds.Last().transform.position + _groundSize.z * Vector3.forward;
        GameObject vNewGround = Instantiate(_groundPrefab, vPosition, Quaternion.identity, transform);
        _grounds.Enqueue(vNewGround);
    }

    public Vector3 GetGroundSize()
    {
        Vector3 vGroundSize = Vector3.Cross(_groundPrefab.transform.localScale, _groundPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.size);
        vGroundSize = new Vector3(Mathf.Abs(vGroundSize.x), Mathf.Abs(vGroundSize.y), Mathf.Abs(vGroundSize.z));
        return vGroundSize;
    }
}
