# Getting the code running

Get the code:
* Clone this repo  
  If you're cloning with SSH / GitKraken, you want this link:  
  `git@github.com:cabinetoffice/gender-pay-gap.git`

* Open the Solution in your IDE by opening the
  `GenderPayGap.sln` file (don't just open the folder in your IDE)

* Ask an existing developer for a copy of
  `GenderPayGap.WebUI/appsettings.secret.json`
  and save it to your local repo
  (the file is git ignored, so it shouldn't get committed)

Create an empty database:
* Open your database UI (e.g. DataGrip)
* Login to your local Postgres database server
* Create a database called `GpgDatabase`
* (Optionally) create a user which has access to this new database  
  If not, you can use the credentials of the root Postgres user
* Edit the `VCAP_SERVICES`
  line in `GenderPayGap.WebUI/appsettings.secret.json`
  to use the username and password for your local Postgres database

Build the code:
* Build the project - this should install all the necessary packages from nuget

* Open a Bash terminal and navigate into the
  `gender-pay-gap/GenderPayGap.WebUI` folder

* Run `npm install` in the folder

* Run `npm run build` or `npm run watch`
  * `npm run build` compiles the SCSS / JS files once
    (which can then be picked up by the C# build process)
  * `npm run watch` compiles the SCSS / JS files every time a SCSS / JS file changes
    This is useful whilst you're editing the SCSS / JS files, but not necessary most of the time

Run the code:
* Run the `GenderPayGap.WebUI` project from your IDE  
  This should automatically be set as the default Startup Project
  so you should just be able to click the "Play" button

* The code should build and run successfully

* The website should be visible at https://localhost:44371/
  The homepage / static pages should all work, but things like Search
  won't return any entries (because there's no data in your database)

Populate the database:
* Create yourself 2 accounts (employer account & admin account) on the Dev environment  
  ['Create an account' page on the Dev environment](https://gender-pay-gap-dev.london.cloudapps.digital/create-user-account)  
  **Note:** see ['Our environments' page](Our%20environments.md) for details about the username/password prompt
  * Sign up for an account
  * Ask an existing developer to send you the email verify link
  * Ask an existing developer how to create an administrator account

* Import a database from the Dev server
  * Stop!  
    Have you created an admin account in the Dev environment?  
    If not, do this first!
  * Run the code from your IDE
  * Go to https://localhost:44371/admin/data-migration/from-remote-server  
    Note: this page is only visible to admins, or when the database is empty  
    Note: this page works better in Chrome than in Firefox
  * Fill in the form:
    * Hostname of remote server: `gender-pay-gap-dev.london.cloudapps.digital` (without https://)
    * Passwords: ask an existing developer for these
    * The database migration password is stored in `appsettings.secrets.json`
  * Click the scary red button
  * Wait for a few minutes whilst the data is imported
  * The database should now be fully populated and ready to use
  * You can now login using the accounts you just created in the Dev environment  
    The credentials will be exactly the same as in Dev (because we just copied the whole Dev database to your local machine)

