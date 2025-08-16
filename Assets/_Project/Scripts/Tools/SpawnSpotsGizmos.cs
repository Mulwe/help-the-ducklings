using UnityEngine;

[ExecuteAlways] // ����� �������� ���� � ��������� ��� ������� ����
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
                Gizmos.DrawSphere(child.position, gizmoRadius);
                UnityEditor.Handles.Label(child.position + Vector3.up * 0.3f, $"{title} {index}");
                index++;
            }
        }
    }
}
