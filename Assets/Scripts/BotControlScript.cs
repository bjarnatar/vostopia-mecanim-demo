using UnityEngine;
using System.Collections;

using Photon;

// Require these components when using this script
[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (CapsuleCollider))]
[RequireComponent(typeof (Rigidbody))]
public class BotControlScript : Photon.MonoBehaviour
{
	[System.NonSerialized]					
	public float lookWeight;					// the amount to transition when using head look
	
	[System.NonSerialized]
	public Transform enemy;						// a transform to Lerp the camera to during head look
	
	public float animSpeed = 1.5f;				// a public setting for overall animator animation speed
	public float lookSmoother = 3f;				// a smoothing setting for camera motion
	public bool useCurves;						// a setting for teaching purposes to show use of curves
	public float minimumFallingHeight = 0.5f;	// If the character is mid-air over this height, the falling flag will be set

	public bool downRayHit = false;
	public float heightAboveGround = 0f;

	private Animator anim;							// a reference to the animator on the character
	private AnimatorStateInfo currentBaseState;			// a reference to the current state of the animator, used for base layer
	private AnimatorStateInfo layer2CurrentState;	// a reference to the current state of the animator, used for layer 2
	private CapsuleCollider col;					// a reference to the capsule collider of the character
	
	private float netSpeed;
	private float netDirection;
	private bool netFalling;
	private bool netJump;
	private bool netWave;
	private bool netLookAtEnemy;

	static int idleState = Animator.StringToHash("Base Layer.Idle");	
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");			// these integers are references to our animator's states
	static int jumpState = Animator.StringToHash("Base Layer.Jump");				// and are used to check state for various actions to occur
	static int jumpDownState = Animator.StringToHash("Base Layer.JumpDown");		// within our FixedUpdate() function below
	static int fallState = Animator.StringToHash("Base Layer.Fall");
	static int rollState = Animator.StringToHash("Base Layer.Roll");
	static int waveState = Animator.StringToHash("Layer2.Wave");
	

	void Start ()
	{
		// initialising reference variables
		anim = GetComponent<Animator>();					  
		col = GetComponent<CapsuleCollider>();				
		enemy = GameObject.Find("Enemy").transform;	
		if(anim.layerCount ==2)
			anim.SetLayerWeight(1, 1);
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			//We own this player: send the others our data
			stream.SendNext(netSpeed);
			stream.SendNext(netDirection);
			stream.SendNext(netFalling);
			stream.SendNext(netJump);
			stream.SendNext(netWave);
			stream.SendNext(netLookAtEnemy);
		}
		else
		{
			//Network player, receive data
			netSpeed = (float)stream.ReceiveNext();
			netDirection = (float)stream.ReceiveNext();
			netFalling = (bool)stream.ReceiveNext();
			netJump = (bool)stream.ReceiveNext();
			netWave = (bool)stream.ReceiveNext();
			netLookAtEnemy = (bool)stream.ReceiveNext();
		}
	}
	
	void FixedUpdate ()
	{
		if (!anim.avatar || !anim.avatar.isValid)
			return;

		anim.speed = animSpeed;								// set the speed of our animator to the public variable 'animSpeed'
		anim.SetLookAtWeight(lookWeight);					// set the Look At Weight - amount to use look at IK vs using the head's animation
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);	// set our currentState variable to the current state of the Base Layer (0) of animation

		if (anim.layerCount == 2)
			layer2CurrentState = anim.GetCurrentAnimatorStateInfo(1);	// set our layer2CurrentState variable to the current state of the second Layer (1) of animation

		if (photonView.isMine)
		{
			netDirection = Input.GetAxis("Horizontal");			// setup h variable as our horizontal input axis
			netSpeed = Input.GetAxis("Vertical");				// setup v variables as our vertical input axis
			// Check if we are falling
			// Raycast down from the center of the character.. 
			Ray downRay = new Ray(transform.position + Vector3.up, -Vector3.up);
			RaycastHit downHitInfo = new RaycastHit();

			//bool downRayHit = Physics.Raycast(downRay, out downHitInfo);
			downRayHit = Physics.Raycast(downRay, out downHitInfo);
			heightAboveGround = 0f;
			if (downRayHit)
			{
				heightAboveGround = downHitInfo.distance;
				if (heightAboveGround > minimumFallingHeight)
				{
					netFalling = true;
				}
				else
				{
					netFalling = false;
				}
			}
			else
			{
				netFalling = false;
			}

			// LOOK AT ENEMY

			// if we hold Alt..
			if (Input.GetButton("Fire2"))
			{
				netLookAtEnemy = true;
			}

			// STANDARD JUMPING

			// if we are currently in a state called Locomotion (see line 25), then allow Jump input (Space) to set the Jump bool parameter in the Animator to true
			if (currentBaseState.nameHash == locoState)
			{
				if (Input.GetButtonDown("Jump"))
				{
					netJump = true;
				}
			}

			// if we are in the jumping state... 
			else if (currentBaseState.nameHash == jumpState)
			{
				//  ..and not still in transition..
				if (!anim.IsInTransition(0))
				{
					if (useCurves)
						// ..set the collider height to a float curve in the clip called ColliderHeight
						col.height = anim.GetFloat("ColliderHeight");

					// reset the Jump bool so we can jump again, and so that the state does not loop 
					netJump = false;
				}

			}
			// IDLE

			// check if we are at idle, if so, let us Wave!
			else if (currentBaseState.nameHash == idleState)
			{
				if (Input.GetButtonUp("Jump"))
				{
					netWave = true;
					//anim.SetBool("Wave", true);
				}
			}
			// if we enter the waving state, reset the bool to let us wave again in future
			if (layer2CurrentState.nameHash == waveState)
			{
				netWave = false;
				//anim.SetBool("Wave", false);
			}
		}
		else
		{ // Set anim variables based on network updates

		}

		anim.SetFloat("Direction", netDirection); 			// set our animator's float parameter 'Direction' equal to the horizontal input axis		
		anim.SetFloat("Speed", netSpeed);					// set our animator's float parameter 'Speed' equal to the vertical input axis				
		anim.SetBool("Falling", netFalling);
		anim.SetBool("Jump", netJump);
		anim.SetBool("Wave", netWave);

		if (netLookAtEnemy)
		{
			// ...set a position to look at with the head, and use Lerp to smooth the look weight from animation to IK (see line 54)
			anim.SetLookAtPosition(enemy.position);
			lookWeight = Mathf.Lerp(lookWeight, 1f, Time.deltaTime * lookSmoother);
		}
		// else, return to using animation for the head by lerping back to 0 for look at weight
		else
		{
			lookWeight = Mathf.Lerp(lookWeight, 0f, Time.deltaTime * lookSmoother);
		}

		// if we are in the jumping state... 
		if (currentBaseState.nameHash == jumpState)
		{
			//  ..and not still in transition..
			if (!anim.IsInTransition(0))
			{
				if (useCurves)
					// ..set the collider height to a float curve in the clip called ColliderHeight
					col.height = anim.GetFloat("ColliderHeight");
			}
		}
		// JUMP DOWN AND ROLL 

		// if we are jumping down, set our Collider's Y position to the float curve from the animation clip - 
		// this is a slight lowering so that the collider hits the floor as the character extends his legs
		else if (currentBaseState.nameHash == jumpDownState)
		{
			col.center = new Vector3(0, anim.GetFloat("ColliderY"), 0);
		}

		// if we are falling, set our Grounded boolean to true when our character's root 
		// position is less that 0.6, this allows us to transition from fall into roll and run
		// we then set the Collider's Height equal to the float curve from the animation clip
		else if (currentBaseState.nameHash == fallState)
		{
			col.height = anim.GetFloat("ColliderHeight");
		}

		// if we are in the roll state and not in transition, set Collider Height to the float curve from the animation clip 
		// this ensures we are in a short spherical capsule height during the roll, so we can smash through the lower
		// boxes, and then extends the collider as we come out of the roll
		// we also moderate the Y position of the collider using another of these curves on line 128
		else if (currentBaseState.nameHash == rollState)
		{
			if (!anim.IsInTransition(0))
			{
				if (useCurves)
					col.height = anim.GetFloat("ColliderHeight");

				col.center = new Vector3(0, anim.GetFloat("ColliderY"), 0);

			}
		}

	}
}
