using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class mainCam : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject moveCamera;
    public GameObject playCamera;
    private bool isMoveCamera = false;
    public PlayableDirector playableDirector;
    public PlayableDirector back_playableDirector;
// Start is called before the first frame update
    void Start()
    {
        mainCamera.SetActive(true);
        moveCamera.SetActive(false);
        playCamera.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Escape)&&isMoveCamera)
        // {
        //     onClick_main();
        //     isMoveCamera = false;
        //     return;
        // }
        // if(Input.GetKeyDown(KeyCode.Escape)&&!isMoveCamera){
        //     onClick_move();
        //     isMoveCamera = true;
        //     return;
        // }

    }
    void beforeplay()
    {
        mainCamera.SetActive(false);
        moveCamera.SetActive(false);
        playCamera.SetActive(true);
    }
    void play()
    {
        playableDirector.Play();
    }
    void backplay()
    {
        back_playableDirector.Play();
    }
    void change()
    {
        mainCamera.SetActive(true);
        moveCamera.SetActive(false);
        playCamera.SetActive(false);
    }
    void change1()
    {
        mainCamera.SetActive(false);
        moveCamera.SetActive(true);
        playCamera.SetActive(false);

    }
    
    public void onClick_main()
    {
        beforeplay();
        backplay();       
        Invoke("change",2f);
        GameObject.Find("feiji1").GetComponent<fps1>().enabled=false;
        // GameObject.Find("SelectObjManager").GetComponent<DragObject>().enabled=true;    
        // GameObject.Find("SelectObjManager").GetComponent<SelectObjManager>().enabled=true;  

    }
    public void onClick_move()
    {
        isMoveCamera=true;
        beforeplay();
        play();
        Invoke("change1",2f);
        // GameObject.Find("SelectObjManager").GetComponent<DragObject>().enabled=false;    
        // GameObject.Find("SelectObjManager").GetComponent<SelectObjManager>().enabled=false;  
        GameObject.Find("feiji1").GetComponent<fps1>().enabled=true;    
    }

    
}
