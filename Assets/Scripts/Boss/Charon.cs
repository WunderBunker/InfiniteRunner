using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Charon : Boss, INoiseSensitive
{
    [SerializeField] GameObject _attack;
    [SerializeField] GameObject _attack2;
    [SerializeField] float _attackTempo;
    [SerializeField] float _attack2Tempo;
    [SerializeField] int _nbAttackBfExposed;
    [SerializeField] int _nbAttackBfHidden;
    [SerializeField] float _sideMoveLatency;
    [SerializeField] float _resetNoiseTempo;
    [SerializeField] AudioClip _hitSound;
    [SerializeField] AudioClip _deathSound;
    [SerializeField] Color _hitColor;

    int _attackCounter;

    Animator _animator;

    byte _noiseCounter;
    float _hellTimer;
    CharonIndicator _bossIndicator;

    float _attackTimer;
    System.Random _random = new();
    Transform _bodyOrigin;


    bool _isExposed;

    float _attackRange;

    float _targetXPosition;
    Vector3 _SDVelocityRef;
    byte _currentLane;

    float _resetNoiseTimer;

    bool _isHit;

    BossFilter _filterAlarm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    override protected void Start()
    {
        base.Start();
        _bodyOrigin = transform.Find("Origin");
        _animator = transform.Find("Body").GetComponent<Animator>();
        _attackRange = _attack.GetComponent<CharonAttack>().GetAttackRange();
        _bossIndicator = _indicator.GetComponent<CharonIndicator>();
        _hellTimer = 3 * 60;
        _noiseCounter = 0;
        _bossIndicator.MajNoiseValue(_noiseCounter);
        _bossIndicator.MajChronoValue(_hellTimer);
        _filterAlarm = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Filters").Find("Alarm").GetComponent<BossFilter>();
    }

    override public void Activate()
    {
        base.Activate();

        _currentLane = _LM.GetLaneFromXPos(transform.position.x + _bodyOrigin.position.x);
        _targetXPosition = (float)_LM.GetLaneCenter(_currentLane) + _bodyOrigin.position.x;
        _body.transform.Find("Cape").GetComponent<SkinnedMeshRenderer>().material.SetColor("_Emission", new Color(_hitColor.r, _hitColor.g, _hitColor.b, 0));

        _isExposed = false;
        _attackCounter = _nbAttackBfExposed;
        //Juste le temps que le bateau se retourne
        _attackTimer = _attackTempo / 4;

        _filterAlarm.SetActive(false);
    }
    override public void DeActivate()
    {
        base.DeActivate();

        _noiseCounter = 0;
        _bossIndicator.MajNoiseValue(_noiseCounter);

        _filterAlarm.SetActive(false);
    }

    override protected void Update()
    {
        base.Update();

        if (_hellTimer > 0)
        {
            _hellTimer -= Time.deltaTime;
            if (_hellTimer <= 0)
            {
                _hellTimer = 0;
                _hasBeenTriggered = true;
                Activate();
            }
            _bossIndicator.MajChronoValue(_hellTimer);
        }

        if (!_isActive)
        {
            if (_resetNoiseTimer > 0)
            {
                _resetNoiseTimer -= Time.deltaTime;
                if (_resetNoiseTimer <= 0) AddNoise(-1);

            }
            return;
        }
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

                int vAttackNumber = (byte)_random.Next(1, _LM.LaneNumber);
                byte vAttackLane = 0;
                byte vNextLane = vAttackLane;
                for (int i = 0; i < vAttackNumber; i++)
                {
                    while (vNextLane == vAttackLane)
                        vNextLane = (byte)_random.Next(0, _LM.LaneNumber);
                    vAttackLane = vNextLane;

                    _attackList.Add(Instantiate(_attack, new Vector3((float)_LM.GetLaneCenter(vAttackLane), _LM.GroundHeight, _playerTransform.position.z), Quaternion.identity, _attacksParent));
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

        //Animation de dégât
        if (_isHit)
        {
            Color vBodyColor = _body.transform.Find("Cape").GetComponent<SkinnedMeshRenderer>().material.GetColor("_Emission");
            if (vBodyColor.a > 0)
            {
                vBodyColor.a -= Time.deltaTime * 0.35f;
                if (vBodyColor.a <= 0.1f) { _isHit = false; vBodyColor.a = 0; }
            }
            else _isHit = false;
            _body.transform.Find("Cape").GetComponent<SkinnedMeshRenderer>().material.SetColor("_Emission", vBodyColor);
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

    public void AddNoise(int pNb)
    {
        if (_isActive) return;
        
        _noiseCounter = (byte)math.clamp(_noiseCounter + pNb, 0, 5);
        if (_noiseCounter > 0) _resetNoiseTimer = _resetNoiseTempo;

        if (pNb > 0 && _noiseCounter == 4) _filterAlarm.SetActive(true);
        else if (pNb < 0 && _noiseCounter == 3) _filterAlarm.SetActive(false);

        _bossIndicator.MajNoiseValue(_noiseCounter);

        if (_noiseCounter == 5)
        {
            Activate();
            _hasBeenTriggered = true;
        }
    }

    IEnumerator WaitForAttack()
    {
        yield return new WaitForSeconds(0.1f);
        float vAnimTime = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(vAnimTime - 1.5f);

        int vAttackNumber = (byte)_random.Next(1, _LM.LaneNumber);
        byte vAttackLane = 0;
        byte vNextLane = vAttackLane;
        for (int i = 0; i < vAttackNumber; i++)
        {
            while (vNextLane == vAttackLane)
                vNextLane = (byte)_random.Next(0, _LM.LaneNumber);
            vAttackLane = vNextLane;

            _attackList.Add(Instantiate(_attack2, new Vector3((float)_LM.GetLaneCenter(vAttackLane), _LM.GroundHeight - 10, _playerTransform.position.z), Quaternion.identity, _attacksParent));
        }
    }

    override protected void OnHit(Projectile pBullet)
    {
        base.OnHit(pBullet);
        AudioManager.Instance.PlaySound(_hitSound, 1);
        _isHit = true;
        Color vColor = _body.transform.Find("Cape").GetComponent<SkinnedMeshRenderer>().material.GetColor("_Emission");
        _body.transform.Find("Cape").GetComponent<SkinnedMeshRenderer>().material.SetColor("_Emission", new Color(vColor.r, vColor.g, vColor.b, 1));
    }

    override protected IEnumerator IsBeaten()
    {
        yield return base.IsBeaten();

        if (_isExposed) _animator.SetTrigger("Sink");

        AudioManager.Instance.PlaySound(_deathSound, 1);

        yield return new WaitForSeconds(0.2f);
        float vAnimTime = _animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(vAnimTime);
        DeActivate();
    }
}

public interface INoiseSensitive
{
    void AddNoise(int pValue);
}

