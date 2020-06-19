using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClickMove : MonoBehaviour
{
    public NavMeshAgent agent;
    private void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //print(hit.point);
                //while (!GameObject.Find("Main Camera").GetComponent<ClickHighlight>().clickTypeKnown) ; //一直等到clickTypeKnown确定下来为止
                if (GameObject.Find("Main Camera").GetComponent<ClickHighlight>().clickType == 1) //点击为移动指令
                {
                    //当该gameobject被选中时，才移动
                    if (GameObject.Find("Main Camera").GetComponent<ClickHighlight>().gameCheckList.Contains(this.gameObject))
                    {
                        agent.SetDestination(hit.point);
                    }        
                }
                    
            }
        }
    }
}
