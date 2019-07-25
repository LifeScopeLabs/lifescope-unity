using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;


public static class LifeScopeLogin {
    public static string oauthClientId = "e33af1dc0124dbf5"; // Put your LifeScope OAuth client ID here.

    public static string[] scopes = new string[] { //If you don't need a certain type of data, remove it from this list
        "basic"
    };

    public static void setScopes (string[] inputScopes) {
        scopes = inputScopes;
    }

    public static void launchAuth() {
        string stringifiedScopes = string.Join(",", scopes);
        string userAuthenticationURL = string.Format("https://unity.lifescope.io/create-session?client_id={0}&scopes={1}", Uri.EscapeDataString(oauthClientId), Uri.EscapeDataString(stringifiedScopes));
        Application.OpenURL(userAuthenticationURL);
    }
}