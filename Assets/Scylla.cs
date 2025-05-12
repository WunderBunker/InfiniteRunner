using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Kraken : MonoBehaviour
{
    [SerializeField] GameObject _preAttack;
    [SerializeField] float _preAttackTime;
    [SerializeField] GameObject _attack;
    [SerializeField] float _attackTime;
    [SerializeField] float _attackTempo;
    float _attackTimer;
    KrakenState _state;

    byte _attackLane;
    LanesManager _laneManager;
    Transform _playerTransform;
    System.Random _random = new();

    List<GameObject> _preAttacksList = new();
    List<GameObject> _attacksList = new();

    float _attackRange;

    bool _isActive = false;
    GameObject _body;

    [SerializeField] float _maxLife;
    float _life;

    void Start()
    {
        _state = KrakenState.waiting;
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _body = transform.GetChild(0).gameObject;
        _body.SetActive(false);
        _attackRange = _attack.GetComponent<MeshFilter>().sharedMesh.bounds.extents.z;
    }

    public void OnRotateInput(InputAction.CallbackContext pContext)
    {
        if (pContext.started)
        {
            if (_isActive) DeActive();
            else Active();
        }
    }


    public void Active()
    {
        if (_isActive) return;
        _isActive = true;
        _body.SetActive(true);
        _life = _maxLife;
        _attackTimer = _attackTempo;
    }
    public void DeActive()
    {
        if (!_isActive) return;
        _isActive = false;
        _body.SetActive(false);

        foreach (GameObject lAttack in _attacksList)
            Destroy(lAttack);
        foreach (GameObject lPreAttack in _preAttacksList)
            Destroy(lPreAttack);
        _attacksList.Clear();
        _preAttacksList.Clear();
    }

    void Update()
    {
        if (!_isActive) return;

        _attackTimer -= Time.deltaTime;

        if (_attackTimer <= 0)
            switch (_state)
            {
                case KrakenState.waiting:
                    //Lancement d'une attaque
                    _attackTimer = _preAttackTime;
                    _state = KrakenState.preparing;

                    _attackLane = (byte)_random.Next(1, _laneManager.LaneNumber);
                    int vAttackNumber = (byte)_random.Next(1, _laneManager.LaneNumber - 1);
                    for (int i = 0; i < vAttackNumber; i++)
                    {
                        GameObject vNewPreAttack = Instantiate(_preAttack, new Vector3((float)_laneManager.GetLaneCenter(_attackLane), _laneManager.GroundHeight, _playerTransform.position.z -_attackRange), Quaternion.identity, transform);
                        _preAttacksList.Add(vNewPreAttack);
                    }

                    break;
                case KrakenState.preparing:
                    //Gestion de l'attaque en préparation
                    _attackTimer = _attackTime;
                    _state = KrakenState.attacking;

                    foreach (GameObject _ in _preAttacksList)
                    {
                        GameObject vNewAttack = Instantiate(_attack, new Vector3((float)_laneManager.GetLaneCenter(_attackLane), _laneManager.GroundHeight, _playerTransform.position.z-_attackRange), Quaternion.identity, transform);
                        _attacksList.Add(vNewAttack);
                    }
                    for (int lCptAttack = 0; lCptAttack < _attacksList.Count; lCptAttack++)
                        Destroy(_preAttacksList[lCptAttack]);
                    _preAttacksList.Clear();
                    break;
                case KrakenState.attacking:
                    _attackTimer = _attackTempo;
                    _state = KrakenState.waiting;

                    for (int lCptAttack = 0; lCptAttack < _attacksList.Count; lCptAttack++)
                        Destroy(_attacksList[lCptAttack]);
                    _attacksList.Clear();

                    break;
                default:
                    break;
            }

        foreach (GameObject lPreAttack in _preAttacksList)
            lPreAttack.transform.position = new Vector3(lPreAttack.transform.position.x, _laneManager.GroundHeight, _playerTransform.position.z-_attackRange);
        foreach (GameObject lAttack in _attacksList)
            lAttack.transform.position = new Vector3(lAttack.transform.position.x, _laneManager.GroundHeight, _playerTransform.position.z -_attackRange);

        _body.transform.position = new Vector3(_body.transform.position.x, _body.transform.position.y, _playerTransform.position.z - 50);
    }


    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Bullet"))
        {
            _life -= 1;
            if (_life <= 0) DeActive();
        }
    }
}

public enum KrakenState
{
    waiting, preparing, attacking
}
