using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charon : MonoBehaviour, IBoss
{
    [SerializeField] GameObject _attack;
    [SerializeField] GameObject _attack2;
    [SerializeField] float _attackTempo;
    [SerializeField] float _attack2Tempo;
    [SerializeField] float _maxLife;
    [SerializeField] int _nbAttackBfExposed;
    [SerializeField] int _nbAttackBfHidden;
    [SerializeField] float _sideMoveLatency;

    int _attackCounter;

    bool _isActive = false;
    GameObject _body;
    Transform _playerTransform;
    Animator _animator;

    float _attackTimer;
    LanesManager _laneManager;
    System.Random _random = new();
    List<GameObject> _attackList = new();
    Transform _attacksParent;
    Transform _bodyOrigin;

    bool _isBeaten = false;
    float _life;

    bool _isExposed;

    float _attackRange;

    LanesManager _LM;
    float _targetXPosition;
    Vector3 _SDVelocityRef;
    byte _currentLane;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _body = transform.GetChild(0).gameObject;
        _body.SetActive(false);

        _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _bodyOrigin = transform.Find("Origin");

        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _animator = transform.Find("Body").GetComponent<Animator>();
        _attackRange = _attack.GetComponent<CharonAttack>().GetAttackRange();
    }

    public void Activate()
    {
        if (_isActive) return;
        _isActive = true;
        _isBeaten = false;

        _currentLane = _LM.GetLaneFromXPos(transform.position.x + _bodyOrigin.position.x);
        _targetXPosition = (float)_LM.GetLaneCenter(_currentLane) + _bodyOrigin.position.x;

        _life = _maxLife;

        _attacksParent = new GameObject().transform;

        _body.SetActive(true);

        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(true);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();

        _isExposed = false;
        _attackCounter = _nbAttackBfExposed;
        //Juste le temps que e beteau se retourne
        _attackTimer = _attackTempo / 4;

    }
    public void DeActivate()
    {
        if (!_isActive) return;
        _isActive = false;
        _body.SetActive(false);

        Destroy(_attacksParent.gameObject);

        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(false);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();
    }

    void Update()
    {
        if (!_isActive) return;
        if (_isBeaten) return;

        _attackTimer -= Time.deltaTime;
        //Lance une attaque
        if (_attackTimer <= 0)
        {
            //Attaque seconaire (quand il est exposé)
            if (_isExposed)
            {
                _attackTimer = _attack2Tempo;
                _animator.SetTrigger("Attack");

                StartCoroutine(WaitForAttack());
            }
            //Attaque princpale (quand il est caché)
            else
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

                    _attackList.Add(Instantiate(_attack, new Vector3((float)_laneManager.GetLaneCenter(vAttackLane), _laneManager.GroundHeight, _playerTransform.position.z), Quaternion.identity, _attacksParent));
                }
            }

            _attackCounter--;
            if (_attackCounter <= 0)
            {
                _isExposed = !_isExposed;
                if (_isExposed)
                {
                    _attackCounter = _nbAttackBfHidden;
                    _animator.SetTrigger("Rise");
                }
                else
                {
                    _attackCounter = _nbAttackBfExposed;
                    _animator.SetTrigger("Sink");
                }
            }
        }

        //Maj de la position en Z
        transform.position = new Vector3(transform.position.x, transform.position.y, _playerTransform.position.z - _attackRange - 10);
        _attacksParent.position = new Vector3(0, 0, transform.position.z);

        //Maj du changement de lane;
        if (_isExposed)
        {
            Vector3 vTempTargetPosition = new Vector3(_targetXPosition, transform.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, vTempTargetPosition,
                    ref _SDVelocityRef, _sideMoveLatency);

            if (Mathf.Abs(transform.position.x - _targetXPosition) < 0.05f)
            {
                transform.position = vTempTargetPosition;

                System.Random vRan = new();
                byte vNewLane = _currentLane;
                float? vNextLaneX = null;
                int vloopCount = 0;
                while (vNextLaneX == null || vNewLane == _currentLane)
                {
                    vloopCount++;
                    if (vloopCount > 100)
                    {
                        Debug.Log("Boucle infinie Charon");
                        break;
                    }

                    vNewLane = (byte)(_currentLane + vRan.Next(-1, 2));
                    vNextLaneX = _LM.GetLaneCenter(vNewLane);
                }
                _targetXPosition = (float)vNextLaneX + _bodyOrigin.localPosition.x;
                _currentLane = vNewLane;
            }
        }
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Bullet"))
        {
            if (_life == 0) return;
            _life -= 1;

            pOther.GetComponent<Projectile>().Explode();

            if (_life == 0) StartCoroutine(IsBeaten());
        }
    }

    IEnumerator WaitForAttack()
    {
        yield return new WaitForSeconds(0.1f);
        float vAnimTime = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(vAnimTime - 1.5f);

        int vAttackNumber = (byte)_random.Next(1, _laneManager.LaneNumber);
        byte vAttackLane = 0;
        byte vNextLane = vAttackLane;
        for (int i = 0; i < vAttackNumber; i++)
        {
            while (vNextLane == vAttackLane)
                vNextLane = (byte)_random.Next(0, _laneManager.LaneNumber);
            vAttackLane = vNextLane;

            _attackList.Add(Instantiate(_attack2, new Vector3((float)_laneManager.GetLaneCenter(vAttackLane), _laneManager.GroundHeight, _playerTransform.position.z), Quaternion.identity, _attacksParent));
        }

    }

    IEnumerator IsBeaten()
    {
        _isBeaten = true;
        _playerTransform.GetComponent<PlayerManager>().HasBeatenABoss();
        
        if (_isExposed) _animator.SetTrigger("Sink");

        yield return new WaitForSeconds(0.2f);
        float vAnimTime = _animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(vAnimTime);
        DeActivate();
    }
}
