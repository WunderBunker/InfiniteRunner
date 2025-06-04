using UnityEngine;

public class bulletItem : MonoBehaviour
{
    [SerializeField] AudioClip _collectSound;

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            pOther.GetComponent<PlayerManager>().GainBullet(1);
            AudioManager.Instance.PlaySound(_collectSound, 1);
            
            Destroy(gameObject);

        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 50, Space.World);
    }
}
