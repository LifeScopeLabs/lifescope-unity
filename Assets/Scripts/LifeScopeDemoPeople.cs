using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using GraphQL.Client;
using GraphQL.Common.Request;


public class LifeScopeDemoPeople : MonoBehaviour {
    public string oauthClientId = "e33af1dc0124dbf5"; // Put your LifeScope OAuth client ID here.

    public Button LoginButton;
    public Button SubmitCodeButton;
    public Button LogoutButton;
    public Canvas LoginCanvas;
    public Canvas TokenCanvas;
    public Canvas FinishCanvas;

    public GameObject prefabPerson;

    public string[] inputScopes = new string[] {
        "basic",
        "people:read"
    };

    private string _accessCode = "";
    private Newtonsoft.Json.Linq.JArray _people;
    private List<GameObject> _peopleInstances;

    static string _exchangeBackendUrl = "https://unity.lifescope.io"; //You should enter your own setup's exchange backend URL here.
    static HttpClient exchangeClient = new HttpClient();
    static GraphQLClient graphQLClient = new GraphQLClient("https://api.lifescope.io/gql");

    private class _OAuthTokenResponse {
        public string oauth_token { get; set; }
    }

    void Start() {
        LoginButton.onClick.AddListener(LoginListener);
        SubmitCodeButton.onClick.AddListener(SubmitCodeListener);
        LogoutButton.onClick.AddListener(LogoutListener);

        SaveLoad.Load();

        LoginCanvas.enabled = false;
        TokenCanvas.enabled = false;
        FinishCanvas.enabled = false;

        if (SaveLoad.credentials != null && SaveLoad.credentials.OAuthToken != null && SaveLoad.credentials.OAuthToken.Length == 64) {
            getPeople();
            FinishCanvas.enabled = true;
        }
        else {
            LoginCanvas.enabled = true;
        }
    }

    void LoginListener() {
        LifeScopeLogin.setScopes(inputScopes);
        LifeScopeLogin.launchAuth();

        LoginCanvas.enabled = false;
        TokenCanvas.enabled = true;
    }

    async void SubmitCodeListener() {
        try {
            _accessCode = GameObject.Find("CodeInputField").GetComponent<InputField>().text;
            HttpResponseMessage response = await exchangeClient.GetAsync(_exchangeBackendUrl + "/exchange-code?access_code=" + _accessCode);

            _accessCode = "";

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<_OAuthTokenResponse>(responseBody);

            SaveLoad.credentials.OAuthToken = jsonResponse.oauth_token;
            SaveLoad.Save();

            TokenCanvas.enabled = false;
            FinishCanvas.enabled = true;

            getPeople();
        }
        catch(HttpRequestException e) {
            Debug.Log("Access code exchange error:");
            Debug.Log(e.Message);
        }
    }

    async void getPeople() {
        GraphQLRequest peopleRequest = new GraphQLRequest{
            Query = @"
                query personMany($filter: FilterFindManyPeopleInput, $sort: SortFindManyPeopleInput) {
                    personMany(filter: $filter, sort: $sort) {
                        id,
                        avatar_url,
                        external_avatar_url,
                        first_name,
                        middle_name,
                        last_name,
                        contact_ids,
                        contact_id_strings,
                        hidden,
                        hydratedContacts {
                            id,
                            avatar_url,
                            handle,
                            name,
                            connection_id,
                            hydratedConnection {
                                id,
                                provider_id,
                                provider {
                                    id,
                                    name
                                },
                            }
                        }
                    }
                }",
            Variables = new {
                sort = "last_name",
                filter = new {
                    self = false
                }
            }
        };

        graphQLClient.DefaultRequestHeaders.Add("authorization", "Bearer " + SaveLoad.credentials.OAuthToken);
        var graphQLResponse = await graphQLClient.PostAsync(peopleRequest);

        if (graphQLResponse.Errors != null) {
            Debug.Log(JsonConvert.SerializeObject(graphQLResponse.Errors));

            TokenCanvas.enabled = false;
            FinishCanvas.enabled = false;
            LoginCanvas.enabled = true;
        }
        else {
            _people = graphQLResponse.Data.personMany;

            populatePeople();
        }
    }

    void LogoutListener() {
        SaveLoad.credentials.OAuthToken = "";
        SaveLoad.Save();

        for (int i = 0; i < _peopleInstances.Count; i++) {
            Destroy(_peopleInstances[i]);
        }

        TokenCanvas.enabled = false;
        FinishCanvas.enabled = false;
        LoginCanvas.enabled = true;
    }

    void populatePeople() {
        _peopleInstances = new List<GameObject>();

        for (int i = 0; i < _people.Count; i++) {
            var LSPerson = _people[i];
            GameObject person = (GameObject)Instantiate(prefabPerson, new Vector3(i * 650.0F, 0, 0), Quaternion.identity);
            var Script = person.GetComponent<LifeScopePersonMod>();

            _peopleInstances.Add(person);

            if (LSPerson["avatar_url"] != null && LSPerson["avatar_url"] != null && LSPerson["avatar_url"].ToString().Length > 0) {
                StartCoroutine(DownloadImage(Script, LSPerson["avatar_url"].ToString()));
            }

            Script.setName(LSPerson["first_name"].ToString(), LSPerson["middle_name"].ToString(), LSPerson["last_name"].ToString());

            var layoutScript = person.GetComponentInChildren<GridLayoutScript>();

            layoutScript.populateContacts(LSPerson["hydratedContacts"]);
        }
    }

    IEnumerator DownloadImage(LifeScopePersonMod Script, string AvatarUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(AvatarUrl);
        yield return request.SendWebRequest();

        if(request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
            Script.setTexture(((DownloadHandlerTexture) request.downloadHandler).texture);
    }
}
