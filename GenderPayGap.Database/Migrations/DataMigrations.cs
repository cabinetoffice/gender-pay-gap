using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.Database
{
    public static class DataMigrations
    {

        /// <summary>
        ///     //Seed the SIC Codes and Categories
        /// </summary>
        /// <param name="context">The database context to initialise</param>
        public static async Task Update_SICSectionsAsync(IDataRepository dataRepository,
            IFileRepository repository,
            string dataPath,
            bool force = false)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(dataPath))
            {
                throw new ArgumentNullException(nameof(dataPath));
            }

            string sectionsPath = Path.Combine(dataPath, Filenames.SicSections);

            if (!await repository.GetDirectoryExistsAsync(dataPath))
            {
                await repository.CreateDirectoryAsync(dataPath);
            }

            IDbContext context = dataRepository.GetDbContext();
            DbSet<SicSection> sicSections = context.Set<SicSection>();
            if (force || !sicSections.Any())
            {
                List<SicSection> sectionRecords = await repository.ReadCSVAsync<SicSection>(sectionsPath);
                if (!sectionRecords.Any())
                {
                    throw new Exception($"No records found in {sectionsPath}");
                }

                sicSections.UpsertRange(sectionRecords);
            }

            await context.SaveChangesAsync();
        }

        public static async Task Update_SICCodesAsync(IDataRepository dataRepository,
            IFileRepository repository,
            string dataPath,
            bool force = false)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(dataPath))
            {
                throw new ArgumentNullException(nameof(dataPath));
            }

            string codesPath = Path.Combine(dataPath, Filenames.SicCodes);

            if (!await repository.GetDirectoryExistsAsync(dataPath))
            {
                await repository.CreateDirectoryAsync(dataPath);
            }

            IDbContext context = dataRepository.GetDbContext();
            List<SicCode> sicCodes = await dataRepository.GetAll<SicCode>().ToListAsync();
            DbSet<SicCode> dset = context.Set<SicCode>();
            DateTime created = VirtualDateTime.Now;
            if (force || !sicCodes.Any())
            {
                List<SicCode> codeRecords = await repository.ReadCSVAsync<SicCode>(codesPath);
                if (!codeRecords.Any())
                {
                    throw new Exception($"No records found in {codesPath}");
                }

                //This is required to prevent a primary key violation
                Parallel.ForEach(
                    codeRecords,
                    code => {
                        code.SicSection = null;
                        if (sicCodes.Any(
                            s => s.SicCodeId == code.SicCodeId && s.SicSectionId == code.SicSectionId && s.Description == code.Description))
                        {
                            return;
                        }

                        code.Created = created;
                    });
                dset.UpsertRange(codeRecords);
            }

            await context.SaveChangesAsync();
        }

    }
}
