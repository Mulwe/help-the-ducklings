using System;
using UnityEngine;

/// <summary>
///
/// </summary>
public class OutOfBoundsController : MonoBehaviour
{
    [SerializeField] private CompositeCollider2D _compositeCollider2D;
    [SerializeField] private Rigidbody2D _rb;

    public static Action<GameObject> OnOutOfBounds;

    private void Awake()
    {
        _compositeCollider2D ??= GetComponent<CompositeCollider2D>();
        _rb ??= GetComponent<Rigidbody2D>();
        if (_compositeCollider2D == null || _rb == null)
            Debug.LogError($"{this.name} missing CompositeCollider2D or Rigidbody2D on this object");
        if (_compositeCollider2D != null && _rb != null)
        {
            _compositeCollider2D.isTrigger = true;
            _compositeCollider2D.enabled = true;
            _rb.bodyType = RigidbodyType2D.Static;
            _rb.simulated = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.isTrigger || collision.CompareTag("Walls")) return;
        if (collision.gameObject != null && collision.gameObject.CompareTag("Duck") || collision.gameObject.CompareTag("Enemy"))
        {
            OnOutOfBounds?.Invoke(collision.gameObject);
            return;
        }
    }
}