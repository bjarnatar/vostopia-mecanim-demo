using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

public class SavedOutfitsExample : MonoBehaviour
{
    public GameObject AvatarController;

    AvatarSavedOutfits savedOutfits;
    private string saveOutfitName = "outfit name";

    void Start()
    {
        //Check that the avatar controller is set and has the AvatarLoaderUserId component
        if (!AvatarController || AvatarController.GetComponent<AvatarLoadingController>() == null)
        {
            Debug.LogError("Please set AvatarController to a game object with the AvatarLoadingController component");
            return;
        }

        StartCoroutine(GetSavedOutfits());
    }

    IEnumerator GetSavedOutfits()
    {
        while (!VostopiaClient.IsAuthenticated)
        {
            yield return null;
        }

        //get list of saved outfits
        ApiCall call = VostopiaClient.Item.BeginListSavedOutfits();
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        savedOutfits = VostopiaClient.Item.EndListSavedOutfits(call);
    }

    IEnumerator LoadSavedOutfit(string savedOutfitId)
    {
        //get the outfit data for the saved outfit
        ApiCall call = VostopiaClient.Item.BeginGetSavedOutfit(savedOutfitId);
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        var outfit = VostopiaClient.Item.EndGetSavedOutfit(call);
        if (outfit != null)
        {
            //Load the outfit on the avatar
            AvatarController.GetComponent<AvatarLoadingController>().SetOutfit(outfit);
        }
    }

    IEnumerator ActivateSavedOutfit(string savedOutfitId)
    {
        //set the current outfit to match the saved outfit
        ApiCall call = VostopiaClient.Item.BeginActivateSavedOutfit(savedOutfitId);
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        if (VostopiaClient.Item.EndActivateSavedOutfit(call))
        {
            //the default outfit is now updated in the backend. 
            //trigger a reload to get the new outfit
            AvatarController.GetComponent<AvatarLoadingController>().SetDefaultOutfit();
        }
    }

    IEnumerator SaveCurrentOutfit(string name)
    {
        var outfit = this.AvatarController.GetComponent<AvatarLoadingController>().CurrentOutfit;
        ApiCall call = VostopiaClient.Item.BeginSaveGameOutfit(outfit, name);
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        if (VostopiaClient.Item.EndSaveGameOutfit(call))
        {
            StartCoroutine(GetSavedOutfits());
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 500, 400));

        GUILayout.Label("Saved Outfits:");
        if (savedOutfits == null)
        {
            GUILayout.Label("loading...");
        }
        else
        {
            if (savedOutfits.SavedOutfits.Count >= 0)
            {
                foreach (var savedOutfit in savedOutfits.SavedOutfits)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("      " + savedOutfit.Name);
                    if (savedOutfit.GameId != null)
                    {
                        GUILayout.Label(" (game specific)");
                    }

                    if (GUILayout.Button("load (this session)"))
                    {
                        StartCoroutine(LoadSavedOutfit(savedOutfit.Id));
                    }
                    if (GUILayout.Button("set active (permanent)"))
                    {
                        StartCoroutine(ActivateSavedOutfit(savedOutfit.Id));
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
        }

        GUILayout.BeginHorizontal();
        saveOutfitName = GUILayout.TextField(saveOutfitName);
        if (GUILayout.Button("Save Outfit"))
        {
            StartCoroutine(SaveCurrentOutfit(saveOutfitName));
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

}
