using UnityEngine;

public class CameraControls : MonoBehaviour
{

    public void ScaleUp()
    {
        Camera camera = GetComponent<Camera>();

        camera.orthographicSize -= 2;
    }
    public void ScaleDown()
    {
        Camera camera = GetComponent<Camera>();

        camera.orthographicSize += 2;
    }
}
