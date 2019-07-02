# 4. Deploy code to Amazon Lambda, create API Gateway, deploy static files to S3
With all of the networking squared away, we now need to upload all of our project files to the appropriate services.

First we’re going to create a Lambda function to serve as the backend API views for rendering the main page, signing up new users, logging users in and out, and handling callbacks from the authentication process.
Go to your [Lambda functions](https://console.aws.amazon.com/lambda/home#/functions?display=list) and Create a new function.
Select ‘Author from scratch’ for the blueprint.
Name the function; for reference we’ll call this ‘lifescope-unity-backend’.
Make sure the runtime is ‘Node.js 10.x’.
Open the Permissions section, then select 'Use an existing role' under 'Execution role'.
under 'Existing role', select the IAM role you created in the previous step.
Finally, click the Create Function button.

You should be taken to the new Lambda function's settings page.

You will need to add several Environment Variables:

* CLIENT_ID (The Client ID of your LifeScope app)
* CLIENT_SECRET (The Client Secret of your LifeScope app - click the Show button to see it)
* DATABASE (the database name you set for the RDS box)
* HOST (the endpoint for the RDS box, <Box name>.<ID>.<Region>.rds.amazonaws.com)
* PASSWORD (the password you set for the RDS box)
* PORT (by default it’s 3306)
* SITE_URL (The URL of the API gateway; this will be filled in later)
* USER (the username you picked for the RDS box)

Under Basic Settings set the timeout to 10 seconds to give the function some breathing room.
Select the ‘lifescope-unity-backend’ VPC we created and add the two ‘private’ subnets we created earlier.
Add the ‘lambda’ security group, then click Save.

Next we will create an API gateway to handle traffic to the endpoints that will serve up the views for this project.
Go to the [API Gateway home](https://console.aws.amazon.com/apigateway/home) and click Create API.
Name the API whatever you want; for reference purposes we’ll call it ‘lifescope-unity-backend’.
Make sure the type is ‘New API’ and the protocol is REST and then click Create.

You should be taken to the API you just created.
Click on the Resources link if you aren’t there already.
Highlight the resource ‘/’ (it should be the only one present), click on the Actions dropdown and select ‘Create Resource’.
Enter ‘create_session’ for the Resource Name, and the Resource Path should be filled in with this automatically as well (with a dash instead of an underscore), which is what we want.
Leave the checkboxes unchecked and click the Create Resource button.
When that’s been created, click on the ‘/create_session’ resource.
Click the Actions dropdown and select 'Create Method'.
Click on the blank dropdown that appears and select the method ‘GET’, then click the checkbox next to it.
Make sure the Integration Type is ‘Lambda Function’.
Check ‘Use Lambda Proxy integration’, select the region your Lambda function is in, and enter the name of that Lambda function, then click Save.
Accept the request to give the API gateway permission to access the Lambda function.

What we’ve just done is configure GET requests to the ‘/create_session’ path on our API to point to the Lambda function that has all of the project’s views.
We’re using API Gateway’s Proxy integration, which passes parameters and headers as-is on both requests to and responses from the Lambda function.

We next need to add sub-routes for our other views.
Repeat the process of adding a Resource and then a GET method for the routes 'complete' and 'exchange_code'.

When you’ve done all of that, you should have three resources under ‘/’: ‘/complete’, ‘/create_session’, ‘/exchange_code’.
Click on the ‘/’ resource, then click on the Actions dropdown and select ‘Deploy API’.
For Deployment Stage select ‘New Stage’ and give it a name; we suggest ‘dev’, but it can be anything.
You can leave both descriptions blank.
Click Deploy when you’re done.

The final thing to do is get the URL at which this API is available.
Click ‘Stages’ on the far left, underneath the ‘Resources’ of this API.
Click on the stage you just created.
The URL should be shown as the ‘Invoke URL’ in the top middle of the page on a blue background.

You need to copy this URL into a few places.
One is the SITE_URL Environment Variable in the Lambda function (don’t forget to Save the Lambda function).
The other is as a Redirect URL in the LifeScope app you created in Step 1, with '/complete' added to the end (e.g. https://abcdefg.execute-api.us-east-1.amazonaws.com/dev/complete).
When you've copied it into the text box and added '/complete', make sure to click the green '+' next to it and then click the Save App button.

Navigate to the top level of the 'lambda' directory and run

```
gulp build
```

to compile and package all of the static files to the dist/ folder.

Next we’re going to create an S3 bucket to host our static files.
Go to S3 and create a new bucket.
Give it a name and select the region that’s closest to you, then click Next.
You can leave Versioning, Logging, and Tags disabled, so click Next.
Uncheck 'Block all public access', and do not check anything else.
Click Next, review everything, then click Create Bucket.

Click on the new bucket, then go to the Objects tab and click Upload to have a modal appear.
Click Add Files in this modal and navigate to the ‘dist’ directory in the lambda directory, then into the directory below that (it’s a unix timestamp of when the build process was completed).
Move the file system window so that you can see the Upload modal.
Click and drag the static folder over the Upload modal (S3 requires that you drag-and-drop folders, and this only works in Chrome and Firefox).
Close the file system window, then click Next.
Under Manage Public Permissions, select 'Grant public read access to this object(s)'.
Click Next, then Next again, then review everything and click Upload.

Lastly, go to src/templates/home.html and src/templates/error.html and replace \*\*\*INSERT S3 BUCKET NAME HERE\*\*\* with the name of the S3 bucket you created earlier.
From the top level of the project run

```
gulp bundle
```

to compile the code for the Lambda function to the dist/ folder.
Go to the Lambda function we created earlier, click on the Code tab, then for ‘Code entry type’ select ‘Upload a .ZIP file’.
Click on the Upload button that appears next to ‘Function package’ and select the .zip file in the /dist folder.
Make sure to Save the function.

If all has gone well, you should be able to go to <SITE_URL>/create-session and, with the appropriate query parameters,
go through the process of obtaining a LifeScope user's OAuth token on the backend, then exchanging the access code it
returns for the oauth token.