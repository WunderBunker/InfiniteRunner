using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] byte _maxLife;
    public byte Life { get; set; }
    public float TotalDistance { get; set; } = 0;
    public float DeltaDistance { get; set; } = 0;
    public int CollectedOboles { get; set; } = 0;
    [SerializeField] GameObject _bullet;
    GameObject _camera;
    float _lastZ;
    playerControls _playerControls;
    Vector3 _playerSize;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("MainCamera");
        _playerControls = gameObject.GetComponent<playerControls>();
        _lastZ = transform.position.z;
        Life = _maxLife;
        _playerSize = gameObject.GetComponent<BoxCollider>().size;
    }

    void Update()
    {
        DeltaDistance = transform.position.z - _lastZ;
        _lastZ = transform.position.z;
        TotalDistance += DeltaDistance;
    }

    public void Hurt(byte pDamage)
    {
        Life = (byte)(Life != 0 ? Life - pDamage : 0);
        StartCoroutine(_camera.GetComponent<FollowPlayer>().Tremour());
    }
    public void Heal(byte pPV)
    {
        Life = (byte)(Life >= 0 && Life < _maxLife ? Life + pPV : Life);
    }


    public void OnShoot(InputAction.CallbackContext pContext)
    {
        if (pContext.started)
        {
            GameObject vBullet = Instantiate(_bullet, transform.Find("CanonPivot").position, Quaternion.identity, null);
            if (_playerControls.ReverseXControls)
            {
                vBullet.transform.position = new Vector3(vBullet.transform.position.x, vBullet.transform.position.y, vBullet.transform.position.z - _playerSize.z);
                vBullet.GetComponent<Projectile>().Direction = -1;
            }
        }
    }

    public void AddOboles(int pNb)
    {
        CollectedOboles += pNb;
    }

}
