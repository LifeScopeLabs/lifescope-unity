using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace PerspecDev {

    public class OAuthCallbackServer : MonoBehaviour {

        private HttpListener _httpListener;
        private string _htmlResponseContent = "";
        private string _listenURL = "";
        private string _authToken = "";
        private object _notifyAuthTokenLock = new object();
        private bool _shouldNotifyAuthTokenReceived = false;
        private Action<string> _onComplete;
        
        private void OnDestroy() {
            // Make sure to clean up our references...
            StopListening();
        }

        private void Update() {
            lock(_notifyAuthTokenLock) { // using a lock here because we'll be modifying _shouldNotifyAuthTokenReceived on both the main thread and on HttpListener's async thread.
                if (_shouldNotifyAuthTokenReceived) {
                    if (_onComplete != null) {
                        _onComplete(_authToken);
                    }
                    _shouldNotifyAuthTokenReceived = false;
                }
            }
        }

        /// <summary>
        /// Starts a local web server to listen for the OAuth callback URL.  Returns the URL that we're listening on, which will be the redirect URI to be passed to the OAuth provider.</summary>
        /// <param name="portNumber"> Can be anything you want, but it should be a port number that is unlikely to be used by any other service.  When you register a redirect URL with the OAuth provider, it will need to be in this format: http://127.0.0.1:[port number]/</param>
        /// <param name="onComplete"> Will be called with the auth token as its string parameter when we are authenticated.</param>
        public string StartListening(int portNumber, Action<string> onComplete) {
            return StartListening(portNumber, onComplete, "<h1>Authentication complete.  You may now close this window.</h1>");
        }

        /// <summary>
        /// Starts a local web server to listen for the OAuth callback URL.  Returns the URL that we're listening on, which will be the redirect URI to be passed to the OAuth provider.</summary>
        /// <param name="portNumber"> Can be anything you want, but it should be a port number that is unlikely to be used by any other service.</param>
        /// <param name="onComplete"> Will be called with the auth token as its string parameter when we are authenticated.</param>
        /// <param name="htmlResponseContent"> This is the HTML content that will be presented to in the user's browser after authentication is complete.  It should tell the user that they can close the browser window and return to your application.</param>
        public string StartListening(int portNumber, Action<string> onComplete, string htmlResponseContent) {
            if (_httpListener == null) {
                _onComplete = onComplete;

                if (ShouldRedirectURIFragmentToQueryString()) {
                    // Appending some JavaScript to the end of htmlResponseContent to detect the existence of a URI fragment, and if one exists, to then tell the browser to redirect to a URL with the fragment in a query string instead.
                    htmlResponseContent += "<script type=\"text/javascript\">if (window.location.hash.indexOf('#') >= 0) {location.href='/?uri_fragment='+encodeURIComponent(window.location.hash.substr(1))}</script>";
                }
                _htmlResponseContent = htmlResponseContent;

                // Start up a new HttpListener.  This is the local web server we use to read the redirection URL that the OAuth provider sends the user to after authentication.
                _httpListener = new HttpListener();
                _httpListener.Prefixes.Add("http://+:" + portNumber.ToString() + "/");
                _httpListener.Start();
                _httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), _httpListener);

                _listenURL = "http://127.0.0.1:" + portNumber.ToString() + "/";
            }

            return _listenURL;
        }

        /// <summary>
        /// Stops the HttpListener server and clears out any references we have to it and to the onComplete callback.</summary>
        public void StopListening() {
            if (_httpListener != null) {
                _httpListener.Abort();
                _httpListener = null;
            }
            _onComplete = null;
        }

        /// <summary>
        /// Used by HttpListener as an async callback for us to be able to process incoming HTTP requests.</summary>
        private void ListenerCallback(IAsyncResult result) {
            if (_httpListener != null) {
                try {
                    HttpListenerContext context = _httpListener.EndGetContext(result);
                    ProcessListenerContext(context);
                    HandleListenerContextResponse(context);

                    context.Response.Close();
                    _httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), _httpListener); // EndGetContext above ends the async listener, so we need to start it up again to continue listening.
                } catch (ObjectDisposedException) {
                    // Intentionally ignoring this exception because it will be thrown when we stop listening.
                } catch (Exception exception) {
                    Debug.Log(exception.Message + " : " + exception.StackTrace); // Just in case...
                }
            }
        }

        /// <summary>
        /// Some HTML response content was passed in to the StartListening() method, and this is where we display it to the user.</summary>
        private void HandleListenerContextResponse(HttpListenerContext context) {
            byte[] buffer = Encoding.UTF8.GetBytes(_htmlResponseContent);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Child classes should implement this to specify whether or not to put the URI fragment (hashtag) into a query string and redirect the user to a URL with that query string.  This is almost always necessary if the OAuth provider returns the auth token in the URI fragment because the browser will not pass the fragment to the server (which, in this case, is us).  The redirection will be in the format: http://127.0.0.1:[port]/?uri_fragment=[fragment data]</summary>
        protected virtual bool ShouldRedirectURIFragmentToQueryString() {
            return false;
        }

        /// <summary>
        /// Child classes should implement this and check the context to see if the auth token is in it.  This method will probably be called more than once.  When the auth token has been successfully retrieved, you need to pass it to the NotifyAuthTokenReceived() method.</summary>
        protected virtual void ProcessListenerContext(HttpListenerContext context) {
            // For child classes to implement...
        }

        /// <summary>
        /// Child classes should call this once the auth token has been successfully retrieved.</summary>
        protected void NotifyAuthTokenReceived(string authToken) {
            lock(_notifyAuthTokenLock) {
                // We're not directly calling _onComplete() here because we're still on HttpListener's async thread.
                // We need _onComplete() to be called on the main thread, so we store the auth token and set a flag
                // that will tell us when we should call _onComplete() in the Update() method, which always executes
                // on the main thread.
                _authToken = authToken;
                _shouldNotifyAuthTokenReceived = true;
            }
        }

    }

}
