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
                $"If not Exists (select loginname from master.dbo.syslogins where name = 'ReportsReaderLogin') CREATE LOGIN [ReportsReaderLogin] WITH PASSWORD = '{ReportsReaderPassword}';");

            #region 201811261437002_Add user ReportReader.cs

            migrationBuilder.Sql("CREATE USER [ReportsReaderDv] FOR LOGIN [ReportsReaderLogin];");

            #endregion

            migrationBuilder.Sql("GRANT SELECT ON OBJECT::[dbo].[OrganisationNames] TO [ReportsReaderDv] AS [dbo];");

            #region 201811261453436_Add view OrganisationAddressInfoView.cs

            migrationBuilder.Sql(
                "CREATE VIEW [dbo].[OrganisationAddressInfoView] AS SELECT organisationAdd.OrganisationId ,CASE organisationAdd.[StatusId] WHEN 0 THEN 'Unknown' WHEN 1 THEN 'New' WHEN 2 THEN 'Suspended' WHEN 3 THEN 'Active' WHEN 5 THEN 'Pending' WHEN 6 THEN 'Retired' ELSE 'Error' END + ' (' + CAST(organisationAdd.StatusId AS VARCHAR(2)) + ')' AS AddressStatus ,REPLACE(COALESCE(RTRIM(LTRIM(organisationAdd.Address1)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.Address2)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.Address3)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.TownCity)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.County)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.PoBox)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.PostCode)), '') + COALESCE(' (' + RTRIM(LTRIM(organisationAdd.Country)) + ')', ''), ' ,', ',') AS FullAddress ,organisationAdd.StatusDetails AS AddressStatusDetails ,organisationAdd.StatusDate AS AddressStatusDate ,organisationAdd.Source AS AddressSource ,organisationAdd.Created AS AddressCreated ,organisationAdd.Modified AS AddressModified FROM ( SELECT AddressId ,CreatedByUserId ,CASE WHEN (LEN([Address1]) > 0) THEN [Address1] ELSE NULL END AS Address1 ,CASE WHEN (LEN([Address2]) > 0) THEN [Address2] ELSE NULL END AS Address2 ,CASE WHEN (LEN([Address3]) > 0) THEN [Address3] ELSE NULL END AS Address3 ,CASE WHEN (LEN([TownCity]) > 0) THEN [TownCity] ELSE NULL END AS TownCity ,CASE WHEN (LEN([County]) > 0) THEN [County] ELSE NULL END AS County ,CASE WHEN (LEN([Country]) > 0) THEN [Country] ELSE NULL END AS Country ,CASE WHEN (LEN([PoBox]) > 0) THEN [PoBox] ELSE NULL END AS PoBox ,CASE WHEN (LEN([PostCode]) > 0) THEN [PostCode] ELSE NULL END AS PostCode ,StatusId ,StatusDate ,StatusDetails ,Created ,Modified ,OrganisationId ,Source FROM dbo.OrganisationAddresses ) AS organisationAdd INNER JOIN dbo.Organisations AS orgs ON orgs.OrganisationId = organisationAdd.OrganisationId");

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
            
            // End of old initial create file
            
            const string viewModificationScript = " ALTER VIEW [dbo].["
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
                                                  + " 	AND firstDates.AccountingDate = ret.AccountingDate ";

            migrationBuilder.Sql(viewModificationScript);

            const string selectPermissionsScript = " GRANT SELECT " + " 	ON OBJECT::dbo.[" + "OrganisationSubmissionInfoView" + "] " + " 	TO [ReportsReaderDv]; ";

            migrationBuilder.Sql(selectPermissionsScript);
            
            // end of modify_view
            
            migrationBuilder.Sql(
                "UPDATE Organisations SET OptedOutFromCompaniesHouseUpdate = 'true'"
                + " where OrganisationId IN (13671, 15264)");
            
            // end of setOptOut
            
            migrationBuilder.Sql(
                "UPDATE Returns SET IsLateSubmission = 'true'"
                + " where ReturnId IN (24553, 23832, 22706, 22783, 24690, 24136, 24846, 24516, 31533, 31534, 29735, 35106, 23265, 23869, 30409, 38134, 32601, 25300, 32964, 35879, 32969, 24652, 24628, 24359, 23981, 24332, 34500, 24433, 28565, 28562, 24303, 25176, 25299, 25205, 27397, 24512, 24134, 25301, 24673, 25020, 26354, 23965, 24838, 24837, 25163, 38326, 24068, 25797, 25366, 25078, 27689, 25166, 27653, 25510, 25509, 26840, 27868, 26620, 34257, 35944, 35950, 25357, 29194, 29178, 23841, 25250, 24659, 23933, 23856, 24000, 24252, 24770, 27654, 24595, 24420, 25318, 26733, 24032, 24714, 26385, 26147, 24836, 25096, 23838, 24202, 24057, 35636, 25230, 25369, 28457, 24854, 24428, 33852, 25222, 24161, 24062, 24006, 24003, 25353, 25038, 25037, 25034, 25036, 24847, 30542, 24985, 26549, 36992, 25039, 24810, 24662, 24797, 24730, 24733, 24663, 24249, 25327, 31920, 23926, 27467, 25334, 30702, 24874, 24486, 25520, 24947, 34013, 25499, 24875, 24698, 24047, 24065, 24784, 25320, 25319, 29729, 28190, 24426, 32468, 24416, 24048, 24737, 24739, 24738, 24186, 23863, 23848, 23825, 38608, 24777, 25421, 24400, 24869, 24785, 32887, 25489, 24327, 26054, 36011, 29362, 24732, 24072, 24807, 32550, 25018, 23875, 21660, 23842, 24004, 24726, 26250, 35649, 25056, 24931, 34846, 25667, 24852, 25232, 36569, 26009, 24386, 24641, 24060, 24580, 23955, 25178, 24251, 24809, 24508, 23810, 24625, 32595, 24937, 35285, 24682, 31309, 31314, 31305, 24778, 24091, 24440, 26099, 34842, 26613, 24695, 26236, 26233, 26234, 26231, 24648, 28252, 25021, 24782, 24873, 33550, 24760, 24593, 24144, 24150, 24513, 22587, 25053, 25351, 23724, 24410, 31277, 24529, 23986, 24394, 31306, 31308, 24845, 25175, 24582, 24866, 23969, 27316, 33583, 24711, 24627, 24913, 24892, 24832, 24347, 36310, 24026, 28304, 24894, 24898, 38104, 24912, 24891, 26403, 32590, 25189, 22722, 35913, 19970, 24755, 25251, 25150, 28323, 23907, 24815, 25208, 25206, 24631, 32530, 23976, 34768, 24620, 28198, 26567, 32593, 24089, 24746, 24786, 21294, 35773, 25379, 25438, 24560, 24765, 24589, 24183, 24308, 24598, 23951, 38239, 37952, 37576, 37574, 36158, 37684, 36111, 37765, 37647, 35392, 32368, 37566, 37729, 33366, 33710, 37554, 31959, 33686, 37476, 34679, 32463, 37672, 35252, 32041, 38311, 36402, 37720, 38002, 37664, 33543, 32752, 32070, 37532, 32308, 37416, 37679, 37994, 37353, 37562, 37191, 37587, 32480, 37777, 38062, 37526, 37999, 37422, 34228, 33264, 37402, 37941, 37906, 37567, 37496, 37962, 37845, 37874, 37896, 37298, 37599, 37443, 37612, 38086, 37734, 37330, 37869, 37404, 37405, 37839, 37842, 37713, 37605, 37732, 38091, 37533, 37849, 37961, 37541, 37604, 37710, 37648, 37354, 37461, 37998, 37489, 37911, 37861, 37374, 37974, 38097, 37694, 37781, 37793, 38327, 37515, 37767, 37386, 37980, 37848, 37289, 37872, 38237, 37412, 37759, 37448, 37844, 37658, 37293, 37314, 37594, 37789, 37683, 37653, 37651, 37560, 37688, 37646, 37486, 37807, 37620, 37300, 38707, 37691, 37843, 37797, 37782, 37808, 37362, 37509, 37806, 37833, 37674, 38177, 37325, 37550, 37669, 37542, 37491, 37492, 37670, 37595, 37449, 37677, 37355, 37480, 37859, 37490, 37666, 38094, 37543, 37698, 37441, 37432, 37868, 37769, 37328, 37969, 37660, 37421, 37505, 37435, 37278, 37371, 38137, 38607, 37570, 37575, 37377, 37481, 37737, 37296, 37656, 37388, 37927, 37356, 37339, 38014, 37966, 37312, 37970, 37957, 37948, 37671, 37788, 37852, 37898, 37690, 37359, 38306, 38018, 37682, 37390, 37968, 37569, 37406, 37456, 37444, 37812, 37983, 38209, 37436, 37494, 37277, 37633, 37753, 37971, 38695, 37947, 37746, 37649, 37333, 37343, 37783, 37282, 37320, 38132, 37391, 37865, 38165, 37622, 23964, 24727, 24248, 24939, 24679, 25169, 24442, 24804, 24417, 24586, 24485, 24554, 24741, 24826, 24461, 24262, 24540, 24464, 24720, 24650, 24916, 24490, 24208, 24744, 24848, 24655, 24541, 24443, 25455, 24588, 25029, 24802, 24458, 24457, 24411, 24228, 24435, 26148, 37634, 24324, 37715, 37498, 37886, 24685, 37938, 31535, 24063, 38033, 26399, 24282, 37728, 28239, 29930, 37930, 37929, 37936, 37933, 37939, 37937, 37935, 37931, 25434, 37306, 25400, 35258, 37851, 32743, 37642, 38013, 24839, 26400, 23938, 24951, 24677, 24881, 24882, 24883, 24884, 24885, 24886, 24887, 24888, 24890, 24893, 24895, 24906, 32038, 24984, 31299, 37468, 24480, 24479, 24527, 24476, 24412, 37535, 24780, 29776, 38021, 37291, 24579, 26666, 37601, 27538, 36079, 24934, 24538, 24572, 30085, 24506, 25418, 35774, 35777, 38017, 37623, 25268, 24573, 37654, 37809, 27837, 36999, 29066, 37311, 24596, 37761, 37798, 24956, 24955, 23812, 38129, 24787, 37836, 24590, 24407, 24829, 24871, 25293, 37749, 24840, 26010, 24564, 37545, 24769, 29765, 24591, 37506, 37469, 38079, 24429, 29908, 24567, 24619, 33360, 37850, 24790, 24095, 37502, 25535, 25563, 24799, 24339, 24794, 33302, 24735, 37946, 25289, 37348, 24791, 37113, 24231, 38147, 38135, 24250, 24449, 37470, 37596, 25009, 25295, 37717, 38360, 37750, 24323, 25049, 38520, 30699, 24548, 37795, 24160, 24597, 25697, 24275, 24283, 31083, 31944, 37510, 34290, 38153, 38171, 38170, 24551, 38701, 24861, 32907, 32897, 24354, 24767, 24498, 24165, 24155, 24261, 24896, 23876, 28570, 33151, 25007, 34295, 25035, 28213, 25292, 24762, 36243, 37409, 32197, 28884, 23878, 24636, 25223, 23943, 38157, 38015, 25309, 32962, 37758, 24821, 25391, 32599, 31302, 37428, 37276, 37247, 37578, 37675, 37764, 23845, 25008, 25149, 37581, 37327, 37437, 37184, 24515, 27920, 37786, 26204, 38305, 37864, 29071, 38081, 37407, 24879, 38130, 24880, 28599, 27417, 37778, 25174, 35113, 24897, 22631, 29574, 26999, 37411, 24028, 24034, 38462, 37408, 25462, 32379, 38029, 37878, 37614, 32609, 37661, 26152, 37745, 23823, 23904, 37967, 32605, 25496, 37711, 38106, 24266, 24860, 24602, 24333, 24388, 27537, 24444, 37888, 23948, 24706, 24599, 24523, 32581, 25207, 38164, 24079, 26495, 24246, 34612, 24615, 24454, 34569, 37673, 24365, 37705, 24828, 37556, 24041, 38266, 26659, 24341, 24546, 37522, 25296, 24045, 24610, 24197, 27578, 23956, 24133, 33203, 24421, 24024, 23882, 37979, 24813, 37875, 24056, 32304, 24002, 38074, 24915, 24544, 25203, 37827, 24563, 38260, 24537, 24849, 24656, 24717, 24772, 24483, 29017, 23877, 24557, 24351, 24734, 24270, 24147, 37910, 23937, 23922, 23944, 24456, 24661, 24578, 24864, 24098, 37951, 24395, 24132, 24723, 31715, 23157, 23855, 24545, 23846, 24255, 28178, 23859, 24010, 37796, 24517, 24877, 24571, 24116, 23990, 24039, 38547, 37043, 24096, 24184, 24059, 23959, 37702, 24194, 24472, 24046, 37976, 23989, 24552, 24375, 24189, 37609, 37685, 25058, 37430, 25177, 23982, 24112, 23908, 24505, 23919, 23980, 24344, 23954, 24687, 24230, 35824, 24427, 24583, 24273, 23925, 24633, 24077, 24040, 23941, 24419, 24462, 37800, 24378, 37626, 23992, 24027, 24305, 24409, 24438, 24276, 24925, 23939, 24145, 24021, 24025, 25016, 37577, 37917, 23935, 24110, 29461, 24637, 24360, 24201, 37275, 24113, 24609, 25277, 24699, 24331, 23934, 24334, 24044, 24148, 23977, 23973, 24729, 24284, 23983, 23971, 24317, 37632, 24109, 24146, 25374, 24825, 24119, 23839, 37006, 24137, 24585, 24105, 24100, 24771, 24233, 24635, 24055, 24511, 24401, 37709, 24510, 24812, 24078, 24349, 24850, 24851, 24638, 24151, 24131, 24569, 24117, 24639, 24291, 24491, 24520, 24239, 24174, 24792, 24195, 25030, 24367, 25324, 23903, 24335, 24247, 24917, 24209, 24719, 24179, 24468, 37975, 24406, 24300, 24843, 24618, 24452, 24320, 24817, 37592, 24362, 24279, 24657, 24974, 24779, 24526, 23899, 24495, 24124, 24164, 24336, 24484, 24835, 24330, 24514, 38728, 24101, 37960, 25170, 24668, 23968, 24693, 24167, 24830, 24093, 24182, 24168, 24296, 24280, 24549, 37826, 37918, 37376, 37546, 37724, 37871, 38087, 38007, 25012, 24982, 24447, 25024, 25025, 35907, 25044, 25045, 25065, 24355, 20708, 24129, 24507, 22538, 23883, 24225, 24122, 24138, 23874, 24570, 24436, 23897, 24581, 24398, 24340, 23914, 24607, 23979, 24518, 24542, 24191, 38527, 24747, 24285, 24576, 24363, 24612, 24288, 24294, 24857, 24803, 24229, 24314, 24528, 24651, 24481, 24575, 24833, 24750, 24776, 24642, 24414, 24487, 24469, 24380, 24748, 24753, 24592, 24562, 24701, 24600, 24611, 24533, 24806, 24616, 24805, 24629, 24371, 24622, 24666, 24587, 24781, 24574, 24492, 24653, 24752, 24704, 24965, 24667, 24683, 24689, 24924, 24764, 24868, 24834, 24715, 24789, 24796, 24749, 24867, 24795, 24943, 24841, 24878, 24876, 24969, 24935, 24958, 24971, 24967, 25121, 37366, 37814, 37772, 25085, 38391, 38390, 19871, 25120, 25125, 26050, 25160, 37899, 38200, 38139, 25406, 25528, 25541, 25512, 25157, 25795, 37500, 24710, 26618, 37524, 26272, 26308, 26310, 30535, 37558, 37739, 28855, 28001, 38203, 27849, 37364, 31234, 28284, 37394, 37393, 31957, 37667, 28835, 35304, 31854, 37363, 37754, 30517, 37714, 35971, 30020, 38241, 37477, 37867, 37680, 30229, 37511, 30639, 37819, 31291, 37341, 37455, 37387, 32460, 37700, 37537, 32577, 32579, 34928, 34913, 37877, 37893, 33067, 37644, 33276, 33693, 37360, 34992, 35354, 37479, 37305, 36298, 37665, 36633, 37748, 37744, 37323, 37676, 37342, 37351, 37774, 37370, 37417, 37583, 37518, 37464, 38058, 37475, 37521, 37678, 38036, 37727, 37701, 37703, 37723, 37752, 37757, 37766, 37799, 37810, 37815, 37823, 37825, 37837, 37855, 37857, 37863, 37876, 37883, 37926, 37940, 37987, 37982, 37985, 38008, 38020, 38043, 38055, 38061, 38167, 38192, 38193, 38273, 38416)");
        
            // end of SetLateFlag
            
            migrationBuilder.Sql(
                "UPDATE [dbo].[Organisations]"
                + "SET"
                + "    [LatestAddressId] = NULL,"
                + "    [LatestRegistration_OrganisationId] = NULL,"
                + "    [LatestRegistration_UserId] = NULL,"
                + "    [LatestReturnId] = NULL,"
                + "    [LatestScopeId] = NULL"
            );
            
            // end of setMisleadingProperties
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
