using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PerspecDev; // APIBuddy;

public class LifeScopeSDK : MonoBehaviour
{

    public class AuthRequestData {
        const string LSurl = "https://app.lifescope.io/auth?";

        string clientID;
        string redirect_uri;
        string scope;
        const string response_type = "code";
        string state;

        public string requestURL() {
            const string url = LSurl + "client_id=" + clientID + "&redirect_uri=" + redirect_uri +
                "&scope=" + scope + "&response_type=" + response_type + "&state=" + state;
            return url;
        }
    }

    public class AuthResponseData : APIBuddy.WebResponseData {
        public string code;
        public string state;
    }

    public class OauthTokenAccessTokenRequestData {
        const string LSurl = "https://app.lifescope.io/auth/access_token?";

        const string grant_type = "authorization_code";
        string client_id;
        string client_secret;
        string redirect_uri;
        string code;

        public string requestURL() {
            const string url = LSurl + "client_id=" + client_id + "&client_secret=" + client_secret +
                "&redirect_uri=" + redirect_uri + "&code=" + code;
            return url;
        }
    }

    public class OauthTokenAccessTokenResponseData : APIBuddy.WebResponseData {
        public string access_token;
        public string refresh_token;
        public string expires_in;
    }

    public class RefreshAccessTokenRequestData {
        const string LSurl = "https://app.lifescope.io/auth/access_token?";

        const string grant_type = "refresh_token";
        string client_id;
        string client_secret;
        string refresh_token;

        public string requestURL() {
            const string url = LSurl + "client_id=" + client_id + "&client_secret=" + client_secret +
                "&refresh_token=" + refresh_token;
            return url;
        }
    }

    public class RefreshAccessTokenResponseData : APIBuddy.WebResponseData {
        public string access_token;
        public string expires_in;
    }
}