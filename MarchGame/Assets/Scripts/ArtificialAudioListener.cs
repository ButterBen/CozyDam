using UnityEngine;

public class ArtificialAudioListener : MonoBehaviour
{
    public Camera targetCamera; 

    void Update()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main; 
        }

        if (targetCamera != null)
        {
            float orthoSize = targetCamera.orthographicSize;

            float newZ = Mathf.Lerp(0f, 15f, (orthoSize - 3f) / (17f - 3f));

            transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
        }
    }
}
