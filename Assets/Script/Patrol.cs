using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.UI;
using System;
public class Patrol : MonoBehaviour
{
    public PlayableDirector playableDirector;

    public Button btn;
    public Text text1;
    private bool ispatrol = false;
    private int time_interval = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(ispatrol){
            time_interval++;
            Debug.Log(time_interval);
            if(time_interval ==1)
            {
                playableDirector.Play();

                text1.text = "巡逻中...";
                // btn.SetActive(false);
                btn.enabled= false;
                btn.interactable = false;
            }            
            else if(time_interval ==3200)
            {
                text1.text = "开始巡逻";
                btn.enabled= true;
                btn.interactable = true;

                // btn.SetActive(true);
                time_interval = 0;
                ispatrol = false;
            }  

        }
    }

    public void onClick()
    {
        ispatrol = true;
    }
}
