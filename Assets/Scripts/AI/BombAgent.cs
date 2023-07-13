using UnityEngine;

public class BombAgent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            FindObjectOfType<GameManagerAgent>().Explode();
        }
    }

}
