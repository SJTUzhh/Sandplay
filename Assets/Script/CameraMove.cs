using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float sensitivityMouse = 2f;
    public float sensitivetyKeyBoard = 0.1f;
    public float sensitivetyMouseWheel = 10f;    
    public float sensitivityAmt=2.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //滚轮实现镜头缩进和拉远
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            this.GetComponent<Camera>().fieldOfView = this.GetComponent<Camera>().fieldOfView - Input.GetAxis("Mouse ScrollWheel") * sensitivetyMouseWheel;
        }
        //按着鼠标右键实现视角转动
        Vector3 fwd = transform.right;
        fwd.Normalize();
        if (Input.GetMouseButton(1))
        {
            transform.Rotate(0, -Input.GetAxis("Mouse X")* sensitivityMouse, 0, Space.World);
            transform.Rotate( -Input.GetAxis("Mouse Y")* sensitivityMouse,0,0);
        }

        if (Input.GetMouseButton(2))
        {
            float sensitivityAmt=2.0f;
            Vector3 p0 = transform.position;
            Vector3 p01 = p0 - transform.right * Input.GetAxisRaw("Mouse X") * sensitivityAmt * Time.timeScale;
            Vector3 p03 = p01 - transform.up * Input.GetAxisRaw("Mouse Y") * sensitivityAmt * Time.timeScale;
            transform.position = p03;
        }
    }
}
