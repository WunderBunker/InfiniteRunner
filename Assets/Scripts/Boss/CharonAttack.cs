using UnityEngine;

public class CharonAttack : MonoBehaviour
{
    [SerializeField] float _attackRange;
    Animation _anim;
    float _groundHeight;
    ParticleSystem _splash;
    Transform _splashTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _anim = transform.Find("Plane").GetComponent<Animation>();
        _anim.Play();
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - _attackRange + 5);
        _groundHeight = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>().GroundHeight;
        _splashTransform = transform.Find("Plane").Find("Splash");
        _splash = _splashTransform.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {

        if (_splashTransform.position.y <= _groundHeight)
        { if (!_splash.isPlaying) _splash.Play(); }
        else if (_splash.isPlaying) _splash.Stop();

        if (!_anim.isPlaying) Destroy(gameObject);
    }

    public float GetAttackRange()
    {
        return _attackRange;
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player")) pOther.GetComponent<PlayerManager>().Hurt(1);
    }
}
