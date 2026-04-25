using UnityEngine;
using UnityEngine.AI;

public class AnimatedLinkTraversal : MonoBehaviour
{
    public float jumpDuration = 0.8f;
    public float jumpHeight = 2.5f;
    public AnimationCurve horizontalCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private NavMeshAgent agent;

    public bool IsJumpingTraversal { get; private set; }
    private float timer;

    private Vector3 start;
    private Vector3 end;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent.isOnOffMeshLink && !IsJumpingTraversal)
        {
            StartJump();
        }

        if (IsJumpingTraversal)
        {
            UpdateJump();
        }
    }

    void StartJump()
    {
        var link = agent.currentOffMeshLinkData;

        // Берём РЕАЛЬНУЮ позицию
        start = transform.position;
        end = link.endPos + Vector3.up * agent.baseOffset;

        timer = 0f;
        IsJumpingTraversal = true;

        agent.updatePosition = false;
    }

    void UpdateJump()
    {
        
        
        timer += Time.deltaTime;
        float t = timer / jumpDuration;

        // горизонталь отдельно
        float horizT = horizontalCurve.Evaluate(t);
        Vector3 flatPos = Vector3.Lerp(start, end, horizT);

        // чистая парабола
        float height = 4 * jumpHeight * t * (1 - t);

        Vector3 finalPos = flatPos + Vector3.up * height;
        transform.position = finalPos;

        Vector3 direction = (end - transform.position);
        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                agent.angularSpeed * Time.deltaTime
            );
        }

        if (t >= 1f)
        {
            FinishJump();
        }
    }

    void FinishJump()
    {
        if (agent.enabled && agent.isOnNavMesh && agent.isOnOffMeshLink)
        {
            agent.CompleteOffMeshLink();
        }

        agent.updatePosition = true;
        IsJumpingTraversal = false;
    }
}