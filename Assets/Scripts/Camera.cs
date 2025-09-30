using UnityEngine;
using UnityEngine.InputSystem;

public class KameraPracenje : MonoBehaviour
{
    public Transform lik;
    public Vector3 offset = new Vector3(5, 3, 0);
    public Vector3 lookOffset = new Vector3(0, 1.5f, 0);

    private float yaw = 0f;
    public float brzinaRotacije = 15f;

    void LateUpdate()
    {
        if (lik == null) return;

        if (Mouse.current != null)
        {
            float mouseX = Mouse.current.delta.ReadValue().x;
            yaw += mouseX * brzinaRotacije * Time.deltaTime;
        }

        Quaternion rotacija = Quaternion.Euler(0, yaw, 0);

        transform.position = lik.position + rotacija * offset;
        transform.LookAt(lik.position + lookOffset);
    }
}
