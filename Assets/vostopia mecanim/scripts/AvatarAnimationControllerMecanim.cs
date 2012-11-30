using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AvatarController))]
[RequireComponent(typeof(Animator))]
public class AvatarAnimationControllerMecanim : AvatarAnimationController
{
    public Avatar FemaleAvatar;
    public Avatar MaleAvatar;

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

    private int GetGenderLayer(AvatarGender gender)
    {
        if (gender == AvatarGender.Female)
        {
            for (int i = 0; i < CachedAnimator.layerCount; i++)
            {
                if (CachedAnimator.GetLayerName(i).ToLower() == "female")
                {
                    return i;
                }
            }
        }
        return 0;
    }

    /** 
     * Called on the same frame the avatar is loaded. Use this to modify the avatar after
     * it's loaded, e.g. set the Mecanim Avatar, scale mesh, etc.
     */
    public void OnAvatarGenderUpdated(AvatarGender gender)
    {
        var avatar = gender == AvatarGender.Male ? MaleAvatar : FemaleAvatar;
        CachedAnimator.avatar = avatar;

        var femaleLayer = GetGenderLayer(AvatarGender.Female);
        if (femaleLayer != 0)
        {
            var femaleLayerWeight = gender == AvatarGender.Female ? 1f : 0f;
            CachedAnimator.SetLayerWeight(femaleLayer, femaleLayerWeight);
        }
    }
}

