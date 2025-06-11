using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Scylla : Boss
{
    [SerializeField] float _maxDistanceToPlayer;
    [SerializeField] GameObject _attack;
    [SerializeField] GameObject _attack2;
    [SerializeField] float _attackTempo;
    [SerializeField] float _attack2Tempo;
    [SerializeField] List<AudioClip> _hurtSounds = new();
    [SerializeField] AudioClip _deathSound;
    [SerializeField] AudioClip _shieldSound;
    [SerializeField] Color _shieldColor;
    [SerializeField] Color _hurtColor;
    [SerializeField] Color _attackColor;

    float _initMaxDistanceToPLayer;

    float _attackTimer;
    float _attack2Timer;

    PlayerControls _playerControls;
    BossFilter _filterAlarm;
    System.Random _random = new();

    float _attackRange;

    float _speed;
    float _distanceToPlayer;
    Slider _bossIndicator;

    bool _shieldFlicker;
    private bool _isHit;

    override protected void Start()
    {
        base.Start();

        _playerControls = _playerTransform.GetComponent<PlayerControls>();
        _filterAlarm = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Filters").Find("Alarm").GetComponent<BossFilter>();

        _attackRange = _attack.GetComponent<ScyllaAttack>().GetAttackRange();

        _distanceToPlayer = _maxDistanceToPlayer;
        _speed = _playerControls.CurrentMaxSpeed / 2;
        _initMaxDistanceToPLayer = _maxDistanceToPlayer;

        _bossIndicator = _indicator.GetComponent<Slider>();
    }

    public override void Activate()
    {
        base.Activate();

        for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
        {
            if (_body.transform.GetChild(lCptChild).GetComponent<Animator>() == null) continue;
            _body.transform.GetChild(lCptChild).GetComponent<Animator>().SetBool("Stopped", false);
            _body.transform.GetChild(lCptChild).GetComponent<Animator>().SetInteger("Head", lCptChild);
            _body.transform.GetChild(lCptChild).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Emission", new Color(_shieldColor.r, _shieldColor.g, _shieldColor.b, 0));
        }

        _attackTimer = _attackTempo;
        _attack2Timer = _attack2Tempo / 2;

        _filterAlarm.SetActive(false);
    }
    public override void DeActivate()
    {
        base.DeActivate();

        for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
            if (_body.transform.GetChild(lCptChild).GetComponent<Animator>() != null) _body.transform.GetChild(lCptChild).GetComponent<Animator>().SetBool("Stopped", true);

        foreach (GameObject lAttack in _attackList) Destroy(lAttack);
        _attackList.Clear();
        _distanceToPlayer = _maxDistanceToPlayer;

        _filterAlarm.SetActive(false);
    }

    override protected void Update()
    {
        base.Update();

        if (!_isActive)
        {
            //On pondère la dstance max en fonction de la vitesse max du joueur (pour garder des temps de rattrappage identiques durant la partie)
            _maxDistanceToPlayer = _initMaxDistanceToPLayer * _playerControls.CurrentMaxSpeed / _playerControls.MaxSpeedInitial;

            _speed = _playerControls.CurrentMaxSpeed * 0.7f;
            float vNewDistanceToPlayer = math.clamp(_distanceToPlayer - (_speed - _playerControls.CurrentSpeed) * Time.deltaTime, 0, _maxDistanceToPlayer);

            if (vNewDistanceToPlayer <= _maxDistanceToPlayer / 4 && _distanceToPlayer > _maxDistanceToPlayer / 4) _filterAlarm.SetActive(true);
            else if (_distanceToPlayer <= _maxDistanceToPlayer / 4 && vNewDistanceToPlayer > _maxDistanceToPlayer / 4) _filterAlarm.SetActive(false);

            _distanceToPlayer = vNewDistanceToPlayer;
            _bossIndicator.value = 1 - _distanceToPlayer / _maxDistanceToPlayer;
            if (_distanceToPlayer == 0)
            {
                _hasBeenTriggered = true;
                Activate();
            }
            return;
        }

        if (_isHit)
        {
            Color vBodyColor = _body.transform.GetChild(0).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().materials[0].GetColor("_Emission");
            if (vBodyColor.a > 0)
            {
                vBodyColor.a -= Time.deltaTime * 0.35f;
                if (vBodyColor.a <= 0.1f) { _isHit = false; vBodyColor.a = 0; }
            }
            else _isHit = false;

            Color vAttackHurtColor = new Color(_hurtColor.r, _hurtColor.g, _hurtColor.b, vBodyColor.a);
            foreach (GameObject lAttack in _attackList)
                if (lAttack != null)
                {
                    Material lMat = lAttack.transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials[0];
                    lMat.SetColor("_Emission", vAttackHurtColor);
                }
            for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
                _body.transform.GetChild(lCptChild).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().sharedMaterials[0].SetColor("_Emission", vBodyColor);
        }

        if (_shieldFlicker)
        {
            Color vShieldColor = _body.transform.GetChild(0).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().materials[0].GetColor("_Emission");
            if (vShieldColor.a > 0f)
            {
                vShieldColor.a -= Time.deltaTime * 0.25f;
                if (vShieldColor.a <= 0.1f) _shieldFlicker = false;
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
            int vAttackNumber = (byte)_random.Next(1, _LM.LaneNumber);
            byte vAttackLane = 0;
            byte vNextLane = vAttackLane;
            for (int i = 0; i < vAttackNumber; i++)
            {
                while (vNextLane == vAttackLane)
                    vNextLane = (byte)_random.Next(0, _LM.LaneNumber);
                vAttackLane = vNextLane;

                _attackList.Add(Instantiate(_attack, new Vector3((float)_LM.GetLaneCenter(vAttackLane), _LM.GroundHeight, transform.position.z), Quaternion.identity, transform));
                _attackList[_attackList.Count - 1].GetComponent<ScyllaAttack>().AttackColor = _attackColor;
            }
        }
        if (_attack2Timer <= 0)
        {
            _attack2Timer = _attack2Tempo;
            int vAttackNumber = (byte)_random.Next(1, _LM.LaneNumber - 1);
            byte vAttackLane = 0;
            byte vNextLane = vAttackLane;
            for (int i = 0; i < vAttackNumber; i++)
            {
                while (vNextLane == vAttackLane)
                    vNextLane = (byte)_random.Next(0, _LM.LaneNumber);
                vAttackLane = vNextLane;

                _attackList.Add(Instantiate(_attack2, new Vector3((float)_LM.GetLaneCenter(vAttackLane), _LM.GroundHeight, transform.position.z), Quaternion.identity, transform));
                _attackList[_attackList.Count - 1].GetComponent<ScyllaAttack2>().AttackColor = _attackColor;
            }
        }

        _body.transform.position = new Vector3(_body.transform.position.x, _body.transform.position.y, _playerTransform.position.z - _attackRange - 5);
    }

    override protected void OnHit(Projectile pBullet)
    {
        base.OnHit(pBullet);
        AudioManager.Instance.PlaySound(_hurtSounds[new System.Random().Next(0, _hurtSounds.Count)], 1);
        Color vCol = new Color(_hurtColor.r, _hurtColor.g, _hurtColor.b, 0.9f);
        foreach (GameObject lAttack in _attackList)
            if (lAttack != null)
            {
                Material lMat = lAttack.transform.Find("Snake").Find("Skin").GetComponent<SkinnedMeshRenderer>().materials[0];
                lMat.SetColor("_Emission", vCol);
            }
        for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
            _body.transform.GetChild(lCptChild).Find("Cylinder").GetComponent<SkinnedMeshRenderer>().sharedMaterials[0].SetColor("_Emission", vCol);
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

    override protected IEnumerator IsBeaten()
    {
        yield return base.IsBeaten();

        for (int lCptChild = 0; lCptChild < _body.transform.childCount; lCptChild++)
            if (_body.transform.GetChild(lCptChild).GetComponent<Animator>() != null) _body.transform.GetChild(lCptChild).GetComponent<Animator>().SetTrigger("Beaten");
        yield return new WaitForSeconds(0.2f);
        float vAnimTime = _body.transform.GetChild(0).GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        AudioManager.Instance.PlaySound(_deathSound, 1);
        yield return new WaitForSeconds(vAnimTime);
        DeActivate();
    }
}


public enum AttackState
{
    preparing, attacking
}
