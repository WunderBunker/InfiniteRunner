using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Scylla : MonoBehaviour, IBoss
{
    [SerializeField] float _maxDistanceToPlayer;
    [SerializeField] GameObject _attack;
    [SerializeField] GameObject _attack2;
    [SerializeField] float _attackTempo;
    [SerializeField] float _attack2Tempo;
    [SerializeField] float _maxLife;
    [SerializeField] List<AudioClip> _hurtSounds = new();
    [SerializeField] AudioClip _deathSound;

    float _initMaxDistanceToPLayer;

    float _attackTimer;
    float _attack2Timer;

    LanesManager _laneManager;
    Transform _playerTransform;
    playerControls _playerControls;
    System.Random _random = new();
    List<GameObject> _attackList = new();
    MeshRenderer _shieldPlan;

    bool _isActive = false;
    bool _isBeaten = false;
    GameObject _body;

    float _life;
    float _attackRange;

    float _speed;
    float _distanceToPlayer;
    Slider _bossIndicator;

    float _hitTime = 1.5f;
    float _hitTimer;

    float _shieldFlickerTimer;

    AudioManager _AM;

    void Start()
    {
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _playerControls = _playerTransform.GetComponent<playerControls>();

        _body = transform.GetChild(0).gameObject;
        _body.SetActive(false);
        _attackRange = _attack.GetComponent<ScyllaAttack>().GetAttackRange();

        _shieldPlan = _body.transform.Find("Shield").GetComponent<MeshRenderer>();

        _distanceToPlayer = _maxDistanceToPlayer;
        _speed = _playerControls.CurrentMaxSpeed / 2;
        _initMaxDistanceToPLayer = _maxDistanceToPlayer;

        _bossIndicator = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BossIndicator").GetComponent<Slider>();

        _AM = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }

    public void Activate()
    {
        if (_isActive) return;
        _isActive = true;

        _body.SetActive(true);
        for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
        {
            if (_body.transform.GetChild(lCptChild).GetComponent<Animator>() == null) continue;
            _body.transform.GetChild(lCptChild).GetComponent<Animator>().SetBool("Stopped", false);
            _body.transform.GetChild(lCptChild).GetComponent<Animator>().SetInteger("Head", lCptChild);
        }

        _life = _maxLife;
        _attackTimer = _attackTempo;
        _attack2Timer = _attack2Tempo/2;
        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(true);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();
    }
    public void DeActivate()
    {
        if (!_isActive) return;
        _isActive = false;
        _body.SetActive(false);

        for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
            if (_body.transform.GetChild(lCptChild).GetComponent<Animator>() != null) _body.transform.GetChild(lCptChild).GetComponent<Animator>().SetBool("Stopped", true);

        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(false);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();
        foreach (GameObject lAttack in _attackList) Destroy(lAttack);
        _attackList.Clear();
        _distanceToPlayer = _maxDistanceToPlayer;
        _isBeaten = false;
    }

    void Update()
    {
        if (!_isActive)
        {
            //On pondère la dstance max en fonction de la vitesse max du joueur (pour garder des temps de rattrappage identiques durant la partie)
            _maxDistanceToPlayer = _initMaxDistanceToPLayer * _playerControls.CurrentMaxSpeed / _playerControls.MaxSpeedInitial;

            _speed = _playerControls.CurrentMaxSpeed * 0.7f;
            _distanceToPlayer = math.clamp(_distanceToPlayer - (_speed - _playerControls.CurrentSpeed) * Time.deltaTime, 0, _maxDistanceToPlayer);
            _bossIndicator.value = 1 - _distanceToPlayer / _maxDistanceToPlayer;
            if (_distanceToPlayer == 0) Activate();
            return;
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

        if (_shieldPlan.enabled)
        {
            _shieldFlickerTimer -= Time.deltaTime;

            Color vPlanColor = _shieldPlan.material.GetColor("_Color");
            if (_shieldFlickerTimer <= 0)
            {
                if (vPlanColor.a > 0) vPlanColor.a -= Time.deltaTime * 5;
                else
                {
                    vPlanColor.a = 0.8f;
                    _shieldPlan.enabled = false;
                }
            }
            else vPlanColor.a = math.clamp(vPlanColor.a + Time.deltaTime * math.sin(Time.time * 20) * 5, 0.1f, 0.8f);
            _shieldPlan.material.SetColor("_Color", vPlanColor);
        }

        if (_isBeaten) return;

        _attackTimer -= Time.deltaTime;
        _attack2Timer -= Time.deltaTime;

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
        if (_attack2Timer <= 0)
        {
            _attack2Timer = _attack2Tempo;
            int vAttackNumber = (byte)_random.Next(1, _laneManager.LaneNumber - 1);
            byte vAttackLane = 0;
            byte vNextLane = vAttackLane;
            for (int i = 0; i < vAttackNumber; i++)
            {
                while (vNextLane == vAttackLane)
                    vNextLane = (byte)_random.Next(0, _laneManager.LaneNumber);
                vAttackLane = vNextLane;

                _attackList.Add(Instantiate(_attack2, new Vector3((float)_laneManager.GetLaneCenter(vAttackLane), _laneManager.GroundHeight, transform.position.z), Quaternion.identity, transform));
            }
        }

        _body.transform.position = new Vector3(_body.transform.position.x, _body.transform.position.y, _playerTransform.position.z - _attackRange - 5);
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Bullet"))
        {
            if (_life == 0) return;
            _life -= 1;
            _hitTimer = _hitTime;
            pOther.GetComponent<Projectile>().Explode();
            _AM.PlaySound(_hurtSounds[new System.Random().Next(0, _hurtSounds.Count)], 1);
            if (_life == 0) StartCoroutine(IsBeaten());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            _shieldFlickerTimer = 2;
            _shieldPlan.enabled = true;
        }
    }

    IEnumerator IsBeaten()
    {
        _isBeaten = true;
        for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
            if (_body.transform.GetChild(lCptChild).GetComponent<Animator>() != null) _body.transform.GetChild(lCptChild).GetComponent<Animator>().SetTrigger("Beaten");
        yield return new WaitForSeconds(0.2f);
        float vAnimTime = _body.transform.GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        _AM.PlaySound(_deathSound, 1);
        yield return new WaitForSeconds(vAnimTime + 0.5f);
        DeActivate();
    }
}


public enum ScyllaAttackState
{
    preparing, attacking
}

interface IBoss
{
    public void Activate();
    public void DeActivate();
}