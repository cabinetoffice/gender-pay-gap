using System;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class CreateViewsForReportsReader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            string ReportsReaderPassword = Config.GetAppSetting("ReportsReaderPassword", "Password");
            migrationBuilder.Sql(
                $"If not Exists (select loginname from master.dbo.syslogins where name = 'ReportsReaderLogin') CREATE "
                + $"LOGIN [ReportsReaderLogin] WITH PASSWORD = '{ReportsReaderPassword}';");

            #region 201811261437002_Add user ReportReader.cs

            migrationBuilder.Sql("CREATE USER [ReportsReaderDv] FOR LOGIN [ReportsReaderLogin];");

            #endregion

            migrationBuilder.Sql("GRANT SELECT ON OBJECT::[dbo].[OrganisationNames] TO [ReportsReaderDv] AS [dbo];");

            #region 201811261453436_Add view OrganisationAddressInfoView.cs

            migrationBuilder.Sql(
                "CREATE VIEW [dbo].[OrganisationAddressInfoView] AS SELECT organisationAdd.OrganisationId ,"
                + "CASE organisationAdd.[StatusId] "
                + "WHEN 0 THEN 'Unknown' "
                + "WHEN 1 THEN 'New' "
                + "WHEN 2 THEN 'Suspended' "
                + "WHEN 3 THEN 'Active' "
                + "WHEN 5 THEN 'Pending' "
                + "WHEN 6 THEN 'Retired' "
                + "ELSE 'Error' END + ' (' + CAST(organisationAdd.StatusId AS VARCHAR(2)) + ')' AS AddressStatus ,REPLACE("
                + "COALESCE(RTRIM(LTRIM(organisationAdd.Address1)), '') + "
                + "COALESCE(', ' + RTRIM(LTRIM(organisationAdd.Address2)), '') + "
                + "COALESCE(', ' + RTRIM(LTRIM(organisationAdd.Address3)), '') + "
                + "COALESCE(', ' + RTRIM(LTRIM(organisationAdd.TownCity)), '') + "
                + "COALESCE(', ' + RTRIM(LTRIM(organisationAdd.County)), '') + "
                + "COALESCE(', ' + RTRIM(LTRIM(organisationAdd.PoBox)), '') + "
                + "COALESCE(', ' + RTRIM(LTRIM(organisationAdd.PostCode)), '') + "
                + "COALESCE(' (' + RTRIM(LTRIM(organisationAdd.Country)) + ')', ''), ' ,', ',') AS FullAddress ,"
                + "organisationAdd.StatusDetails AS AddressStatusDetails ,organisationAdd.StatusDate AS AddressStatusDate ,"
                + "organisationAdd.Source AS AddressSource ,organisationAdd.Created AS AddressCreated ,"
                + "organisationAdd.Modified AS AddressModified FROM ( SELECT AddressId ,CreatedByUserId ,"
                + "CASE WHEN (LEN([Address1]) > 0) THEN [Address1] ELSE NULL END AS Address1 ,C"
                + "ASE WHEN (LEN([Address2]) > 0) THEN [Address2] ELSE NULL END AS Address2 ,"
                + "CASE WHEN (LEN([Address3]) > 0) THEN [Address3] ELSE NULL END AS Address3 ,"
                + "CASE WHEN (LEN([TownCity]) > 0) THEN [TownCity] ELSE NULL END AS TownCity ,"
                + "CASE WHEN (LEN([County]) > 0) THEN [County] ELSE NULL END AS County ,"
                + "CASE WHEN (LEN([Country]) > 0) THEN [Country] ELSE NULL END AS Country ,"
                + "CASE WHEN (LEN([PoBox]) > 0) THEN [PoBox] ELSE NULL END AS PoBox ,"
                + "CASE WHEN (LEN([PostCode]) > 0) THEN [PostCode] ELSE NULL END AS "
                + "PostCode ,StatusId ,StatusDate ,StatusDetails ,Created ,Modified ,OrganisationId ,Source FROM dbo.OrganisationAddresses ) "
                + "AS organisationAdd INNER JOIN dbo.Organisations AS orgs ON orgs.OrganisationId = organisationAdd.OrganisationId");

            migrationBuilder.Sql("GRANT SELECT ON OBJECT::[dbo].[OrganisationAddressInfoView] TO [ReportsReaderDv] AS [dbo];");

            #endregion
            
            #region 201811261523023_Add view OrganisationInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationInfoView] "
                + " AS "
                + " SELECT orgs.OrganisationId "
                + " 	,orgs.EmployerReference "
                + " 	,orgs.DUNSNumber "
                + " 	,orgs.CompanyNumber "
                + " 	,orgs.OrganisationName "
                + " 	,CASE orgs.[SectorTypeId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'Private' "
                + " 		WHEN 2 "
                + " 			THEN 'Public' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(orgs.SectorTypeId AS VARCHAR(2)) + ')' AS SectorType "
                + " 	,CASE orgs.StatusId "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'New' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Active' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		WHEN 5 "
                + " 			THEN 'Pending' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(orgs.StatusId AS VARCHAR(2)) + ')' AS OrganisationStatus "
                + " 	,orgs.[SecurityCode] "
                + " 	,orgs.[SecurityCodeExpiryDateTime] "
                + " 	,orgs.[SecurityCodeCreatedDateTime] "
                + " FROM dbo.Organisations AS orgs ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261527380_Add view OrganisationRegistrationInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationRegistrationInfoView] "
                + " AS "
                + " SELECT uo.OrganisationId "
                + " 	,u.Firstname + ' ' + u.Lastname + ' [' + u.JobTitle + ']' AS UserInfo "
                + " 	,u.ContactFirstName + ' ' + u.ContactLastName + ' [' + u.ContactJobTitle + ']' AS ContactInfo "
                + " 	,CASE uo.[MethodId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'PinInPost' "
                + " 		WHEN 2 "
                + " 			THEN 'EmailDomain' "
                + " 		WHEN 3 "
                + " 			THEN 'Manual' "
                + " 		WHEN 4 "
                + " 			THEN 'Fasttrack' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(uo.MethodId AS VARCHAR(2)) + ')' AS RegistrationMethod "
                + " 	,uo.PINSentDate "
                + " 	,uo.PINConfirmedDate "
                + " 	,uo.ConfirmAttemptDate "
                + " 	,uo.ConfirmAttempts "
                + " FROM dbo.UserOrganisations AS uo "
                + " INNER JOIN dbo.Users AS u ON u.UserId = uo.UserId");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationRegistrationInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261557329_Add view OrganisationScopeInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationScopeInfoView] "
                + " AS "
                + " SELECT OrganisationId "
                + " 	,CASE scopes.scopestatusid "
                + " 		WHEN 1 "
                + " 			THEN 'In Scope' "
                + " 		WHEN 2 "
                + " 			THEN 'Out of Scope' "
                + " 		WHEN 3 "
                + " 			THEN 'Presumed in Scope' "
                + " 		WHEN 4 "
                + " 			THEN 'Presumed out of Scope' "
                + " 		ELSE 'Unknown' "
                + " 		END + ' (' + CAST(ScopeStatusId AS VARCHAR(2)) + ')' AS ScopeStatus "
                + " 	,ScopeStatusDate "
                + " 	,CASE scopes.RegisterStatusid "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'RegisterSkipped' "
                + " 		WHEN 2 "
                + " 			THEN 'RegisterPending' "
                + " 		WHEN 3 "
                + " 			THEN 'RegisterComplete' "
                + " 		WHEN 4 "
                + " 			THEN 'RegisterCancelled' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(RegisterStatusId AS VARCHAR(2)) + ')' AS RegisterStatus "
                + " 	,RegisterStatusDate "
                + " 	,YEAR(SnapshotDate) AS snapshotYear "
                + " FROM dbo.OrganisationScopes AS scopes ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationScopeInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261601550_Add view OrganisationSearchInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationSearchInfoView] "
                + " AS "
                + " SELECT orgs.OrganisationId "
                + " 	,orgs.EmployerReference "
                + " 	,orgs.DUNSNumber "
                + " 	,orgs.CompanyNumber "
                + " 	,orgs.OrganisationName "
                + " 	,CASE orgs.[SectorTypeId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'Private' "
                + " 		WHEN 2 "
                + " 			THEN 'Public' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(orgs.SectorTypeId AS VARCHAR(2)) + ')' AS SectorType "
                + " 	,CASE orgs.StatusId "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'New' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Active' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		WHEN 5 "
                + " 			THEN 'Pending' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(orgs.StatusId AS VARCHAR(2)) + ')' AS OrganisationStatus "
                + " FROM dbo.Organisations AS orgs ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationSearchInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261605329_Add view OrganisationSubmissionInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationSubmissionInfoView] "
                + " AS "
                + " SELECT ret.OrganisationId "
                + " 	,CONVERT(DATE, ret.AccountingDate) AS LatestReturnAccountingDate "
                + " 	,DATEADD(second, - 1, DATEADD(year, 1, ret.AccountingDate)) AS ReportingDeadline "
                + " 	,CASE ret.[StatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'Draft' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Submitted' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		WHEN 5 "
                + " 			THEN 'Deleted' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(ret.StatusId AS VARCHAR(2)) + ')' AS latestReturnStatus "
                + " 	,ret.StatusDate AS latestReturnStatusDate "
                + " 	,ret.StatusDetails AS LatestReturnStatusDetails "
                + " 	,CASE  "
                + " 		WHEN ([Modified] > dateadd(second, - 1, dateadd(year, 1, accountingdate))) "
                + " 			THEN 'true' "
                + " 		ELSE 'false' "
                + " 		END AS ReportedLate "
                + " 	,ret.LateReason AS LatestReturnLateReason "
                + " 	,CASE retstat.[StatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'Draft' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Submitted' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		WHEN 5 "
                + " 			THEN 'Deleted' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(retstat.StatusId AS VARCHAR(2)) + ')' AS StatusId "
                + " 	,retstat.StatusDate "
                + " 	,retstat.StatusDetails "
                + " 	,ret.Modifications AS ReturnModifiedFields "
                + " 	,CASE ret.[EHRCResponse] "
                + " 		WHEN 1 "
                + " 			THEN 'true' "
                + " 		ELSE 'false' "
                + " 		END AS EHRCResponse "
                + " 	,ret.FirstName + ' ' + ret.LastName + ' [' + ret.JobTitle + ']' AS SubmittedBy "
                + " 	,CASE  "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 0 "
                + " 				AND [MaxEmployees] = 0 "
                + " 				) "
                + " 			THEN 'Not provided' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 0 "
                + " 				AND [MaxEmployees] = 249 "
                + " 				) "
                + " 			THEN 'Employees 0 to 249' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 250 "
                + " 				AND [MaxEmployees] = 499 "
                + " 				) "
                + " 			THEN 'Employees 250 to 499' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 500 "
                + " 				AND [MaxEmployees] = 999 "
                + " 				) "
                + " 			THEN 'Employees 500 to 999' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 1000 "
                + " 				AND [MaxEmployees] = 4999 "
                + " 				) "
                + " 			THEN 'Employees 1,000 to 4,999' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 5000 "
                + " 				AND [MaxEmployees] = 19999 "
                + " 				) "
                + " 			THEN 'Employees 5,000 to 19,999' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 20000 "
                + " 				AND [MaxEmployees] = 2147483647 "
                + " 				) "
                + " 			THEN 'Employees 20,000 or more' "
                + " 		ELSE 'Error' "
                + " 		END AS OrganisationSize "
                + " 	,ret.DiffMeanHourlyPayPercent "
                + " 	,ret.DiffMedianHourlyPercent "
                + " 	,ret.DiffMeanBonusPercent "
                + " 	,ret.DiffMedianBonusPercent "
                + " 	,ret.MaleMedianBonusPayPercent "
                + " 	,ret.FemaleMedianBonusPayPercent "
                + " 	,ret.MaleLowerPayBand "
                + " 	,ret.FemaleLowerPayBand "
                + " 	,ret.MaleMiddlePayBand "
                + " 	,ret.FemaleMiddlePayBand "
                + " 	,ret.MaleUpperPayBand "
                + " 	,ret.FemaleUpperPayBand "
                + " 	,ret.MaleUpperQuartilePayBand "
                + " 	,ret.FemaleUpperQuartilePayBand "
                + " FROM dbo.[Returns] AS ret "
                + " LEFT OUTER JOIN dbo.ReturnStatus AS retstat ON retstat.ReturnId = ret.ReturnId");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationSubmissionInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion
            
            #region 201811261637321_Add view OrganisationSicCodeInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationSicCodeInfoView] "
                + " AS "
                + " SELECT osc.OrganisationId "
                + " 	,sc.[SicCodeId] "
                + " 	,rank() OVER ( "
                + " 		PARTITION BY osc.OrganisationId ORDER BY sc.[SicCodeId], osc.[Source] ASC "
                + " 		) AS SicCodeRankWithinOrganisation "
                + " 	, CASE WHEN (CHARINDEX( '@', osc.[Source]) > 0) "
                + " 			THEN 'User' "
                + " 		ELSE	 "
                + " 			osc.[Source] "
                + " 		END AS [Source] "
                + " 	,sc.[Description] AS CodeDescription "
                + " 	,ss.SicSectionId "
                + " 	,ss.[Description] AS SectionDescription "
                + " FROM [dbo].[SicSections] AS ss "
                + " RIGHT JOIN [dbo].[SicCodes] AS sc ON sc.SicSectionId = ss.SicSectionId "
                + " RIGHT JOIN ( "
                + " 	SELECT sicCodeId "
                + " 		,organisationid "
                + " 		,[source] "
                + " 	FROM dbo.[OrganisationSicCodes] "
                + " 	GROUP BY sicCodeId "
                + " 		,organisationid "
                + " 		,[source] "
                + " 	) AS osc ON osc.SicCodeId = sc.SicCodeId ");

            migrationBuilder.Sql(" GRANT SELECT ON OBJECT::dbo.[OrganisationSicCodeInfoView] TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261645007_Add view UserInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[UserInfoView] "
                + " AS "
                + " SELECT UserId "
                + " 	,CASE [StatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'New' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Active' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(StatusId AS VARCHAR(2)) + ')' AS StatusId "
                + " , StatusDate "
                + " , StatusDetails "
                + " , Firstname "
                + " , Lastname "
                + " , JobTitle "
                + " , ContactFirstName "
                + " , ContactLastName "
                + " , ContactJobTitle "
                + " , ContactOrganisation "
                + " , ContactPhoneNumber "
                + " , EmailVerifySendDate "
                + " , EmailVerifiedDate "
                + " , LoginAttempts "
                + " , LoginDate "
                + " , ResetSendDate "
                + " , ResetAttempts "
                + " , VerifyAttemptDate "
                + " , VerifyAttempts "
                + " FROM dbo.Users ");

            migrationBuilder.Sql("  GRANT SELECT " + " 	ON OBJECT::dbo.[UserInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261649336_Add view UserStatusInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[UserStatusInfoView] "
                + " AS "
                + " SELECT ust.ByUserId AS UserId "
                + " 	,modifyingUser.Firstname + ' ' + modifyingUser.Lastname + ' [' + modifyingUser.JobTitle + ']' AS UserName "
                + " 	,CASE usr.[StatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'New' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Active' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(usr.StatusId AS VARCHAR(2)) + ')' AS StatusId "
                + " 	,ust.StatusDate "
                + " 	,ust.StatusDetails "
                + " 	,usr.Firstname + ' ' + usr.Lastname + ' [' + usr.JobTitle + ']' AS StatusChangedBy "
                + " FROM dbo.UserStatus AS ust "
                + " INNER JOIN dbo.Users AS usr ON usr.UserId = ust.UserId "
                + " INNER JOIN dbo.Users AS modifyingUser ON modifyingUser.UserId = ust.ByUserId ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[UserStatusInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201812131233344_Add function OrganisationStatusIdToString.cs

            migrationBuilder.Sql(
                " CREATE FUNCTION [dbo].[OrganisationStatusIdToString]"
                + " ("
                + " 	@organisationStatusId tinyint"
                + " )"
                + " RETURNS varchar(20)"
                + " AS"
                + " BEGIN"
                + " 	DECLARE @Result varchar(20)"
                + " "
                + " 	SELECT @Result = CASE @organisationStatusId "
                + " 						WHEN 0 THEN 'Unknown' "
                + " 						WHEN 1 THEN 'New' "
                + " 						WHEN 2 THEN 'Suspended' "
                + " 						WHEN 3 THEN 'Active' "
                + " 						WHEN 4 THEN 'Retired' "
                + " 						WHEN 5 THEN 'Pending' "
                + " 						WHEN 6 THEN 'Deleted' "
                + " 						ELSE 'Error' "
                + " 					END + ' (' + CAST(@organisationStatusId AS VARCHAR(3)) + ')'"
                + " "
                + " 	RETURN @Result		"
                + " END ");

            migrationBuilder.Sql(" GRANT EXECUTE " + " 	ON OBJECT::[dbo].[OrganisationStatusIdToString] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201812131245269_Add function OrganisationSectorTypeIdToString.cs

            migrationBuilder.Sql(
                " CREATE FUNCTION [dbo].[OrganisationSectorTypeIdToString]"
                + " ("
                + " 	@organisationSectorTypeId tinyint"
                + " )"
                + " RETURNS varchar(20)"
                + " AS"
                + " BEGIN"
                + " 	DECLARE @Result varchar(20)"
                + " "
                + " 	SELECT @Result = CASE @organisationSectorTypeId"
                + " 						WHEN 0 THEN 'Unknown' "
                + " 						WHEN 1 THEN 'Private' "
                + " 						WHEN 2 THEN 'Public' "
                + " 						ELSE 'Error' "
                + " 					END + ' (' + CAST(@organisationSectorTypeId AS VARCHAR(3)) + ')'"
                + " "
                + " 	RETURN @Result"
                + " "
                + " END ");

            migrationBuilder.Sql(" GRANT EXECUTE " + " 	ON OBJECT::[dbo].[OrganisationSectorTypeIdToString] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201812131250589_Modify view OrganisationInfoView.cs

            migrationBuilder.Sql(
                " ALTER VIEW [dbo].[OrganisationInfoView] "
                + " AS "
                + " SELECT orgs.OrganisationId "
                + " 	, orgs.EmployerReference "
                + " 	, orgs.DUNSNumber "
                + " 	, orgs.CompanyNumber "
                + " 	, orgs.OrganisationName "
                + " 	, dbo.OrganisationSectorTypeIdToString(orgs.[SectorTypeId]) AS SectorType "
                + " 	, dbo.OrganisationStatusIdToString(orgs.StatusId) AS OrganisationStatus "
                + " 	, orgs.[SecurityCode] "
                + " 	, orgs.[SecurityCodeExpiryDateTime] "
                + " 	, orgs.[SecurityCodeCreatedDateTime] "
                + " FROM dbo.Organisations AS orgs ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::[dbo].[OrganisationInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201812131257437_Add view UserLinkedOrganisationsView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[UserLinkedOrganisationsView] "
                + " AS "
                + " 	SELECT usrOrgs.UserId "
                + " 		,orgs.OrganisationId "
                + " 		,orgs.DUNSNumber "
                + " 		,orgs.EmployerReference "
                + " 		,orgs.CompanyNumber "
                + " 		,orgs.OrganisationName "
                + " 		,dbo.OrganisationSectorTypeIdToString(orgs.SectorTypeId) AS SectorTypeId "
                + " 		,dbo.OrganisationStatusIdToString(orgs.StatusId) AS StatusId "
                + " 	FROM dbo.UserOrganisations AS usrOrgs "
                + " 	INNER JOIN dbo.Organisations AS orgs ON orgs.OrganisationId = usrOrgs.OrganisationId ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[UserLinkedOrganisationsView] " + " 	TO [ReportsReaderDv]; ");

            #endregion
            
            #region 201901161519381_Add view OrganisationScopeAndReturnInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW dbo.OrganisationScopeAndReturnInfoView "
                + " AS "
                + " SELECT orgs.OrganisationId "
                + " 	,orgs.OrganisationName "
                + " 	,orgs.EmployerReference "
                + " 	,orgs.CompanyNumber "
                + " 	,dbo.OrganisationStatusIdToString(orgs.StatusId) AS OrganisationStatus "
                + " 	,dbo.OrganisationSectorTypeIdToString(orgs.SectorTypeId) AS SectorType "
                + " 	,CASE OrgScopes.[ScopeStatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'In scope' "
                + " 		WHEN 2 "
                + " 			THEN 'Out of scope' "
                + " 		WHEN 3 "
                + " 			THEN 'Presumed in scope' "
                + " 		WHEN 4 "
                + " 			THEN 'Presumed out of scope' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(OrgScopes.ScopeStatusId AS VARCHAR(3)) + ')' AS ScopeStatus "
                + " 	,YEAR(OrgScopes.SnapshotDate) AS SnapshotDate "
                + " 	,OrgSicCodeView.SectionDescription AS SicCodeSectionDescription "
                + " 	,retrns.ReturnId "
                + " 	,CASE  "
                + " 		WHEN retrns.MinEmployees = 0 "
                + " 			AND retrns.MaxEmployees = 0 "
                + " 			THEN 'Not Provided' "
                + " 		WHEN retrns.MinEmployees = 0 "
                + " 			AND retrns.MaxEmployees = 249 "
                + " 			THEN 'Less than 250' "
                + " 		WHEN ( "
                + " 				retrns.MinEmployees = 250 "
                + " 				AND retrns.MaxEmployees = 499 "
                + " 				) "
                + " 			OR ( "
                + " 				retrns.MinEmployees = 500 "
                + " 				AND retrns.MaxEmployees = 999 ) OR"
                + Environment.NewLine
                + "                         (retrns.MinEmployees = 1000 "
                + " 				AND retrns.MaxEmployees = 4999 "
                + " 				) "
                + " 			THEN CAST(retrns.MinEmployees AS VARCHAR) + ' to ' + CAST(retrns.MaxEmployees AS VARCHAR) "
                + " 		WHEN retrns.MinEmployees = 5000 "
                + " 			AND retrns.MaxEmployees = 19999 "
                + " 			THEN CAST(retrns.MinEmployees AS VARCHAR) + ' to 19,999' "
                + " 		WHEN retrns.MinEmployees = 20000 "
                + " 			AND retrns.MaxEmployees = 2147483647 "
                + " 			THEN '20,000 or more' "
                + " 		ELSE 'Error min(' + CAST(retrns.MinEmployees AS VARCHAR) + '), max (' + CAST(retrns.MaxEmployees AS VARCHAR) + ')' "
                + " 		END AS OrganisationSize "
                + " 	,OrgPubSectType.PublicSectorDescription "
                + " FROM dbo.Organisations AS orgs "
                + " INNER JOIN dbo.OrganisationScopes AS OrgScopes ON OrgScopes.OrganisationId = orgs.OrganisationId "
                + " 	AND OrgScopes.StatusId = 3 "
                + " LEFT OUTER JOIN dbo.OrganisationSicCodeInfoView AS OrgSicCodeView ON OrgSicCodeView.OrganisationId = orgs.OrganisationId "
                + " 	AND OrgSicCodeView.SicCodeRankWithinOrganisation = 1 "
                + " LEFT OUTER JOIN dbo.[Returns] AS retrns ON retrns.OrganisationId = OrgScopes.OrganisationId "
                + " 	AND retrns.AccountingDate = OrgScopes.SnapshotDate "
                + " 	AND retrns.StatusId = 3 "
                + " LEFT OUTER JOIN ( "
                + " 	SELECT opst.OrganisationId "
                + " 		,pst.Description AS PublicSectorDescription "
                + " 	FROM dbo.OrganisationPublicSectorTypes AS opst "
                + " 	INNER JOIN dbo.PublicSectorTypes AS pst ON pst.PublicSectorTypeId = opst.PublicSectorTypeId "
                + " 		AND opst.Retired IS NULL "
                + " 	) AS OrgPubSectType ON OrgPubSectType.OrganisationId = orgs.OrganisationId ");

            migrationBuilder.Sql("  GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationScopeAndReturnInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion
            
            migrationBuilder.Sql(" ALTER VIEW [dbo].["
                                                  + "OrganisationSubmissionInfoView"
                                                  + "] "
                                                  + " AS "
                                                  + " SELECT ret.OrganisationId "
                                                  + " 	,CONVERT(DATE, ret.AccountingDate) AS LatestReturnAccountingDate "
                                                  + " 	,DATEADD(second, - 1, DATEADD(year, 1, ret.AccountingDate)) AS ReportingDeadline "
                                                  + " 	,CASE ret.[StatusId] "
                                                  + " 		WHEN 0 "
                                                  + " 			THEN 'Unknown' "
                                                  + " 		WHEN 1 "
                                                  + " 			THEN 'Draft' "
                                                  + " 		WHEN 2 "
                                                  + " 			THEN 'Suspended' "
                                                  + " 		WHEN 3 "
                                                  + " 			THEN 'Submitted' "
                                                  + " 		WHEN 4 "
                                                  + " 			THEN 'Retired' "
                                                  + " 		WHEN 5 "
                                                  + " 			THEN 'Deleted' "
                                                  + " 		ELSE 'Error' "
                                                  + " 		END + ' (' + CAST(ret.StatusId AS VARCHAR(2)) + ')' AS latestReturnStatus "
                                                  + " 	,ret.StatusDate AS latestReturnStatusDate "
                                                  + " 	,firstDates.dateFirstReportedInYear "
                                                  + " 	,ret.StatusDetails AS LatestReturnStatusDetails "
                                                  + " 	,CASE  "
                                                  + " 		WHEN ([Modified] > dateadd(second, - 1, dateadd(year, 1, ret.accountingdate))) "
                                                  + " 			THEN 'true' "
                                                  + " 		ELSE 'false' "
                                                  + " 		END AS ReportedLate "
                                                  + " 	,ret.LateReason AS LatestReturnLateReason "
                                                  + " 	,CASE retstat.[StatusId] "
                                                  + " 		WHEN 0 "
                                                  + " 			THEN 'Unknown' "
                                                  + " 		WHEN 1 "
                                                  + " 			THEN 'Draft' "
                                                  + " 		WHEN 2 "
                                                  + " 			THEN 'Suspended' "
                                                  + " 		WHEN 3 "
                                                  + " 			THEN 'Submitted' "
                                                  + " 		WHEN 4 "
                                                  + " 			THEN 'Retired' "
                                                  + " 		WHEN 5 "
                                                  + " 			THEN 'Deleted' "
                                                  + " 		ELSE 'Error' "
                                                  + " 		END + ' (' + CAST(retstat.StatusId AS VARCHAR(2)) + ')' AS StatusId "
                                                  + " 	,retstat.StatusDate "
                                                  + " 	,retstat.StatusDetails "
                                                  + " 	,ret.Modifications AS ReturnModifiedFields "
                                                  + " 	,CASE ret.[EHRCResponse] "
                                                  + " 		WHEN 1 "
                                                  + " 			THEN 'true' "
                                                  + " 		ELSE 'false' "
                                                  + " 		END AS EHRCResponse "
                                                  + " 	,ret.FirstName + ' ' + ret.LastName + ' [' + ret.JobTitle + ']' AS SubmittedBy "
                                                  + " 	,CASE  "
                                                  + " 		WHEN ( "
                                                  + " 				[MinEmployees] = 0 "
                                                  + " 				AND [MaxEmployees] = 0 "
                                                  + " 				) "
                                                  + " 			THEN 'Not provided' "
                                                  + " 		WHEN ( "
                                                  + " 				[MinEmployees] = 0 "
                                                  + " 				AND [MaxEmployees] = 249 "
                                                  + " 				) "
                                                  + " 			THEN 'Employees 0 to 249' "
                                                  + " 		WHEN ( "
                                                  + " 				[MinEmployees] = 250 "
                                                  + " 				AND [MaxEmployees] = 499 "
                                                  + " 				) "
                                                  + " 			THEN 'Employees 250 to 499' "
                                                  + " 		WHEN ( "
                                                  + " 				[MinEmployees] = 500 "
                                                  + " 				AND [MaxEmployees] = 999 "
                                                  + " 				) "
                                                  + " 			THEN 'Employees 500 to 999' "
                                                  + " 		WHEN ( "
                                                  + " 				[MinEmployees] = 1000 "
                                                  + " 				AND [MaxEmployees] = 4999 "
                                                  + " 				) "
                                                  + " 			THEN 'Employees 1,000 to 4,999' "
                                                  + " 		WHEN ( "
                                                  + " 				[MinEmployees] = 5000 "
                                                  + " 				AND [MaxEmployees] = 19999 "
                                                  + " 				) "
                                                  + " 			THEN 'Employees 5,000 to 19,999' "
                                                  + " 		WHEN ( "
                                                  + " 				[MinEmployees] = 20000 "
                                                  + " 				AND [MaxEmployees] = 2147483647 "
                                                  + " 				) "
                                                  + " 			THEN 'Employees 20,000 or more' "
                                                  + " 		ELSE 'Error' "
                                                  + " 		END AS OrganisationSize "
                                                  + " 	,ret.DiffMeanHourlyPayPercent "
                                                  + " 	,ret.DiffMedianHourlyPercent "
                                                  + " 	,ret.DiffMeanBonusPercent "
                                                  + " 	,ret.DiffMedianBonusPercent "
                                                  + " 	,ret.MaleMedianBonusPayPercent "
                                                  + " 	,ret.FemaleMedianBonusPayPercent "
                                                  + " 	,ret.MaleLowerPayBand "
                                                  + " 	,ret.FemaleLowerPayBand "
                                                  + " 	,ret.MaleMiddlePayBand "
                                                  + " 	,ret.FemaleMiddlePayBand "
                                                  + " 	,ret.MaleUpperPayBand "
                                                  + " 	,ret.FemaleUpperPayBand "
                                                  + " 	,ret.MaleUpperQuartilePayBand "
                                                  + " 	,ret.FemaleUpperQuartilePayBand "
                                                  + " 	,ret.CompanyLinkToGPGInfo "
                                                  + " FROM dbo.[Returns] AS ret "
                                                  + " LEFT OUTER JOIN dbo.ReturnStatus AS retstat ON retstat.ReturnId = ret.ReturnId "
                                                  + " LEFT OUTER JOIN ( "
                                                  + "     SELECT [OrganisationId] "
                                                  + "         ,[AccountingDate] "
                                                  + "         ,min([Modified]) AS dateFirstReportedInYear "
                                                  + "     FROM [dbo].[Returns] AS ret "
                                                  + "     JOIN [dbo].[ReturnStatus] AS retst ON retst.ReturnId = ret.ReturnId "
                                                  + "     GROUP BY [OrganisationId] "
                                                  + "         ,[AccountingDate] "
                                                  + " 	) AS firstDates ON firstDates.OrganisationId = ret.OrganisationId "
                                                  + " 	AND firstDates.AccountingDate = ret.AccountingDate ");
            
            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[" + "OrganisationSubmissionInfoView" + "] " + " 	TO [ReportsReaderDv]; ");
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
