using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

/** 
 * Authentication Controller. This class connects the game to the api server and ensures
 * that we have a vostopia guest or authenticated user to load avatars for.
 * 
 * This class is a part of the VostopiaApiController prefab, and the preferred way of
 * connecting to the vostopia api is to add an instance of the VostopiApiController to your
 * first scene.
 * 
 * By default, the authentication process will start on the Start event. If that does not fit 
 * with the game flow, <ConnectOnAwake> may be set to false, and <Connect> invoked manually 
 * when authentication should start.
 * 
 * Depending on the environment the game is running in, this class behaves differently.
 * 
 * Unity Editor / Stand Alone Executable:
 * 
 *   If the game is run in the unity editor or as a stand alone executable, an authentication
 *   dialog is shown, where the users can log in with their vostopia username or password, 
 *   or sign up for a new account if they don't already have a vostopia account.
 *   
 *   Players can also elect to "continue as guest", where a guest user will be created for them
 *   On subsequent visits, the authentication process will try to use the same guest user 
 *   (by storing an authentication key in PlayerPrefs). The guest user can then later
 *   be upgraded to a full user by completing the registration process.
 * 
 * Web Player:
 * 
 * When running in the web player, the authentication process can use a single signon provider
 * to authenticate the user by implementing some javascript in the surrounding html. 
 * 
 * During <Connect>, the javascript method GetVostopiaParameters will be invoked if it exists.
 * By implementing this method and sending the sso details back to the <ApiClientInitializer>,
 * the authentication process can be completely transparent to the user.
 * 
 * At the moment, only facebook connect/canvas is supported. Use auth method "facebook" and the
 * facebook access token you have got from the facebook user:
 * 
 * (begin code)
 *   
 *   <script type="text/javascript">
 *       function GetVostopiaParameters(obj, meth)
 *       {
 *           var unity = unityObject.getObjectById("unity-player");
 *           var params = {
 *               auth: {
 *                   method: "facebook",
 *                   oauthToken: "<facebook access token>"
 *               }
 *           };
 *           unity.SendMessage(obj, meth, JSON.stringify(params));
 *       }
 *   </script>
 * 
 * (end)
 * 
 * If GetVostopiaParameters is not defined, or does not respond in a timely fashion, a normal
 * authentication process will be started.
 * 
 */
public class VOGControllerAuth : VOGController
{
    public enum AuthenticationResult
    {
        COMPLETED,
        CANCELED,
    }

    public bool ConnectOnStart = true;
    public VOGStateBase AuthenticationSelectorScreen;
    public VOGStateBase UserSelectorScreen;
    public VOGStateBase PasswordScreen;

    public VostopiaAuthenticationSettings AuthenticationSettings = new VostopiaAuthenticationSettings();

    public bool EnableGooglePayments = true;
    public bool GooglePaymentsSandbox = false;

    [HideInInspector]
    public VostopiaApiSettings Settings;

    private bool isConnecting = false;
    private bool isStarted = false;
    private JObject webplayerParameters = null;
    private string vostopiaJavascriptUrl = null;

    /**
     * Returns true if the the client is currently authenticating. This includes
     * any time the authentication dialogs are open.
     */
    public bool IsConnecting
    {
        get
        {
            return (!isStarted && ConnectOnStart) || isConnecting;
        }
    }

    public override void Start()
    {
        base.Start();

        isStarted = true;
        if (ConnectOnStart)
        {
            Connect();
        }
    }

    /**
     * Starts authenticating with the API server. Check <IsConnected> to detect when
     * authentication is completed.
     */
    public void Connect()
    {
        StartCoroutine(ConnectRunner());
    }

    private IEnumerator ConnectRunner()
    {
        isConnecting = true;
        //Load Vostopia Api Settings
        try
        {
            Settings = (VostopiaApiSettings)Resources.Load(System.IO.Path.GetFileNameWithoutExtension(VostopiaApiSettings.SettingsAssetPath), typeof(VostopiaApiSettings));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Unable to load vostopia settings " + ex.ToString());
            Settings = null;
        }
        if (Settings == null)
        {
            SetConnectionError("Vostopia Api Settings Not Found", "The Vostopia api settings resource was not found at '" + VostopiaApiSettings.SettingsAssetPath + "'. Perhaps you haven't set the game id/api key or moved the settings file somewhere else?", false);
            yield break;
        }
        if (Settings.ApiUrl == null || Settings.ApiUrl == "")
        {
            Settings.ApiUrl = "https://vostopia.com/api";
        }
        if (Settings.GameId == null || Settings.GameId == "")
        {
            SetConnectionError("Vostopia Game Id Not Set", "You must set up a game id and api key in order to use Vostopia avatars. Visit https://vostopia.com/developers to setup your game, and fill out the GameId and ApiKey on the VostopiaClientInitializer", false);
            yield break;
        }
        if (Settings.ApiKey == null || Settings.ApiKey == "")
        {
            SetConnectionError("Vostopia Api Key Not Set", "You must set up a game id and api key in order to use Vostopia avatars. Visit https://vostopia.com/developers to setup your game, and fill out the GameId and ApiKey on the VostopiaClientInitializer", false);
            yield break;
        }

        //Set parameters from settings
        VostopiaClient.ApiKey = Settings.ApiKey;
        VostopiaClient.GameId = Settings.GameId;
        VostopiaClient.ApiUrl = Settings.ApiUrl;

        //Reset session
        VostopiaClient.Authentication.Disconnect();
        VostopiaClient.IsAuthenticated = false;

        IEnumerator e = null;

        //Query web surround to get parameters
        e = QueryWebSurround();
        while (e.MoveNext()) { yield return e.Current; };

        //Check if we have startup data
        if (webplayerParameters != null)
        {
            if (webplayerParameters["startupData"] != null)
            {
                JObject startupData = (JObject)webplayerParameters["startupData"];
                VostopiaClient.ReceiveStartupData(startupData);
            }
        }

        //Connect to get session
        ApiCall call = VostopiaClient.Authentication.BeginConnect();
        e = call.Wait();
        while (e.MoveNext()) { yield return e.Current; }
        if (!VostopiaClient.Authentication.EndConnect(call))
        {
            SetConnectionError("Unable to Connect to Vostopia", "Unfortunately we were not able to connect to the Vostopia server. Make sure you are connected to the internet and try again.", true);
            yield break;
        }
        if (call.ResponseData["javascriptUrl"] != null)
        {
            vostopiaJavascriptUrl = (string)call.ResponseData["javascriptUrl"];
        }

        //Load javascripts
        e = WaitForJavascriptLoad();
        while (e.MoveNext()) { yield return e; }

        //Check if we have startup data
        if (webplayerParameters != null && webplayerParameters["auth"] != null)
        {
            call = VostopiaClient.Authentication.BeginSignInSSO((JObject)webplayerParameters["auth"]);
            e = call.Wait();
            while (e.MoveNext()) { yield return e.Current; }
            if (VostopiaClient.Authentication.EndSignIn(call))
            {
                AuthenticationCompleted();
                yield break;
            }
        }

        //Show authentication screens. The auth flow should end in the Completed-screen
        //being shown. The completed screen calls the AuthenticationCompleted function
        //to mark authentication being over

        if (AuthenticationSettings.Mode == VostopiaAuthenticationSettings.AuthenticationMode.PRIMARY_AUTHENTICATION)
        {
            AuthenticatePrimary();
        }
        else if (AuthenticationSettings.Mode == VostopiaAuthenticationSettings.AuthenticationMode.LINKED_ACCOUNT)
        {
            e = AuthenticateLinkedAccount();
            while (e.MoveNext()) { yield return e.Current; }
        }
        else
        {
            Debug.LogError("Unknown Authentication Mode " + AuthenticationSettings.Mode.ToString() + ", can not authenticate");
        }

    }

    private void AuthenticatePrimary()
    {
        StoredAuthenticationKey userAuthKey = null;
        userAuthKey = GetStoredUserAuthenticationKey();

        if (userAuthKey != null && UserSelectorScreen != null)
        {
            //Show user selector screen if we have login credentials
            UserSelectorScreen.OnDataChanged(userAuthKey);
            StartTransition(UserSelectorScreen);
        }
        else
        {
            //Otherwise, show the authentication screen
            var authSelectData = new VOGStateAuthSelect.AuthSelectData();
            authSelectData.EnableCancel = false;
            AuthenticationSelectorScreen.OnDataChanged(authSelectData);
            StartTransition(AuthenticationSelectorScreen);
        }
    }

    /**
     * Authenticate when we're in the Linked Account mode. If we have a valid authentication
     * token in AuthenticationSettings, use it to automatically log in user. If we have an invalid
     * authentication token, force the same user to log in again. If we don't have any authentication token,
     * let the user login or create a new vostopia user.
     */
    public IEnumerator AuthenticateLinkedAccount()
    {
        StoredAuthenticationKey userAuthKey = null;
        if (!string.IsNullOrEmpty(AuthenticationSettings.AuthenticationToken))
        {
            try
            {
                userAuthKey = JObject.Parse(AuthenticationSettings.AuthenticationToken).ToObject<StoredAuthenticationKey>();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Unable to parse AuthenticationToken, " + ex.ToString());
            }
        }

        if (userAuthKey != null)
        {
            //If we have auth key, try to authenticate automatically
            ApiCall call = VostopiaClient.Authentication.BeginSignInAuthKey(userAuthKey.AuthKey);
            IEnumerator e = call.Wait();
            while (e.MoveNext()) { yield return e.Current; }
            if (VostopiaClient.Authentication.EndSignIn(call))
            {
                //authentication completed successfully, nothing more to do
                AuthenticationCompleted();
                yield break;
            }

            //If we failed to log in automatically, find the user and get him to supply the password
            call = VostopiaClient.Authentication.BeginFindUser(userAuthKey.UserName);
            e = call.Wait();
            while (e.MoveNext()) { yield return e.Current; }
            VostopiaUser user = VostopiaClient.Authentication.EndFindUser(call);
            if (user != null)
            {
                var passwordData = new VOGStateAuthPassword.PasswordData();
                passwordData.CancelOnBack = true;
                passwordData.User = user;
                StartTransition(PasswordScreen, passwordData);
                yield break;
            }
        }

        //Otherwise, show the authentication screen
        var authSelectData = new VOGStateAuthSelect.AuthSelectData();
        authSelectData.EnableCancel = true;
        StartTransition(AuthenticationSelectorScreen, authSelectData);
    }

    public void AuthenticationCompleted()
    {
        //Dismiss UI
        StartTransition(null);

        VostopiaClient.IsAuthenticated = true;
        isConnecting = false;

        //Get authentication key
        string authToken = null;
        var userKey = GetStoredUserAuthenticationKey();
        if (userKey != null)
        {
            authToken = JObject.FromObject(userKey).ToString();
        }

        //Send OnAuthenticationCompleted event
        var args = new VostopiaAuthenticationSettings.AuthenticationCompletedArgs();
        args.User = VostopiaClient.Authentication.ActiveUser;
        args.AuthenticationToken = authToken;
        AuthenticationSettings.AuthenticationCompleted(args);

        Debug.Log("Vostopia: Authenticated as " + VostopiaClient.Authentication.ActiveUser.Username);
    }

    public void AuthenticationCanceled()
    {
        //Dismiss UI
        StartTransition(null);

        VostopiaClient.Authentication.Disconnect();
        VostopiaClient.IsAuthenticated = false;

        //Send OnAuthenticationCompleted event
        var args = new VostopiaAuthenticationSettings.AuthenticationCanceledArgs();
        AuthenticationSettings.AuthenticationCanceled(args);
    }

    #region Web Surround

    private IEnumerator QueryWebSurround()
    {
        if (Application.isWebPlayer)
        {
            webplayerParameters = null;
            int retriesRemaining = Settings.WebSurroundRetryCount;
            while (retriesRemaining > 0 && webplayerParameters == null)
            {
                //On the web, call the external javascript to supply us with parameters
                yield return null;
                Debug.Log("Invoking web surround to get vostopia parameters");
                Application.ExternalEval(string.Format(@"
					(function() {{
						var checkVostopiaParameters = function() {{
                    	    if (typeof(GetVostopiaParameters) != 'undefined')
                    	    {{
                    	        GetVostopiaParameters('{0}', 'SetVostopiaParameters'); 
                    	    }}
                    	    else
                    	    {{
                    	        var unity = GetUnity();
                    	        unity.SendMessage('{0}', 'SetVostopiaParameters', '{{}}');
                    	    }}
                    	}};

                    	if (typeof($) != 'undefined' && typeof($.ready) != 'undefined')
                    	{{
                    	    $(document).ready(function () {{ 
                    	        checkVostopiaParameters();
                    	    }});
                    	}}
                    	else
                    	{{
                    	    checkVostopiaParameters();
                    	}}
					}})();
                    ", gameObject.name));

                float time = Time.time;
                bool hasWebplayerCallback = false;
                bool hasTimeout = false;
                while (!hasWebplayerCallback && !hasTimeout)
                {
                    yield return new WaitForSeconds(Settings.PollInterval);
                    if (webplayerParameters != null)
                    {
                        hasWebplayerCallback = true;
                    }
                    else if ((Time.time - time) > Settings.WebSurroundTimeoutSeconds)
                    {
                        hasTimeout = true;
                    }
                }

                retriesRemaining--;
            }

            if (webplayerParameters == null)
            {
                Debug.LogError("Timing out querying web surround, check browser console for error messages");
            }
        }
    }

    private bool JavascriptLoaded;
    private IEnumerator WaitForJavascriptLoad()
    {
        JavascriptLoaded = false;
        if (Application.isWebPlayer)
        {
            string googlePaymentsEnvironment = GooglePaymentsSandbox ? "sandbox_config" : "production_config";

            //Run javascript to load external javascripts (jquery, vostopia, google payments)
            string javascript = string.Format(@"
        			(function () {{

                        function log(message) {{
                            if (console !== undefined) {{
                                console.log(message);
                            }}
                        }}
        			 
        			    function loadScript(url, callback) {{
        			 
        			        var script = document.createElement('script')
        			        script.type = 'text/javascript';
        			 
        			        if (script.readyState) {{ //IE
        			            script.onreadystatechange = function () {{
        			                if (script.readyState == 'loaded' || script.readyState == 'complete') {{
        			                    script.onreadystatechange = null;
        			                    callback();
        			                }}
        			            }};
        			        }} else {{ //Others
        			            script.onload = function () {{
        			                callback();
        			            }};
        			        }}
        			 
        			        script.src = url;
        			        document.getElementsByTagName('head')[0].appendChild(script);
        			    }}
        
        				var waitingFor = {{
        					vostopia: true,
        					jquery: true,
        					googlePayments: {2}
        				}};
        
        				function allLoadedCallback()
        				{{
        					//check that we've loaded all the required libs
        					allLoaded = true;
        					for (var key in waitingFor)
        					{{
        						if (waitingFor[key]) {{
        							allLoaded = false;
        							break;
        						}}
        					}}
        
        					//if all loaded, send message back to unity
        					if (allLoaded)
        					{{
        						vostopia.GetUnity().SendMessage('{0}', 'JavascriptLoadedCallback', '');
        					}}
        				}}
        
        				function vostopiaLoadedCallback() {{
                            log('vostopia loaded');
        					waitingFor.vostopia = false;
        					allLoadedCallback();
        				}}
        
        				function jQueryLoadedCallback() {{
                            log('jquery loaded');
        					waitingFor.jquery = false;
        					allLoadedCallback();
        				}}
        
        				function googlePaymentsLoadedCallback() {{
                            log('google payments loaded');
        					waitingFor.googlePayments = false;
        					allLoadedCallback();
        				}}
        
        				//Load vostopia js api
        				if (waitingFor.vostopia) {{
        					loadScript('{1}', vostopiaLoadedCallback);
        				}}
        	
        				//Load jQuery
        				if (waitingFor.jquery)
        				{{
        					if (window.jQuery)
        					{{
        						jQueryLoadedCallback();
        					}}
        					else
        					{{
        						loadScript('https://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js', jQueryLoadedCallback);
        					}}
        				}}
        
        				//Load Google Payments
        				if (waitingFor.googlePayments)
        				{{
        					loadScript('https://www.google.com/jsapi', function () {{
        						google.load('payments', '1.0', {{ 
        							'packages': ['{3}'],
        							'callback': googlePaymentsLoadedCallback
        
        						}});
        					}});
        				}}
        			}})();
        		",
                gameObject.name,
                AssetDownloadManager.AssetBaseUrlDownload(vostopiaJavascriptUrl),
                EnableGooglePayments ? "true" : "false",
                googlePaymentsEnvironment
            );
            Application.ExternalEval(javascript);


            //Wait for callback
            float javascriptTimeoutSeconds = 30;
            float startTime = Time.time;
            bool hasTimeout = false;
            while (!JavascriptLoaded)
            {
                if ((Time.time - startTime) > javascriptTimeoutSeconds)
                {
                    hasTimeout = true;
                    break;
                }
                yield return new WaitForSeconds(0.2f);
            }

            if (hasTimeout)
            {
                Debug.LogError("Error loading external javascript, timed out while waiting for callback. Check browser console for error messages. Continuing, although other features might not work.");
            }
        }
    }

    public void JavascriptLoadedCallback(string parameters)
    {
        JavascriptLoaded = true;
    }

    public void SetVostopiaParameters(string parametersString)
    {
        Debug.Log("Got Vostopia Parameters: " + parametersString);

        JObject parameters = JObject.Parse(parametersString);
        if (parameters["apiUrl"] != null)
        {
            VostopiaClient.ApiUrl = (string)parameters["apiUrl"];
        }
        webplayerParameters = parameters;
    }

    #endregion

    #region "Stored Authentication Keys"

    public void StoreGuestAuthenticationKey(string authKey)
    {
        PlayerPrefs.SetString("_vostopia_guest_auth_key", authKey);
    }

    public string GetStoredGuestAuthenticationKey()
    {
        return PlayerPrefs.GetString("_vostopia_guest_auth_key", null);
    }

    public void StoreUserAuthenticationKey(VostopiaUser user, string authKey)
    {
        PlayerPrefs.SetString("_vostopia_auth_userid", user.Id);
        PlayerPrefs.SetString("_vostopia_auth_key", authKey);
        PlayerPrefs.SetString("_vostopia_auth_username", user.Username);
        PlayerPrefs.SetString("_vostopia_auth_displayname", user.DisplayName);
    }

    public class StoredAuthenticationKey
    {
        public string UserId;
        public string UserName;
        public string DisplayName;
        public string AuthKey;
    }

    public StoredAuthenticationKey GetStoredUserAuthenticationKey()
    {
        if (PlayerPrefs.HasKey("_vostopia_auth_userid")
            && PlayerPrefs.HasKey("_vostopia_auth_key")
            && PlayerPrefs.HasKey("_vostopia_auth_username")
            && PlayerPrefs.HasKey("_vostopia_auth_displayname"))
        {
            try
            {
                return new StoredAuthenticationKey()
                {
                    UserId = PlayerPrefs.GetString("_vostopia_auth_userid"),
                    AuthKey = PlayerPrefs.GetString("_vostopia_auth_key"),
                    UserName = PlayerPrefs.GetString("_vostopia_auth_username"),
                    DisplayName = PlayerPrefs.GetString("_vostopia_auth_displayname"),
                };
            }
            catch (System.Exception ex)
            {
                Debug.Log("Unable to parse stored user authentication key, " + ex.ToString());
            }
        }
        return null;
    }

    #endregion

    public void OnAuthenticationSettingsChanged(VostopiaAuthenticationSettings settings)
    {
        AuthenticationSettings = settings;
        TransitionAudioVolume = settings.UIVolume;
    }

    private void SetConnectionError(string error, string message, bool retry)
    {
        System.Action action = () => { };
        if (retry)
        {
            action = Connect;
        }

        ShowMessageDialog(error, message, action);
    }

}
