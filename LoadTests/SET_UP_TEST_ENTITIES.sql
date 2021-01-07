DO $$

    DECLARE
        -- A high starting number to ensure that the IDs don't interfere with any existing data
        STARTING_ID INTEGER := 300000000;
        NUM_OF_USERS INTEGER := 20000;
        INDEX INTEGER := 1;

-- Remove all test entities
    BEGIN
        DELETE FROM "AuditLogs" WHERE "OrganisationId" > STARTING_ID;
        DELETE FROM "UserStatus" WHERE "ByUserId" > STARTING_ID;
        DELETE FROM "OrganisationStatus" WHERE "OrganisationId" > STARTING_ID;
        DELETE FROM "OrganisationStatus" WHERE "ByUserId" > STARTING_ID;
        DELETE FROM "UserOrganisations" WHERE "UserOrganisations"."OrganisationId" > STARTING_ID;
        DELETE FROM "OrganisationScopes" WHERE "OrganisationScopes"."OrganisationScopeId" > STARTING_ID;
        DELETE FROM "OrganisationNames" WHERE "OrganisationNames"."OrganisationId" > STARTING_ID;
        DELETE FROM "OrganisationSicCodes" WHERE "OrganisationSicCodes"."OrganisationId" > STARTING_ID;
        DELETE FROM "OrganisationAddresses" WHERE "OrganisationAddresses"."AddressId" > STARTING_ID;
        DELETE FROM "Returns" WHERE "Returns"."OrganisationId" > STARTING_ID;

        -- Delete users created using registration journey
        DELETE FROM "UserStatus" WHERE "ByUserId" IN (SELECT "UserId" FROM "Users" WHERE "Users"."Firstname" = 'Test' AND "Users"."Lastname" = 'Example' AND "Users"."JobTitle" = 'Tester');
        DELETE FROM "Users" WHERE "Users"."Firstname" = 'Test' AND "Users"."Lastname" = 'Example' AND "Users"."JobTitle" = 'Tester';

        DELETE FROM "Users" WHERE "Users"."UserId" > STARTING_ID;
        DELETE FROM "Organisations" WHERE "Organisations"."OrganisationId" > STARTING_ID;

        WHILE INDEX <= NUM_OF_USERS LOOP
                -- Create test users
                INSERT INTO "Users"
                ("UserId", "Firstname", "Lastname", "JobTitle", "StatusId", "StatusDate", "LoginAttempts", "LoginDate", "ResetAttempts", "VerifyAttempts", "Created", "Modified", "EmailAddress", "PasswordHash", "Salt", "HashingAlgorithm", "EmailVerifiedDate", "SendUpdates", "AllowContact")
                VALUES
                (STARTING_ID + INDEX, 'test', 'test', 'test', 3, '01/10/2020 12:26:44', 0, '01/10/2020 12:26:44', 0, 0, '01/10/2020 12:26:44', '01/10/2020 12:26:44', 'loadtest' || CAST(INDEX AS VARCHAR(16)) || '@example.com', 'EsbXTRJaMnDBhGEerRu1eqbMoInXkOz4P5rNZyq1VKU=','9jjDqTsUzGqk/+Rl8JeR4A==', 2, '01/10/2020 12:26:44', false, false);

                -- Create test organisations that are linked to test users
                INSERT INTO "Organisations"
                ("OrganisationId", "CompanyNumber", "OrganisationName", "SectorTypeId", "StatusId", "StatusDate", "StatusDetails", "Created", "Modified", "OptedOutFromCompaniesHouseUpdate", "EmployerReference")
                VALUES
                (STARTING_ID + INDEX, '99999' || CAST(NUM_OF_USERS + INDEX AS VARCHAR(16)), 'test_' || CAST(INDEX + NUM_OF_USERS AS VARCHAR(16)), 1, 3, '01/10/2020 12:26:44', 'PIN Confirmed', '01/10/2020 12:26:44', '01/10/2020 12:26:44', true, 'ABCDE' || CAST(INDEX AS VARCHAR(16)));

                INSERT INTO "OrganisationNames"
                ("OrganisationNameId", "OrganisationId", "Name", "Source", "Created")
                VALUES
                (STARTING_ID + INDEX, STARTING_ID + INDEX, 'test_' || CAST(INDEX AS VARCHAR(16)), 'User', '01/10/2020 12:26:44');

                INSERT INTO "OrganisationScopes"
                ("OrganisationScopeId", "OrganisationId", "ScopeStatusId", "ScopeStatusDate", "RegisterStatusId", "RegisterStatusDate", "SnapshotDate", "StatusId")
                VALUES
                (STARTING_ID + INDEX, STARTING_ID + INDEX, 1, '01/10/2020 12:26:44', 0, '01/10/2020 12:26:44', '05/04/2020 00:00:00', 3),
                (STARTING_ID + 2 * NUM_OF_USERS + INDEX, STARTING_ID + INDEX, 1, '01/10/2019 12:26:44', 0, '01/10/2019 12:26:44', '05/04/2019 00:00:00', 3);

                INSERT INTO "OrganisationAddresses"
                ("AddressId", "OrganisationId", "CreatedByUserId", "Address1", "Country", "PostCode", "StatusId", "StatusDate", "Created", "Source", "IsUkAddress")
                VALUES
                (STARTING_ID + INDEX, STARTING_ID + INDEX, STARTING_ID + INDEX, 'test street', 'United Kingdom', 'NWN NWN', 3, '01/10/2020 12:26:44', '01/10/2020 12:26:44', 'User', 'True');

                INSERT INTO "OrganisationSicCodes"
                ("OrganisationSicCodeId", "OrganisationId", "SicCodeId", "Created", "Source")
                VALUES
                (STARTING_ID + INDEX, STARTING_ID + INDEX, 2200, '01/10/2020 12:26:44', 'User');

                INSERT INTO "UserOrganisations"
                ("UserId", "OrganisationId", "ConfirmAttempts", "Created", "Modified", "PINConfirmedDate")
                VALUES
                (STARTING_ID + INDEX, STARTING_ID + INDEX, 0, '01/10/2020 12:26:44', '01/10/2020 12:26:44', '01/10/2020 12:26:44');

                -- Create test organisations that are NOT linked to test users
                -- These have the same names as linked Organisations, so they appear in the search results together
                INSERT INTO "Organisations"
                ("OrganisationId", "CompanyNumber", "OrganisationName", "SectorTypeId", "StatusId", "StatusDate", "StatusDetails", "Created", "Modified", "OptedOutFromCompaniesHouseUpdate", "EmployerReference")
                VALUES
                (STARTING_ID + NUM_OF_USERS + INDEX, '99999' || CAST(INDEX + 2 * NUM_OF_USERS AS VARCHAR(16)), 'test_' || CAST(INDEX + NUM_OF_USERS  AS VARCHAR(16)), 1, 3, '01/10/2020 12:26:44', 'PIN Confirmed', '01/10/2020 12:26:44', '01/10/2020 12:26:44', true, 'ABCDE' || CAST(INDEX + NUM_OF_USERS AS VARCHAR(16)));

                INSERT INTO "OrganisationNames"
                ("OrganisationNameId", "OrganisationId", "Name", "Source", "Created")
                VALUES
                (STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, 'test_' || CAST(INDEX + NUM_OF_USERS AS VARCHAR(16)), 'User', '01/10/2020 12:26:44');

                INSERT INTO "OrganisationScopes"
                ("OrganisationScopeId", "OrganisationId", "ScopeStatusId", "ScopeStatusDate", "RegisterStatusId", "RegisterStatusDate", "SnapshotDate", "StatusId")
                VALUES
                (STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, 1, '01/10/2020 12:26:44', 0, '01/10/2020 12:26:44', '05/04/2020 00:00:00', 3);

                INSERT INTO "OrganisationAddresses"
                ("AddressId", "OrganisationId", "CreatedByUserId", "Address1", "Country", "PostCode", "StatusId", "StatusDate", "Created", "Source", "IsUkAddress")
                VALUES
                (STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, 'test street', 'United Kingdom', 'NWN NWN', 3, '01/10/2020 12:26:44', '01/10/2020 12:26:44', 'User', 'True');

                INSERT INTO "OrganisationSicCodes"
                ("OrganisationSicCodeId", "OrganisationId", "SicCodeId", "Created", "Source")
                VALUES
                (STARTING_ID + NUM_OF_USERS + INDEX, STARTING_ID + NUM_OF_USERS + INDEX, 2200, '01/10/2020 12:26:44', 'User');

                INDEX := INDEX + 1;
            END LOOP;

        PERFORM * from "Organisations" where "Organisations"."OrganisationId" >= STARTING_ID;
        PERFORM * from "Users" where "Users"."UserId" >= STARTING_ID;
    END $$
