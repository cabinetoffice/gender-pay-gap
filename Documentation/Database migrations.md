# Database migration

We use [Entity Framework Core code-first migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)

## How to create a database migration

* Make a change to the data model classes  
  The data model classes live in `GenderPayGap.Database/Models`  
  e.g. imagine we add a new property to the `User.cs` file  
  ```csharp
  public string FavouriteColour { get; set; }
  ```

* Open a command line / bash  and navigate to the `GenderPayGap.Database` folder

* Check that you have Entity Framework Core installed  
  Run the command `dotnet ef`  
  You should see a screen like this:  
  <img src="screenshot-of-terminal-running-dotnet-ef.png"
       alt="Screenshot of a terminal running 'dotnet ef'"
       width="445px">

* Run the command  
  ```bash
  dotnet ef migrations add "MigrationName"
  ```
  where `MigrationName` is the file name you want the migration to have

* This should work out what changes you've made and create the migration.  
  **Q:** how does it know what I've chnaged?  
  **A:** EF looks at the difference between your model classes and `GpgDatabaseContextModelSnapshot.cs`  
  
* The migration should create 3 changes:
  * **Add `Migrations/YYYYMMDDHHMMSS_MigrationName.cs`**  
    This is the migration definiton.  
    You should review this to check that the migration is doing exactly what you expect.  
    If you make a mistake somewhere, it can sometimes create a migration that has loads of "drop index" / "re-create index", which is rarely what we want

  * **Add `Migrations/YYYYMMDDHHMMSS_MigrationName.Designer.cs`**  
    You can ignore this file (but should commit it)

  * **Change `Migrations/GpgDatabaseContextModelSnapshot.cs`**  
    This file contains details about all the entities in the database.  
    You should see changes that match the changes to the model.

  You should commit all 3 of these files

## How to run the database migrations

When the app starts up, it will automatically run any new database migrations.  
The code that does this is in `GenderPayGap.Database/GpgDatabaseContext.cs`

You can also run the migrations manually:

* Check which database EF is pointing to, by running  
  `dotnet ef dbcontext info`  
  If you need to change the database it's pointing to, you'll have to
  modify the `ConnectionString` constant in file
 `GenderPayGap.Database/GpgDatabaseContext.cs`

* To update the database, run the command  
  `dotnet ef database update`

* To roll back the database:
  * Find the previous migration that you are happy with  
    (e.g. `20201012170000_PreviousMigration.cs`)
  * Migrate to that migration by running the command  
    `dotnet ef database update "20201012170000_PreviousMigration"`