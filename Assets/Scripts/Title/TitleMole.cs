using UnityEngine;

public class Mole : MonoBehaviour
{
    private Animator animator;

    public float lifeTime = 2f; // •\¦‚³‚ê‚éŠÔ

    public MoleSpawnPoint mySpawnPoint;


    void Start()
    {
        {
            GetComponent<Animator>().Play("TitleMole");


            // ˆê’èŠÔŒã‚É©“®‚ÅÁ‚¦‚é
            Destroy(gameObject, lifeTime);
        }
    }
    void OnDestroy()
    {
        if (mySpawnPoint != null)
        {
            mySpawnPoint.isOccupied = false; // ”jŠü‚ÉŠJ•ú
        }
    }
}
