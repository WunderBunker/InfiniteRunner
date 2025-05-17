using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] byte _maxLife;
    [SerializeField] byte _maxBulletStock;
    [SerializeField] float _bullletReloadTime;

    public byte Life { get; set; }
    public float TotalDistance { get; set; } = 0;
    public float DeltaDistance { get; set; } = 0;
    public int CollectedOboles { get; set; } = 0;
    [SerializeField] GameObject _bullet;
    GameObject _camera;
    float _lastZ;
    playerControls _playerControls;
    Vector3 _playerSize;

    byte _bulletCount;
    float _reloadTimer;
    bool _mustReload;
    BulletsUI _bulletsUI;

    float _invincibilityTimer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("MainCamera");
        _playerControls = gameObject.GetComponent<playerControls>();
        _lastZ = transform.position.z;
        Life = _maxLife;
        _playerSize = gameObject.GetComponent<BoxCollider>().size;
        _bulletCount = _maxBulletStock;
        _bulletsUI = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BulletStock").GetComponent<BulletsUI>();
        _bulletsUI.SetStockMaxValue(_maxBulletStock);
    }

    void Update()
    {
        DeltaDistance = transform.position.z - _lastZ;
        _lastZ = transform.position.z;
        TotalDistance += DeltaDistance;

        if (_invincibilityTimer > 0) _invincibilityTimer = math.max(_invincibilityTimer - Time.deltaTime,0);

        if (_mustReload)
        {
            _reloadTimer -= Time.deltaTime;
            _bulletsUI.SetReloadValue(1 - math.max(_reloadTimer, 0) / _bullletReloadTime);
            if (_reloadTimer <= 0)
            {
                _reloadTimer = _bullletReloadTime;
                _bulletCount += 1;
                _bulletsUI.SetStockValue(_bulletCount);
                if (_bulletCount == _maxBulletStock) _mustReload = false;
            }
        }
    }

    public void Hurt(byte pDamage)
    {
        if (_invincibilityTimer > 0) return;

        _invincibilityTimer = 1.5f;

        Life -= pDamage;
        if (Life == 0) { Die(); return; }
        _playerControls.SlowDown(0.25f);
        StartCoroutine(_camera.GetComponent<FollowPlayer>().Tremour());
    }
    public void Heal(byte pPV)
    {
        Life = (byte)(Life >= 0 && Life < _maxLife ? Life + pPV : Life);
    }


    public void OnShoot(InputAction.CallbackContext pContext)
    {
        if (_bulletCount == 0) return;

        if (pContext.started)
        {
            GameObject vBullet = Instantiate(_bullet, transform.Find("CanonPivot").position, Quaternion.identity, null);
            _bulletCount -= 1;
            _bulletsUI.SetStockValue(_bulletCount);
            _mustReload = true;

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
    }

}
