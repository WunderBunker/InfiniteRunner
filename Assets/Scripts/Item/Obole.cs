using UnityEngine;

public class Obole : MonoBehaviour
{
    [SerializeField] GameObject _particles;

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            pOther.gameObject.GetComponent<PlayerManager>().AddOboles(1);
            Instantiate(_particles, transform.position, _particles.transform.rotation, transform.parent);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up  *Time.deltaTime*50, Space.World);
    }
}
