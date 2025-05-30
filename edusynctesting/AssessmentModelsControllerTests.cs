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
    public class AssessmentModelsControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private AssessmentModelsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>();
            _controller = new AssessmentModelsController(_mockContext.Object);
        }

        [Test]
        public async Task GetAssessmentModel_WithValidId_ReturnsAssessment()
        {
            var id = Guid.NewGuid();
            var assessment = new AssessmentModel { AssessmentId = id, CourseId = Guid.NewGuid(), Title = "A1", Questions = "Q1", MaxScore = 100 };
            var assessments = new List<AssessmentModel> { assessment };
            var mockDbSet = assessments.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(id)).ReturnsAsync(assessment);
            _mockContext.Setup(c => c.AssessmentModels).Returns(mockDbSet.Object);

            var result = await _controller.GetAssessmentModel(id);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            if (result.Result is OkObjectResult okResult && okResult.Value is AssessmentDto returned)
            {
                Assert.That(returned!.AssessmentId, Is.EqualTo(id));
            }
            else
            {
                Assert.Fail("Result is not OkObjectResult with AssessmentDto");
            }
        }

        [Test]
        public async Task GetAssessmentModel_WithInvalidId_ReturnsNotFound()
        {
            var id = Guid.NewGuid();
            var assessments = new List<AssessmentModel>();
            var mockDbSet = assessments.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(id)).ReturnsAsync((AssessmentModel)null);
            _mockContext.Setup(c => c.AssessmentModels).Returns(mockDbSet.Object);

            var result = await _controller.GetAssessmentModel(id);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task PostAssessmentModel_ReturnsCreatedAssessment()
        {
            var assessmentDto = new AssessmentDto
            {
                CourseId = Guid.NewGuid(),
                Title = "A1",
                Questions = "Q1",
                MaxScore = 100
            };
            _mockContext.Setup(c => c.AssessmentModels.Add(It.IsAny<AssessmentModel>()));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _controller.PostAssessmentModel(assessmentDto);

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            if (result.Result is CreatedAtActionResult createdResult && createdResult.Value is AssessmentDto returned)
            {
                Assert.That(returned!.Title, Is.EqualTo(assessmentDto.Title));
            }
            else
            {
                Assert.Fail("Result is not CreatedAtActionResult with AssessmentDto");
            }
        }

        [Test]
        public async Task DeleteAssessmentModel_WithValidId_DeletesAssessment()
        {
            var id = Guid.NewGuid();
            var assessment = new AssessmentModel { AssessmentId = id };
            var assessments = new List<AssessmentModel> { assessment };
            var mockDbSet = assessments.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(id)).ReturnsAsync(assessment);
            _mockContext.Setup(c => c.AssessmentModels).Returns(mockDbSet.Object);
            _mockContext.Setup(c => c.AssessmentModels.Remove(assessment));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _controller.DeleteAssessmentModel(id);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
    }
} 