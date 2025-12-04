using UnityEngine;

public class BirdSpawnPoint : MonoBehaviour
{
    public bool isOccupied = false; // Žg—p’†‚©‚Ç‚¤‚©

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
}
