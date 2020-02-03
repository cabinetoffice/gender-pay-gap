DECLARE @STARTING_ID int
SET @STARTING_ID = 10000000

DECLARE @NUM_OF_USERS INT
SET @NUM_OF_USERS = 10

DECLARE @INDEX INT
SET @INDEX = 1

-- Remove all test entities
UPDATE Organisations
	SET LatestRegistration_OrganisationId = null, LatestRegistration_UserId = null WHERE OrganisationId > @STARTING_ID AND OrganisationId <= @STARTING_ID + @NUM_OF_USERS
	DELETE FROM Users WHERE UserId > @STARTING_ID AND UserId <= @STARTING_ID + @NUM_OF_USERS
	DELETE FROM Organisations WHERE OrganisationId > @STARTING_ID AND OrganisationId <= @STARTING_ID + 2 * @NUM_OF_USERS
	DELETE FROM OrganisationScopes WHERE OrganisationScopeId > @STARTING_ID AND OrganisationScopeId <= @STARTING_ID + 2* @NUM_OF_USERS
	DELETE FROM UserOrganisations WHERE OrganisationId > @STARTING_ID AND OrganisationId <= @STARTING_ID + 2 * @NUM_OF_USERS
	DELETE FROM OrganisationNames WHERE OrganisationId > @STARTING_ID AND OrganisationId <= @STARTING_ID + 2 * @NUM_OF_USERS
	DELETE FROM OrganisationSicCodes WHERE OrganisationId > @STARTING_ID AND OrganisationId <= @STARTING_ID + 2 * @NUM_OF_USERS


WHILE @INDEX <= @NUM_OF_USERS
BEGIN
	-- Create test users
	SET IDENTITY_INSERT Users ON
	INSERT INTO Users 
	(UserId, FirstName, LastName, JobTitle, StatusId, StatusDate, LoginAttempts, LoginDate, ResetAttempts, VerifyAttempts, Created, Modified, EmailAddress, PasswordHash, Salt, HashingAlgorithm, EmailVerifiedDate)
	VALUES
	(@STARTING_ID + @INDEX, 'test', 'test', 'test', 3, '01/10/2019 12:26:44', 0, '01/10/2019 12:26:44', 0, 0, '01/10/2019 12:26:44', '01/10/2019 12:26:44', 'user'+ CAST(@INDEX AS VARCHAR(16)) +'@example.com', 'EsbXTRJaMnDBhGEerRu1eqbMoInXkOz4P5rNZyq1VKU=','9jjDqTsUzGqk/+Rl8JeR4A==', 2, '01/10/2019 12:26:44');
	SET IDENTITY_INSERT Users OFF
	
	-- Create test organisations that are linked to test users
	SET IDENTITY_INSERT Organisations ON
	INSERT INTO Organisations 
	(OrganisationId, CompanyNumber, OrganisationName, SectorTypeId, StatusId, StatusDate, StatusDetails, Created , Modified, OptedOutFromCompaniesHouseUpdate, EmployerReference)
	VALUES
	(@STARTING_ID + @INDEX, '99999' + CAST(@INDEX AS VARCHAR(16)), 'test' + CAST(@INDEX AS VARCHAR(16)), 1, 3, '01/10/2019 12:26:44', 'PIN Confirmed', '01/10/2019 12:26:44', '01/10/2019 12:26:44', 1, 'ABCDE' + CAST(@INDEX AS VARCHAR(16)));
	SET IDENTITY_INSERT Organisations OFF

	SET IDENTITY_INSERT OrganisationNames ON
	INSERT INTO OrganisationNames 
	(OrganisationNameId, OrganisationId, Name, Source, Created)
	VALUES
	(@STARTING_ID + @INDEX, @STARTING_ID + @INDEX, 'test' + CAST(@INDEX AS VARCHAR(16)), 'User', '01/10/2019 12:26:44');
	SET IDENTITY_INSERT OrganisationNames OFF

	SET IDENTITY_INSERT OrganisationScopes ON
	INSERT INTO OrganisationScopes 
	(OrganisationScopeId, OrganisationId, ScopeStatusId, ScopeStatusDate, RegisterStatusId, RegisterStatusDate, SnapshotDate, StatusId)
	VALUES
	(@STARTING_ID + @INDEX, @STARTING_ID + @INDEX, 1, '01/10/2019 12:26:44', 0, '01/10/2019 12:26:44', '05/04/2017 00:00:00', 3);
	SET IDENTITY_INSERT OrganisationScopes OFF

	SET IDENTITY_INSERT OrganisationAddresses ON
	INSERT INTO OrganisationAddresses 
	(AddressId, OrganisationId, CreatedByUserId, Address1, Country, PostCode, StatusId, StatusDate, Created, Modified, Source, IsUkAddress)
	VALUES
	(@STARTING_ID + @INDEX, @STARTING_ID + @INDEX, @STARTING_ID + @INDEX, 'test street', 'United Kingdom', 'NWN NWN', 3, '01/10/2019 12:26:44', '01/10/2019 12:26:44', '05/04/2017 00:00:00', 'User', 'True');
	SET IDENTITY_INSERT OrganisationAddresses OFF

	SET IDENTITY_INSERT OrganisationSicCodes ON
	INSERT INTO OrganisationSicCodes 
	(OrganisationSicCodeId, OrganisationId, SicCodeId, Created, Source)
	VALUES
	(@STARTING_ID + @INDEX, @STARTING_ID + @INDEX, 2200, '01/10/2019 12:26:44', 'User');
	SET IDENTITY_INSERT OrganisationSicCodes OFF

	UPDATE Organisations
	SET LatestScopeId = @STARTING_ID + @INDEX, LatestAddressId = @STARTING_ID + @INDEX WHERE OrganisationId = @STARTING_ID + @INDEX;
		
	INSERT INTO UserOrganisations 
	(UserId, OrganisationId, ConfirmAttempts, Created , Modified, PINConfirmedDate)
	VALUES
	(@STARTING_ID + @INDEX, @STARTING_ID + @INDEX, 0, '01/10/2019 12:26:44', '01/10/2019 12:26:44', '01/10/2019 12:26:44')

	UPDATE Organisations
	SET LatestRegistration_OrganisationId = @STARTING_ID + @INDEX, LatestRegistration_UserId = @STARTING_ID + @INDEX WHERE OrganisationId = @STARTING_ID + @INDEX

	-- Create test organisations that are NOT linked to test users
	SET IDENTITY_INSERT Organisations ON
	INSERT INTO Organisations 
	(OrganisationId, CompanyNumber, OrganisationName, SectorTypeId, StatusId, StatusDate, StatusDetails, Created , Modified, OptedOutFromCompaniesHouseUpdate, EmployerReference)
	VALUES
	(@STARTING_ID + @NUM_OF_USERS + @INDEX, '99999' + CAST(@INDEX + @NUM_OF_USERS AS VARCHAR(16)), 'test' + CAST(@INDEX + @NUM_OF_USERS  AS VARCHAR(16)), 1, 3, '01/10/2019 12:26:44', 'PIN Confirmed', '01/10/2019 12:26:44', '01/10/2019 12:26:44', 1, 'ABCDE' + CAST(@INDEX + @NUM_OF_USERS AS VARCHAR(16)));
	SET IDENTITY_INSERT Organisations OFF

	SET IDENTITY_INSERT OrganisationNames ON
	INSERT INTO OrganisationNames 
	(OrganisationNameId, OrganisationId, Name, Source, Created)
	VALUES
	(@STARTING_ID + @NUM_OF_USERS + @INDEX, @STARTING_ID + @NUM_OF_USERS + @INDEX, 'test' + CAST(@INDEX + @NUM_OF_USERS AS VARCHAR(16)), 'User', '01/10/2019 12:26:44');
	SET IDENTITY_INSERT OrganisationNames OFF

	SET IDENTITY_INSERT OrganisationScopes ON
	INSERT INTO OrganisationScopes 
	(OrganisationScopeId, OrganisationId, ScopeStatusId, ScopeStatusDate, RegisterStatusId, RegisterStatusDate, SnapshotDate, StatusId)
	VALUES
	(@STARTING_ID + @NUM_OF_USERS + @INDEX, @STARTING_ID + @NUM_OF_USERS + @INDEX, 1, '01/10/2019 12:26:44', 0, '01/10/2019 12:26:44', '05/04/2017 00:00:00', 3);
	SET IDENTITY_INSERT OrganisationScopes OFF

	SET IDENTITY_INSERT OrganisationAddresses ON
	INSERT INTO OrganisationAddresses 
	(AddressId, OrganisationId, CreatedByUserId, Address1, Country, PostCode, StatusId, StatusDate, Created, Modified, Source, IsUkAddress)
	VALUES
	(@STARTING_ID + @NUM_OF_USERS + @INDEX, @STARTING_ID + @NUM_OF_USERS + @INDEX, @STARTING_ID + @NUM_OF_USERS + @INDEX, 'test street', 'United Kingdom', 'NWN NWN', 3, '01/10/2019 12:26:44', '01/10/2019 12:26:44', '05/04/2017 00:00:00', 'User', 'True');
	SET IDENTITY_INSERT OrganisationAddresses OFF

	SET IDENTITY_INSERT OrganisationSicCodes ON
	INSERT INTO OrganisationSicCodes 
	(OrganisationSicCodeId, OrganisationId, SicCodeId, Created, Source)
	VALUES
	(@STARTING_ID + @NUM_OF_USERS + @INDEX, @STARTING_ID + @NUM_OF_USERS + @INDEX, 2200, '01/10/2019 12:26:44', 'User');
	SET IDENTITY_INSERT OrganisationSicCodes OFF

	UPDATE Organisations
	SET LatestScopeId = @STARTING_ID + @NUM_OF_USERS + @INDEX, LatestAddressId = @STARTING_ID + @NUM_OF_USERS + @INDEX WHERE OrganisationId = @STARTING_ID + @NUM_OF_USERS + @INDEX;

	SET @INDEX = @INDEX + 1;
END

select * from Organisations where OrganisationId >= @STARTING_ID
select * from Users where UserId >= @STARTING_ID
