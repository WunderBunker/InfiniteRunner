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
    [SerializeField] AudioClip _shieldSound;
    [SerializeField] Color _shieldColor;


    float _initMaxDistanceToPLayer;

    float _attackTimer;
    float _attack2Timer;

    LanesManager _laneManager;
    Transform _playerTransform;
    playerControls _playerControls;
    FiltreAlerte _filtreAlerte;
    System.Random _random = new();
    List<GameObject> _attackList = new();

    bool _isActive = false;
    bool _isBeaten = false;
    GameObject _body;

    float _life;
    float _attackRange;

    float _speed;
    float _distanceToPlayer;
    Slider _bossIndicator;

    bool _shieldFlicker;
    private bool _isHit;

    void Start()
    {
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _playerControls = _playerTransform.GetComponent<playerControls>();
        _filtreAlerte = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Filtre").GetComponent<FiltreAlerte>();

        _body = transform.GetChild(0).gameObject;
        _body.SetActive(false);
        _attackRange = _attack.GetComponent<ScyllaAttack>().GetAttackRange();

        _distanceToPlayer = _maxDistanceToPlayer;
        _speed = _playerControls.CurrentMaxSpeed / 2;
        _initMaxDistanceToPLayer = _maxDistanceToPlayer;

        _bossIndicator = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BossIndicator").GetComponent<Slider>();
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
            _body.transform.GetChild(lCptChild).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Emission", _shieldColor);
        }

        _life = _maxLife;
        _attackTimer = _attackTempo;
        _attack2Timer = _attack2Tempo / 2;
        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(true);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();

        _filtreAlerte.SetActive(false);
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

        _filtreAlerte.SetActive(false);
    }

    void Update()
    {
        if (!_isActive)
        {
            //On pondère la dstance max en fonction de la vitesse max du joueur (pour garder des temps de rattrappage identiques durant la partie)
            _maxDistanceToPlayer = _initMaxDistanceToPLayer * _playerControls.CurrentMaxSpeed / _playerControls.MaxSpeedInitial;

            _speed = _playerControls.CurrentMaxSpeed * 0.7f;
            float vNewDistanceToPlayer = math.clamp(_distanceToPlayer - (_speed - _playerControls.CurrentSpeed) * Time.deltaTime, 0, _maxDistanceToPlayer);

            if (vNewDistanceToPlayer <= _maxDistanceToPlayer / 4 && _distanceToPlayer > _maxDistanceToPlayer / 4) _filtreAlerte.SetActive(true);
            else if (_distanceToPlayer <= _maxDistanceToPlayer / 4 && vNewDistanceToPlayer > _maxDistanceToPlayer / 4) _filtreAlerte.SetActive(false);

            _distanceToPlayer = vNewDistanceToPlayer;
            _bossIndicator.value = 1 - _distanceToPlayer / _maxDistanceToPlayer;
            if (_distanceToPlayer == 0) Activate();
            return;
        }

        if (_isHit)
        {
            Color vBodyColor = _body.transform.GetChild(0).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().materials[0].GetColor("_Emission");
            if (vBodyColor.a > 0.1f)
            {
                vBodyColor.a -= Time.deltaTime * 0.35f;
                if (vBodyColor.a <= 0.1f) _isHit = false;
            }
            else _isHit = false;

            Color vAttackColor = new Color(1, 0, 0, vBodyColor.a);

            foreach (GameObject lAttack in _attackList)
                if (lAttack != null)
                {
                    Material lMat = lAttack.transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials[0];
                    lMat.SetColor("_Emission", vAttackColor);
                }
            for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
                _body.transform.GetChild(lCptChild).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().sharedMaterials[0].SetColor("_Emission", vBodyColor);
        }

        if (_shieldFlicker)
        {
            Color vShieldColor = _body.transform.GetChild(0).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().materials[0].GetColor("_Emission");
            if (vShieldColor.a > 0.1f)
            {
                vShieldColor.a -= Time.deltaTime * 0.35f;
                if (vShieldColor.a <= 0.2f) _shieldFlicker = false;
            }
            else _shieldFlicker = false;

            for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
                _body.transform.GetChild(lCptChild).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().sharedMaterials[0].SetColor("_Emission", vShieldColor);
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
            _isHit = true;

            pOther.GetComponent<Projectile>().Explode();

            AudioManager.Instance.PlaySound(_hurtSounds[new System.Random().Next(0, _hurtSounds.Count)], 1);

            Color vCol = new Color(1, 0, 0, 0.9f);
            foreach (GameObject lAttack in _attackList)
                if (lAttack != null)
                {
                    Material lMat = lAttack.transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials[0];
                    lMat.SetColor("_Emission", vCol);
                }
            for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
                _body.transform.GetChild(lCptChild).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().sharedMaterials[0].SetColor("_Emission", vCol);

            if (_life == 0) StartCoroutine(IsBeaten());
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Le bouclier s'active et le bouelt rebondit
        if (collision.gameObject.CompareTag("Bullet"))
        {
            _shieldFlicker = true;
            Color vColor = _shieldColor;
            vColor.a = 0.8f;
            for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
                _body.transform.GetChild(lCptChild).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().sharedMaterials[0].SetColor("_Emission", vColor);

            AudioManager.Instance.PlaySound(_shieldSound, 1);
        }
    }

    IEnumerator IsBeaten()
    {
        _isBeaten = true;
        for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
            if (_body.transform.GetChild(lCptChild).GetComponent<Animator>() != null) _body.transform.GetChild(lCptChild).GetComponent<Animator>().SetTrigger("Beaten");
        yield return new WaitForSeconds(0.2f);
        float vAnimTime = _body.transform.GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        AudioManager.Instance.PlaySound(_deathSound, 1);
        yield return new WaitForSeconds(vAnimTime);
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