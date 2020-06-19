using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform lookAt;
    public Transform camTransform;
    public Transform firePoint;
    public Transform canon;
    public Camera cam;

    private float currentX = 0f;
    private float currentY = 0f;
    private float minY = -40f;
    private float maxY = 35f;
    private float minX = -15f;
    private float maxX = 40f;
    private Quaternion startRotation;
    

    void Start () {

        camTransform = transform;
        // Fire cursor
        firePoint.transform.position = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, cam.nearClipPlane + 12f));
        startRotation = canon.transform.rotation;

    }
	
	void Update () {

        // Canon rotation
        canon.transform.forward = cam.transform.forward;
        canon.transform.rotation *= startRotation;

        currentY += Input.GetAxis("Mouse X");
        currentX += Input.GetAxis("Mouse Y");

        // max and min rotation
        currentY = Mathf.Clamp(currentY, minY, maxY);
        currentX = Mathf.Clamp(currentX, minX, maxX);

    }


    void LateUpdate()
    {

        lookAt.localRotation = Quaternion.Euler(currentX, currentY, 0);

    }
}
