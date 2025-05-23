using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Mermaid : MonoBehaviour
{
    [SerializeField] float _chantTime;
    [SerializeField] float _sideMoveLatency;
    [SerializeField] float _blockingPlayerTime;
    [SerializeField] List<AudioClip> _hurtSounds = new();
    [SerializeField] List<AudioClip> _chantSounds = new();

    LanesManager _LM;
    GameObject _player;

    float _chantRadius;

    bool _hasBlocked;
    bool _isMovingLane;
    bool _isChanting;
    float _chantTimer;

    float _targetXPosition;
    Vector3 _SDVelocityRef;
    byte _currentLane;

    ParticleSystem _PS;
    Material _sphereMaterial;
    float _maxAlpha;

    GameObject _body;
    bool _isDead;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");

        _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _currentLane = _LM.GetLaneFromXPos(transform.position.x);

        _chantRadius = _LM.LaneWidth * 1.3f;

        GameObject vSphere = transform.Find("Sphere").gameObject;
        vSphere.transform.localScale = new Vector3(_chantRadius * 2 / transform.localScale.x, _chantRadius * 2 / transform.localScale.y, _chantRadius * 2 / transform.localScale.z);
        vSphere.GetComponent<MeshRenderer>().material = new(vSphere.GetComponent<MeshRenderer>().material);
        _sphereMaterial = vSphere.GetComponent<MeshRenderer>().material;
        _maxAlpha = _sphereMaterial.GetColor("_Color").a;

        _PS = transform.Find("Particles").GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule vShape = _PS.shape;
        vShape.radius = _chantRadius;

        _body = transform.Find("Body").gameObject;

        _chantTimer = _chantTime;
        _isChanting = true;
        _PS.Play();

        AudioManager.Instance.PlaySound(_chantSounds[new System.Random().Next(0, _chantSounds.Count)], 1, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        Color vSphereColor = _sphereMaterial.GetColor("_Color");

        if (_isDead)
        {
            if (vSphereColor.a > 0)
            {
                vSphereColor.a -= Time.deltaTime * _maxAlpha / _sideMoveLatency * 2;
                _sphereMaterial.SetColor("_Color", vSphereColor);
            }
            return;
        }

        if (_isChanting)
        {
            _chantTimer -= Time.deltaTime;

            if (vSphereColor.a < _maxAlpha)
            {
                vSphereColor.a += Time.deltaTime * _maxAlpha / _sideMoveLatency * 2;
                _sphereMaterial.SetColor("_Color", vSphereColor);
            }

            if (!_hasBlocked && Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                                new Vector2(_player.transform.position.x, _player.transform.position.z)) <= _chantRadius)
            {
                _player.GetComponent<playerControls>().BlockLane(_currentLane, _blockingPlayerTime);
                _hasBlocked = true;
            }

            if (_chantTimer <= 0)
            {
                _isChanting = false;
                _hasBlocked = false;

                _PS.Stop();

                System.Random vRan = new();
                byte vNewLane = _currentLane;
                float? vNextLaneX = null;
                int vloopCount = 0;
                while (vNextLaneX == null || vNewLane == _currentLane)
                {
                    vloopCount++;
                    if (vloopCount > 100)
                    {
                        Debug.Log("Boucle infinie mermaid");
                        break;
                    }

                    vNewLane = (byte)(_currentLane + vRan.Next(-1, 2));
                    vNextLaneX = _LM.GetLaneCenter(vNewLane);
                }

                //On retourne le corps sur l'axe Y pour être dans le sens de la direction
                _body.transform.localScale = new Vector3(math.abs(_body.transform.localScale.x) * (_currentLane - vNewLane), _body.transform.localScale.y, _body.transform.localScale.z);
                _body.GetComponent<Animator>().SetTrigger("Move");

                _currentLane = vNewLane;
                _targetXPosition = (float)vNextLaneX;
                _isMovingLane = true;
            }
        }
        else if (_isMovingLane)
        {
            if (vSphereColor.a > 0)
            {
                vSphereColor.a -= Time.deltaTime * _maxAlpha / _sideMoveLatency * 2;
                _sphereMaterial.SetColor("_Color", vSphereColor);
            }

            Vector3 vTempTargetPosition = new Vector3(_targetXPosition, transform.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, vTempTargetPosition,
                    ref _SDVelocityRef, _sideMoveLatency);

            if (Mathf.Abs(transform.position.x - _targetXPosition) < 0.05f)
            {
                transform.position = vTempTargetPosition;
                _isMovingLane = false;
                _isChanting = true;
                _chantTimer = _chantTime;
                _PS.Play();
                AudioManager.Instance.PlaySound(_chantSounds[new System.Random().Next(0, _chantSounds.Count)], 1, transform.position);
            }
        }
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (_isDead) return;

        if (pOther.CompareTag("Player")) pOther.GetComponent<PlayerManager>().Hurt(1);
        else if (pOther.CompareTag("Bullet"))
        {
            pOther.gameObject.GetComponent<Projectile>().Explode();
            StartCoroutine(Death());
        }
    }

    IEnumerator Death()
    {
        _isDead = true;
        AudioManager.Instance.PlaySound(_hurtSounds[new System.Random().Next(0, _hurtSounds.Count)], 1, transform.position);
        _body.GetComponent<Animator>().SetTrigger("Death");
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(_body.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        Destroy(gameObject);
    }
}
