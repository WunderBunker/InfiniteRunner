using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CLASSE PARENTE DES BOSS
public abstract class Boss : MonoBehaviour
{
    [SerializeField] protected GameObject _indicator;
    [SerializeField] protected GameObject _lifeUIPrefab;
    [SerializeField] protected GameObject _warningFilter;
    [SerializeField] protected GameObject _bulletItem;
    [SerializeField] protected byte _maxLife;
    protected bool _isActive = false;
    protected GameObject _body;
    protected Transform _playerTransform;
    protected LanesManager _LM;
    protected bool _hasBeenTriggered;

    protected List<GameObject> _attackList = new();
    protected Transform _attacksParent;

    protected bool _isBeaten = false;
    protected byte _life;

    protected float _bulletTimer;
    protected List<GameObject> _bulletList = new();
    System.Random _random = new();

    protected GameObject _lifeUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        _body = transform.GetChild(0).gameObject;
        _body.SetActive(false);

        _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        //On instancie les éventuels indicateurs UI et pannel de warning
        if (_indicator != null) _indicator = Instantiate(_indicator, GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BossIndicator"));
        if (_warningFilter != null) StartCoroutine(InstanciateWarning());
    }

    protected virtual void Update()
    {
        if (!_isActive) return;

        //Maj du timer pour le spawn de munition
        _bulletTimer -= Time.deltaTime;
        if (_bulletTimer <= 0)
        {
            _bulletTimer = 7;
            _bulletList.Add(Instantiate(_bulletItem, new Vector3((float)_LM.GetLaneCenter((byte)_random.Next(0, _LM.LaneNumber)), _LM.GroundHeight - 10, _playerTransform.position.z),
                _bulletItem.transform.rotation, _attacksParent));
        }
        //Maj de a position des munition
        foreach (var lBullet in _bulletList) if (lBullet != null)
            {
                float vYPos = lBullet.transform.position.y;
                //si elle émmerge à la surface
                if (lBullet.transform.position.y < _LM.GroundHeight + 2) vYPos += Time.deltaTime * 10;

                lBullet.transform.position = new Vector3(lBullet.transform.position.x, vYPos, _playerTransform.position.z);
            }
    }

    //Active le boss
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

        _lifeUI = Instantiate(_lifeUIPrefab, GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BossLifeBar"));
        _lifeUI.transform.Find("Life").GetComponent<LifeUI>().SetMaxLife(_maxLife);
    }

    //Désactive le boss
    public virtual void DeActivate()
    {
        if (!_isActive) return;
        _isActive = false;
        _body.SetActive(false);

        Destroy(_attacksParent.gameObject);

        GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>().SwitchBossMode(false);
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowPlayer>().ChangeDirection();

        Destroy(_lifeUI);
    }

    void OnTriggerEnter(Collider pOther)
    {
        //Collision avec un tir de canon
        if (pOther.CompareTag("Bullet")) OnHit(pOther.GetComponent<Projectile>());
    }

    //Collision avec un tir de canon
    virtual protected void OnHit(Projectile pBullet)
    {
        if (_life == 0) return;
        _life -= 1;

        pBullet.Explode();
        _lifeUI.transform.Find("Life").GetComponent<LifeUI>().SetLifeNb(_life);

        if (_life == 0) StartCoroutine(IsBeaten());
    }


    virtual protected IEnumerator IsBeaten()
    {
        _isBeaten = true;
        _playerTransform.GetComponent<PlayerManager>().HasBeatenABoss();

        yield return null;
    }

    virtual protected IEnumerator InstanciateWarning()
    {
        yield return new WaitForSeconds(1.5f);
        _warningFilter = Instantiate(_warningFilter, GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Filters").Find("BossSpecific"));
    }
}
