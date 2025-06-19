using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

//GESTION GENERAL DU JOUEUR (HORS MOUVEMENTS ET COSMETIQUES)
public class PlayerManager : MonoBehaviour
{
    [SerializeField] byte _maxLife;
    [SerializeField] byte _maxBulletStock;
    [SerializeField] float _bulletFireRate;
    [SerializeField] List<AudioClip> _canonSounds = new();
    [SerializeField] List<AudioClip> _hurtSounds = new();
    [SerializeField] List<AudioClip> _healSounds = new();
    [SerializeField] List<AudioClip> _oboleSounds = new();

    public float TotalDistance { get; private set; } = 0;
    public float Score { get; private set; } = 0;
    public float DeltaDistance { get; private set; } = 0;
    public int CollectedOboles { get; private set; } = 0;

    [SerializeField] GameObject _bullet;

    GameObject _camera;
    float _lastZ;
    PlayerControls _playerControls;
    Vector3 _playerSize;
    byte _life { get; set; }

    byte _bulletCount;
    float _fireRateTimer;

    ScoreUI _scoreUI;
    BulletsUI _bulletsUI;
    LifeUI _lifeUI;

    float _invincibilityTimer;

    float _coinSerieTimer;
    int _coinSerieCount;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("MainCamera");
        _playerControls = gameObject.GetComponent<PlayerControls>();
        _lastZ = transform.position.z;
        _playerSize = gameObject.GetComponent<BoxCollider>().size;

        //Initialisations des indicateurs UI

        _bulletCount = _maxBulletStock;
        _bulletsUI = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BulletStock").GetComponent<BulletsUI>();
        _bulletsUI.SetStockMaxValue(_maxBulletStock);
        _bulletsUI.SetStockValue(_maxBulletStock);

        _life = _maxLife;
        _lifeUI = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Life").GetComponent<LifeUI>();
        _lifeUI.SetMaxLife(_maxLife);

        _scoreUI = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Score").GetComponent<ScoreUI>();
    }

    void Update()
    {
        //Maj de la distance parcourue jusqu'ici
        DeltaDistance = transform.position.z - _lastZ;
        _lastZ = transform.position.z;
        TotalDistance += DeltaDistance;

        //Maj du score (en fonction de la distance parcourue et de à quel point la vitesse max initiale est dépassée)
        if (!_playerControls.IsBossMode)
        {
            float vScoreCoef = 1;
            if (_playerControls.CurrentSpeed >= _playerControls.MaxSpeedInitial)
                vScoreCoef = math.lerp(1, 3, (_playerControls.CurrentSpeed - _playerControls.MaxSpeedInitial) / (_playerControls.MaxSpeedFinal - _playerControls.MaxSpeedInitial));
            Score += DeltaDistance * vScoreCoef;
        }

        //Maj du timer d'invincibilité après coup
        if (_invincibilityTimer > 0) _invincibilityTimer = math.max(_invincibilityTimer - Time.deltaTime, 0);

        //Maj du fire rate qui érgule l'enchainement des tirs de canon
        if (_fireRateTimer > 0)
        {
            _fireRateTimer -= Time.deltaTime;
            _bulletsUI.SetFireRateAvancement(math.max(_fireRateTimer, 0) / _bulletFireRate);
        }

        //Maj du timer permettant les combos de pièces
        if (_coinSerieTimer > 0)
        {
            _coinSerieTimer -= Time.deltaTime;
            if (_coinSerieTimer <= 0)
                _coinSerieCount = 0;
        }
    }

    //inflige des dégât
    public void Hurt(byte pDamage)
    {
        if (_invincibilityTimer > 0) return;

        _invincibilityTimer = 1.5f;

        _life -= pDamage;
        _lifeUI.SetLifeNb(_life);

        AudioManager.Instance.PlaySound(_hurtSounds[new System.Random().Next(0, _hurtSounds.Count)], 1);

        if (_life == 0) { Die(); return; }

        _playerControls.SlowDown(0.25f);

        StartCoroutine(_camera.GetComponent<FollowPlayer>().Tremour());
    }

    //Récupère des PV
    public void Heal(byte pPV)
    {
        _life = (byte)(_life >= 0 && _life < _maxLife ? _life + pPV : _life);
        _lifeUI.SetLifeNb(_life);
        AudioManager.Instance.PlaySound(_healSounds[new System.Random().Next(0, _healSounds.Count)], 1);
    }

    //Récupère un boulet de canon
    public void GainBullet(byte pNb)
    {
        if (_bulletCount >= _maxBulletStock) return;
        _bulletCount += pNb;
        _bulletsUI.SetStockValue(_bulletCount);
    }

    //Méthode déclenchée par l'Input de tir de canon
    public void OnShoot(InputAction.CallbackContext pContext)
    {
        if (_bulletCount == 0 || _fireRateTimer > 0) return;


        if (pContext.started)
        {
            GameObject vBullet = Instantiate(_bullet, transform.Find("CanonPivot").position, Quaternion.identity, null);
            _bulletCount -= 1;
            _bulletsUI.SetStockValue(_bulletCount);
            _fireRateTimer = _bulletFireRate;
            BiomesManager.Instance.RaiseNoise(1);

            AudioManager.Instance.PlaySound(_canonSounds[new System.Random().Next(0, _canonSounds.Count)], 1);

            //On tir à l'envers si phase de boss
            if (_playerControls.IsBossMode)
            {
                vBullet.transform.position = new Vector3(vBullet.transform.position.x, vBullet.transform.position.y, vBullet.transform.position.z - _playerSize.z);
                vBullet.GetComponent<Projectile>().Direction = -1;
            }
        }
    }

    //Mort du joueur
    void Die()
    {
        GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<MainCanvas>().LaunchEndMenu();
    }

    //Gain d'oboles
    public void AddOboles(int pNb)
    {
        CollectedOboles += pNb;

        _coinSerieCount++;

        AudioManager.Instance.PlaySound(_oboleSounds[_coinSerieCount - 1], 1);

        //Gestion du combo de pièce
        if (_coinSerieCount == 10)
        {
            _coinSerieCount = 0;
            AddPonctualScore(10);
        }
        else
        {
            AddPonctualScore(1);
            _coinSerieTimer = 1.5f;
        }
    }

    //Ajou de points supplémentaires quand boss vaincu
    public void HasBeatenABoss(bool pBossWasTriggered)
    {
        if (!pBossWasTriggered) AddPonctualScore(2000);
    }

    public void AddPonctualScore(int pValue)
    {
        Score += pValue;
        _scoreUI.AddPonctualScore(pValue);
    }

}
