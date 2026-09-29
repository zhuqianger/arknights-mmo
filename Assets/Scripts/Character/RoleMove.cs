using UnityEngine;

public class RoleMove : MonoBehaviour
{
    void Update()
    {
        float x = 0f;
        float z = 0f;
        if (Input.GetKey(KeyCode.W))
            z += 1f;
        if (Input.GetKey(KeyCode.S))
            z -= 1f;
        if (Input.GetKey(KeyCode.A))
            x -= 1f;
        if (Input.GetKey(KeyCode.D))
            x += 1f;

        Vector3 dir = new Vector3(x, 0f, z);
        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        transform.position += dir * 5f * Time.deltaTime;
    }
}
