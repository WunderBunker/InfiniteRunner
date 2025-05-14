using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Scylla : MonoBehaviour
{
    [SerializeField] float _maxDistanceToPlayer;
    [SerializeField] GameObject _attack;
    [SerializeField] float _attackTempo;
    [SerializeField] float _maxLife;

    float _attackTimer;

    LanesManager _laneManager;
    Transform _playerTransform;
    playerControls _playerControls;
    System.Random _random = new();
    List<GameObject> _attackList = new();

    bool _isActive = false;
    GameObject _body;

    float _life;
    float _attackRange;

    float _speed;
    float _distanceToPlayer;
    Slider _bossIndicator;

    void Start()
    {
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _playerControls = _playerTransform.GetComponent<playerControls>();

        _body = transform.GetChild(0).gameObject;
        _body.SetActive(false);
        _attackRange = _attack.GetComponent<ScyllaAttack>().GetAttackRange();

        _distanceToPlayer = _maxDistanceToPlayer;
        _speed = _playerControls.CurrentMaxSpeed / 2;

        _bossIndicator =GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BossIndicator").GetComponent<Slider>();
    }

    public void OnRotateInput(InputAction.CallbackContext pContext)
    {
        if (pContext.started)
        {
            if (_isActive) DeActive();
            else Active();
        }
    }

    void Active()
    {
        if (_isActive) return;
        _isActive = true;
        _body.SetActive(true);
        _life = _maxLife;
        _attackTimer = _attackTempo;
        GameObject.FindGameObjectWithTag("ObstaclesManager").GetComponent<ObstaclesManager>().SwitchBossMode(true);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();
    }
    void DeActive()
    {
        if (!_isActive) return;
        _isActive = false;
        _body.SetActive(false);
        GameObject.FindGameObjectWithTag("ObstaclesManager").GetComponent<ObstaclesManager>().SwitchBossMode(false);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();
        foreach (GameObject lAttack in _attackList) Destroy(lAttack);
        _attackList.Clear();
    }

    void Update()
    {
        if (!_isActive)
        {
            _speed = _playerControls.CurrentMaxSpeed / 2;
            _distanceToPlayer = math.clamp(_distanceToPlayer - (_speed - _playerControls.CurrentSpeed) * Time.deltaTime, 0, _maxDistanceToPlayer);
            _bossIndicator.value = 1-_distanceToPlayer/_maxDistanceToPlayer;
            if (_distanceToPlayer == 0) Active();
            return;
        }

        _attackTimer -= Time.deltaTime;

        if (_attackTimer <= 0)
        {
            _attackTimer = _attackTempo;
            int vAttackNumber = (byte)_random.Next(1, _laneManager.LaneNumber);
            byte vAttackLane = 0;
            byte vNextLane = vAttackLane;
            for (int i = 0; i < vAttackNumber; i++)
            {
                while (vNextLane == vAttackLane)
                    vNextLane = (byte)_random.Next(0, _laneManager.LaneNumber);
                vAttackLane = vNextLane;

                _attackList.Add(Instantiate(_attack, new Vector3((float)_laneManager.GetLaneCenter(vAttackLane), _laneManager.GroundHeight, transform.position.z), Quaternion.identity, transform));
            }
        }

        _body.transform.position = new Vector3(_body.transform.position.x, _body.transform.position.y, _playerTransform.position.z - _attackRange - 25);
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

public enum ScyllaAttackState
{
    preparing, attacking
}