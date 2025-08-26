using UnityEngine;

[ExecuteAlways] // excutes even if game not run

// This script can be used outside the Editor folder (otherwise Unity won't allow it to be attached to a GameObject).
// Before building, remove this script from all objects to avoid warnings.
// This is due to the use of the Handles library from UnityEditor.

#if UNITY_EDITOR
public class AlwaysSpawnSpotsGizmos : MonoBehaviour
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
#endif