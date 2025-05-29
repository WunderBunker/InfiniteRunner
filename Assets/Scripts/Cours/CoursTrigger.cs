using UnityEngine;

public class CoursTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.name == "Capsule")
            other.GetComponent<Cours_player>().ChangeColor();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.name == "Capsule")
            other.GetComponent<Cours_player>().ChangeBackColor();
    }
}
