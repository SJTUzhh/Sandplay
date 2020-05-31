using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class mainCam : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject moveCamera;
    public GameObject playCamera;

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
    }
    public void onClick_move()
    {
        beforeplay();

        play();
        Invoke("change1",2f);

    }

    
}
