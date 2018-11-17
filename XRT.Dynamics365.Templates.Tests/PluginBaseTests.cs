using Microsoft.Xrm.Sdk;
using System;
using XRT.Dynamics365.Templates.Tests.Helpers;
using Xunit;

namespace XRT.Dynamics365.Templates.Tests
{
    /// <summary>
    /// Tests the base plugin.
    /// </summary>
    public class PluginBaseTests : IClassFixture<PluginBaseFixture>
    {
        //Properties

        private PluginBaseFixture Fixture { get; set; }

        //Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PluginBaseTests(PluginBaseFixture fixture)
        {
            Fixture = new PluginBaseFixture();
        }

        //Tests

        /// <summary>
        /// Instantiating a plugin sets the depth property.
        /// </summary>
        [Fact]
        public void Sets_Depth()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.Equal(1, worker.Depth);
        }

        /// <summary>
        /// Instantiating a plugin sets the user Id property.
        /// </summary>
        [Fact]
        public void Sets_UserId()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.Equal(Fixture.UserId, worker.UserId);
        }

        /// <summary>
        /// Instantiating a plugin sets the initiating user id property.
        /// </summary>
        [Fact]
        public void Sets_InitiatingUserId()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.Equal(Fixture.InitiatingUserId, worker.InitiatingUserId);
        }

        /// <summary>
        /// Instantiating a plugin sets the initiating user id property.
        /// </summary>
        [Fact]
        public void Sets_Message()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.Equal("Create", worker.Message);
        }

        /// <summary>
        /// Instantiating a plugin sets the organization services.
        /// </summary>
        [Fact]
        public void Instantiates_The_Org_Services()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.Service);
            Assert.NotNull(worker.AdminService);
        }

        /// <summary>
        /// Instantiating a plugin sets the context.
        /// </summary>
        [Fact]
        public void Instantiates_The_Context()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.Context);
        }

        /// <summary>
        /// Instantiating a plugin sets the factory.
        /// </summary>
        [Fact]
        public void Instantiates_The_Factory()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.Factory);
        }

        /// <summary>
        /// Instantiating a plugin sets the tracing service.
        /// </summary>
        [Fact]
        public void Instantiates_The_Trace()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.Trace);
        }

        /// <summary>
        /// Instantiating a plugin sets the target Entity.
        /// </summary>
        [Fact]
        public void Target_Is_Not_Null_For_Create()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.Target);
        }

        /// <summary>
        /// Instantiating a plugin sets the target Entity Reference.
        /// </summary>
        [Fact]
        public void TargetReference_Is_Not_Null_For_Create()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreCreate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.TargetReference);
        }

        /// <summary>
        /// Instantiating a plugin sets the target Entity.
        /// </summary>
        [Fact]
        public void Target_Is_Not_Null_For_Update()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreUpdate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.Target);
        }

        /// <summary>
        /// Instantiating a plugin sets the target Entity Reference.
        /// </summary>
        [Fact]
        public void TargetReference_Is_Not_Null_For_Update()
        {
            //Arrange
            var serviceProvider = FakePluginServiceProviders.GeneratePreUpdate(Fixture.Target, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.TargetReference);
        }

        /// <summary>
        /// Instantiating a set state plugin sets the target Entity Reference.
        /// </summary>
        [Fact]
        public void TargetReference_Is_Not_Null_For_SetState()
        {
            //Arrange
            var serviceProvider =
                FakePluginServiceProviders.GenerateSetState(Fixture.Target.ToEntityReference(), 0, 1, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.TargetReference);
        }

        /// <summary>
        /// Instantiating a set state plugin sets the state and status properties.
        /// </summary>
        [Fact]
        public void State_And_Status_Is_Not_Null_For_SetState()
        {
            //Arrange
            var serviceProvider =
                FakePluginServiceProviders.GenerateSetState(Fixture.Target.ToEntityReference(), 0, 1, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.State);
            Assert.NotNull(worker.Status);
        }

        /// <summary>
        /// Instantiating an assign plugin sets the Target Reference property.
        /// </summary>
        [Fact]
        public void TargetReference_Is_Not_Null_For_Assign()
        {
            //Arrange
            var assignee = new EntityReference("contact", Guid.NewGuid());
            var serviceProvider =
                FakePluginServiceProviders.GenerateAssign(Fixture.Target.ToEntityReference(), assignee, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.TargetReference);
        }

        /// <summary>
        /// Instantiating an assign plugin sets the assignee property.
        /// </summary>
        [Fact]
        public void Assignee_Is_Not_Null_For_Assign()
        {
            //Arrange
            var assignee = new EntityReference("contact", Guid.NewGuid());
            var serviceProvider =
                FakePluginServiceProviders.GenerateAssign(Fixture.Target.ToEntityReference(), assignee, Fixture.UserId, Fixture.InitiatingUserId);

            //Act
            Fixture.Plugin.Execute(serviceProvider);

            //Assert
            var worker = Fixture.GetWorkerFromCache();
            Assert.NotNull(worker.Assignee);
        }
    }
}