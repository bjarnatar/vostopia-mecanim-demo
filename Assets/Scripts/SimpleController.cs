using UnityEngine;
using System.Collections;

public class SimpleController : MonoBehaviour 
{

    CharacterController mController;
    public float WalkSpeed = 2.0f;

	// Use this for initialization
	void Start () 
    {
        mController = GetComponent<CharacterController>();
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), 0, -Input.GetAxis("Vertical"));
        float length = inputVector.magnitude;
        if (length > 0.01f)
        {

            Vector3 worldInputVector = Camera.mainCamera.cameraToWorldMatrix * inputVector;
            worldInputVector.y = 0.0f;
            worldInputVector.Normalize();
            if (mController != null)
            {
                mController.SimpleMove(worldInputVector * WalkSpeed);
            }
            transform.rotation = Quaternion.LookRotation(worldInputVector);
        }
        

	}
}
