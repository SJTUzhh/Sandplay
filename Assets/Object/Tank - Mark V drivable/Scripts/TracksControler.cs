using UnityEngine;
using System.Collections;

public class TracksControler : MonoBehaviour {

    private float offsetL = 0f;
    private float offsetR = 0f;
    public Renderer trackLeft;
    public Renderer trackRight;
    public Rigidbody Rig;
    private Vector3 vel;
    private float speed;
    private bool Front = false;
    private bool Back = false;
    private bool turn = true;


    void Start() {
        
    }

    void pressFunc()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if (speed < 0.3f)
            {
                Front = true;
                Back = false;
            }
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (speed < 0.3f)
            {
                Back = true;
                Front = false;
            }
        }
        
    }

    void FixedUpdate()
    {

       

    }

    void Update () {

        pressFunc();

        // Tracks rotation
        if(Rig.angularVelocity.magnitude > 0.1f && speed < 1.5f)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                offsetL = offsetL + 0.002f;
                offsetR = offsetR - 0.002f;
                
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                offsetL = offsetL - 0.002f;
                offsetR = offsetR + 0.002f;
                
            }
            turn = true;

        }
        else
        {
            turn = false;
        }

        // Tracks move ,depends on current speed
        if (speed > 0 && !turn)
        {
            if (Front)
            {
                offsetL = offsetL - speed / 1000;
                offsetR = offsetR - speed / 1000;
            }
            

            if (Back)
            {
                offsetL = offsetL + speed / 1000;
                offsetR = offsetR + speed / 1000;
            }
              
        }
        // Speed 
        vel = Rig.velocity;
        speed = vel.magnitude;
        // scrolling
        trackLeft.material.SetTextureOffset("_MainTex", new Vector2(offsetL, 0));
        trackRight.material.SetTextureOffset("_MainTex", new Vector2(offsetR, 0));

    }

}
