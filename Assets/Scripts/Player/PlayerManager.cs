using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] byte _maxLife;
    [SerializeField] byte _maxBulletStock;
    [SerializeField] float _bulletReloadTime;
    [SerializeField] float _bulletFireRate;
    [SerializeField] List<AudioClip> _canonSounds = new();
    [SerializeField] List<AudioClip> _hurtSounds = new();
    [SerializeField] List<AudioClip> _healSounds = new();
    [SerializeField] List<AudioClip> _oboleSounds = new();

    public float TotalDistance { get; set; } = 0;
    public float DeltaDistance { get; set; } = 0;
    public int CollectedOboles { get; set; } = 0;

    [SerializeField] GameObject _bullet;

    GameObject _camera;
    AudioManager _AM;
    float _lastZ;
    playerControls _playerControls;
    Vector3 _playerSize;
    byte _life { get; set; }

    byte _bulletCount;
    float _reloadTimer;
    bool _mustReload;
    float _fireRateTimer;

    BulletsUI _bulletsUI;
    LifeUI _lifeUI;

    float _invincibilityTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("MainCamera");
        _playerControls = gameObject.GetComponent<playerControls>();
        _lastZ = transform.position.z;
        _playerSize = gameObject.GetComponent<BoxCollider>().size;

        _bulletCount = _maxBulletStock;
        _bulletsUI = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BulletStock").GetComponent<BulletsUI>();
        _bulletsUI.SetStockMaxValue(_maxBulletStock);
        _bulletsUI.SetStockValue(_maxBulletStock);

        _life = _maxLife;
        _lifeUI = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Life").GetComponent<LifeUI>();
        _lifeUI.SetMaxLife(_maxLife);

        _AM = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }

    void Update()
    {
        DeltaDistance = transform.position.z - _lastZ;
        _lastZ = transform.position.z;
        TotalDistance += DeltaDistance;

        if (_invincibilityTimer > 0) _invincibilityTimer = math.max(_invincibilityTimer - Time.deltaTime, 0);

        if (_mustReload)
        {
            _reloadTimer -= Time.deltaTime;
            _bulletsUI.SetReloadValue(1 - math.max(_reloadTimer, 0) / _bulletReloadTime);
            if (_reloadTimer <= 0)
            {
                _reloadTimer = _bulletReloadTime;
                _bulletCount += 1;
                _bulletsUI.SetStockValue(_bulletCount);
                if (_bulletCount == _maxBulletStock) _mustReload = false;
            }
        }
        if (_fireRateTimer > 0)
        {
            _fireRateTimer -= Time.deltaTime;
            _bulletsUI.SetFireRateAvancement(math.max(_fireRateTimer,0) / _bulletFireRate);
        }
    }

    public void Hurt(byte pDamage)
    {
        if (_invincibilityTimer > 0) return;

        _invincibilityTimer = 1.5f;

        _life -= pDamage;
        _lifeUI.SetLifeNb(_life);

        _AM.PlaySound(_hurtSounds[new System.Random().Next(0, _hurtSounds.Count)],1);

        if (_life == 0) { Die(); return; }

        _playerControls.SlowDown(0.25f);

        StartCoroutine(_camera.GetComponent<FollowPlayer>().Tremour());
    }
    public void Heal(byte pPV)
    {
        _life = (byte)(_life >= 0 && _life < _maxLife ? _life + pPV : _life);
        _lifeUI.SetLifeNb(_life);
        _AM.PlaySound(_healSounds[new System.Random().Next(0, _healSounds.Count)],1);
    }


    public void OnShoot(InputAction.CallbackContext pContext)
    {
        if (_bulletCount == 0 || _fireRateTimer > 0) return;
        

        if (pContext.started)
        {
            GameObject vBullet = Instantiate(_bullet, transform.Find("CanonPivot").position, Quaternion.identity, null);
            _bulletCount -= 1;
            _bulletsUI.SetStockValue(_bulletCount);
            _mustReload = true;
            _fireRateTimer = _bulletFireRate;

            _AM.PlaySound(_canonSounds[new System.Random().Next(0, _canonSounds.Count)], 1);

            if (_playerControls.IsBossMode)
            {
                vBullet.transform.position = new Vector3(vBullet.transform.position.x, vBullet.transform.position.y, vBullet.transform.position.z - _playerSize.z);
                vBullet.GetComponent<Projectile>().Direction = -1;
            }
        }
    }

    void Die()
    {
        GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<MainCanvas>().LaunchEndMenu();
        //gameObject.SetActive(false);
    }

    public void AddOboles(int pNb)
    {
        CollectedOboles += pNb;
        
        _AM.PlaySound(_oboleSounds[new System.Random().Next(0, _oboleSounds.Count)],1);
    }

}
