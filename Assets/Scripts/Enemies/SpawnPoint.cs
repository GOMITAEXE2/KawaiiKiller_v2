using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField, Range(0.1f, 10f)] private float weight = 1f;
    public float Weight => weight;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
#endif
}