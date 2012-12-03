using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AvatarLoadingController))]
public class AvatarLoaderNetwork : Photon.MonoBehaviour
{
    private AvatarLoadingController mLoadingController;

    void Awake()
    {
        mLoadingController = GetComponent<AvatarLoadingController>();
    }

    // Use this for initialization
    void Start()
    {
        if(photonView.isMine)
        {
            mLoadingController.SetDefaultOutfit("");
        }
    }

    public void LoadAvatar()
    {
        
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //Debug.Log(string.Format("OnPhotonSerializeView {0}", name));

        if (stream.isWriting)
        {
            string userId = VostopiaClient.Authentication.ActiveUser.Id;
            //Debug.Log(string.Format("Sending UserId {0}", userId),this);
            
            //We own this player: send the others our data
            stream.SendNext(userId);
        }
        else
        {
            //Network player, receive data
            //controllerScript._characterState = (CharacterState)(int)stream.ReceiveNext();
            string userId = (string)stream.ReceiveNext();
            Debug.Log(string.Format("Recevieved User Id {0}", userId), this);
            mLoadingController.SetDefaultOutfit(userId);
        }
    }

}
