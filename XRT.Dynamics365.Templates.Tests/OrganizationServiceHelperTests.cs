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

        /// <summary>
        /// Tests that if the queue with name provided does not exist, an exception is thrown.
        /// </summary>
        [Fact]
        public void AddToQueue_Throws_Exception_For_Queue_Not_Found()
        {
            //Arrange & Assert
            Assert.ThrowsAny<Exception>(() => Fixture.Helper.AddToQueue(new EntityReference(), "Fake Queue Name"));
        }

        /// <summary>
        /// Tests that if the target is null, an exception is thrown.
        /// </summary>
        [Fact]
        public void AddToQueue_Throws_Exception_For_Null_Target()
        {
            //Arrange
            var queue = new Entity("queue");
            queue.Attributes.Add("name", "Test Name");
            queue.Id = Fixture.Service.Create(queue);

            Assert.ThrowsAny<Exception>(() => Fixture.Helper.AddToQueue(null, "Test Name"));
        }

        /// <summary>
        /// Tests that if the queue Id provided is an empty guid, an exception is thrown.
        /// </summary>
        [Fact]
        public void AddToQueue_Throws_Exception_For_Invalid_Queue_Id()
        {
            //Arrange
            Assert.ThrowsAny<Exception>(() => Fixture.Helper.AddToQueue(new EntityReference(), Guid.Empty));
        }

        /// <summary>
        /// Tests that if a valid queue name is provided, the call succeeds.
        /// </summary>
        [Fact]
        public void AddToQueue_By_Name_Passes()
        {
            //Arrange
            var queue = new Entity("queue");
            queue.Attributes.Add("name", "Test Name");
            queue.Id = Fixture.Service.Create(queue);

            var account = new Entity("account");
            account.Attributes.Add("name", "Test Account");
            account.Id = Fixture.Service.Create(account);

            //Act
            var outcome = Record.Exception(() => Fixture.Helper.AddToQueue(account.ToEntityReference(), "Test Name"));

            //Assert
            Assert.Null(outcome);
        }

        /// <summary>
        /// Tests that if a valid queue Id is provided, the call succeeds.
        /// </summary>
        [Fact]
        public void AddToQueue_By_Id_Passes()
        {
            //Arrange
            var queue = new Entity("queue");
            queue.Attributes.Add("name", "Test Name");
            queue.Id = Fixture.Service.Create(queue);

            var account = new Entity("account");
            account.Attributes.Add("name", "Test Account");
            account.Id = Fixture.Service.Create(account);

            //Act
            var outcome = Record.Exception(() => Fixture.Helper.AddToQueue(account.ToEntityReference(), queue.Id));

            //Assert
            Assert.Null(outcome);
        }
        
        /// <summary>
        /// Tests an invalid URL returns Guid.Empty.
        /// </summary>
        [Fact]
        public void GetIdFromRecordURL_Returns_Empty_Guid_For_Invalid_Id()
        {
            //Arrange
            var url = "Not a record url";

            //Act
            var outcome = Fixture.Helper.GetIdFromRecordURL(url);

            //Assert
            Assert.Equal(Guid.Empty, outcome);
        }

        /// <summary>
        /// Tests an empty URL returns Guid.Empty.
        /// </summary>
        [Fact]
        public void GetIdFromRecordURL_Returns_Empty_Guid_For_Empty_Url()
        {
            //Arrange
            var url = string.Empty;

            //Act
            var outcome = Fixture.Helper.GetIdFromRecordURL(url);

            //Assert
            Assert.Equal(Guid.Empty, outcome);
        }

        /// <summary>
        /// Tests a record Id is parsed from a url.
        /// </summary>
        [Fact]
        public void GetIdFromRecordURL_Returns_Guid_For_Valid_Url()
        {
            //Arrange
            var id = Guid.NewGuid();
            var url = $"https://fakecrm.crm4.dynamics.com/main.aspx?etc=1234&id={id}&pagetype=entityrecord";

            //Act
            var outcome = Fixture.Helper.GetIdFromRecordURL(url);

            //Assert
            Assert.Equal(id, outcome);
        }

        /// <summary>
        /// Tests an invalid URL returns -1.
        /// </summary>
        [Fact]
        public void GetETCFromRecordURL_Returns_Minus1_For_Invalid_Url()
        {
            //Arrange
            var url = "Not a record url";

            //Act
            var outcome = Fixture.Helper.GetETCFromRecordURL(url);

            //Assert
            Assert.Equal(-1, outcome);
        }
        
        /// <summary>
        /// Tests an ETC is parsed from a url.
        /// </summary>
        [Fact]
        public void GetETCFromRecordURL_Returns_Guid_For_Valid_Url()
        {
            //Arrange
            var etc = 1234;
            var url = $"https://fakecrm.crm4.dynamics.com/main.aspx?etc=1234&id={Guid.NewGuid()}&pagetype=entityrecord";

            //Act
            var outcome = Fixture.Helper.GetETCFromRecordURL(url);

            //Assert
            Assert.Equal(etc, outcome);
        }

        /// <summary>
        /// Tests the CreateActivityParty helper throws an exception for a null target.
        /// </summary>
        [Fact]
        public void CreateActivityParty_Throws_Exception_For_Null_Target()
        {
            //Arrange
            Entity entity = null;

            //Act & Assert
            Assert.Throws<ArgumentNullException>("target", () => Fixture.Helper.CreateActivityParty(entity));
        }

        /// <summary>
        /// Tests the CreateActivityParty helper returns an Activity Party record.
        /// </summary>
        [Fact]
        public void CreateActivityParty_Returns_Activity_Party_For_Entity()
        {
            //Arrange
            var entityRef = new EntityReference("rob_fakeentity", Guid.NewGuid());

            //Act
            var outcome = Fixture.Helper.CreateActivityParty(entityRef);

            //Assert
            Assert.Equal("activityparty", outcome.Entities[0].LogicalName);
            Assert.Equal(entityRef.Id, outcome.Entities[0].GetAttributeValue<EntityReference>("partyid").Id);
            Assert.Equal("rob_fakeentity", outcome.Entities[0].GetAttributeValue<EntityReference>("partyid").LogicalName);
        }

        /// <summary>
        /// Tests the CreateActivityParty helper returns an Activity Party record.
        /// </summary>
        [Fact]
        public void CreateActivityParty_Returns_Activity_Party_For_EntityReference()
        {
            //Arrange
            var entityRef = new Entity("rob_fakeentity", Guid.NewGuid());

            //Act
            var outcome = Fixture.Helper.CreateActivityParty(entityRef);

            //Assert
            Assert.Equal("activityparty", outcome.Entities[0].LogicalName);
            Assert.Equal(entityRef.Id, outcome.Entities[0].GetAttributeValue<EntityReference>("partyid").Id);
            Assert.Equal("rob_fakeentity", outcome.Entities[0].GetAttributeValue<EntityReference>("partyid").LogicalName);
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