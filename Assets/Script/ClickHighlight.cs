using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickHighlight : MonoBehaviour
{
    //public GameObject gameCheck;
    public int clickType; //0 为选择物体， 1 为move物体
    //public bool clickTypeKnown;
    public bool leftCtrl;
    public List<GameObject> gameCheckList = new List<GameObject>();

    void Start()
    {
        //clickTypeKnown = true;
        leftCtrl = false;
    }


    void Update()
    {
        //clickTypeKnown = true;
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            leftCtrl = true;
            Debug.Log("ctrl");
        }
        else if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            leftCtrl = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            //clickTypeKnown = false;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var hitObj = hit.collider.gameObject;
                SetObjectHighlight(hitObj);
                Debug.Log(hitObj);
            }
            //clickTypeKnown = true;
        }
    }
    /// <summary>
    /// 设置物体高亮
    /// </summary>
    /// <param name="obj"></param>
    public void SetObjectHighlight(GameObject obj)
    {
        //if (gameCheck == null)
        if(!(gameCheckList != null && gameCheckList.Count > 0)) //gameCheckList为空
        {
            if(obj != GameObject.Find("Terrain") && obj != GameObject.Find("sea") && obj != GameObject.Find("land"))
            {
                AddComponent(obj);
                gameCheckList.Add(obj);
                clickType = 0;
            }
            else
            {
                //clickType = 1;
            }
                
        }
        else              //gameCheckList非空
        {
            if (obj == GameObject.Find("Terrain") || obj == GameObject.Find("sea") || obj == GameObject.Find("land"))
                clickType = 1;

            else if(gameCheckList.Contains(obj)) //重复点击某物体
            {
                if(gameCheckList.Count == 1)
                {
                    RemoveComponent(obj); //取消该物体的选中
                    gameCheckList.Remove(obj);
                }
                else
                {
                    if (leftCtrl)
                    {
                        RemoveComponent(obj); //取消该物体的选中
                        gameCheckList.Remove(obj);
                    }
                    else //没有按下leftCtrl
                    {
                        //取消之前所有物体的选中
                        for (int i = gameCheckList.Count - 1; i >= 0; i--)
                        {
                            if(gameCheckList[i] != obj)
                                RemoveComponent(gameCheckList[i]);
                        }
                        //Debug.Log("here");
                        for (int i = gameCheckList.Count - 1; i >= 0; i--)//倒序可避免删除引起索引变化而出错
                        {
                            if (gameCheckList[i] != obj)
                                gameCheckList.Remove(gameCheckList[i]);
                        }

                        //先全删，后再加obj的方式不行，对obj进行RemoveComponent后再AddComponent无效，原因不明，
                        //AddComponent(obj); 
                        //gameCheckList.Add(obj);
                    }
                }
                clickType = 0;

            }
            else //更换或增加选中的物体
            {
                if (leftCtrl)
                {
                    AddComponent(obj);
                    gameCheckList.Add(obj);
                }
                else
                {
                    //取消之前所有物体的选中
                    for (int i = 0; i < gameCheckList.Count; i++)
                    {
                        RemoveComponent(gameCheckList[i]);    
                    }
                    gameCheckList.Clear();//
                    AddComponent(obj);
                    gameCheckList.Add(obj);

                }             
                clickType = 0;
            }
        }
    }
    /// <summary>
    /// 添加高亮组件
    /// </summary>
    /// <param name="obj"></param>
    public void AddComponent(GameObject obj)
    {
        if (obj.GetComponent<SpectrumController>() == null)
        {
            obj.AddComponent<SpectrumController>();
        }
        
    }
    /// <summary>
    /// 移出组件
    /// </summary>
    /// <param name="obj"></param>
    public void RemoveComponent(GameObject obj)
    {
        if (obj.GetComponent<SpectrumController>() != null)
        {
            Destroy(obj.GetComponent<SpectrumController>());
        }

        if (obj.GetComponent<HighlightableObject>() != null)
        {
            Destroy(obj.GetComponent<HighlightableObject>());
        }

    }
}
