using System;
using System.IO;
using System.Threading.Tasks;
using DataPrivacyAuditTool.Core.Interfaces;
using DataPrivacyAuditTool.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Reqnroll;
using Xunit;

namespace DataPrivacyAuditTool.Tests.BDD.Steps
{
    [Binding]
    public class FileValidationSteps
    {
        private readonly IFileValidationService _fileValidationService;
        private IFormFile _testFile;
        private bool _validationResult;

        public FileValidationSteps()
        {
            // Create the file validation service
            _fileValidationService = new FileValidationService();
        }

        [Given("I have a valid Settings.json file")]
        public void GivenIHaveAValidSettingsJsonFile()
        {
            var content = @"{
                ""App Settings"": [],
                ""App Lists"": [],
                ""Search Engines"": [],
                ""Apps"": [],
                ""Preferences"": [],
                ""Themes"": [],
                ""Priority Preferences"": [],
                ""Web Apps"": []
            }";

            _testFile = CreateMockFile("Settings.json", content);
        }

        [Given("I have an invalid Settings.json file")]
        public void GivenIHaveAnInvalidSettingsJsonFile()
        {
            var content = "This is not valid JSON";
            _testFile = CreateMockFile("Settings.json", content);
        }

        [When("I validate the Settings.json file")]
        public async Task WhenIValidateTheSettingsJsonFile()
        {
            _validationResult = await _fileValidationService.ValidateSettingsFileAsync(_testFile);
        }
        [Then("the validation should pass")]
        public void ThenTheValidationShouldPass()
        {
            // Instead of Assert.True
            if (!_validationResult)
            {
                throw new Exception("Expected validation to pass but it failed");
            }
        }

        [Then("the validation should fail")]
        public void ThenTheValidationShouldFail()
        {
            // Instead of Assert.False
            if (_validationResult)
            {
                throw new Exception("Expected validation to fail but it passed");
            }
        }

        private IFormFile CreateMockFile(string fileName, string content)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(bytes.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.ContentDisposition).Returns($"inline; filename={fileName}");

            return fileMock.Object;
        }
    }
}
