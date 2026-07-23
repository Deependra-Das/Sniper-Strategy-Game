using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float _lifeTime = 5f;
    private Rigidbody _rigidbodyObj;    

    private void Awake()
    {
        _rigidbodyObj = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 direction, float speed)
    {
        _rigidbodyObj.linearVelocity = direction.normalized * speed;

        Destroy(gameObject, _lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Hit");
        }

        Destroy(gameObject);
    }
}