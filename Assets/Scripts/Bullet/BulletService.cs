using UnityEngine;

namespace SniperStrategyGame.Bullet
{
    public class BulletService
    {
        private readonly PlayerBullet _playerBulletPrefab;

        public BulletService(PlayerBullet bulletPrefab)
        {
            _playerBulletPrefab = bulletPrefab;
        }

        public PlayerBullet SpawnPlayerBullet(Vector3 position, Quaternion rotation)
        {
            return Object.Instantiate(_playerBulletPrefab, position, rotation);
        }
    }
}