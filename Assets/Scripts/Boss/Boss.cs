using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Boss : MonoBehaviour
{
    [SerializeField] protected GameObject _indicator;
    [SerializeField] protected GameObject _warningFilter;
    [SerializeField] protected AudioClip _warningNoise;
    [SerializeField] GameObject _bulletItem;
    [SerializeField] protected float _maxLife;
    protected bool _isActive = false;
    protected GameObject _body;
    protected Transform _playerTransform;
    protected LanesManager _LM;
    protected bool _hasBeenTriggered;


    protected List<GameObject> _attackList = new();
    protected Transform _attacksParent;

    protected bool _isBeaten = false;
    protected float _life;

    protected float _bulletTimer;
    protected List<GameObject> _bulletList = new();
    System.Random _random = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        _body = transform.GetChild(0).gameObject;
        _body.SetActive(false);

        _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        if (_indicator != null) _indicator = Instantiate(_indicator, GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BossIndicator"));
        if (_warningFilter != null)
        {
            _warningFilter = Instantiate(_warningFilter, GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Filters").Find("BossSpecific"));
            if (_warningNoise != null) AudioManager.Instance.PlaySound(_warningNoise, 1);
        }
    }

    protected virtual void Update()
    {
        if (!_isActive) return;

        _bulletTimer -= Time.deltaTime;
        if (_bulletTimer <= 0)
        {
            _bulletTimer = 10;
            _bulletList.Add(Instantiate(_bulletItem, new Vector3((float)_LM.GetLaneCenter((byte)_random.Next(0, _LM.LaneNumber)), _LM.GroundHeight, _playerTransform.position.z),
                _bulletItem.transform.rotation, _attacksParent));
        }
        foreach (var lBullet in _bulletList) if (lBullet != null) lBullet.transform.position = new Vector3(lBullet.transform.position.x, lBullet.transform.position.y, _playerTransform.position.z);
    }

    public virtual void Activate()
    {
        if (_isActive) return;
        _isActive = true;
        _isBeaten = false;
        _bulletTimer = 10;

        _life = _maxLife;

        _attacksParent = new GameObject().transform;

        _body.SetActive(true);

        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(true);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();

    }
    public virtual void DeActivate()
    {
        if (!_isActive) return;
        _isActive = false;
        _body.SetActive(false);

        Destroy(_attacksParent.gameObject);

        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(false);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Bullet")) OnHit(pOther.GetComponent<Projectile>());
    }

    virtual protected void OnHit(Projectile pBullet)
    {
        if (_life == 0) return;
        _life -= 1;

        pBullet.Explode();

        if (_life == 0) StartCoroutine(IsBeaten());
    }


    virtual protected IEnumerator IsBeaten()
    {
        _isBeaten = true;
        _playerTransform.GetComponent<PlayerManager>().HasBeatenABoss();

        yield return null;
    }
}
