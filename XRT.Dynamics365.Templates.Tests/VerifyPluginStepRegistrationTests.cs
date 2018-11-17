using System;
using System.Collections.Generic;
using XRT.Dynamics365.Templates.Tests.Helpers;
using Xunit;

namespace XRT.Dynamics365.Templates.Tests
{
    /// <summary>
    /// Tests the verification of plugin step registrations.
    /// </summary>
    public class VerifyPluginStepRegistrationTests : IClassFixture<PluginBaseFixture>
    {
        private PluginBaseFixture Fixture { get; set; }

        //Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public VerifyPluginStepRegistrationTests(PluginBaseFixture fixture)
        {
            Fixture = new PluginBaseFixture();
        }

        //Tests

        /// <summary>
        /// Passing a null collection passes.
        /// </summary>
        [Fact]
        public void Verify_Null_Collection_Equals_A_Pass()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            var outcome = Record.Exception(() => worker.VerifyRegistration("Test Plugin", null));

            //Assert
            Assert.Null(outcome);
        }

        /// <summary>
        /// Passing a complete match passes.
        /// </summary>
        [Fact]
        public void Verify_Complete_Match_Equals_A_Pass()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            var outcome = Record.Exception(() => worker.VerifyRegistration("Test Plugin", new List<PluginStepRegistration>
            {
                new PluginStepRegistration
                {
                    EntityName = "contact",
                    MaximumDepth = 2,
                    Message = "Create",
                    PluginStage = (int)PluginStage.Pre
                }
            }));

            //Assert
            Assert.Null(outcome);
        }

        /// <summary>
        /// Passing a collection with an empty entity name equals a pass.
        /// </summary>
        [Fact]
        public void Verify_EntityName_Not_Set_Still_Equals_A_Pass()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            var outcome = Record.Exception(() => worker.VerifyRegistration("Test Plugin", new List<PluginStepRegistration>
            {
                new PluginStepRegistration
                {
                    EntityName = string.Empty,
                    MaximumDepth = 2,
                    Message = "Create",
                    PluginStage = (int)PluginStage.Pre
                }
            }));

            //Assert
            Assert.Null(outcome);
        }

        /// <summary>
        /// Passing a collection with an empty entity name equals a pass.
        /// </summary>
        [Fact]
        public void Verify_Depth_Not_Set_Still_Equals_A_Pass()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            var outcome = Record.Exception(() => worker.VerifyRegistration("Test Plugin", new List<PluginStepRegistration>
            {
                new PluginStepRegistration
                {
                    EntityName = "Contact",
                    MaximumDepth = -1,
                    Message = "Create",
                    PluginStage = (int)PluginStage.Pre
                }
            }));

            //Assert
            Assert.Null(outcome);
        }

        /// <summary>
        /// Passing a collection with an empty message equals a pass.
        /// </summary>
        [Fact]
        public void Verify_Message_Not_Set_Still_Equals_A_Pass()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            var outcome = Record.Exception(() => worker.VerifyRegistration("Test Plugin", new List<PluginStepRegistration>
            {
                new PluginStepRegistration
                {
                    EntityName = "Contact",
                    MaximumDepth = 2,
                    Message = string.Empty,
                    PluginStage = (int)PluginStage.Pre
                }
            }));

            //Assert
            Assert.Null(outcome);
        }

        /// <summary>
        /// Passing a collection with an empty stage equals a pass.
        /// </summary>
        [Fact]
        public void Verify_Stage_Not_Set_Still_Equals_A_Pass()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            var outcome = Record.Exception(() => worker.VerifyRegistration("Test Plugin", new List<PluginStepRegistration>
            {
                new PluginStepRegistration
                {
                    EntityName = "Contact",
                    MaximumDepth = 2,
                    Message = "Create",
                    PluginStage = -1
                }
            }));

            //Assert
            Assert.Null(outcome);
        }

        /// <summary>
        /// Incorrect entity name causes a fail.
        /// </summary>
        [Fact]
        public void Incorrect_Entity_Name_Causes_A_Fail()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            Assert.ThrowsAny<Exception>(() => worker.VerifyRegistration("Test Plugin", new List<PluginStepRegistration>
            {
                new PluginStepRegistration
                {
                    EntityName = "account",
                    MaximumDepth = 1,
                    Message = "Create",
                    PluginStage = (int)PluginStage.Pre
                }
            }));
        }

        /// <summary>
        /// Incorrect depth causes a fail.
        /// </summary>
        [Fact]
        public void Incorrect_Depth_Causes_A_Fail()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            Assert.ThrowsAny<Exception>(() => worker.VerifyRegistration("Test Plugin", new List<PluginStepRegistration>
            {
                new PluginStepRegistration
                {
                    EntityName = "contact",
                    MaximumDepth = 1,
                    Message = "Create",
                    PluginStage = (int)PluginStage.Pre
                }
            }));
        }

        /// <summary>
        /// Incorrect message causes a fail.
        /// </summary>
        [Fact]
        public void Incorrect_Message_Causes_A_Fail()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            Assert.ThrowsAny<Exception>(() => worker.VerifyRegistration("Test Plugin", new List<PluginStepRegistration>
            {
                new PluginStepRegistration
                {
                    EntityName = "contact",
                    MaximumDepth = 1,
                    Message = "Update",
                    PluginStage = (int)PluginStage.Pre
                }
            }));
        }

        /// <summary>
        /// Incorrect stage causes a fail.
        /// </summary>
        [Fact]
        public void Incorrect_Stage_Causes_A_Fail()
        {
            //Arrange
            var worker = Fixture.GenerateProviderForVerify();

            //Act
            Assert.ThrowsAny<Exception>(() => worker.VerifyRegistration("Test Plugin", new List<PluginStepRegistration>
            {
                new PluginStepRegistration
                {
                    EntityName = "contact",
                    MaximumDepth = 1,
                    Message = "Create",
                    PluginStage = (int)PluginStage.Post
                }
            }));
        }
    }
}