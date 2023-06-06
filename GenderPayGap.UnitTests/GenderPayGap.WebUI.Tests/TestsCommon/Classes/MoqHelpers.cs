using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using MockQueryable.Moq;
using Moq;

namespace GenderPayGap.Tests.Common.Classes
{
    public static class MoqHelpers
    {

        public static Mock<IDataRepository> CreateMockDataRepository()
        {
            var mockDataRepo = new Mock<IDataRepository>();
            mockDataRepo.SetupEmptyDataRepository();
            return mockDataRepo;
        }

        public static void SetupEmptyDataRepository(this Mock<IDataRepository> mockDataRepo)
        {
            mockDataRepo.SetupGetAllItemsOfType(new Organisation[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationAddress[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationName[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationScope[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationSicCode[] { });
            mockDataRepo.SetupGetAllItemsOfType(new OrganisationStatus[] { });
            mockDataRepo.SetupGetAllItemsOfType(new Return[] { });
            mockDataRepo.SetupGetAllItemsOfType(new SicCode[] { });
            mockDataRepo.SetupGetAllItemsOfType(new SicSection[] { });
            mockDataRepo.SetupGetAllItemsOfType(new User[] { });
            mockDataRepo.SetupGetAllItemsOfType(new UserOrganisation[] { });
            mockDataRepo.SetupGetAllItemsOfType(new UserStatus[] { });
        }

        public static Mock<IDataRepository> SetupGetAll(this Mock<IDataRepository> mockDataRepo, params object[] items)
        {
            var objects = new List<object>();
            foreach (object item in items)
            {
                var enumerable = item as IEnumerable<object>;
                if (enumerable == null)
                {
                    objects.Add(item);
                }
                else
                {
                    objects.AddRange(enumerable);
                }
            }

            mockDataRepo.SetupGetAll(objects);

            return mockDataRepo;
        }

        private static void SetupGetAll(this Mock<IDataRepository> mockDataRepo, IEnumerable<object> items)
        {
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<Organisation>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationAddress>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationName>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationScope>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationSicCode>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<OrganisationStatus>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<Return>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<SicCode>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<SicSection>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<User>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<UserOrganisation>());
            mockDataRepo.SetupGetAllItemsOfType(items.OfType<UserStatus>());
        }

        private static void SetupGetAllItemsOfType<T>(this Mock<IDataRepository> mockDataRepo, IEnumerable<T> items) where T : class
        {
            List<T> list = items.OfType<T>().ToList();
            if (!list.Any())
            {
                list = new List<T>(new T[] { });
            }

            Mock<IQueryable<T>> mockItems = list.AsQueryable().BuildMock();
            mockDataRepo.Setup(x => x.GetAll<T>()).Returns(mockItems.Object);
        }

    }
}
