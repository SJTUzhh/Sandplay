using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public Rigidbody Rigid;
    public float speedPower; // engine power
    public Transform centerOfmass;
    private float torque = 100f;
    private Vector3 vel;
    public float currentSpeed; // actual tank speed
    public float maxSpeed = 2.5f; // maximal tank speed
    public AudioSource engineSound;


	void Start () {
        // set centre of mass
        Rigid.centerOfMass = centerOfmass.localPosition;
        engineSound.pitch = 0.6f;
        // max rotation speed
        Rigid.maxAngularVelocity = 0.6f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    void FixedUpdate()
    {

        float turn = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(0.0f, 0.0f, moveVertical);

        if (currentSpeed < maxSpeed)
        {
            Rigid.AddRelativeForce(movement * speedPower); 
        }

        Rigid.AddTorque(transform.up * torque * turn);

    }


    void Update () {

        if (currentSpeed > 1.0f)
        {
            torque = 50f;
        }
        else
        {
            torque = 100f;
        }
        vel = Rigid.velocity;
        currentSpeed = vel.magnitude;

        engineSound.pitch = 0.6f + currentSpeed / 10;

    }
}
