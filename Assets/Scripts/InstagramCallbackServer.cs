using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace PerspecDev {

    public class InstagramCallbackServer : OAuthCallbackServer {

        /// <summary>
        /// Instagram passes the auth token in the URI fragment (hashtag).  Since the browser doesn't pass that to us by default, we need to do a redirect with the fragment in the query string instead.</summary>
        protected override bool ShouldRedirectURIFragmentToQueryString() {
            return true;
        }

        /// <summary>
        /// Processes the input request when the OAuth provider sends the user to the redirect URI after completing authentication.  This allows us to retrieve the auth token.</summary>
        protected override void ProcessListenerContext(HttpListenerContext context) {
            // Attempt to pull out the URI fragment as a part of the query string.
            string uriFragment = context.Request.QueryString["uri_fragment"];
            if (uriFragment != null) { // If it worked, that means we're being passed the auth token from Instagram, so pull it out and notify that we received it.
                string authToken = uriFragment.Replace("access_token=", "");
                NotifyAuthTokenReceived(authToken);
            }
        }

    }

}
