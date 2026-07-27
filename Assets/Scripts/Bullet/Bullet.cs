using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 100f;
    [SerializeField] private Rigidbody _rigidbodyObj;
    private PlayerController controller;

    public void Initialize(Vector3 direction, float speed)
    {
        _rigidbodyObj.linearVelocity = direction.normalized * speed;

        Destroy(gameObject, _lifeTime);
    }

    public void SetController(PlayerController player)
    {
        controller = player;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider hitCollider = collision.collider;

        if (hitCollider.CompareTag("EnemyShield"))
        {
            Debug.Log("EnemyShield Hit");
        }
        if (hitCollider.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Hit");
        }

        controller.RestorePlayerCamera();
        Destroy(gameObject);
    }
}