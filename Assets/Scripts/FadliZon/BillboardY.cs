using UnityEngine;

public class BillboardY : MonoBehaviour
{
    [Tooltip("Biasanya Main Camera di XR Origin")]
    public Transform target;

    [Tooltip("Kalau kebalik, centang ini")]
    public bool invertFacing = false;

    void LateUpdate()
    {
        if (target == null) return;

        // Arah dari UI ke kamera
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        // Kalau masih kebalik, balikkan arah
        if (invertFacing)
            dir = -dir;

        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        transform.rotation = rot;
    }
}
