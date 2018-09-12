using InvoicePrint.Managers;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace InvoicePrint.Tests.Managers
{
    public class ApiManagerTests
    {

        private readonly MockRepository _mockRepository;
        private readonly Mock<IApiManager> _mockApiManager;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;


        public ApiManagerTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _mockApiManager = _mockRepository.Create<IApiManager>();
            _mockHttpContextAccessor = _mockRepository.Create<IHttpContextAccessor>();
        }

        //private ApiManager CreateManager()
        //{
        //    return new ApiManager(_mockContractsApi.Object);
        //}

        //[Fact(DisplayName = "ApiManager Get Contract Features")]
        //public void GetContractFeatures_ReturnsData()
        //{
        //    // Arrange
        //    var testdata = new Features("123123", "CRTEST123123");
        //    var apiManager = CreateManager();
        //    _mockContractsApi.Setup(x => x.GetContractFeatures(It.IsAny<string>())).Returns(testdata);

        //    // Act
        //    var result = apiManager.GetContractFeatures("123123");

        //    // Assert
        //    Assert.Equal("123123", result.ContractDid);
        //    Assert.Equal("CRTEST123123", result.ContractNumber);

        //}

        //[Fact(DisplayName = "ApiManager Post Contract Features")]
        //public void PostContractFeatures()
        //{
        //    // Arrange
        //    var testData = new Features("123123", "CRTEST123123");
        //    var apiManager = CreateManager();
        //    _mockContractsApi.Setup(x => x.PostContractFeatures(It.IsAny<Features>())).Returns(true);

        //    // Act
        //    var result = apiManager.PostContractFeatures(testData);

        //    // Assert
        //    Assert.True(result);
        //}
    }
}
