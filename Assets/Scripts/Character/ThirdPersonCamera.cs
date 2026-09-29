using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 focus = target.position;
        float distance = 4f;
        float height = 1.6f;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            focus = bounds.center;
            float size = bounds.extents.magnitude;
            distance = Mathf.Max(size * 3f, 0.5f);
            height = size * 0.5f;
        }

        transform.position = focus + new Vector3(0f, height, -distance);
        transform.LookAt(focus);
    }
}
