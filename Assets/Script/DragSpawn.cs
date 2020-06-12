using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
 
public class DragSpawn : MonoBehaviour, IPointerDownHandler
{
    public GameObject prefab;
    //正在拖拽的物体
    private GameObject _objDragSpawning;
 
    //是否正在拖拽
    private bool _isDragSpawning = false;
    
	// Update is called once per frame
	void Update () {
        if (_isDragSpawning)
        {
            //刷新位置
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            _objDragSpawning.transform.position = ray.GetPoint(10);
            //结束拖拽
            if (Input.GetMouseButtonUp(0))
            {
                _isDragSpawning = false;
                _objDragSpawning = null;
            }
        }
    }

    //按下鼠标时开始生成实体
    public void OnPointerDown(PointerEventData eventData)
    {
        // GameObject prefab = Resources.Load<GameObject>("Sphere");
        Debug.Log("1");
        if(prefab != null)
        {
            Debug.Log("has prefab");

            _objDragSpawning = Instantiate(prefab);
            _isDragSpawning = true;
        }
        else{
            Debug.Log("no prefab");
        }
            
    }
 
}