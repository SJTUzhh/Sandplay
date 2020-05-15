using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mainCam : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject moveCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick_main()
    {
        mainCamera.SetActive(true);
        moveCamera.SetActive(false);
    }
    public void onClick_move()
    {
        mainCamera.SetActive(false);
        moveCamera.SetActive(true);
    }
}
