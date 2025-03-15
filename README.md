# SnapWeb-Dotnet

## Projects
This repository holds the 3 pieces required for snapweb sites to work:

### SnapWebManager
This is the site that is used as a management panel for snapweb users. In it, admins can handle certain settings that will be picked up by
Snapweb sites.

### SnapWebModels
This project holds models that are shared between the SnapWebManager and SnapWeb projects

### TaskBoard
This is the SnapWeb project which is the site that we expose to users.

## Infrastructure

To be able to more easily manage instances of SnapWeb, we have setup a registry as well as a docker image holding the site.
The same has been done for SnapWebManager, so that every site can easily be deployed as a service in docker using `docker-compose`

### Cloud Servers

The infrastructure is being hosted in Hetzner and is comprised of 3 servers inside a `docker swarm` and one server that provides
access to a glusterfs volume.

### Portainer

Portainer has been deployed as a docker container to be able to manage the services in the docker swarm instance.
The URL: [portainer.jsnap.io](https://portainer.jsnap.io)

### Traefik

Traefik is the reverse proxy solution used within the docker swarm. This reverse proxy configuration is linked to the different stacks
that Portainer uses to bring up services, so no extra setup is required on it

### Docker Registry

We have our own docker registry where the SnapWeb and SnapWebManager images are being pushed to. The registry can be accessed through docker commands
with its URL: `registry.jsnap.io`

## Build Process

There are 2 docker files that define the images for (SnapWeb)[SnapWeb.Dockerfile] and (SnapWebManager)[SnapWebManager.Dockerfile].

### Snapweb

#### Building SnapWeb in one command
`> docker build -f SnapWeb.Dockerfile -t registry.jsnap.io/snapweb .`

#### Pushing
`> docker push registry.jsnap.io/snapweb`

### SnapWebManager

### Building SnapWebManager in one command
`> docker build -f SnapWebManager.Dockerfile -t registry.jsnap.io/snapwebmanager .`

### Pushing
`> docker push registry.jsnap.io/snapwebmanager`

### Deploying
The SnapWebManager website needs to be deployed to manage SnapWeb clients. For this, the stack definition in [stacks/snapwebmanager.yml](./stacks/snapwebmanager.yml) is to be used.
One environment variable needs to be defined when deploying the stack:
* `DEFAULT_APIKEY` - The JSnap Api key to use by ALL snapweb deployments

## Deploy a new Snapweb site

To deploy a new client there's a few steps that need to be performed

### 1. Get required information

A new site will need the following info, which is used for the site configuration when deploying in portainer
* Subdomain name (e.g. snapnet or justxn or client-name
* JSNAP Api key
* Service Port, used to enable access to the site. The port number cannot be greater than 65535 and we are deploying websites in the 10000-11000 range

### 2. Create client record

Create a new client in [SnapWebManager](https://snapwebmanager.jsnap.io)
    * When defining the client, make note of the chosen Client ID since this will be used later for configuring the website

### 3. Create folder where data will be saved
1. Create the required folder in the glusterfs volume from any of the 3 nodes that are in the swarm:
    * `> sudo mkdir /mnt/storage-pool/snapweb/<**name of subdomain**>`

### 4. Deploy new portainer stack

In [Portainer](https://portainer.jsnap.io), go to `Stacks` and click on `Add Stack`. Once there:
* Define a name for the stack. e.g. snapweb-<subdomainname>
* Click on `Custom Template` and choose `snapweb` from the drop down
* Click on the `Add an environment variable` button and add these 3 variables:
  * `SERVICE_HOSTNAME` - The subdomain name chosen for the site.
  * `SERVICE_PORT` - The port chosen for the site
  * `CLIENT_ID` - The Client ID that will be assigned to this site. This Client ID is defined in SnapWebManager
* Click on `Deploy the stack` at the bottom

After this, the new stack should be created and the service would be spooled up. The template includes all the required configuration so that Traefik picks up the new service
and starts sending requests its way

### Validating the service

There are different pieces that can be used to validate if a server is working or not:

#### Service Logs

In portainer, navigate to the `Services` option and choose the service you wish to look at. Inside the service details page, there is a `Service Logs` button that will display the logs in a webpage.

On the service details page, we can also see that the environment variables have been correctly set and that the configuration contains the right values.

When trying to troubleshoot connectivity issues, we can also look at Traefik's logs in this part of portainer, because Traefik is a container deployed in docker in one of the swarm nodes.

#### Traefik

In [Traefik](https://traefik.jsnap.io), we can see the routers and services that have been picked up by Traefik. When going to `Routers`, we can look for the `Host*` defined for the service we just deployed. If it's not there, chances are the service is not up or that the configuration has not been set properly.

If the service is there, then we can click on it and validate that it is using the correct entry point (:443), and that it goes to the right service `snapweb-<subdomain>`."# revive" 
