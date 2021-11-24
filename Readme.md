# Candid Contributions Website

![GitHub](https://img.shields.io/github/license/candidcontributions/CanConCloud)

CandidContributions.com is an Umbraco v9 website hosted in Umbraco Cloud.

The codebase has been copied into a GitHub repository so it is publicly accessible. It cannot be a 'mirror' repository as the Cloud repo has file(s) that should not be made public. We shall endeavour to keep the two repos manually in sync. In other words, contributions and comments are welcome!

## Working locally

Unzip `db/CanConCloudGH.zip` and restore the extracted sql .bak to your local SQL Server 2019.

Open `src/CandidContributions.sln` in Visual Studio and build the solution.

You will need to set your database connection string before trying to run the website locally. Right click on the `CandidContributions.Web` project and select 'Manage User Secrets' to open your local `secrets.json` file for this project. Copy the following into that file and add your local connection string:

```
{
  "ConnectionStrings": {
    "umbracoDbDSN": ""
  }
}
```

You should now be able to run this Umbraco 9 website.

### Backoffice login

To log in to the backoffice use:

- Email: `hello@candidcontributions.com`
- Password: `h5yr!h5yr!`

## Questions?

Create an issue and let's start a conversation!