using UnityEngine;

public class JellyFish : MonoBehaviour
{
    [SerializeField] float _sideMoveLatency;
    LanesManager _LM;

    float _targetXPosition;
    Vector3 _SDVelocityRef;
    byte _currentLane;
    bool _isDying = false;
    Material _mat1;
    Material _mat2;
    Material _mat3;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _LM = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _currentLane = _LM.GetLaneFromXPos(transform.position.x);
        _targetXPosition = transform.position.x;

        _mat1 = transform.Find("Circle").GetComponent<SkinnedMeshRenderer>().materials[0];
        _mat1 = new(_mat1);
        transform.Find("Circle").GetComponent<SkinnedMeshRenderer>().materials[0] = _mat1;
        _mat2 = transform.Find("Circle").GetComponent<SkinnedMeshRenderer>().materials[1];
        _mat2 = new(_mat2);
        transform.Find("Circle").GetComponent<SkinnedMeshRenderer>().materials[1] = _mat2;

        _mat3 = transform.Find("Top").GetComponent<MeshRenderer>().material;
        _mat3 = new(_mat3);
        transform.Find("Top").GetComponent<MeshRenderer>().material = _mat3;
    }

    // Update is called once per frame
    void Update()
    {

        if (_isDying)
        {
            transform.position = new Vector3(transform.position.x,transform.position.y -Time.deltaTime * 10, transform.position.z);
            transform.Rotate(new Vector3(0,0,Time.deltaTime*100), Space.World);
            if (transform.position.z <= -10) Destroy(gameObject);
            return;
        }

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
                    Debug.Log("Boucle infinie spectre");
                    break;
                }

                vNewLane = (byte)(_currentLane + vRan.Next(-1, 2));
                vNextLaneX = _LM.GetLaneCenter(vNewLane);
            }
            _targetXPosition = (float)vNextLaneX;
            _currentLane = vNewLane;
        }
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (_isDying) return; 
        
        if (pOther.gameObject.CompareTag("Player"))
        {
            pOther.gameObject.GetComponent<PlayerManager>().Hurt(1);
            _isDying = true;
        }
        else if (pOther.gameObject.CompareTag("Bullet"))
        {
            pOther.gameObject.GetComponent<Projectile>().Explode();
            _isDying = true;
        }
    }
}
