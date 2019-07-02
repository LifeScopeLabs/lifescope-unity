# 1 . Create LifeScope account
First, you need to [create a Lifescope account](https://lifescope.io/signup) as well as [an AWS account](https://portal.aws.amazon.com/billing/signup) and a Google account.

When you're logged into LifeScope, go to the [Developer Settings page](https://app.lifescope.io/settings/developer) and create a new app.
Fill in all of the required information.
When it's been created, click on it to be taken to the settings page.
Take note of the Client ID and Client Secret fields (the latter requires you to click 'Show' to see it).
These will need to be added as Environment Variables in the Lambda function you'll create in a later step.

ALso not the Redirect URLs field.
Once you've created the API Gateway in a later step, its URL will be entered here.