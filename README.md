# LifeScopeUnitySDK
LifeScope Unity SDK

  # Outgoing Parameters
    client_id	        string:    The client_id for your registered OAuth app.
    redirect_uri	    string:	    A redirect URI registered to your Oauht app.
    scope	            string:	    A comma-delimited list of scopes that your app is requesting access to.
    response_type	    string:	
    Must be 'code'.state	string: A unique identifier that is passed through the authorization process a
  ## Incoming Parameters
    Name	Type	Value
    error	string	access_denied
    error_description	string	The user denied the request

  ** note **
    Most other errors, such as invalid scopes or missing parameters, will be returned to your redirect_uri in a similar fashion. The only     exceptions are if the client_id is invalid or the redirect_uri is not associated with that oauth app. In those situations, the errors     will just be displayed on the screen so not to redirect the user to potentially malicious URLs. If the user allows the request and    
    there are no errors, they will be redirected to the redirect URI with two query parameters

# GraphQL
  ## Outgoing Variables
    Name	Type	Description
    grant_type	string	Must be 'authorization_code' for this operation.
    client_id	string	The client_id for your registered OAuth app.
    client_secret	string	The client_secret for your registered OAuth app.
    redirect_uri	string	The redirect_uri that was used to obtain the code.
    code	string	The code you received back from the authorization step.
    
   ## Returned Fields
    Name	Type	Description
    access_token	string	The scoped token that you can use to access the user's information.
    refresh_token	string	A token used to generate a new access_token when the current one expires.
    expires_in	string	The number of seconds until the access_token becomes invalid. 
mutation oauthTokenAccessToken($grant_type: String!, $code: String, $redirect_uri: String!, $client_id: String!, $client_secret: String!) {
  oauthTokenAccessToken(grant_type: $grant_type, code: $code, redirect_uri: $redirect_uri, client_id: $client_id, client_secret: $client_secret) {
    access_token,
    refresh_token,
    expires_in
  }
}
Save the access_token and refresh_token somewhere for later use.

## Outgoing Parameters (Required are bolded)

Name	Type	Description
grant_type	string	Must be 'authorization_code' for this operation.
client_id	string	The client_id for your registered OAuth app.
client_secret	string	The client_secret for your registered OAuth app.
redirect_uri	string	The redirect_uri that was used to obtain the code. It must exactly match the redirect_uri used to obtain the code.
code	string	The code you received back from the authorization step.
