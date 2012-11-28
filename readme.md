AzureDemos-CloudServices-InterRoleCommunication
==============

A Windows Azure cloud services solutions with both web and worker roles, that demonstrates direct communication from the web role instance/s to the worker role instance/s via a load balanced HTTP endpoint with an HttpListener running on the worker roles.

Here’s well look at a simple MVC 4 WebRole, that communicates with a WorkerRole via HTTP. Because the PaaS VM configuration for worker roles, does not include IIS, will use the HTTP.sys module using an HttpListener. Endpoints on roles must be configured explicitly (webroles by default will have a port 80 HTTP endpoint). Using system level modules such as HTTP.sys requires elevated execution model, configured via ServiceDefinition.csdef.

1.	Open the InterRoleCommunicationDemo solution.
2.	Review the solution:
3.	Simple solution with 2 roles.
4.	The worker role, kicks of an HttpListener in OnStart. Because this library wrap HTTP.sys, will fail if not run in an elevated context.
5.	Note: That the HttpListener will host on the endpoint defined by the Cloud Project’s configuration.
6.	ServiceDefinintion.csdef with the <Runtime executionContext="elevated" />. Not the default instance VM sizes.
7.	Bring the role view up for the Rorker Role, and review its endpoint on 10100.
8.	Go to the WebRole, and note how it has a default endpoint of 80.
9.	Review the HomeController for the WebRole, and note the simple code that uses an WebRequest to communicate with the Worker Role.
10.	Note the different configuration for Local vs Cloud.
11.	Run the solution locally in the emulator.
