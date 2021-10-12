## Usage
> **⚠️ Important**  
> Your portal must be accessible via https. For example, you can redirect https to http in nginx  

To get started, make sure you have [Docker installed](https://docs.docker.com/engine/install/) and [Docker-compose installed](https://docs.docker.com/compose/install/) on your system, and then clone this repository.

You need to make changes to the `appsettings.json` file in these fields:

```jsonc
"NoPassSettings": {
    // Admin login specified when registering for NoPass.
    "RegistrationAdminId": "",
    // The password you entered when registering for NoPass.
    "RegistrationSCode": "",
    // Your portal URL
    "ThisPortalURL": "",
    // URL NoPass
    "NoPassServerURL": ""
  }
```

Next, navigate in your terminal to the directory you cloned this, and spin up the containers for the web server and PostgreSQL by running:

```
docker-compose up --build -d
```

Next, you need to go through the registration procedure for your portal at NoPass