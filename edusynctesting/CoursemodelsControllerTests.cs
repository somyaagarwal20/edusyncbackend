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
using EduSyncwebapi.Services;
using MockQueryable.Moq;
using Microsoft.AspNetCore.Http;

namespace edusynctesting
{
    [TestFixture]
    public class CoursemodelsControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private Mock<IBlobStorageService> _mockBlobService;
        private CoursemodelsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>();
            _mockBlobService = new Mock<IBlobStorageService>();
            _controller = new CoursemodelsController(_mockContext.Object, _mockBlobService.Object);
        }

        [Test]
        public async Task GetCoursemodel_WithValidId_ReturnsCourse()
        {
            var courseId = Guid.NewGuid();
            var course = new Coursemodel { Cousreld = courseId, Title = "Course", Description = "Desc", InstructorId = Guid.NewGuid(), MediaUrl = "url" };
            var courses = new List<Coursemodel> { course };
            var mockDbSet = courses.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(courseId)).ReturnsAsync(course);
            _mockContext.Setup(c => c.Coursemodels).Returns(mockDbSet.Object);

            var result = await _controller.GetCoursemodel(courseId);

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            if (result.Result is OkObjectResult okResult && okResult.Value is CourseDto returnedCourse)
            {
                Assert.That(returnedCourse!.Cousreld, Is.EqualTo(courseId));
            }
            else
            {
                Assert.Fail("Result is not OkObjectResult with CourseDto");
            }
        }

        [Test]
        public async Task GetCoursemodel_WithInvalidId_ReturnsNotFound()
        {
            var courseId = Guid.NewGuid();
            var courses = new List<Coursemodel>();
            var mockDbSet = courses.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(courseId)).ReturnsAsync((Coursemodel)null);
            _mockContext.Setup(c => c.Coursemodels).Returns(mockDbSet.Object);

            var result = await _controller.GetCoursemodel(courseId);

            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task PostCoursemodel_WithFile_UploadsAndReturnsCreatedCourse()
        {
            var courseDto = new CourseDto
            {
                Title = "New Course",
                Description = "Desc",
                InstructorId = Guid.NewGuid(),
                MediaUrl = null
            };
            var fileMock = new Mock<IFormFile>();
            _mockBlobService.Setup(s => s.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>())).ReturnsAsync("mediaUrl");
            _mockContext.Setup(c => c.Coursemodels.Add(It.IsAny<Coursemodel>()));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _controller.PostCoursemodel(courseDto, fileMock.Object);

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            if (result.Result is CreatedAtActionResult createdResult && createdResult.Value is CourseDto returnedCourse)
            {
                Assert.That(returnedCourse!.Title, Is.EqualTo(courseDto.Title));
                Assert.That(returnedCourse!.MediaUrl, Is.EqualTo("mediaUrl"));
            }
            else
            {
                Assert.Fail("Result is not CreatedAtActionResult with CourseDto");
            }
        }

        [Test]
        public async Task DeleteCoursemodel_WithValidId_DeletesCourseAndFile()
        {
            var courseId = Guid.NewGuid();
            var course = new Coursemodel { Cousreld = courseId, MediaUrl = "http://blobstorage.com/file.pdf" };
            var courses = new List<Coursemodel> { course };
            var mockDbSet = courses.AsQueryable().BuildMockDbSet();
            mockDbSet.Setup(d => d.FindAsync(courseId)).ReturnsAsync(course);
            _mockContext.Setup(c => c.Coursemodels).Returns(mockDbSet.Object);
            _mockBlobService.Setup(s => s.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockContext.Setup(c => c.Coursemodels.Remove(course));
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var result = await _controller.DeleteCoursemodel(courseId);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
    }
} 