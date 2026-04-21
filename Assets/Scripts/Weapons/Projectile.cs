using UnityEngine;

namespace KawaiiKiller.Weapons
{
    public class Projectile : MonoBehaviour
    {
        private DamagePayload _payload;
        private Vector3 _direction;
        private float _speed;
        private Vector3 _lastPosition;
        private bool _isInitialized;
        private WeaponInstance _sourceWeapon;

        public void Initialize(DamagePayload payload, Vector3 direction, float speed, float lifetime)
        {
            _payload = payload;
            _direction = direction.normalized;
            _speed = speed;
            _lastPosition = transform.position;
            _isInitialized = true;
            if (lifetime > 0f) Destroy(gameObject, lifetime);
        }

        public void SetSourceWeapon(WeaponInstance sourceWeapon)
        {
            _sourceWeapon = sourceWeapon;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = currentPosition + (_direction * _speed * Time.deltaTime);
            Vector3 rayDirection = targetPosition - _lastPosition;
            float rayDistance = rayDirection.magnitude;

            if (rayDistance > 0f && Physics.Raycast(_lastPosition, rayDirection.normalized, out RaycastHit hit, rayDistance))
            {
                DamagePayload hitPayload = _payload;
                if (_sourceWeapon != null)
                {
                    _sourceWeapon.OnProjectileHit(hit.collider.gameObject, ref hitPayload);
                }

                if (hit.collider.TryGetComponent<DamageableLink>(out var damageableLink) && damageableLink.Damageable != null)
                {
                    damageableLink.Damageable.TakeDamage(hitPayload);
                }
                transform.position = hit.point;
                Destroy(gameObject);
                return;
            }

            transform.position = targetPosition;
            _lastPosition = transform.position;
        }
    }
}
