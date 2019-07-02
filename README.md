Support Priority

3D > VR > AR
Android > iOS > Windows > Mac > Magic Leap

 1. Sign In URL to Broker (unity.lifescope.io)
      Current Broker Url in Prod
      https://r4o1ekqi1d.execute-api.us-east-1.amazonaws.com/dev/create-session?client_id=e33af1dc0124dbf5&scope=events:read
      
 2. Add token broker code from URL
 
      2.1     Ability To type in Code
      
      2.2     Paste Support
      
      2.3     Exchange code for token from broker service
              https://r4o1ekqi1d.execute-api.us-east-1.amazonaws.com/dev/exchange-code?access_code=<code> is the endpoint you have to hit to exchange the code for the oauth token.
              
      2.4     Save LifeScope OAuth API token to disk in plain text file. Save token to in memory global variable too?
              
              https://answers.unity.com/questions/1320236/what-is-binaryformatter-and-how-to-use-it-and-how.html
              Catch failed API calls and return an error.
              
  3.  Call for User info as a test Query. https://api.lifescope.io/gql-p  userBasic
              https://lifescope.io/platform
              https://lifescope.io/schema
              
              Ensure the Header is Correct https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
              Call The query. Look at diff from GET/POST vers with and without forms
                  https://docs.unity3d.com/Manual//UnityWebRequest-RetrievingTexture.html
                  https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.Post.html
              Format the Json into an object https://docs.unity3d.com/ScriptReference/JsonUtility.html
                    https://docs.unity3d.com/Manual/JSONSerialization.html
              Catch failed API calls and return an error.
      
 4.    Session Refresh
  
       If a request fails with an expired token error, go to "logged out" state and redirect the user to the beginning to re-signin and get another token from the broker.
 
 3. Example 1: Show List of People
 
    https://api.lifescope.io/gql-p FilterFindManyPeopleInput 
  
    Parse JSON To Menue Elements 
        https://docs.unity3d.com/Manual/JSONSerialization.html
        https://learn.unity.com/tutorial/live-sessions-on-ui#

    Get profile photos https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequestTexture.GetTexture.html

 4. Example 2: Show VR / AR map of events with locations

        Requires Mapbox Unity SDK / Android / iOS / ARCore / AR Kit

        Execute Examples w/ location Query https://api.lifescope.io/gql-p SearchesFiltersInput ASK KYLE

        3D / VR View
        https://docs.mapbox.com/unity/maps/examples/city-simulator/

        AR Tabletop
        https://docs.mapbox.com/unity/maps/examples/tabletop-ar/

        Worldscale
        https://docs.mapbox.com/unity/maps/examples/world-scale-manual-align-ar/