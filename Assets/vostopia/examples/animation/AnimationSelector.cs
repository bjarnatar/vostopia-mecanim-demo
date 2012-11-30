using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AnimationMixingSettings
{
    public AnimationClip Clip;
    public Transform MixingTransform;
	public bool Additive;
}

[RequireComponent(typeof(AvatarLoadingController))]
public class AnimationSelector : MonoBehaviour
{
    public AnimationClip BasicIdleMale;
    public AnimationClip BasicIdleFemale;
    
    public AnimationMixingSettings[] GenericIdles = new AnimationMixingSettings[] {};
    public AnimationMixingSettings[] MaleIdles = new AnimationMixingSettings[] { };
    public AnimationMixingSettings[] FemaleIdles = new AnimationMixingSettings[] { };

    public float IdleDelayMin = 2;
    public float IdleDelayMax = 10;

    public float BasicIdleSpeadSpread = 0.2f;

    public AvatarGender currentGender = AvatarGender.Male;

    AvatarController avatarCtrl;
    AvatarLoadingController loadingCtrl;
    Animation animCtrl;

    AnimationClip currentBasicIdle;
    List<AnimationMixingSettings> currentIdles;

    void Start()
    {
        avatarCtrl = GetComponent<AvatarController>();
        animCtrl = GetComponent<Animation>();
        loadingCtrl = GetComponent<AvatarLoadingController>();

        //Setup the basic male and female idle animations
        if (BasicIdleMale == null)
        {
            this.enabled = false;
            Debug.LogError("BasicIdleMale anim is not set");
            return;
        }
        if (BasicIdleFemale == null)
        {
            this.enabled = false;
            Debug.LogError("BasicIdleFemale anim is not set");
            return;
        }

        // Add male animations to the animation component and setup
        animCtrl.AddClip(BasicIdleMale, BasicIdleMale.name);
        animCtrl[BasicIdleMale.name].layer = 0;
        animCtrl[BasicIdleMale.name].speed = 1 + Random.Range(-BasicIdleSpeadSpread, BasicIdleSpeadSpread);
        animCtrl[BasicIdleMale.name].wrapMode = WrapMode.Loop;

        //Add female aniations to the animation component and setup.
        animCtrl.AddClip(BasicIdleFemale, BasicIdleFemale.name);
        animCtrl[BasicIdleFemale.name].layer = 0;
        animCtrl[BasicIdleFemale.name].speed = 1 + Random.Range(-BasicIdleSpeadSpread, BasicIdleSpeadSpread);
        animCtrl[BasicIdleFemale.name].wrapMode = WrapMode.Loop;

        //Create a list of all animations, add them to the animation component and setup variables
        List<AnimationMixingSettings> allIdles = new List<AnimationMixingSettings>(GenericIdles.Length + MaleIdles.Length + FemaleIdles.Length);
        allIdles.AddRange(GenericIdles);
        allIdles.AddRange(FemaleIdles);
        allIdles.AddRange(MaleIdles);
        foreach (AnimationMixingSettings animMix in allIdles)
        {
            if (animMix != null && animMix.Clip != null)
            {
                animCtrl.AddClip(animMix.Clip, animMix.Clip.name);
                AnimationState state = animCtrl[animMix.Clip.name];
                state.layer = 1;
                state.wrapMode = WrapMode.Once;
                if (animMix.MixingTransform) 
                {
                    state.AddMixingTransform(animMix.MixingTransform);
                }
				if (animMix.Additive)
				{
					state.blendMode = AnimationBlendMode.Additive;
				}
            }
        }

        // setup animations for the current gender
        LoadAnims(currentGender);
        StartCoroutine(IdleAnims());
    }

    private bool forceUpdate = false;
    public void OnLevelWasLoaded()
    {
        forceUpdate = true;
    }

    public void Update()
    {
        //Update anims when gender changes
        var loadedOutfit = avatarCtrl.AvatarLoadedOutfit;
        if (loadedOutfit != null && (loadedOutfit.Gender != currentGender || forceUpdate))
        {
            forceUpdate = false;
            LoadAnims(loadedOutfit.Gender);
        }
    }

    public void OnAvatarLoadingCompleted()
    {
        // This will get called when this avatar has finished loading.  At this point we know
        // the avatar gender. - Reload anims if the gender is different to the previous gender
        if (loadingCtrl != null && loadingCtrl.CurrentOutfit.Gender != currentGender)
        {
            LoadAnims(loadingCtrl.CurrentOutfit.Gender);
        }
    }

    public void LoadAnims(AvatarGender gender)
    {
        currentGender = gender;

        //crossfade to new basic idle
        currentIdles = new List<AnimationMixingSettings>(GenericIdles);
        if (gender == AvatarGender.Male)
        {
            currentBasicIdle = BasicIdleMale;
            currentIdles.AddRange(MaleIdles);
        }
        else
        {
            currentBasicIdle = BasicIdleFemale;
            currentIdles.AddRange(FemaleIdles);
        }

        animCtrl.CrossFade(currentBasicIdle.name);
    }

    // Play an idle animation at a random time interval.
    public IEnumerator IdleAnims()
    {
        while (true)
        {
            float wait = Random.Range(IdleDelayMin, IdleDelayMax);
            yield return new WaitForSeconds(wait);

            if (currentIdles.Count == 0)
            {
                continue;
            }

            int animIdx = Random.Range(0, currentIdles.Count);
            if (currentIdles[animIdx] != null && currentIdles[animIdx].Clip != null)
            {
                var clip = currentIdles[animIdx].Clip;

                string animName = clip.name;
                animCtrl.CrossFade(animName);

                //wait until anim is done
                yield return new WaitForSeconds(clip.length);
            }
        }
    }
}
