using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class VostopiaAuthenticationSettings
{
    /**
     * In which mode the authentication should work. 
     * 
     * PRIMARY_AUTHENTICATION - The vostopia account is the primary account for this game. This is the default mode, 
     *                          and the authentication works like a normal authentication system. The user can 
     *                          choose which account he wants to log in with. 
     * LINKED_ACCOUNT         - The user is first authenticated with another authentication system. Vostopia authentication
     *                          is started after the user is authenticated with the other authentication system. See 
     *                          <Linked Accounts> for more information about how to implement this.
     */
    public enum AuthenticationMode
    {
        PRIMARY_AUTHENTICATION,
        LINKED_ACCOUNT,
    }

    public AuthenticationMode Mode = AuthenticationMode.PRIMARY_AUTHENTICATION;
    public float UIVolume = 1;

    /**
     * If set, the authentication will try to authenticate the user with this authentication
     * key. Used together with <AutomaticLogin>, this can be used to provide automatic login
     * when the user is authenticated by another authentication system. See provided example for 
     * more details
     */
    [HideInInspector]
    public string AuthenticationToken;

    /**
     * If set, the authentication will authenticte the facebook user, and automatically log in 
     * or create a vostopia user for him.
     */
    //TODO: public string FacebookAccessToken;

    /**
     * Called when authentication is completed. 
     */
    public event AuthenticationCompletedDelegate OnAuthenticationCompleted;
    public delegate void AuthenticationCompletedDelegate(AuthenticationCompletedArgs e);
    public class AuthenticationCompletedArgs
    {
        /**
         * The authenticated user
         */
        public VostopiaUser User;

        /**
         * Authentication key for the authenticated user, that can be used to automatically
         * log on the user on subsequent visits
         */
        public string AuthenticationToken;
    }


    /**
     * Called when authentication is cancelled. 
     */
    public event AuthenticationCanceledDelegate OnAuthenticationCanceled;
    public delegate void AuthenticationCanceledDelegate(AuthenticationCanceledArgs e);
    public class AuthenticationCanceledArgs
    {
    }
    

    /**
     * Called after a user has been found, but before the 
     * user has been authenticated. Gives the game a way of ensuring that 
     * there is a one-to-one mapping of vostopia users and game users.
     * 
     * This method is not called a user is logged in from an authentication key
     */
    public event AuthenticationCheckUserDelegate OnAuthenticationCheckUser;
    public delegate void AuthenticationCheckUserDelegate(AuthenticationCheckUserArgs e);
    public class AuthenticationCheckUserArgs
    {
        /**
         * The user about to be authenticated
         */
        public VostopiaUser User;

        /**
         * Cancel can be set to true by event listeners if the authentication should be canceled.
         * 
         * The reason for setting this could be to ensure that there is a one-to-one mapping between
         * vostopia users and an external user system. The game would then listen for the <OnAuthenticationCheckUser>,
         * and verify that the vostopia user is not connected to another external user.
         */
        public bool Cancel;

        /**
         * The message to show the user if the authentication was cancelled. Set to a user understandable
         * message 
         */
        public string CancelMessage;
    }

    /** 
     * Called from vostopia authentication, not intended for external usage
     */
    public void AuthenticationCheckUser(AuthenticationCheckUserArgs e)
    {
        if (OnAuthenticationCheckUser != null)
        {
            OnAuthenticationCheckUser(e);
        }
    }

    /** 
     * Called from vostopia authentication, not intended for external usage
     */
    public void AuthenticationCompleted(AuthenticationCompletedArgs e)
    {
        if (OnAuthenticationCompleted != null)
        {
            OnAuthenticationCompleted(e);
        }
    }

    /** 
     * Called from vostopia authentication, not intended for external usage
     */
    public void AuthenticationCanceled(AuthenticationCanceledArgs e)
    {
        if (OnAuthenticationCompleted != null)
        {
            OnAuthenticationCanceled(e);
        }
    }
}


[System.Serializable]
public class VostopiaShopSettings
{
    public bool PaymentsEnabled = true;
    public VostopiaPaymentProvider PaymentProvider = VostopiaPaymentProvider.GOOGLE_WALLET_WEB;
    public bool PaymentSandbox = false;
}


/** 
 * Api Controller. Makes sure that we are connected to the vostopia server. If not, it creates an instance of the authentication
 * prefab which will run the authentication flow.
 */
[RequireComponent(typeof(VostopiaClientInitializer))]
public class VostopiaApiController : MonoBehaviour
{
    /**
     * If true, the connect process will begin on the Start event. If this is set to false,
     * <Connect> must be called manually to start the connect/authenticate process.
     */
    public bool ConnectOnStart = true;

    /**
     * The prefab containing the UI to perform authentication. It is safest to leave
     * this pointing to Assets/vostopia/authentication/AuthenticationPrefab
     */
    public GameObject AuthenticationPrefab;

    /**
     * Authentication Settings
     */
    public VostopiaAuthenticationSettings AuthenticationSettings = new VostopiaAuthenticationSettings();

    /**
     * Shop Settings
     */
    public VostopiaShopSettings ShopSettings = new VostopiaShopSettings();

    private GameObject AuthenticationObject;

    public void Start()
    {
        if (ConnectOnStart && !VostopiaClient.IsAuthenticated)
        {
            Connect();
        }
    }

    public void Connect()
    {
        if (AuthenticationObject != null)
        {
            GameObject.Destroy(AuthenticationObject);
        }
        AuthenticationObject = (GameObject)GameObject.Instantiate(AuthenticationPrefab);
        AuthenticationObject.SendMessage("OnAuthenticationSettingsChanged", AuthenticationSettings);
    }

}
