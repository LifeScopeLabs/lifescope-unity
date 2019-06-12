using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PerspecDev;

public class InstagramClient : MonoBehaviour {

    /*
        Instagram uses OAuth 2.0 to authenticate its API clients.  If you don't need OAuth, check out IPtoLocation.cs or TeamFortressNews.cs for simpler examples of using APIBuddy.

        If you want to run the example below, you'll need to have registered an Instagram app.  If you haven't done that already, you can do so here:
        https://www.instagram.com/developer/
    */

    public int portNumber = 36894; // This can be anything you want, but it should be a port number that is unlikely to be used by any other service.  When you register a redirect URL with Instagram, it will need to be in this format: http://127.0.0.1:[port number]/
    public string oauthClientId = ""; // Put your Instagram OAuth client ID here.

    private InstagramCallbackServer _oauthCallbackServer;
    private bool _isRequestingPhotoURLs = false;
    private string _outputText = "";

    // InstagramDataResponse reflects the Instagram JSON response, but only contains the fields we actually want to retrieve.
    // No need to include every JSON field that Instagram provides here.  Just the ones we want.  These field names do
    // need to match the JSON response fields returned by Instagram.  The class name can be whatever you want.
    public class InstagramDataResponse : APIBuddy.WebResponseData {

        // The Instagram API returns a "data" object as a wrapper for all the fields it provides.
        // The MediaItem type here is defined below, and it reflects the fields we're interested in that reside inside the "data" object.
        public MediaItem[] data;

        // In Instagram's JSON response, each "data" entry is a single piece of media.  MediaItem represents a single entry.
        public class MediaItem : APIBuddy.WebResponseData {
            // Instagram returns videos and images when we request media.  We only want images, so we're going to pull the "type" field from the JSON response to check if it's an image.
            public string type;

            // Instagram returns an "images" object that contains all the different variations (e.g. resolutions) on a media object.
            // The ImageData type here is defined below, and it reflects the "standard_definition" field we're interested in on the 
            public ImageData images;
        }

        public class ImageData : APIBuddy.WebResponseData {
            // Instagram provides several image variations, but the only one we care about is "standard_resolution" so that's the only one we've defined here.
            // We could have defined another subclass of APIBuddy.WebResponseData to represent the standard_resolution object, but since it just as easily
            // be represented as a dictionary, we may as well use a dictionary.
            public Dictionary<string, string> standard_resolution;
        }

    }

    private void Start() {
        // We need an OAuth callback server in order to retrieve the auth token after the user has authenticated with the OAuth provider.
        _oauthCallbackServer = gameObject.AddComponent<InstagramCallbackServer>();
    }

    private void OnGUI() {
        if (!_isRequestingPhotoURLs) {
            if (GUI.Button(new Rect(20.0f, 20.0f, 200.0f, 50.0f), "Request recent photo URLs")) {
                GetRecentPhotoURLs();
            }
        } else {
            if (GUI.Button(new Rect(20.0f, 20.0f, 200.0f, 50.0f), "Cancel request")) {
                _oauthCallbackServer.StopListening();
                _outputText = "";
                _isRequestingPhotoURLs = false;
            }
        }

        GUI.TextArea(new Rect(20.0f, 100.0f, 500.0f, 400.0f), _outputText);
    }

    private void GetRecentPhotoURLs() {
        _outputText = "";
        _isRequestingPhotoURLs = true;

        if (string.IsNullOrEmpty(oauthClientId)) {
            _outputText = "Oops!  Be sure to set the OAuth Client ID provided to you by Instagram.  Check the Inspector panel on the Instagram object and make sure the OAuth Client Id field is not empty.";
            _isRequestingPhotoURLs = false;
            return;
        }

        // This is where we tell the OAuth server to start listening on whatever port number we've specified, and to send us the auth token when authentication is complete.
        string serverURL = _oauthCallbackServer.StartListening(portNumber, (string authToken) => {

            // At this point, authentication has completed and we've received the auth token.  Now we'll construct a URL for Instagram's API with that auth token and request some photos.
            string recentMediaURL = "https://api.instagram.com/v1/users/self/media/recent/?access_token=" + Uri.EscapeDataString(authToken);
            APIBuddy.GetInstance().SendGetRequest<InstagramDataResponse>(recentMediaURL, 30.0f, (bool success, string errorMessage, int statusCode, object responseObject) => {
                if (success) {
                    int numPhotosRetrieved = 0;

                    // APIBuddy handles populating InstagramDataResponse for us, based on the fields we set up in it up above.  All we need to do now is
                    // cast responseObject to the InstagramDataResponse type and then read our data!
                    InstagramDataResponse instagramResponse = (InstagramDataResponse)responseObject;
                    foreach (InstagramDataResponse.MediaItem responseData in instagramResponse.data) {
                        if (responseData.type.Equals("image")) { // Make sure we're processing an image entry, and not a video.
                            if (responseData.images != null && responseData.images.standard_resolution != null) {
                                if (responseData.images.standard_resolution.ContainsKey("url")) {
                                    _outputText += responseData.images.standard_resolution["url"] + "\n\n";
                                    numPhotosRetrieved++;
                                }
                            }
                        }
                    }

                    _outputText += "Number of photo URLs retrieved: " + numPhotosRetrieved;
                    _isRequestingPhotoURLs = false;
                } else {
                    _outputText = "Unable to retrieve photo URLs.\n" + errorMessage;
                    _isRequestingPhotoURLs = false;
                }
            });

        });

        // Now that we have the server URL, we can pass it as the redirect URI to Instagram's OAuth API.  When authentication is complete, Instagram will
        // send the user to the redirect URI with the auth token appended, and the server we started up above will capture that auth token so we can make
        // a request to the Instagram API for recent photos.
        string userAuthenticationURL = string.Format("https://api.instagram.com/oauth/authorize/?client_id={0}&redirect_uri={1}&response_type=token", Uri.EscapeDataString(oauthClientId), Uri.EscapeDataString(serverURL));
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
