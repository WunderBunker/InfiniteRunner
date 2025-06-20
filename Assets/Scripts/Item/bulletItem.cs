using UnityEngine;

//MUNITION
public class bulletItem : MonoBehaviour
{
    [SerializeField] GameObject _particles;
    [SerializeField] AudioClip _collectSound;

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            pOther.GetComponent<PlayerManager>().GainBullet(1);
            AudioManager.Instance.PlaySound(_collectSound, 1);
            Instantiate(_particles, transform.position, _particles.transform.rotation, transform.parent);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 50, Space.World);
    }
}
