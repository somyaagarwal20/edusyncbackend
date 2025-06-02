using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using System.Text.Json;
using EduSyncwebapi.Models;
using EduSyncwebapi.Dtos;

namespace EduSyncwebapi.Services
{
    public class EventHubService
    {
        private readonly EventHubProducerClient _producerClient;
        private readonly ILogger<EventHubService> _logger;

        public EventHubService(IConfiguration configuration, ILogger<EventHubService> logger)
        {
            string connectionString = configuration["AzureEventHubs:ConnectionString"];
            string eventHubName = configuration["AzureEventHubs:EventHubName"];

            _producerClient = new EventHubProducerClient(connectionString, eventHubName);
            _logger = logger;
        }

        // Event for user registration
        public async Task SendUserRegistrationEventAsync(UserDto user)
        {
            await SendEventAsync(user, "UserRegistration");
        }

        // Event for course creation
        public async Task SendCourseCreationEventAsync(CourseDto course)
        {
            await SendEventAsync(course, "CourseCreation");
        }

        // Event for assessment submission
        public async Task SendAssessmentSubmissionEventAsync(ResultDto result)
        {
            await SendEventAsync(result, "AssessmentSubmission");
        }

        // Event for user login
        public async Task SendUserLoginEventAsync(string email)
        {
            await SendEventAsync(new { Email = email, Timestamp = DateTime.UtcNow }, "UserLogin");
        }

        // Generic method to send events
        private async Task SendEventAsync<T>(T eventData, string eventType)
        {
            _logger.LogInformation($"Sending event of type: {eventType}");
            try
            {
                // Create the event data
                var eventBody = JsonSerializer.Serialize(eventData);
                var eventBytes = Encoding.UTF8.GetBytes(eventBody);

                // Create the event
                var eventDataBatch = await _producerClient.CreateBatchAsync();
                var eventDataItem = new EventData(eventBytes);
                eventDataItem.Properties["EventType"] = eventType;
                eventDataItem.Properties["Timestamp"] = DateTime.UtcNow.ToString("o");

                // Add the event to the batch
                if (!eventDataBatch.TryAdd(eventDataItem))
                {
                    _logger.LogError("Event is too large for the batch");
                    throw new Exception("Event is too large for the batch");
                }

                // Send the batch
                await _producerClient.SendAsync(eventDataBatch);
                _logger.LogInformation($"Event of type {eventType} sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending event of type {eventType}");
                throw;
            }
        }

        public async Task DisposeAsync()
        {
            if (_producerClient != null)
            {
                await _producerClient.DisposeAsync();
            }
        }
    }
} 