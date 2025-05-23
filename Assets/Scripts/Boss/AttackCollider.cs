using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.gameObject.CompareTag("Player")) pOther.gameObject.GetComponent<PlayerManager>().Hurt(1);
    }
}