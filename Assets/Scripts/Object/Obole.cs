using UnityEngine;

public class Obole : MonoBehaviour
{
    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            pOther.gameObject.GetComponent<PlayerManager>().AddOboles(1);
            Destroy(gameObject);
        }
    }
}
