# Welcome to Umbraco Cloud

In order to run Umbraco 9 locally you will need to [install the .NET 5.0 SDK](https://dotnet.microsoft.com/download) (if you do not have this already).

With dotnet installed, run the following commands in your terminal application of choice:

```
cd src/CandidContributions.Web
dotnet build
dotnet run
```

The terminal output will show the application starting up and will include localhost URLs which you can use to browse to your local Umbraco site.

The first time the project is run locally, you will see the restore boot up screen from Umbraco Cloud. If the environment you have cloned already contained Umbraco Deploy metadata files (such as Document Types), these will automatically be extracted with the option to restore content from the Cloud environment into the local installation.

```
Note: When running locally, we recommend that you setup a developer certificate and run the website under HTTPS. If you haven't configured one already, then run the following dotnet command:

dotnet dev-certs https --trust
```


## Developing Locally

All you need to know to run your Umbraco 9 Cloud project locally.


### Run locally on Windows
Running Umbraco locally on Windows will automatically use LocalDB if you have [SQL Server Express with Advanced Services](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) installed. Alternatively, you can use SQL CE (which is the fallback option on Windows). In both cases the database schema will be automatically created, so it starts up ready for use.

Browse to the URLs from the terminal output of `dotnet run` to see your Umbraco site or alternatively open the `src/CandidContributions.Web/CandidContributions.Web.csproj` file in Visual Studio or JetBrains Rider.


### Run locally on Mac
When running Umbraco locally on a Mac you will need to configure a SQL Server connection string, so Umbraco knows which database to use. Look for the connection string in appsettings.json or add it to appsettings.Development.json (the launchSettings.json configures the local instance to run as 'Development'):

```
{
  "ConnectionStrings": {
    "umbracoDbDSN": ""
  }
}
```

Additionally, we recommend that you configure the local instance to install unattended with the following settings in appsettings.Development.json. Note that the UserName, Email and Password properties are optional and only needed if you want to setup a local backoffice user. You can otherwise use your Umbraco ID to sign-in to the backoffice.

```
{
  "Umbraco": {
    "CMS": {
      "Unattended": {
        "InstallUnattended": true,
        "UnattendedUserName": "",
        "UnattendedUserEmail": "",
        "UnattendedUserPassword": ""
      }
    }
  }
}
```

Now you can type `dotnet run` from within the `src/CandidContributions.Web` folder to start the project. When running it for the first time, the database schema will be automatically created (with `"InstallUnattended": true` in appsettings.Development.json), so it starts up ready for use.

## Project Structure

Below is an overview and description of the different parts contained within this git repository, which makes up your Umbraco 9 Project.
```
.
├── src
│   └── CandidContributions.Web                              (Project folder - can be renamed)
│       ├── Properties                              
│       │   └──── launchSettings.json               (.NET launch settings file)
│       ├── umbraco                                 
│       │   ├──── Data                              (This folder is where Umbraco data such as local database files, generated models and temporary data is stored)
│       │   ├──── Deploy                            (This folder is where the metadata files from Umbraco Deploy are stored)
│       │   └──── Licenses                          (License files for Umbraco Deploy and Umbraco Forms)
│       ├── Views                                   (Directory containing templates, partial views and partial view macros)
│       │   └──── ...
│       ├── wwwroot                                 (Directory containing static assets such as images, CSS and JS)
│       │   └──── ...
│       ├── appsettings.json                        (Umbraco 9 appsettings file)
│       ├── appsettings.Development.json            (Local Development & Cloud Development specific configuration)
│       ├── appsettings.Staging.json                (Cloud Staging specific configuration)
│       ├── appsettings.Production.json             (Cloud Production/Live specfic configuration)
│       ├── Program.cs
│       ├── Startup.cs
│       ├── umbraco-cloud.json                      (Umbraco Cloud specific configuration file - this should only be updated by Umbraco Cloud)
│       └── CandidContributions.Web.csproj                   (The Project file used to build and run the Umbraco 9 project - can be renamed)
├── .dockerignore                             
├── .editorconfig                           
├── .gitattributes                            
├── .gitignore                            
├── .umbraco                                        (Umbraco Project settings used by Umbraco Cloud to build, run and maintain the project in the git repository)
├── NuGet.config                            
└── Readme.md                                       (This file)
...
```

### Renaming the Project file and folder
The file called `.umbraco` at the root of the project contains the following:

```
[project]
base = "src/CandidContributions.Web"
csproj = "CandidContributions.Web.csproj"
```

These two properties help inform us the folder location which contains the application and the second is the name of the .csproj file to build.

You can rename the folder and .csproj file to whatever you want, you may also want to update any C# code namespaces to reflect the name of your project.

In addition to this you are able to add additional Class Library projects that are referenced by the Umbraco application .csproj file, if you prefer to organise your code that way. 

So you could rename `CandidContributions.Web.csproj` to `MyAwesomeProject.Web.csproj` and have one or more additional class library projects such as `MyAwesomeProject.Code.csproj`

```
[project]
base = "src/MyAwesomeProject/MyAwesomeProject.Web"
csproj = "MyAwesomeProject.Web.csproj"
```

Its's a good idea to also update the namespace used in the Program.cs, Startup.cs and _ViewImports.cshtml files, so the naming is consistent throughout your project structure. Once updated you will need to clear out the bin and obj folders locally to avoid build errors. When you are done, commit the changes and push them to Cloud (and that's it).

### Build Process on Umbraco Cloud
When you push your commits to Umbraco Cloud from your local machine, the build process is as follows:
* The `.umbraco` file is used to determine the location of the Umbraco application and the name of the .csproj file to build with MSBuild
* If the git commit contains any Umbraco Deploy metadata files such as Document Types, then these will be deployed to the environment.
* Any additional files such as views, CSS, JS etc will then be copied over to the website

> Recommendation: When pushing to Umbraco Cloud Git repositiories, you should use the terminal or a git desktop application that allows you to view the output. As this shows the build process happening and if you have a build error you would be unaware.

### Adding a Solution file
If you are using Visual Studio you will likely want a solution file, so you and your team can easily work with the Umbraco Cloud project from within Visual Studio and have the option to add additional projects.

From the terminal of your choice navigate to the root of the git repository for your Umbraco Cloud project, and enter the following command.
```
dotnet new sln --name MyAwesomeSolution
```

> Recommendation: When creating a solution file we recommend that you place it in the root of the git repository.

If you want to add additional projects to your solution, you can do that from the command line as well using the following `dotnet` commands
```
dotnet new classlib --name MyAwesomeProject.Code --output src/MyAwesomeProject.Code -f net5.0
dotnet sln add .\src\MyAwesomeProject.Code\MyAwesomeProject.Code.csproj
dotnet sln add .\src\MyAwesomeProject.Web\MyAwesomeProject.Web.csproj
dotnet add .\src\MyAwesomeProject.Web\MyAwesomeProject.Web.csproj reference .\src\MyAwesomeProject.Code\MyAwesomeProject.Code.csproj
```

> Recommendation: When creating new projects along side the default CandidContributions.Web, we recommend that they are added to the src folder in the git repository.

# Documentation

For further documentation please visit [Umbraco Docs](https://our.umbraco.com/documentation/)
