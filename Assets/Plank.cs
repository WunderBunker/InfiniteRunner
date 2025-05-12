using UnityEngine;

public class Plank : MonoBehaviour
{
    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            pOther.gameObject.GetComponent<PlayerManager>().Heal(1);
            Destroy(gameObject);
        }
    }
}
