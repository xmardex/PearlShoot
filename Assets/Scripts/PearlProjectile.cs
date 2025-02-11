using UnityEngine;

public class PearlProjectile : MonoBehaviour
{
    [SerializeField] private PearlType _type;
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private PearlPlanet _planet;

    public PearlType Type => _type;

    private bool _used = false;
    private bool _isGhost;

    private void Awake()
    {
        _used = false;
    }

    public void Fire(Vector3 velocity, bool isGhost, PearlPlanet pearlPlanet)
    {
        _isGhost = isGhost;

        if (!_isGhost)
        {
            _planet = pearlPlanet;
            SetAsKinematic(false);
        }

        _rb.AddForce(velocity, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isGhost || _used) return;

        if (collision.collider.TryGetComponent(out Pearl collidedPearl))
        {
            if (collidedPearl.Type == _type)
            {
                _planet.Collect();
                collidedPearl.RightPearl();
                Debug.Log("OK!");
                _used = true;
            }
            else
            {
                _planet.Reject();
                collidedPearl.WrongPearl();
                Debug.Log("Bad");
                _used = true;
            }
        }
    }

    private void Update()
    {
        if(transform.position.y < -30)
            Destroy(gameObject);
    }

    public void SetAsKinematic(bool isKinematic)
    {
        _rb.isKinematic = isKinematic;
    }
}
