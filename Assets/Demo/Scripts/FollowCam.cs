using UnityEngine;
using System.Collections;

public class FollowCam : MonoBehaviour 
{
    public GameObject Target;
    public Vector3 LookAtOffset = new Vector3(0.0f, 1.5f, 0.0f);
    public Vector3 PositionOffset = new Vector3(0, 1, 2);

    private int mLayerMask;

	// Use this for initialization
	void Start () 
    {
        mLayerMask  = ~LayerMask.NameToLayer("Level");
	}
	
	// Update is called once per frame
	void LateUpdate () 
    {
        if (Target != null)
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = Target.transform.position;

            // attempt to follow the target
            Quaternion offsetRotation = Quaternion.FromToRotation(new Vector3(PositionOffset.x, 0, PositionOffset.z), new Vector3(currentPosition.x - targetPosition.x, 0, currentPosition.z - targetPosition.z));
            Vector3 desiredOffset = offsetRotation * new Vector4(PositionOffset.x, 0.0f, PositionOffset.z, 0.0f);
            desiredOffset.y = PositionOffset.y;

            // Check for camera collisions with the level geometry.  
            // In a real game this gets way more complicated, but this will do to prevent the demo camera blatantly clipping through the level boundaries.
            RaycastHit rayHitInfo;
            if (Physics.Raycast(targetPosition, desiredOffset.normalized, out rayHitInfo, desiredOffset.magnitude,mLayerMask))
            {
                desiredOffset = (rayHitInfo.point-targetPosition);
            }

            Vector3 desiredPosition = targetPosition + desiredOffset;

            transform.position = desiredPosition;
            transform.LookAt(targetPosition+LookAtOffset);
        }
	}
}
