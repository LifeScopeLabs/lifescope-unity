# 5. Using the Unity token exchange

There's a three-step process for using this backend token exchange to obtain a user's LifeScope oauth credentials.

## 1. Create a session and redirect the user to the LifeSope OAuth flow

You'll first need to send the user to the /create-session endpoint, e.g. https://abcd1234.execute-api.us-east-1.amazonaws.com/dev/create-session.
You must provide a comma-delimited set of scopes that you want to request on behalf of that user.
The scopes that can be requested are as follows:

```
basic
contacts:read
content:read
events:read
locations:read
people:read
```

So, if you wanted to get a user's basic info, events, and people, you'd redirect them to
```
https://abcd1234.execute-api.us-east-1.amazonaws.com/dev/create-session?scopes=basic,events:read,people:read
```

This endpoint will create a session ID for that request and save a cookie on the user's browser called 'unity_session_id'.
The cookie expires after five minutes, and each invocation of create-session clears any sessions older than five minutes as well.

Once that's done, the user is redirected to the LifeScope OAuth workflow.

## 2. User approves OAuth grant and is redirected to /complete, where they are given an access_code

The user will be prompted to log in to LifeScope if they aren't already.
When they're logged in, they'll be asked whether they approve of the scopes your app is requesting.
If they do, LifeScope will redirect them back to the /complete endpoint.
This endpoint will receive a code from the OAuth flow, which it will exchange with the LifeScope servers for an OAuth token.
The endpoint will also generate a random 8-character alphanumeric access_code that is does not conflict with any current auth sessions.

Both the OAuth token and the access_code will be saved to their auth_session, which is retrieved from the unity_session_id cookie.
The user is then presented with a simple page showing the access_code.
They will need to enter this code somehwere in your Unity app for you app to get the OAuth token.

## 3. access_code is exchanged for the OAuth token via /exchange-code

The third endpoint is /exchange-code.
You must make a GET request to this endpoint from your Unity application with the query parameter 'access_code', e.g.

```
GET https://abcd1234.execute-api.us-east-1.amazonaws.com/dev/exchange-code?access_code=a1B2c3D4
```

The endpoint takes the access_code, looks up the corresponding auth_session, and then sends back a JSON response
with the OAuth token like thus:

```
{
    "oauth_token": a78378234bc2348778d
}
```

If you want to re-use this token in future sessions, your app will need to store it somewhere permanent.