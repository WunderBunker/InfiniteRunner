using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int Direction = 1;
    [SerializeField] float _speed;
    [SerializeField] float _range;
    [SerializeField] List<AudioClip> _splashSounds = new();
    [SerializeField] GameObject _splash;

    float _groundDistance;
    float _groundHeight;
    float _bulletHalfSize;

    float _ZInit;
    float _initSpeed;
    bool _isBouncing;

    void Start()
    {
        _groundHeight = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>().GroundHeight;
        _groundDistance = math.abs(transform.position.y - _groundHeight);
        _groundHeight *= 1.2f;
        _ZInit = transform.position.z;
        float vNewSpeed = _speed + (Direction > 0 ? GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControls>().CurrentSpeed : 0);
        _range *= vNewSpeed / _speed;
        _initSpeed = vNewSpeed;
        _bulletHalfSize = gameObject.GetComponent<MeshFilter>().mesh.bounds.extents.x;
    }
    void Update()
    {
        float vCurveCoef = 1 - Tools.InExpo(math.abs(transform.position.z - _ZInit) / _range);

        _speed = (math.max(vCurveCoef, 0) + 0.5f) * _initSpeed;

        transform.position += (_isBouncing ? -1 : 1) * Direction * Vector3.forward * _speed * Time.deltaTime;

        float vHeight = vCurveCoef * _groundDistance + _groundHeight;
        transform.position = new Vector3(transform.position.x, vHeight, transform.position.z);
        if (vHeight + _bulletHalfSize < _groundHeight)
        {
            AudioManager.Instance.PlaySound(_splashSounds[new System.Random().Next(0, _splashSounds.Count)], 1);
            Instantiate(_splash, transform.position, _splash.transform.rotation, transform.parent);
            Explode();
        }
    }

    public void Explode()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        _isBouncing = !_isBouncing;
        float vRangeLeft = math.abs(transform.position.z - _ZInit);
        _range = _range - vRangeLeft * 0.95f;
        _speed /= 5;
    }


}
