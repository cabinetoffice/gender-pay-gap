DO $$

DECLARE
   STARTING_ID INTEGER := 300000000;
   NUM_OF_USERS INTEGER := 20000;
   INDEX INTEGER := 1;

-- Remove all test entities
BEGIN
    DELETE FROM Users WHERE UserId > STARTING_ID AND UserId <= STARTING_ID + NUM_OF_USERS;
    DELETE FROM Organisations WHERE OrganisationId > STARTING_ID AND OrganisationId <= STARTING_ID + 2 * NUM_OF_USERS;
    DELETE FROM UserOrganisations WHERE OrganisationId > STARTING_ID AND OrganisationId <= STARTING_ID + 2 * NUM_OF_USERS;
    DELETE FROM OrganisationScopes WHERE OrganisationScopeId > STARTING_ID AND OrganisationScopeId <= STARTING_ID + 2* NUM_OF_USERS;
    DELETE FROM OrganisationNames WHERE OrganisationId > STARTING_ID AND OrganisationId <= STARTING_ID + 2 * NUM_OF_USERS;
    DELETE FROM OrganisationSicCodes WHERE OrganisationId > STARTING_ID AND OrganisationId <= STARTING_ID + 2 * NUM_OF_USERS;
    DELETE FROM OrganisationAddresses WHERE AddressId > STARTING_ID AND AddressId <= STARTING_ID + 2 * NUM_OF_USERS;
    DELETE FROM Returns WHERE OrganisationId > STARTING_ID AND OrganisationId <= STARTING_ID + NUM_OF_USERS;

    -- Delete users created using registration journey
    DELETE FROM Users WHERE Firstname = 'Test' AND Lastname = 'Example' AND JobTitle = 'Tester';

    WHILE INDEX <= NUM_OF_USERS LOOP
        -- Create test users
        INSERT INTO Users
        ("UserId", "FirstName", "LastName", "JobTitle", "StatusId", "StatusDate", "LoginAttempts", "LoginDate", "ResetAttempts", "VerifyAttempts", "Created", "Modified", "EmailAddress", "PasswordHash", "Salt", "HashingAlgorithm", "EmailVerifiedDate")
        VALUES
        (STARTING_ID + INDEX, 'test', 'test', 'test', 3, '01/10/2019 12:26:44', 0, '01/10/2019 12:26:44', 0, 0, '01/10/2019 12:26:44', '01/10/2019 12:26:44', 'loadtest'+ CAST(INDEX AS VARCHAR(16)) +'@example.com', 'EsbXTRJaMnDBhGEerRu1eqbMoInXkOz4P5rNZyq1VKU=','9jjDqTsUzGqk/+Rl8JeR4A==', 2, '01/10/2019 12:26:44');

        -- Create test organisations that are linked to test users
        INSERT INTO Organisations
        ("OrganisationId", "CompanyNumber", "OrganisationName", "SectorTypeId", "StatusId", "StatusDate", "StatusDetails", "Created", "Modified", "OptedOutFromCompaniesHouseUpdate", "EmployerReference")
        VALUES
        (STARTING_ID + INDEX, '99999' + CAST(INDEX AS VARCHAR(16)), 'test_' + CAST(INDEX AS VARCHAR(16)), 1, 3, '01/10/2019 12:26:44', 'PIN Confirmed', '01/10/2019 12:26:44', '01/10/2019 12:26:44', 1, 'ABCDE' + CAST(INDEX AS VARCHAR(16)));

        INSERT INTO OrganisationNames
        ("OrganisationNameId", "OrganisationId", "Name", "Source", "Created")
        VALUES
        (STARTING_ID + INDEX, STARTING_ID + INDEX, 'test_' + CAST(INDEX AS VARCHAR(16)), 'User', '01/10/2019 12:26:44');

        INSERT INTO OrganisationScopes
        ("OrganisationScopeId", "OrganisationId", "ScopeStatusId", "ScopeStatusDate", "RegisterStatusId", "RegisterStatusDate", "SnapshotDate", "StatusId")
        VALUES
        (STARTING_ID + INDEX, STARTING_ID + INDEX, 1, '01/10/2019 12:26:44', 0, '01/10/2019 12:26:44', '05/04/2017 00:00:00', 3);

        INSERT INTO OrganisationAddresses
        ("AddressId", "OrganisationId", "CreatedByUserId", "Address1", "Country", "PostCode", "StatusId", "StatusDate", "Created", "Modified", "Source", "IsUkAddress")
        VALUES
        (STARTING_ID + INDEX, STARTING_ID + INDEX, STARTING_ID + INDEX, 'test street', 'United Kingdom', 'NWN NWN', 3, '01/10/2019 12:26:44', '01/10/2019 12:26:44', '05/04/2017 00:00:00', 'User', 'True');

        INSERT INTO OrganisationSicCodes
        ("OrganisationSicCodeId", "OrganisationId", "SicCodeId", "Created", "Source")
        VALUES
        (STARTING_ID + INDEX, STARTING_ID + INDEX, 2200, '01/10/2019 12:26:44', 'User');

        INSERT INTO UserOrganisations
        ("UserId", "OrganisationId", "ConfirmAttempts", "Created", "Modified", "PINConfirmedDate")
        VALUES
        (STARTING_ID + INDEX, STARTING_ID + INDEX, 0, '01/10/2019 12:26:44', '01/10/2019 12:26:44', '01/10/2019 12:26:44')

        -- Create test organisations that are NOT linked to test users
        INSERT INTO Organisations
        ("OrganisationId", "CompanyNumber", "OrganisationName", "SectorTypeId", "StatusId", "StatusDate", "StatusDetails", "Created", "Modified", "OptedOutFromCompaniesHouseUpdate", "EmployerReference")
        VALUES
        (STARTING_ID + NUM_OF_USERS + INDEX, '99999' + CAST(INDEX + NUM_OF_USERS AS VARCHAR(16)), 'test_' + CAST(INDEX + NUM_OF_USERS  AS VARCHAR(16)), 1, 3, '01/10/2019 12:26:44', 'PIN Confirmed', '01/10/2019 12:26:44', '01/10/2019 12:26:44', 1, 'ABCDE' + CAST(INDEX + NUM_OF_USERS AS VARCHAR(16)));

        INSERT INTO OrganisationNames
        ("OrganisationNameId", "OrganisationId", "Name", "Source", "Created")
        VALUES
        (STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, 'test_' + CAST(INDEX + NUM_OF_USERS AS VARCHAR(16)), 'User', '01/10/2019 12:26:44');

        INSERT INTO OrganisationScopes
        ("OrganisationScopeId", "OrganisationId", "ScopeStatusId", "ScopeStatusDate", "RegisterStatusId", "RegisterStatusDate", "SnapshotDate", "StatusId")
        VALUES
        (STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, 1, '01/10/2019 12:26:44', 0, '01/10/2019 12:26:44', '05/04/2017 00:00:00', 3);

        INSERT INTO OrganisationAddresses
        ("AddressId", "OrganisationId", "CreatedByUserId", "Address1", "Country", "PostCode", "StatusId", "StatusDate", "Created", "Modified", "Source", "IsUkAddress")
        VALUES
        (STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, 'test street', 'United Kingdom', 'NWN NWN', 3, '01/10/2019 12:26:44', '01/10/2019 12:26:44', '05/04/2017 00:00:00', 'User', 'True');

        INSERT INTO OrganisationSicCodes
        ("OrganisationSicCodeId", "OrganisationId", "SicCodeId", "Created", "Source")
        VALUES
        (STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, 2200, '01/10/2019 12:26:44', 'User');

        SET INDEX = INDEX + 1;
    END LOOP
END $$

select * from "Organisations" where "Organisations"."OrganisationId" >= @STARTING_ID;
select * from "Users" where "Users"."UserId" >= @STARTING_ID;
