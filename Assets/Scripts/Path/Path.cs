using UnityEngine;

public class Path : MonoBehaviour
{
    public Transform[] waypoints;
    public PathTypeEnum pathType = PathTypeEnum.Loop;

    private int direction = 1;
    int index;

    public Vector3 GetCurrentWayPoint()
    {
        return waypoints[index].position;
    }

    public Vector3 GetNextWayPoint()
    {
        if (waypoints.Length == 0)
        {
            return transform.position;
        }

        index = GetNextWayPointIndex();
        Vector3 nextWaypoint = waypoints[index].position;

        return nextWaypoint;
    }

    public int GetNextWayPointIndex()
    {
        index += direction;

        switch(pathType)
        {
            case PathTypeEnum.Loop:

                index %= waypoints.Length; 
                break;

            case PathTypeEnum.ReverseWhenComplete:

                if(index >= waypoints.Length || index < 0)
                {
                    direction *= -1;
                    index += direction * 2;
                }
                break;
        }

        return index;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.white;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }

        if(pathType == PathTypeEnum.Loop)
        {
            Gizmos.DrawLine(waypoints[waypoints.Length-1].position, waypoints[0].position);
        }

        Gizmos.color= Color.red;

        foreach (Transform waypoint in waypoints)
        {
            Gizmos.DrawSphere(waypoint.position, 0.2f);
        }
    }
}
