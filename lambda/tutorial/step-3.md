# 3. Set up an RDS box and configure networking
There are several AWS services that need to be set up to run this demo.
We’re first going to tackle the networking and creating the SQL server that will hold our user database.
We’re going to create everything from scratch so that you don’t interfere with anything you may already have in AWS.

Go to [IAM roles](https://console.aws.amazon.com/iam/home#/roles) and create a new role. 
Select "AWS Service" as the trusted entity, then click and highlight "Lambda" for the service and click the button
Next: Permissions.
You will need to add four policies to this role:
AWSLambdaBasicExecution
AWSCloudFormationReadOnlyAccess
AWSLambdaVPCAccessExecution
CloudWatchLogsFullAccess

Click Next: Tags, then Next: Review.
Give the role a name, and then click Create Role.
This role will be used by the Lambda function to specify what it has permission to access.

Go to your [VPCs](https://console.aws.amazon.com/vpc/home#vpcs:) and create a new one.
Tag it with something like ‘lifescope-unity-backend’ so you can easily identify it later.
For the IPv4 CIDR block, enter 10.0.0.0/16, or something similar if that is already taken.
Leave IPv6 CIDR block and tenancy as their defaults and create the VPC.
Once it's created, highlight it in the list of your VPC, select Actions -> Edit DNS hostnames, then check the DNS hostnames box to enable it and click Save.

Go to [Internet Gateways](https://console.aws.amazon.com/vpc/home#igws) and create a new one, giving it a useful name.
One it's created, highlight it, then click Actions -> Attach to VPC and attach it to the VPC you created.

View your [Subnets](https://console.aws.amazon.com/vpc/home#subnets).
You should create four new subnets.
Two of these will be public subnets, and two will be private.
Call the public ones ‘public1’ and ‘public2’, and the private ones ‘private1’ and ‘private2’.
Make sure they are all on the ‘lifescope-unity-backend’ VPC we created.
One public and one private subnet should be in the same availability zone, and the other public and private subnets should be in different AZs, e.g. public1 in us-east-1a, public2 in us-east-1c, private1 in us-east-1a, and private2 in us-east-1b.
Remember which AZ is shared between a public and private subnet for later.
The CIDR block needs to be different for each subnet and they all need to fall within the CIDR block of the VPC; if the VPC block is 10.0.0.0/16, you could use 10.0.0.0/24, 10.0.1.0/24, 10.0.2.0/24, and 10.0.3.0/24.
AWS will let you know if anything overlaps.

Go view your [NAT Gateways](https://console.aws.amazon.com/vpc/home#NatGateways).
Create a new Gateway, and for the subnet pick the public subnet that shares an AZ with a private subnet, e.g. ‘public1’ in the example above.
Click Create New EIP and then Create the gateway.
This new gateway should have an ID nat-<ID>.
It should be noted that, while almost everything in this demo is part of AWS’ free tier, NAT gateways are NOT free.
They’re pretty cheap, at about $0.05 per hour and $0.05 per GB of data processed, but don’t forget to delete this when you’re done with the demo (and don’t forget to create a new one and point the private route table to the new one if you revisit this demo).

Go to [Route Tables](https://console.aws.amazon.com/vpc/home#routetables) and create two new ones.
Name one ‘public’ and the other ‘private’, and make sure they’re in the ‘lifescope-unity-backend’ VPC.
When they’re created, click on the ‘private’ one and select the Routes tab at the bottom of the page.
Click Edit, and add another route with a destination of 0.0.0.0/0 and a target of the NAT gateway we just created (so nat-<ID>, not igw-<ID>).
Save the private route table.

Go back to the subnets and click on one of the ‘private’ ones.
Click on the Route Table tab, click Edit, and change it in the dropdown to the ‘private’ Route Table that you created in the previous step.
Then click Save.
Repeat this for the other ‘private’ subnet.

You also need to create a couple of [Security Groups](https://console.aws.amazon.com/vpc/home#securityGroups:).
Name the first one 'lambda' and make sure it's in the 'lifescope-unity-backend' VPC, then create it.
It should have inbound rules allowing all HTTP and HTTPS traffic.
Since Security groups by default allow all outbound traffic, you don't need to change anything there.

Name the other one ‘rds’ and make sure it’s in the ‘lifescope-unity-backend’ VPC, then create it.
Click on it in the list, click on the Inbound Rules tab, and then click Edit.
You’ll want to add a MySQL/Aurora rule (port 3306) where the source is the lambda security group so Lambda can access the RDS box internally.
You do not need to add any Outbound Rules.

Go to [Subnet Groups](https://console.aws.amazon.com/rds/home#db-subnet-groups).
Create a new group and name it something notable, also giving it a description.
Choose the lifescope-unity-backend VPC.
You need to add the two public subnets, which requires first selecting one of the Availability Zones that one of them is in,
then selecting the appropriate subnet.
Remember that one AZ is host to a public and private subnet, so make sure you select the public one.
When both have been added, click Create.


Finally, you will set up the [RDS](https://console.aws.amazon.com/rds/home) box to store the data that will be generated.
Click on Databases and select the Create Databases button.
For this demo we are using MySQL; if you wish to use a different database, you may have to install a different library in the demo project and change the Sequelize dialect to that db.

Click on MySQL (or whatever Engine you want) and then select the version you would like to use from the dropdown (this setup was tested with MySQL 8.0.15).
Under Templates we'd suggest selecting 'Free Tier' if you're doing this for the first time so as not to incur costs, or one of the other options if you're comfortable with the process.

Under Settings, give the DB instance an Identifier such as lifescope-unity-backend so you can easily identify which one it is.
Open the sub-heading 'Credentials Settings', then set the master username and password (maximum 41 characters) to whatever you'd like.
Save these for later reference, as you will need to set Environment Variables in the Lambda function for them so that the function can connect to the DB.

Under DB Instance Size, class that you think will match your needs; db.t2.micro is normally free and should be sufficient for a test run.
If you're using Free Tier, your options will be limited.
Under Storage, the 20GB minimum should be fine.
Enabling autoscaling is your choice.

Under Connectivity, select the ‘lifescope-unity-backend’ VPC.
The Subnet Group should default to the one you recently created.
Under VPC Security Group(s), select the ‘rds’ group we created earlier, and deselect any default groups that may have been selected already.
Also make sure the box is publicly accessible, and make sure the Availability Zone is the one that’s shared between a public and private subnet (us-east-1a in the above example).
Click Launch DB Instance.

Open Additional Configuration and give the database a name, which you will also need to save for later use as a Lambda Environment Variable.
Disable automatic backups if it's not supported for the engine type you're running or you don't think you'll need it.

When all of the above is done, click the Create Database button at the bottom of the page. 

Go to your [RDS instances](https://console.aws.amazon.com/rds/home#dbinstances).
When the box you just created is ready, click on its name to open the summary.
Click on Connectivity & security if it's not already selected.
Take note of the Endpoint field and save this for later use, as it will be used in another Environment Variable.
