using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using XRT.Dynamics365.Templates.Tests.Helpers;
using Xunit;

namespace XRT.Dynamics365.Templates.Tests
{
    /// <summary>
    /// Tests the helpers for IOrganizationService.
    /// </summary>
    public class IOrganizationServiceHelperTests : IClassFixture<OrganizationServiceHelperFixture>
    {
        //Properties

        private OrganizationServiceHelperFixture Fixture { get; set; }

        //Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IOrganizationServiceHelperTests(OrganizationServiceHelperFixture fixture)
        {
            Fixture = new OrganizationServiceHelperFixture();
        }

        //Tests

        /// <summary>
        /// Tests the associate helper throws an exception for a null target.
        /// </summary>
        [Fact]
        public void Associate_Throws_Exception_For_Null_Target()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>("target", () => Fixture.Helper.Associate("test", null, new EntityReferenceCollection()));
        }

        /// <summary>
        /// Tests the associate helper throws an exception for a null children collection.
        /// </summary>
        [Fact]
        public void Associate_Throws_Exception_For_Null_Children()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>("children", () => Fixture.Helper.Associate("test", new EntityReference(), null));
        }

        /// <summary>
        /// Tests the associate helper throws an exception for an empty relationship name.
        /// </summary>
        [Fact]
        public void Associate_Throws_Exception_For_Empty_Relationship_Name()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>("relationshipName", () => Fixture.Helper.Associate(string.Empty, new EntityReference(), new EntityReferenceCollection()));
        }

        /// <summary>
        /// Tests the associate helper throws specific exception for null target.
        /// </summary>
        [Fact]
        public void Associate_Associates_Entites()
        {
            //Arrange
            var fakeRelationship = new XrmFakedRelationship("rob_fake1id", "rob_fake2id", "rob_fake1", "rob_fake2");
            Fixture.Context.AddRelationship("rob_fake1_fake2", fakeRelationship);

            var primary = GetER("rob_fake1");
            var secondary = GetER("rob_fake2");

            //Act
            var output = Record.Exception(() =>
            {
                Fixture.Helper.Associate("rob_fake1_fake2", primary, new EntityReferenceCollection { secondary });
            });

            //Assert
            Assert.Null(output);
        }

        /// <summary>
        /// Tests the disassociate helper throws an exception for a null target.
        /// </summary>
        [Fact]
        public void Disassociate_Throws_Exception_For_Null_Target()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>("target", () => Fixture.Helper.Disassociate("test", null, new EntityReferenceCollection()));
        }

        /// <summary>
        /// Tests the disassociate helper throws an exception for a null children collection.
        /// </summary>
        [Fact]
        public void Disassociate_Throws_Exception_For_Null_Children()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>("children", () => Fixture.Helper.Disassociate("test", new EntityReference(), null));
        }

        /// <summary>
        /// Tests the disassociate helper throws an exception for an empty relationship name.
        /// </summary>
        [Fact]
        public void Disassociate_Throws_Exception_For_Empty_Relationship_Name()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>("relationshipName", () => Fixture.Helper.Disassociate(string.Empty, new EntityReference(), new EntityReferenceCollection()));
        }

        /// <summary>
        /// Tests the disassociate helper does not throw an exception.
        /// </summary>
        [Fact]
        public void Disassociate_Disassociates_Entities()
        {
            //Arrange
            var fakeRelationship = new XrmFakedRelationship("rob_fake1id", "rob_fake2id", "rob_fake1", "rob_fake2");
            Fixture.Context.AddRelationship("rob_fake1_fake2", fakeRelationship);

            var primary = GetER("rob_fake1");
            var secondary = GetER("rob_fake2");
            Fixture.Service.Associate("rob_fake1id", primary.Id, new Relationship("rob_fake1_fake2"), new EntityReferenceCollection { secondary });

            //Act
            var output = Record.Exception(() =>
            {
                Fixture.Helper.Disassociate("rob_fake1_fake2", primary, new EntityReferenceCollection { secondary });
            });

            //Assert
            Assert.Null(output);
        }

        /// <summary>
        /// Tests the associate helper throws an exception for a null target.
        /// </summary>
        [Fact]
        public void QueryByValue_Throws_Exception_For_Null_TargetEntityName()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>("targetEntityName", () => Fixture.Helper.QueryByValue(string.Empty, new ColumnSet(), new Dictionary<string,object>()));
        }

        /// <summary>
        /// Tests the associate helper throws an exception for a null children collection.
        /// </summary>
        [Fact]
        public void QueryByValue_Throws_Exception_For_Null_Filters()
        {
            //Act & Assert
            Assert.Throws<ArgumentNullException>("filters", () => Fixture.Helper.QueryByValue("test", new ColumnSet(), null));
        }

        /// <summary>
        /// Tests the query helper handles a single filter.
        /// </summary>
        [Fact]
        public void QueryByValue_Returns_Entities_For_Single_Filter()
        {
            //Arrange
            for(var i = 0; i < 20; i++)
            {
                var e = new Entity("rob_fake1");
                e.Attributes.Add("rob_name", $"My Test Entity {i}");
                e.Id = Fixture.Service.Create(e);
            }

            //Act
            var results = Fixture.Helper.QueryByValue("rob_fake1", new ColumnSet(true), "rob_name", "My Test Entity 6");

            //Assert
            Assert.Single(results.Entities);
            Assert.Contains("My Test Entity 6", results.Entities.First().GetAttributeValue<string>("rob_name"));
        }

        /// <summary>
        /// Tests the query helper handles multiple filters.
        /// </summary>
        [Fact]
        public void QueryByValue_Handles_No_Entities_Found()
        {
            //Arrange
            var related = new Entity("rob_fake2");
            related.Id = Fixture.Service.Create(related);
            for (var i = 0; i < 20; i++)
            {
                var e = new Entity("rob_fake1");
                e.Attributes.Add("rob_name", $"My Test Entity {i}");
                e.Attributes.Add("rob_osv", new OptionSetValue(i));
                e.Attributes.Add("rob_money", new Money(i));
                e.Attributes.Add("rob_refid", related.ToEntityReference());
                e.Id = Fixture.Service.Create(e);
            }

            //Act
            var results = Fixture.Helper.QueryByValue("rob_fake1", new ColumnSet(true), new Dictionary<string, object>
            {
                { "rob_osv", 4 },
                { "rob_refid", related.Id }
            });

            //Assert
            Assert.Single(results.Entities);
            Assert.Contains("My Test Entity 4", results.Entities.First().GetAttributeValue<string>("rob_name"));
        }

        /// <summary>
        /// Tests the query helper handles multiple filters.
        /// </summary>
        [Fact]
        public void QueryByValue_Returns_Entities_For_Multiple_Filters()
        {
            //Arrange
            var related = new Entity("rob_fake2");
            related.Id = Fixture.Service.Create(related);
            for (var i = 0; i < 20; i++)
            {
                var e = new Entity("rob_fake1");
                e.Attributes.Add("rob_name", $"My Test Entity {i}");
                e.Id = Fixture.Service.Create(e);
            }

            //Act
            var results = Fixture.Helper.QueryByValue("rob_fake1", new ColumnSet(true), new Dictionary<string, object>
            {
                { "rob_name", "My Test Entity 21" }
            });

            //Assert
            Assert.Empty(results.Entities);
        }

        //Helpers

        private EntityReference GetER(string logicalName)
        {
            var ent = new Entity(logicalName);
            ent.Id = Fixture.Service.Create(ent);
            return ent.ToEntityReference();
        }
    }
}