using UnityEngine;

public class Rock : MonoBehaviour
{

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.gameObject.CompareTag("Player")) pOther.gameObject.GetComponent<PlayerManager>().Hurt(1);
        else if (pOther.gameObject.CompareTag("Bullet")) pOther.gameObject.GetComponent<Projectile>().Explode();
    }
}
