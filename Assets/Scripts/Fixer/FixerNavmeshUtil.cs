using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public static class FixerNavmeshUtil
{
    
    public static bool TryGetSafeRandomPoint(
        Vector3 origin,
        float radius,
        float minEdgeDistance,
        out Vector3 result,
        int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            Vector3 randomPoint = origin + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                if (IsFarEnoughFromEdge(hit.position, minEdgeDistance))
                {
                    result = hit.position;
                    return true;
                }
            }
        }

        result = origin;
        return false;
    }

    public static bool TryGetSafeRoomPoint(
        Bounds roomBounds,
        float sampleHeight,
        float minEdgeDistance,
        out Vector3 result,
        int maxAttempts = 10)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(roomBounds.min.x, roomBounds.max.x),
                sampleHeight,
                Random.Range(roomBounds.min.z, roomBounds.max.z)
            );

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                bool insideRoom =
                    hit.position.x >= roomBounds.min.x && hit.position.x <= roomBounds.max.x &&
                    hit.position.z >= roomBounds.min.z && hit.position.z <= roomBounds.max.z;

                if (insideRoom && IsFarEnoughFromEdge(hit.position, minEdgeDistance))
                {
                    result = hit.position;
                    return true;
                }
            }
        }

        result = Vector3.zero;
        return false;
    }
    
    private static bool IsFarEnoughFromEdge(Vector3 point, float minEdgeDistance)
    {
        if (minEdgeDistance <= 0f) return true;

        if (NavMesh.FindClosestEdge(point, out NavMeshHit edgeHit, NavMesh.AllAreas))
        {
            return edgeHit.distance >= minEdgeDistance;
        }

        return false;
    }
}