using UnityEngine;

public class CharonAttack : MonoBehaviour
{
    [SerializeField] float _attackRange;
    Animation _anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _anim = transform.Find("Plane").GetComponent<Animation>();
        _anim.Play();
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - _attackRange + 5);
    }

    // Update is called once per frame
    void Update()
    {
        
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
