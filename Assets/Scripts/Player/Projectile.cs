using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

//BOULET DE CANON
public class Projectile : MonoBehaviour
{
    public int Direction = 1;
    [SerializeField] float _speedZ;
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
        float vNewSpeed = _speedZ + (Direction > 0 ? GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControls>().CurrentSpeed : 0);
        _range *= vNewSpeed / _speedZ;
        _initSpeed = vNewSpeed;
        _bulletHalfSize = gameObject.GetComponent<MeshFilter>().mesh.bounds.extents.x;
    }

    void Update()
    {
        //Calcul d'un rapport à la disatnce au sol en fonction de la distance parcourue et du range souhaité
        float vCurveCoef = 1 - Tools.InExpo(math.abs(transform.position.z - _ZInit) / _range);

        //La vitesse en Z diminue avec la distance
        _speedZ = (math.max(vCurveCoef, 0) + 0.5f) * _initSpeed;

        //Maj de la position en Z
        transform.position += (_isBouncing ? -1 : 1) * Direction * Vector3.forward * _speedZ * Time.deltaTime;

        //Maj de la position en y
        float vHeight = vCurveCoef * _groundDistance + _groundHeight;
        transform.position = new Vector3(transform.position.x, vHeight, transform.position.z);
        //Crash dans l'eau
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
        float vRangeLeft = _range - math.abs(transform.position.z - _ZInit);
        _range = vRangeLeft * 0.9f;
        _ZInit = transform.position.z;
        _speedZ /= 5;
    }
}
