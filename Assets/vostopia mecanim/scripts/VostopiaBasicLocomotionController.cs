using UnityEngine;
using System.Collections;

public class VostopiaBasicLocomotionController : MonoBehaviour
{
    public float DirectionDampTime = 0.25f;

    private Animator _CachedAnimator;
    private Animator CachedAnimator
    {
        get
        {
            if (_CachedAnimator == null)
            {
                _CachedAnimator = GetComponent<Animator>();
            }
            return _CachedAnimator;
        }
    }

    void Update()
    {
        if (CachedAnimator && CachedAnimator.avatar != null)
        {
            AnimatorStateInfo stateInfo = CachedAnimator.GetCurrentAnimatorStateInfo(0);

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            CachedAnimator.SetFloat("Speed", h * h + v * v);
            CachedAnimator.SetFloat("Direction", h, DirectionDampTime, Time.deltaTime);
            CachedAnimator.SetBool("Jump", Input.GetButton("Jump"));
        }
    }

    void OnAnimatorMove()
    {
        CharacterController controller = GetComponent<CharacterController>();

        if (controller && CachedAnimator)
        {
            Vector3 deltaPosition = CachedAnimator.deltaPosition;
            controller.Move(deltaPosition);
            transform.rotation = CachedAnimator.rootRotation;
        }
    }
}
