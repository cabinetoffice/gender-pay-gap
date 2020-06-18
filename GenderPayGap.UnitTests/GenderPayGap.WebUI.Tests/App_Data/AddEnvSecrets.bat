@echo off
REM To see which values are going to be USED here look in Azure --> AppServices --> application settings --> AppSettingName --> Value
REM To see where they are going to be READ FROM look in VSTS --> Pipelines --> Builds --> (select one build i.e. Auto-run selenium on PREPROD) --> Edit --> Variables --> Pipeline variables --> Name/Value 
SET ASPNETCORE_ENVIRONMENT=%~3
set AdminApiKey=%~5
@echo ASPNETCORE_ENVIRONMENT=%ASPNETCORE_ENVIRONMENT%
@echo AdminApiKey=%AdminApiKey%