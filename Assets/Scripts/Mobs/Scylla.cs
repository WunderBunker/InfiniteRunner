using System.Collections;
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

    float _hitTime = 1.5f;
    float _hitTimer;

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

        _bossIndicator = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BossIndicator").GetComponent<Slider>();

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
        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(true);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();
    }
    void DeActive()
    {
        if (!_isActive) return;
        _isActive = false;
        _body.SetActive(false);
        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(false);
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
            _bossIndicator.value = 1 - _distanceToPlayer / _maxDistanceToPlayer;
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

        if (_hitTimer > 0)
        {
            _hitTimer -= Time.deltaTime;
            foreach (GameObject lAttack in _attackList)
                if (lAttack != null)
                    foreach (Material lMaterial in lAttack.transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials)
                        lMaterial.SetColor("_BaseColor", lMaterial.GetColor("_Color") + new Color(math.sin(Time.time * 25), 0, 0));

            if (_hitTimer <= 0)
            {
                _hitTimer = 0;
                foreach (GameObject lAttack in _attackList)
                    if (lAttack != null)
                        foreach (Material lMaterial in lAttack.transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials)
                            lMaterial.SetColor("_BaseColor", lMaterial.GetColor("_Color") + new Color(math.sin(Time.time * 25), 0, 0));
            }
        }

        _body.transform.position = new Vector3(_body.transform.position.x, _body.transform.position.y, _playerTransform.position.z - _attackRange - 25);
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Bullet"))
        {
            _life -= 1;
            _hitTimer = _hitTime;
            pOther.GetComponent<Projectile>().Explode();
            if (_life <= 0) DeActive();
        }
    }

}

public enum ScyllaAttackState
{
    preparing, attacking
}