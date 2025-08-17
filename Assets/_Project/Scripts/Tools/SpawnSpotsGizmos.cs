using UnityEngine;

[ExecuteAlways] // чтобы работало даже в редакторе без запуска игры
public class SpawnSpotsGizmos : MonoBehaviour
{
    public Color gizmoColor = Color.green;
    public string title = "_default";
    public float gizmoRadius = 0.25f;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        int index = 0;
        foreach (Transform child in transform)
        {
            if (child != null)
            {

                Gizmos.DrawWireSphere(child.position, gizmoRadius);
                UnityEditor.Handles.Label(child.position + Vector3.up * (gizmoRadius + 0.5f), $"{title} {index}");
                index++;
            }
        }
    }
}
