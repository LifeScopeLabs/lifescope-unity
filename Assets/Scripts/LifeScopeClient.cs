using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PerspecDev;


/*
 * 1. Sign In URL to Broker (unity.lifescope.io)
 *      Current Broker Url in Prod
 *      https://r4o1ekqi1d.execute-api.us-east-1.amazonaws.com/dev/create-session?client_id=e33af1dc0124dbf5&scope=events:read
 *      
 *      
 *      
 * 2. Add token broker code from URL
 * 
 *      2.1     Ability To type in Code
 *      
 *      2.2     Paste Support
 *      
 *      2.3     Exchange code for token from broker service
 *              https://r4o1ekqi1d.execute-api.us-east-1.amazonaws.com/dev/exchange-code?access_code=<code> is the endpoint you have to hit to exchange the code for the oauth token.
 *              
 *      2.4     Save LifeScope OAuth API token to disk in plain text file. Save token to in memory global variable too?
 *              
 *              https://answers.unity.com/questions/1320236/what-is-binaryformatter-and-how-to-use-it-and-how.html
 *              Catch failed API calls and return an error.
 *              
 *  3.  Call for User info as a test Query. https://api.lifescope.io/gql-p  userBasic
 *              https://lifescope.io/platform
 *              https://lifescope.io/schema
 *              
 *              Ensure the Header is Correct https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
 *              Call The query. Look at diff from GET/POST vers with and without forms
 *                  https://docs.unity3d.com/Manual//UnityWebRequest-RetrievingTexture.html
 *                  https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.Post.html
 *              Format the Json into an object https://docs.unity3d.com/ScriptReference/JsonUtility.html
 *              Catch failed API calls and return an error.
 *      
 * 4.    Session Refresh
 * 
 *      If a request fails with an expired token error.
 * 
 * 3. Example 1: Show List of People
 * 
 *      https://api.lifescope.io/gql-p FilterFindManyPeopleInput 
 *  
 *      Parse JSON To Menue Elements https://docs.unity3d.com/Manual/JSONSerialization.html
 *      
 *      
 * 4. Example 2: Show AR map of events with locations
 * 
 */







public class LifeScopeClient : MonoBehaviour {

    /*
        LifeScope uses OAuth 2.0 to authenticate its API clients.  If you don't need OAuth, check out IPtoLocation.cs or TeamFortressNews.cs for simpler examples of using APIBuddy.

        If you want to run the example below, you'll need to have registered an LifeScope app.  If you haven't done that already, you can do so here:
        https://app.lifescope.io/settings/developer
    */

public int portNumber = 36894; // This can be anything you want, but it should be a port number that is unlikely to be used by any other service.  When you register a redirect URL with LifeScope, it will need to be in this format: http://127.0.0.1:[port number]/
    public string oauthClientId = "e33af1dc0124dbf5"; // Put your LifeScope OAuth client ID here.

    private LifeScopeCallbackServer _oauthCallbackServer;
    private bool _isRequestingPerson = false;
    private string _outputText = "";

    // LifeScopePersonOneDataResponse reflects the LifeScope JSON response, but only contains the fields we actually want to retrieve.
    // No need to include every JSON field that LifeScope provides here.  Just the ones we want.  These field names do
    // need to match the JSON response fields returned by LifeScope.  The class name can be whatever you want.
    public class LifeScopePersonOneDataResponse : APIBuddy.WebResponseData {

        // The LifeScope API returns a "data" object as a wrapper for all the fields it provides.
        // The PersonOne type here is defined below, and it reflects the fields we're interested in that reside inside the "data" object.
        public PersonOneResponse data;

        // In LifeScope's JSON response, each "data" entry contains the GraphQL call made to to retrieve the data, in this case 'personOne'.
        public class PersonOneResponse : APIBuddy.WebResponseData {
            // LifeScope returns account information through this SDK. To work correctly, we're going to pull the "type" field from the JSON response to check if it's correctly labeled.
            public PersonOneData personOne;
        }

        // 'personOne' contains the actual LifeScope data for a person.
        public class PersonOneData : APIBuddy.WebResponseData
        {
            public string id;

            public string avatar_url;

            public string first_name;

            public string middle_name;

            public string last_name;

            public string[] contact_id_strings;
        }
    }

    private void Start() {
        // We need an OAuth callback server in order to retrieve the auth token after the user has authenticated with the OAuth provider.
        _oauthCallbackServer = gameObject.AddComponent<LifeScopeCallbackServer>();
    }

    private void OnGUI() {
        if (!_isRequestingPerson) {
            if (GUI.Button(new Rect(20.0f, 20.0f, 200.0f, 50.0f), "Get LifeScope User data")) {
                GetPerson();
            }
        } else {
            if (GUI.Button(new Rect(20.0f, 20.0f, 200.0f, 50.0f), "Cancel request")) {
                _oauthCallbackServer.StopListening();
                _outputText = "";
                _isRequestingPerson = false;
            }
        }

        GUI.TextArea(new Rect(20.0f, 100.0f, 500.0f, 400.0f), _outputText);
    }


    private void GetPerson() {
        _outputText = "";
        _isRequestingPerson = true;

        if (string.IsNullOrEmpty(oauthClientId)) {
            _outputText = "Oops!  Be sure to set the OAuth Client ID provided to you by LifeScope.  Check the Inspector panel on the LifeScope object and make sure the OAuth Client Id field is not empty.";
            _isRequestingPerson = false;
            return;
        }

        // This is where we tell the OAuth server to start listening on whatever port number we've specified, and to send us the auth token when authentication is complete.
        string serverURL = _oauthCallbackServer.StartListening(portNumber, (string authToken) => {

            // At this point, authentication has completed and we've received the auth token.  Now we'll construct a URL for Instagram's API with that auth token and request some photos.
            string recentMediaURL = "https://api.instagram.com/v1/users/self/media/recent/?access_token=" + Uri.EscapeDataString(authToken);
            APIBuddy.GetInstance().SendGetRequest<LifeScopePersonOneDataResponse>(recentMediaURL, 30.0f, (bool success, string errorMessage, int statusCode, object responseObject) => {
                if (success) {
                    int numPhotosRetrieved = 0;

                    // APIBuddy handles populating LifeScopePersonOneDataResponse for us, based on the fields we set up in it up above.  All we need to do now is
                    // cast responseObject to the LifeScopePersonOneDataResponse type and then read our data!
                    LifeScopePersonOneDataResponse LifeScopeResponse = (LifeScopePersonOneDataResponse)responseObject;

                    _outputText += "Username: " + LifeScopeResponse.data.personOne.first_name + ' ' + LifeScopeResponse.data.personOne.middle_name + ' ' + LifeScopeResponse.data.personOne.last_name;

                    _outputText += "\nAvatar URL: " + LifeScopeResponse.data.personOne.avatar_url;
                    _isRequestingPerson = false;
                } else {
                    _outputText = "Unable to retrieve LifeScope Person.\n" + errorMessage;
                    _isRequestingPerson = false;
                }
            });

        });

        // Now that we have the server URL, we can pass it as the redirect URL to the LifeScope SDK token broker example.  When authentication is complete, LifeScope will
        // send the user to the redirect URL with the auth token appended, and the server we started up above will capture that auth token so we can make
        // a request to the LifeScope token broker example.
        string userAuthenticationURL = string.Format("https://r4o1ekqi1d.execute-api.us-east-1.amazonaws.com/dev/create-session?client_id=e33af1dc0124dbf5&scope=events:read", Uri.EscapeDataString(oauthClientId), Uri.EscapeDataString(serverURL));
        Application.OpenURL(userAuthenticationURL);

        // If you're wanting to do OAuth 2.0 on iOS or Android, you won't be able to use the _oauthCallbackServer approach above.  Instead, you'll need to
        // register a custom URL scheme:
        // https://developer.apple.com/library/ios/documentation/iPhone/Conceptual/iPhoneOSProgrammingGuide/Inter-AppCommunication/Inter-AppCommunication.html
        // https://developer.android.com/training/basics/intents/filters.html
        // 
        // Then for example, if your custom url scheme is myapp://, you'll tell the OAuth provider (Instagram in this example) that your client redirect URL
        // is something along the lines of myapp://oauth/.  Then after Instagram authenticates the user, it will redirect the user to myapp://oauth/#access_token=abcd1234
        // From there, you will need to read the access token in native Objective C, Swift, or Java (depending on your platform).  Then you can pass that data
        // back into your Unity app.  See this Stack Overflow post for an example of how data can be passed from iOS or Android to Unity:
        // http://stackoverflow.com/questions/33628779/shared-preferences-between-unity-android-and-ios
        // Once you have the auth token in Unity, you can call APIBuddy.GetInstance().SendGetRequest() in the same way as shown above.  You do not need to call
        // _oauthCallbackServer.StartListening().
    }

}
