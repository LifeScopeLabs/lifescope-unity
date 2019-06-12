using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SimpleJSON;

namespace PerspecDev {

    public class APIBuddy : MonoBehaviour {

        /*
            WebResponseData is the class you should extend from in order to define the data you're
            interested in receiving from the API you're connecting to.
            
            Some JSON APIs return an array of objects, rather than a single object.  In that case,
            you need to have an array field called "entries" in your WebResponseData subclass, and the
            element type of the array should also be a subclass of WebResponseData.
        */
        public abstract class WebResponseData { }

        /*
            WebRequestDetails is used internally by APIBuddy to keep track of each request.
        */
        protected class WebRequestDetails {
            public WWW www;
            public object responseObject;
            public Type responseObjectType;
            public float startTime;
            public float timeout;
            public WebResponsePopulateDelegate webResponsePopulateDelegate;
            public WebRequestFinishedDelegate webRequestFinishedDelegate;

            public WebRequestDetails(string url, WWWForm form, float timeout, Type responseObjectType
                                     , WebResponsePopulateDelegate webResponsePopulateDelegate
                                     , WebRequestFinishedDelegate webRequestFinishedDelegate) {
                if (form != null) {
                    this.www = new WWW(url, form);
                } else {
                    this.www = new WWW(url);
                }
                this.responseObject = null;
                this.responseObjectType = responseObjectType;
                this.startTime = 0.0f;
                this.timeout = timeout;
                this.webResponsePopulateDelegate = webResponsePopulateDelegate;
                this.webRequestFinishedDelegate = webRequestFinishedDelegate;
            }

            public void MarkStartTime() {
                startTime = Time.time;
            }

            public bool HasTimedOut() {
                return Time.time - startTime > timeout;
            }
        }
        
        // You will provide a delegate matching this signature in order to recieve a response from the API when you call one of the SendXYZRequest methods below.
        public delegate void WebRequestFinishedDelegate(bool success, string errorMessage, int statusCode, object responseObject);

        // See the DefaultJSONPopulateDelegate method below for descriptions of these parameters.
        public delegate bool WebResponsePopulateDelegate(WWW response, out object responseObjectToPopulate, Type responseObjectType); // Should return success bool.

        private static APIBuddy _instance;
        private static bool _applicationIsQuitting = false;

        protected Dictionary<string, WebRequestDetails> _webRequests = new Dictionary<string, WebRequestDetails>();
        
        public static APIBuddy GetInstance() {
            // We check to make sure the application isn't quitting before we create an API Buddy GameObject.
            // Otherwise, if some class were to call GetInstance() in its OnDestroy or OnDisable methods, we would
            // potentially be creating a new GameObject when the game quits, and we would get warning messages
            // about that in Unity.
            if (_instance == null && !_applicationIsQuitting) {
                GameObject apiBuddyGameObject = new GameObject("API Buddy");
                _instance = apiBuddyGameObject.AddComponent<APIBuddy>();
            }

            return _instance;
        }

        private void OnApplicationQuit() {
            _applicationIsQuitting = true;
        }

        private void Update() {
            // We're setting up this webRequestsToRemove list and the keysArray so that we can remove specific web requests
            // down below, since we don't want to remove them while iterating over the _webRequests dictionary.
            List<string> webRequestsToRemove = new List<string>();
            string[] keysArray = _webRequests.Keys.ToArray();
            int total = keysArray.Length;
            for (int i = 0; i < total; ++i) {
                string key = keysArray[i];

                WebRequestDetails webRequestDetails = _webRequests[key];
                if (webRequestDetails.www.isDone) {
                    // The web request is done, so populate any data we need, call the finished delegate, then remove it from the _webRequests dictionary.
                    bool success = false;
                    if (string.IsNullOrEmpty(webRequestDetails.www.error)) {
                        if (webRequestDetails.webResponsePopulateDelegate != null) {
                            success = webRequestDetails.webResponsePopulateDelegate(webRequestDetails.www, out webRequestDetails.responseObject, webRequestDetails.responseObjectType);
                        }
                    }

                    if (webRequestDetails.webRequestFinishedDelegate != null) {
                        webRequestDetails.webRequestFinishedDelegate(success, webRequestDetails.www.error, ParseStatusCode(webRequestDetails.www), webRequestDetails.responseObject);
                    }

                    webRequestsToRemove.Add(key);
                } else if (webRequestDetails.HasTimedOut()) {
                    // The web request timed out, so call the finished delegate, then remove it from the _webRequests dictionary.
                    if (webRequestDetails.webRequestFinishedDelegate != null) {
                        webRequestDetails.webRequestFinishedDelegate(false, "Request timed out.", ParseStatusCode(webRequestDetails.www), null);
                    }

                    webRequestsToRemove.Add(key);
                }
            }

            foreach (string key in webRequestsToRemove) {
                _webRequests.Remove(key);
            }
        }

        /// <summary>
        /// Returns the upload progress for a specific requestIdentifier.</summary>
        /// <param name="requestIdentifier"> The requestIdentifier that was returned to you when you called SendPostRequest or SendWebRequest.</param>
        public float GetUploadProgress(string requestIdentifier) {
            if (_webRequests.ContainsKey(requestIdentifier)) {
                WWW www = _webRequests[requestIdentifier].www;
                if (www != null) {
                    return www.uploadProgress;
                }
            }

            return 0.0f;
        }

        /// <summary>
        /// Sends a GET request to an API endpoint.</summary>
        /// <param name="url"> The URL of the API endpoint.</param>
        /// <param name="timeout"> How long to wait for a response from the server before we give up.</param>
        /// <param name="onComplete"> This will be called when the request has completed (for both success and failure).</param>
        public string SendGetRequest<T>(string url, float timeout, WebRequestFinishedDelegate onComplete) {
            return SendWebRequest<T>(url, null, timeout, DefaultJSONPopulateDelegate, onComplete);
        }

        /// <summary>
        /// Sends a POST request to an API endpoint.</summary>
        /// <param name="url"> The URL of the API endpoint.</param>
        /// <param name="form"> When sending a POST request, you should create and populate a WWWForm object that defines the variables you are sending to the server.</param>
        /// <param name="timeout"> How long to wait for a response from the server before we give up.</param>
        /// <param name="onComplete"> This will be called when the request has completed (for both success and failure).</param>
        public string SendPostRequest<T>(string url, WWWForm form, float timeout, WebRequestFinishedDelegate onComplete) {
            return SendWebRequest<T>(url, form, timeout, DefaultJSONPopulateDelegate, onComplete);
        }

        /// <summary>
        /// Sends a POST request to an API endpoint.</summary>
        /// <param name="url"> The URL of the API endpoint.</param>
        /// <param name="form"> When sending a POST request, you should create and populate a WWWForm object that defines the variables you are sending to the server.  If this parameter is not null, the request is assumed to be a POST request.  Otherwise it's a GET request.</param>
        /// <param name="timeout"> How long to wait for a response from the server before we give up.</param>
        /// <param name="webResponsePopulateDelegate"> This will be called when we have data back from the server, in order to allow us to populate response data into an object.  If you are calling a JSON API, you don't have to worry about this, since then you can just call either the SendGetRequest or SendPostRequest method, and it will take care of populating your response object for you.</param>
        /// <param name="onComplete"> This will be called when the request has completed (for both success and failure).</param>
        public string SendWebRequest<T>(string url, WWWForm form, float timeout
                                      , WebResponsePopulateDelegate webResponsePopulateDelegate, WebRequestFinishedDelegate onComplete) {

            // We're creating a unique identifier and passing it back out of this method so that upload progress can be tracked, if we're uploading a file.
            string requestIdentifier = Guid.NewGuid().ToString();

            WebRequestDetails webRequestDetails = new WebRequestDetails(url, form, timeout, typeof(T), webResponsePopulateDelegate, onComplete);
            webRequestDetails.MarkStartTime();
            _webRequests.Add(requestIdentifier, webRequestDetails);

            return requestIdentifier;
        }
        
        /// <summary>
        /// Looks at the HTTP headers of a WWW response and attempts to parse out the status code.</summary>
        protected int ParseStatusCode(WWW response) {
            int statusCode = 0;

            if (response.responseHeaders != null) {
                if (response.responseHeaders.ContainsKey("STATUS")) {
                    string statusLine = response.responseHeaders["STATUS"];

                    string[] components = statusLine.Split(' ');
                    if (components.Length == 2) {
                        int.TryParse(components[0], out statusCode);
                    } else if (components.Length >= 3) {
                        int.TryParse(components[1], out statusCode);
                    }
                }
            }

            return statusCode;
        }

        /// <summary>
        /// Reads in the data from a JSON API response and populates a response object for you.</summary>
        /// <param name="response"> The raw WWW response object, as populated by Unity with the API's response.</param>
        /// <param name="responseObjectToPopulate"> The response object that we'll create and populate with data from the API.  We'll use the fields on this object's type definition in order to determine what data we want from the JSON response.</param>
        /// <param name="responseObjectType"> The type definition of the response object that we'll create and populate.</param>
        protected bool DefaultJSONPopulateDelegate(WWW response, out object responseObjectToPopulate, Type responseObjectType) {
            int statusCode = ParseStatusCode(response);
            if (statusCode >= 200 && statusCode <= 299) { // Make sure the server didn't give us an error.
                responseObjectToPopulate = Activator.CreateInstance(responseObjectType);

                // Strip out comments and attempt to stringify nulls, both of which would otherwise mess up the SimpleJSON parser.
                string jsonText = response.text;
                Regex regexRemoveComments = new Regex(@"\/\*.*?\*\/", RegexOptions.Singleline);
                jsonText = regexRemoveComments.Replace(jsonText, "");
                Regex regexReplaceNulls = new Regex(@"('|"")\w?:\w?null", RegexOptions.Singleline);
                jsonText = regexReplaceNulls.Replace(jsonText, ": \"null\"");

                JSONNode nodeToUse;
                JSONNode rootNote = JSON.Parse(jsonText);

                if (jsonText.StartsWith("[")) {
                    // If the JSON response starts with a [ then the top level element is an array of objects, and we need someplace to store them,
                    // so create a node called "entries" and put them there.  This way, we'll look for an "entries" field on the responseObject we
                    // created and that's what we'll use to store the entries.
                    JSONNode newParentNode = new JSONClass();
                    newParentNode.Add("entries", rootNote);
                    nodeToUse = newParentNode;
                } else {
                    nodeToUse = rootNote;
                }

                // Use a little bit of reflection magic to find and populate the top level fields in the responseObject.  The GetValueFromJSONNode method
                // is recursive, so it will handle the rest.
                FieldInfo[] publicFields = responseObjectType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo publicField in publicFields) {
                    publicField.SetValue(responseObjectToPopulate, GetValueFromJSONNode(nodeToUse, publicField.Name, publicField.FieldType));
                }

                return true;
            } else {
                responseObjectToPopulate = null;

                return false;
            }
        }

        /// <summary>
        /// Recursively gets values from a JSON node, based on the field name and type we pass in from the WebResponseData subclass we're populating as a response object.</summary>
        /// <param name="node"> The JSON node to pull the value from.</param>
        /// <param name="fieldName"> The field name that we want to match in the JSON node.  We can also pass null if we just want to use the JSON node as is, and don't want a child of it.</param>
        /// <param name="fieldType"> The type definition of the value we want to return.  A JSON node is agnostic to type, so we need to know what type we're looking for in order to be able to return a sensible value.</param>
        protected object GetValueFromJSONNode(JSONNode node, string fieldName, Type fieldType) {
            JSONNode nodeToUse;
            if (fieldName != null) { // If the field name isn't null, we're wanting to grab the value of a child of the JSON node, rather than the value of the node itself.
                nodeToUse = node[fieldName];
            } else {
                nodeToUse = node;
            }

            // Check which type we're looking for, and return a sensible value based on that.
            if (typeof(string).IsAssignableFrom(fieldType)) {
                return nodeToUse.Value;
            } else if (typeof(int).IsAssignableFrom(fieldType)) {
                return nodeToUse.AsInt;
            } else if (typeof(bool).IsAssignableFrom(fieldType)) {
                return nodeToUse.AsBool;
            } else if (typeof(float).IsAssignableFrom(fieldType)) {
                return nodeToUse.AsFloat;
            } else if (typeof(double).IsAssignableFrom(fieldType)) {
                return nodeToUse.AsDouble;
            } else if (typeof(Array).IsAssignableFrom(fieldType)) {
                JSONArray jsonArray = nodeToUse.AsArray;
                if (jsonArray == null) return null;

                var array = Array.CreateInstance(fieldType.GetElementType(), jsonArray.Count);
                for (int i = 0; i < jsonArray.Count; ++i) {
                    array.SetValue(GetValueFromJSONNode(jsonArray[i], null, fieldType.GetElementType()), i);
                }
                return array;
            } else if (typeof(List<>).IsAssignableFrom(fieldType)) {
                JSONArray jsonArray = nodeToUse.AsArray;
                if (jsonArray == null) return null;

                var array = Array.CreateInstance(fieldType.GetElementType(), jsonArray.Count);
                for (int i = 0; i < jsonArray.Count; ++i) {
                    array.SetValue(GetValueFromJSONNode(jsonArray[i], null, fieldType.GetElementType()), i);
                }

                Type listType = typeof(List<>).MakeGenericType(fieldType.GetElementType());
                var list = Activator.CreateInstance(listType, array);

                return list;
            } else if (typeof(WebResponseData).IsAssignableFrom(fieldType)) {
                JSONClass jsonClass = nodeToUse.AsObject;
                if (jsonClass == null) return null;

                var classObject = Activator.CreateInstance(fieldType);

                FieldInfo[] publicFields = fieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo publicField in publicFields) {
                    publicField.SetValue(classObject, GetValueFromJSONNode(jsonClass, publicField.Name, publicField.FieldType));
                }

                return classObject;
            } else if (typeof(Dictionary<string,string>).IsAssignableFrom(fieldType)) {
                JSONClass jsonClass = nodeToUse.AsObject;
                if (jsonClass == null) return null;


                Dictionary<string, string> dictionary = new Dictionary<string, string>();

                // JSONClass doesn't expose its backing dictionary, but we need to know what its keys are in order to recursively
                // get the values, so we're using reflection to find expose the backing dictionary.  The alternative would be to
                // modify SimpleJSON.cs to make the backing dictionary public, but since SimpleJSON.cs is 3rd party code, I wanted
                // to modify it as little as possible so that you can use your own copy of SimpleJSON.cs if you've already got it
                // set up in your project.
                FieldInfo fieldInfo = typeof(JSONClass).GetField("m_Dict", BindingFlags.Instance | BindingFlags.NonPublic);
                Dictionary<string, JSONNode> backingDictionary = (Dictionary<string, JSONNode>)fieldInfo.GetValue(jsonClass);

                foreach (string key in backingDictionary.Keys) {
                    dictionary.Add(key, (string)GetValueFromJSONNode(jsonClass[key], null, typeof(string)));
                }

                return dictionary;
            } else {
                return null;
            }
        }

    }

}