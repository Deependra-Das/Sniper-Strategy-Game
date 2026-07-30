using UnityEngine;

namespace SniperStrategyGame.Path
{
    public class PatrolPath : MonoBehaviour
    {
        [SerializeField] private PathTypeEnum _pathType = PathTypeEnum.Loop;
        [SerializeField] private Transform[] _wayPoints;
        private const int Forward = 1;
        private const int Backward = -1;
        private int _direction = Forward;
        private int _index;

        public Vector3 GetCurrentWayPoint()
        {
            if (_wayPoints == null || _wayPoints.Length == 0)
            {
                return transform.position;
            }

            return _wayPoints[_index].position;
        }

        public Vector3 GetNextWayPoint()
        {
            if (_wayPoints == null || _wayPoints.Length == 0)
            {
                return transform.position;
            }

            _index = GetNextWayPointIndex();
            Vector3 nextWaypoint = _wayPoints[_index].position;

            return nextWaypoint;
        }

        public int GetNextWayPointIndex()
        {
            if (_wayPoints == null || _wayPoints.Length == 0)
            {
                return 0;
            }

            _index += _direction;

            switch (_pathType)
            {
                case PathTypeEnum.Loop:

                    _index %= _wayPoints.Length;
                    break;

                case PathTypeEnum.Backtrack:

                    if (_index >= _wayPoints.Length || _index < 0)
                    {
                        _direction = _direction == Forward ? Backward : Forward;
                        _index += _direction * 2;
                    }
                    break;
            }

            return _index;
        }

        private void OnDrawGizmos()
        {
            if (_wayPoints == null || _wayPoints.Length == 0) return;

            Gizmos.color = Color.white;

            for (int i = 0; i < _wayPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(_wayPoints[i].position, _wayPoints[i + 1].position);
            }

            if (_pathType == PathTypeEnum.Loop)
            {
                Gizmos.DrawLine(_wayPoints[_wayPoints.Length - 1].position, _wayPoints[0].position);
            }

            Gizmos.color = Color.red;

            foreach (Transform waypoint in _wayPoints)
            {
                Gizmos.DrawSphere(waypoint.position, 0.2f);
            }
        }
    }
}
