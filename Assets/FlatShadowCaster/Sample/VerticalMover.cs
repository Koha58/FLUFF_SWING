using System.Collections;
using UnityEngine;

public class VerticalMover : MonoBehaviour
{
    [SerializeField] private float distance = 2f;
    [SerializeField] private float time = 1f;

    private Vector3 initialPosition;
    private Coroutine moveCoroutine;
    private bool isMoving = false;

    void Start()
    {
        initialPosition = transform.position;
        StartMoving();
    }

    public void StartMoving()
    {
        if (!isMoving)
        {
            moveCoroutine = StartCoroutine(VerticalMovementLoop());
        }
    }

    public void StopMoving()
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
            isMoving = false;
        }
    }

    private IEnumerator VerticalMovementLoop()
    {
        isMoving = true;

        while (true)
        {
            yield return StartCoroutine(MoveToPosition(initialPosition + Vector3.down * distance));
            yield return StartCoroutine(MoveToPosition(initialPosition));
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / time;
            progress = Mathf.SmoothStep(0f, 1f, progress);
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        transform.position = targetPosition;
    }

    void OnDestroy()
    {
        StopMoving();
    }
} 