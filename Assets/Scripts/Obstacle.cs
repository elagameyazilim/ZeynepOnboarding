using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float pushBackForce = 0.02f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                Vector3 pushDirection = collision.transform.position - transform.position;
                pushDirection.y = 0;
                pushDirection.Normalize();

                playerRb.AddForce(pushDirection * pushBackForce, ForceMode.Impulse);

                
                playerRb.velocity *= 0.5f;
            }
        }
    }
}