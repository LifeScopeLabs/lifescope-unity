using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;


public class LifeScopeClient : MonoBehaviour {

    /*
        LifeScope uses OAuth 2.0 to authenticate its API clients.  If you don't need OAuth, check out IPtoLocation.cs or TeamFortressNews.cs for simpler examples of using APIBuddy.

        If you want to run the example below, you'll need to have registered an LifeScope app.  If you haven't done that already, you can do so here:
        https://app.lifescope.io/settings/developer
    */

    public string oauthClientId = "e33af1dc0124dbf5"; // Put your LifeScope OAuth client ID here.
    public string[] scopes = new string[] { //If you don't need a certain type of data, remove it from this list
        "basic",
        "contacts:read",
        "content:read",
        "events:read",
        "locations:read",
        "people:read"
    };

    private string _state = "loginRequired";
    private bool _hasToken = false;
    private string _outputText = "";
    private string _accessCode = "";
    private string _oauthToken = "";
    private string _userId = "";

    static string _exchangeBackendUrl = "https://unity.lifescope.io"; //You should enter your own setup's exchange backend URL here.
    static HttpClient exchangeClient = new HttpClient();

    static GraphQLClient graphQLClient = new GraphQLClient("https://api.lifescope.io/gql");


    public class OauthTokenResponse {
        public string oauth_token { get; set; }
    }

    private void Start() {

    }

    private async void OnGUI() {
        if (_state == "loginRequired") {
            if (GUI.Button(new Rect(20.0f, 20.0f, 200.0f, 50.0f), "Log in to LifeScope")) {
                string stringifiedScopes = string.Join(",", scopes);
                string userAuthenticationURL = string.Format("https://unity.lifescope.io/create-session?client_id={0}&scopes={1}", Uri.EscapeDataString(oauthClientId), Uri.EscapeDataString(stringifiedScopes));
                Application.OpenURL(userAuthenticationURL);

                _state = "enterCode";
            }
        }
        else if (_state == "enterCode") {
            GUI.TextArea(new Rect(20.0f, 20.0f, 200.0f, 50.0f), "Enter the 8-character code you should have gotten after completing the previous step.");
            _accessCode = GUI.TextField(new Rect(20.0f, 90.0f, 200.0f, 20.0f), _accessCode, 8);

            if (GUI.Button(new Rect(20.0f, 120.0f, 100.0f, 50.0f), "Submit")) {
                try {
                    HttpResponseMessage response = await exchangeClient.GetAsync(_exchangeBackendUrl + "/exchange-code?access_code=" + _accessCode);

                    _accessCode = "";

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<OauthTokenResponse>(responseBody);

                    _oauthToken = jsonResponse.oauth_token;

                    _hasToken = true;
                    _state = "getUserData";
                }
                catch(HttpRequestException e) {
                    Debug.Log("Access code exchange error:");
                    Debug.Log(e.Message);
                }
            }
        }
        else if (_state == "getUserData") {
            GraphQLRequest userRequest = new GraphQLRequest{
                Query = @"
                {
                    userBasic {
                        id
                    }
                }"
            };

            graphQLClient.DefaultRequestHeaders.Add("authorization", "Bearer " + _oauthToken);
            var graphQLResponse = await graphQLClient.PostAsync(userRequest);

            if (graphQLResponse.Errors != null) {
                Debug.Log(graphQLResponse.Errors);
            }
            else {
                _userId = graphQLResponse.Data.userBasic.id;

                Debug.Log(_userId);

                _state = "populateData";
            }
        }
        else if (_state == "populateData") {
            GUI.TextArea(new Rect(20.0f, 20.0f, 50.0f, 50.0f), _userId);
        }
    }
}
