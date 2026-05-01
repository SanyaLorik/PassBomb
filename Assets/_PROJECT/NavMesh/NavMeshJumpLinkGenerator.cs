using System.Collections.Generic;
using Unity.AI.Navigation; // Для NavMeshSurface
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshJumpLinkGenerator : MonoBehaviour
{
    [Header("Настройки обнаружения прыжков")]
    [SerializeField] private float maxJumpHeight = 2f;
    [SerializeField] private float maxJumpDistance = 5f;
    [SerializeField] private float minJumpDistance = 1f;
    [SerializeField] private LayerMask obstacleMask = -1;

    [Header("Настройки генерации")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool visualizeGizmos = true;
    [SerializeField] private Color gizmoColor = Color.green;

    [Header("Настройки линков")]
    [SerializeField] private float linkWidth = 0.5f;
    [SerializeField] private int linkCostModifier = 2;
    [SerializeField] private bool biDirectional = true;

    [Header("Результаты")]
    [SerializeField] private List<NavMeshLink> generatedLinks = new List<NavMeshLink>();

    private NavMeshSurface navMeshSurface;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateJumpLinks();
        }
    }

    [ContextMenu("Generate Jump Links")]
    public void GenerateJumpLinks()
    {
        // Очищаем старые линки
        ClearGeneratedLinks();

        // Получаем NavMeshSurface
        navMeshSurface = GetComponent<NavMeshSurface>();
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface не найден на этом объекте!");
            return;
        }

        // Получаем все вершины NavMesh
        List<Vector3> navMeshVertices = GetNavMeshVertices();

        // Ищем возможные прыжки
        List<JumpLinkData> jumpLinks = FindJumpLinks(navMeshVertices);

        // Создаем NavMeshLink'и
        CreateNavMeshLinks(jumpLinks);

        Debug.Log($"Создано {generatedLinks.Count} прыжковых линков");
    }

    private List<Vector3> GetNavMeshVertices()
    {
        List<Vector3> vertices = new List<Vector3>();

        // Получаем триангуляцию NavMesh
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        for (int i = 0; i < triangulation.vertices.Length; i++)
        {
            Vector3 vertex = triangulation.vertices[i];

            // Проверяем, принадлежит ли вершина нашей поверхности
            if (IsPointOnNavMesh(vertex))
            {
                vertices.Add(vertex);
            }
        }

        // Удаляем дубликаты
        vertices = RemoveDuplicates(vertices, 0.5f);

        return vertices;
    }

    private bool IsPointOnNavMesh(Vector3 point)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(point, out hit, 0.5f, NavMesh.AllAreas);
    }

    private List<Vector3> RemoveDuplicates(List<Vector3> points, float tolerance)
    {
        List<Vector3> uniquePoints = new List<Vector3>();

        foreach (Vector3 point in points)
        {
            bool isDuplicate = false;

            foreach (Vector3 unique in uniquePoints)
            {
                if (Vector3.Distance(point, unique) < tolerance)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                uniquePoints.Add(point);
            }
        }

        return uniquePoints;
    }

    private List<JumpLinkData> FindJumpLinks(List<Vector3> vertices)
    {
        List<JumpLinkData> jumpLinks = new List<JumpLinkData>();

        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = i + 1; j < vertices.Count; j++)
            {
                Vector3 start = vertices[i];
                Vector3 end = vertices[j];

                // Проверяем, подходит ли для прыжка
                if (IsValidJump(start, end))
                {
                    jumpLinks.Add(new JumpLinkData(start, end));
                }
            }
        }

        return jumpLinks;
    }

    private bool IsValidJump(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        float heightDifference = Mathf.Abs(start.y - end.y);
        Vector3 direction = (end - start).normalized;

        // Проверяем расстояние
        if (distance < minJumpDistance || distance > maxJumpDistance)
            return false;

        // Проверяем высоту прыжка
        if (heightDifference > maxJumpHeight)
            return false;

        // Проверяем, нет ли препятствий между точками
        if (Physics.Linecast(start, end, obstacleMask))
            return false;

        // Проверяем, что точки не соединены напрямую на NavMesh
        if (!ArePointsConnectedOnNavMesh(start, end))
        {
            return true;
        }

        return false;
    }

    private bool ArePointsConnectedOnNavMesh(Vector3 start, Vector3 end)
    {
        NavMeshPath path = new NavMeshPath();

        if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
        {
            // Если путь существует и он очень короткий или прямой
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                // Проверяем длину пути
                float pathLength = GetPathLength(path);
                float directDistance = Vector3.Distance(start, end);

                // Если путь ненамного длиннее прямого расстояния, то они уже соединены
                if (pathLength < directDistance * 1.2f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private float GetPathLength(NavMeshPath path)
    {
        float length = 0f;

        if (path.corners.Length < 2)
            return length;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }

    private void CreateNavMeshLinks(List<JumpLinkData> jumpLinks)
    {
        GameObject linksParent = new GameObject("GeneratedJumpLinks");
        linksParent.transform.SetParent(transform);

        foreach (JumpLinkData link in jumpLinks)
        {
            GameObject linkObject = new GameObject($"NavMeshJumpLink_{link.startPoint.x:F1}_{link.startPoint.z:F1}_to_{link.endPoint.x:F1}_{link.endPoint.z:F1}");
            linkObject.transform.SetParent(linksParent.transform);

            // Добавляем компонент NavMeshLink (современный)
            NavMeshLink navMeshLink = linkObject.AddComponent<NavMeshLink>();

            // Настраиваем линк
            navMeshLink.startPoint = link.startPoint;
            navMeshLink.endPoint = link.endPoint;
            navMeshLink.width = linkWidth;
            navMeshLink.costModifier = linkCostModifier;
            navMeshLink.bidirectional = biDirectional;
            navMeshLink.area = 0;
            navMeshLink.autoUpdate = false;

            generatedLinks.Add(navMeshLink);
        }
    }

    [ContextMenu("Clear Generated Links")]
    public void ClearGeneratedLinks()
    {
        foreach (NavMeshLink link in generatedLinks)
        {
            if (link != null)
                DestroyImmediate(link.gameObject);
        }
        generatedLinks.Clear();
    }

    private void OnDrawGizmos()
    {
        if (!visualizeGizmos)
            return;

        foreach (NavMeshLink link in generatedLinks)
        {
            if (link != null)
            {
                Gizmos.color = gizmoColor;
                Vector3 start = link.startPoint;
                Vector3 end = link.endPoint;

                Gizmos.DrawLine(start, end);

                // Рисуем стрелку направления
                Vector3 direction = (end - start).normalized;
                Vector3 midPoint = (start + end) / 2;
                Gizmos.DrawRay(midPoint, direction * 0.5f);

                // Рисуем сферы на концах
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(start, 0.2f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(end, 0.2f);

                // Рисуем ширину линка
                Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
                Gizmos.DrawLine(start - perpendicular * link.width * 0.5f, end - perpendicular * link.width * 0.5f);
                Gizmos.DrawLine(start + perpendicular * link.width * 0.5f, end + perpendicular * link.width * 0.5f);
            }
        }
    }

    [ContextMenu("Rebake NavMesh")]
    public void RebuildNavMesh()
    {
        if (navMeshSurface == null)
            navMeshSurface = GetComponent<NavMeshSurface>();

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh перестроен");
        }
    }

    [System.Serializable]
    private class JumpLinkData
    {
        public Vector3 startPoint;
        public Vector3 endPoint;

        public JumpLinkData(Vector3 start, Vector3 end)
        {
            startPoint = start;
            endPoint = end;
        }
    }
}