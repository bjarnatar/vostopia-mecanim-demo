using UnityEngine;
using UnityEditor;
using System.Collections;

public class FixFemaleAnimation
{
    /**
     * Creates a new static curve on RightArm and LeftArm if none exist in the animation clip. 
     * This is done to make the female scaling additive animation clip work properly
     */
    [MenuItem("Assets/Vostopia/Fix Female Animation Clip")]
    public static void Execute()
    {
        AnimationClip clip = Selection.activeObject as AnimationClip;
        if (clip == null)
        {
            Debug.LogError("Please select an animation clip");
            return;
        }

        EnsureArmCurve(clip, -0.1445461f, "Reference/Hips/Spine/Spine1/Spine2/Spine3/LeftShoulder/LeftArm", "m_LocalPosition.x");
        EnsureArmCurve(clip, 0.1445461f, "Reference/Hips/Spine/Spine1/Spine2/Spine3/RightShoulder/RightArm", "m_LocalPosition.x");
    }

    private static void EnsureArmCurve(AnimationClip clip, float position, string path, string propertyName)
    {
        AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, path, typeof(Transform), propertyName);
        if (curve == null)
        {
            Debug.Log("No curve found for " + path + "." + propertyName + ", creating new");
            curve = new AnimationCurve(new Keyframe[] {
                new Keyframe(0, position),
                new Keyframe(clip.length, position),
            });
            clip.SetCurve(path, typeof(Transform), "m_LocalPosition.x", curve);
        }
    }
}
