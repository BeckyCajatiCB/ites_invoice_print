//using Moq;

using System;
using InvoicePrint.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace InvoicePrint.Tests.Controllers
{
    public class InvoicePrintControllerTests : IDisposable
    {

        public void Dispose()
        {
            //_mockRepository.VerifyAll();
        }


        [Fact(DisplayName = "Index_ReturnsViewResult")]
        public void InvoicePrintController_Index_ReturnsViewResult()
        {
            //Arrange
            var controller = new InvoicePrintController();

            //Act
            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }

    }
}
