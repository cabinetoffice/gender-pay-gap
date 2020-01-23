@echo off
REM To see which values are going to be USED here look in Azure --> AppServices --> application settings --> AppSettingName --> Value
REM To see where they are going to be READ FROM look in VSTS --> Pipelines --> Builds --> (select one build i.e. Auto-run selenium on PREPROD) --> Edit --> Variables --> Pipeline variables --> Name/Value 
set ClientId=%~1
set ClientSecret=%~2
REM SET DatabaseConnectionName=GpgSelenium <-- search for addenvsecrets.bat in the selenium project
SET ASPNETCORE_ENVIRONMENT=%~3
set EXTERNAL_HOST=%~4
set AdminApiKey=%~5
@echo ClientId=%ClientId%
@echo ClientSecret=%ClientSecret%
@echo ASPNETCORE_ENVIRONMENT=%ASPNETCORE_ENVIRONMENT%
@echo EXTERNAL_HOST=%EXTERNAL_HOST%
@echo AdminApiKey=%AdminApiKey%