using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using EduSyncwebapi.Controllers;
using EduSyncwebapi.Data;
using EduSyncwebapi.Models;
using EduSyncwebapi.Dtos;
using MockQueryable.Moq;

namespace edusynctesting
{
    [TestFixture]
    public class ResultModelsControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private ResultModelsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>();
            _controller = new ResultModelsController(_mockContext.Object);
        }

        [Test]
        public async Task GetResultModel_WithValidId_ReturnsResult()
        {
            var id = Guid.NewGuid();
            var resultModel = new ResultModel { ResultId = id, AssessmentId = Guid.NewGuid(), UserId = Guid.NewGuid(), Score = 90, AttemptDate = DateTime.UtcNow };
            var results = new List<ResultModel> { resultModel };
            var mockDbSet = results.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(id)).ReturnsAsync(resultModel);
            _mockContext.Setup(c => c.ResultModels).Returns(mockDbSet.Object);

            var result = await _controller.GetResultModel(id);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            if (result.Result is OkObjectResult okResult && okResult.Value is ResultDto returned)
            {
                Assert.That(returned!.ResultId, Is.EqualTo(id));
            }
            else
            {
                Assert.Fail("Result is not OkObjectResult with ResultDto");
            }
        }

        [Test]
        public async Task GetResultModel_WithInvalidId_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            var results = new List<ResultModel>();
            var mockDbSet = results.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(id)).ReturnsAsync((ResultModel)null);
            _mockContext.Setup(c => c.ResultModels).Returns(mockDbSet.Object);

            var result = await _controller.GetResultModel(id);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task PostResultModel_ReturnsCreatedResult()
        {
            var resultDto = new ResultDto
            {
                AssessmentId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Score = 95,
                AttemptDate = DateTime.UtcNow
            };
            _mockContext.Setup(c => c.ResultModels.Add(It.IsAny<ResultModel>()));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _controller.PostResultModel(resultDto);

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            if (result.Result is CreatedAtActionResult createdResult && createdResult.Value is ResultDto returned)
            {
                Assert.That(returned!.Score, Is.EqualTo(resultDto.Score));
            }
            else
            {
                Assert.Fail("Result is not CreatedAtActionResult with ResultDto");
            }
        }

        [Test]
        public async Task DeleteResultModel_WithValidId_DeletesResult()
        {
            var id = Guid.NewGuid();
            var resultModel = new ResultModel { ResultId = id };
            var results = new List<ResultModel> { resultModel };
            var mockDbSet = results.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(id)).ReturnsAsync(resultModel);
            _mockContext.Setup(c => c.ResultModels).Returns(mockDbSet.Object);
            _mockContext.Setup(c => c.ResultModels.Remove(resultModel));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _controller.DeleteResultModel(id);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
    }
} 