using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

[RequireComponent(typeof(AvatarLoadingController))]
public class AvatarLoaderSavedOutfit : MonoBehaviour
{
    public string UserId = "";
    public string OutfitName = "";
    public bool LoadNow = true;

    public void Update()
    {
        if (LoadNow && VostopiaClient.IsAuthenticated)
        {
            StartCoroutine(LoadOutfit());
            LoadNow = false;
        }
    }

    public IEnumerator LoadOutfit()
    {
        ApiCall call = VostopiaClient.Item.BeginGetSavedOutfit(UserId, OutfitName);
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        AvatarOutfit outfit = VostopiaClient.Item.EndGetSavedOutfit(call);
        if (outfit != null)
        {
            GetComponent<AvatarLoadingController>().SetOutfit(outfit);
        }
    }
}
