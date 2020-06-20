using UnityEngine;
using System.Collections;
using System;

public class InteractiveFly : MonoBehaviour
{
    
    public TextAsset TxtFile;   //建立TextAsset
    private string Mytxt;       //用来存放文本内容

    private Vector3[] startPosition;
    private Vector3[] endPosition;
    private Quaternion[] startRotation;
    private Quaternion[] endRotation;
    private float[] pathLength; // Total distance in a path.

    public float speed;// Movement speed in units per second.
    private float startTime; // Time when the movement started in a path.
    private int currentPath; //当前路径索引
    private int pathNum;

    void Start()
    {
        startTime = Time.time;
        currentPath = 0;
        speed = 15.0f;

        //解析FlyInput.txt文件到data
        Mytxt = ((TextAsset)Resources.Load("FlyInput")).text;
        string[] lines = Mytxt.Split('\n');
        float[,] data = new float[lines.Length, 6];
        for (int i =0; i < lines.Length; i++)
        {
            //Debug.Log(lines.Length);
            string[] line = lines[i].Split(' ');
            for (int j = 0; j < 6; j++) 
                data[i,j] = Convert.ToSingle(line[j]);
        }

        //解析data，对各路径的起点和终点赋值(data.Length个点有data.Length-1条路径)
        pathNum = lines.Length - 1;
        startPosition = new Vector3[pathNum];
        endPosition = new Vector3[pathNum];
        startRotation = new Quaternion[pathNum];
        endRotation = new Quaternion[pathNum];
        pathLength = new float[pathNum];
        for (int i = 0; i < pathNum; i++)
        {
            //Debug.Log(pathNum);
            startPosition[i] = new Vector3(data[i, 0], data[i, 1], data[i, 2]);
            endPosition[i] = new Vector3(data[i+1, 0], data[i+1, 1], data[i+1, 2]);
            startRotation[i] = Quaternion.Euler(data[i, 3], data[i, 4], data[i, 5]);
            endRotation[i] = Quaternion.Euler(data[i+1, 3], data[i+1, 4], data[i+1, 5]);

            // Calculate the path length.
            pathLength[i] = Vector3.Distance(startPosition[i], endPosition[i]);

        }

    }
    void Update()
    {  
        float distCovered = (Time.time - startTime) * speed;//当前路径走了多长
        float fractionOfPath = distCovered / pathLength[currentPath];
       
        //通过插值改变物体的位置和姿态角
        transform.position = Vector3.Lerp(startPosition[currentPath], endPosition[currentPath], fractionOfPath);
        transform.rotation = Quaternion.Lerp(startRotation[currentPath], endRotation[currentPath], fractionOfPath);

        //若当前路径走完了，且未到终点，则切换到下一个路径，并重置startTime
        //Debug.Log(fractionOfPath);
        if (fractionOfPath > 1.0f && currentPath < pathNum-1 )
        {
            startTime = Time.time;
            currentPath++;
        }
    }
}
