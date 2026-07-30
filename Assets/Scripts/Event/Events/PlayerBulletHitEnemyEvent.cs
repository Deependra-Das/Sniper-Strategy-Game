using UnityEngine;

public class PlayerBulletHitEnemyEvent 
{
    public Vector3 enemyPosition { get; }
    public Vector3 shotDirection { get; }

    public PlayerBulletHitEnemyEvent(Vector3 enemyPosition, Vector3 shotDirection)
    {
        this.enemyPosition = enemyPosition;
        this.shotDirection = shotDirection;
    }
}