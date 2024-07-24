using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Models;
using TaskManager.Service;
using Xunit;

namespace TaskManager.Tests
{
    public class TaskServiceTest
    {
        private readonly Mock<TaskContext> _mockContext;
        private readonly Mock<ILogger<TaskService>> _mockLogger;
        private readonly TaskService _taskService;
        private readonly List<TaskItem> _testTaskItems;

         public TaskServiceTest()
        {
            _testTaskItems = new List<TaskItem>
            {
                new TaskItem
                {
                    TaskItemId = 1,
                    TaskItemName = "Test Task Item",
                    TaskItemDescription = "It is a unit test task",
                    DateCreated = new DateOnly(2024, 07, 23),
                    DueDate = new DateOnly(2024, 07, 28),
                    IsCompleted = false,
                    SubTasks = new List<SubTask>
                    {
                        new SubTask
                        {
                            SubTaskId = 1,
                            SubTaskName = "SubTask 1",
                            SubTaskDescription = "SubDescription 1",
                            DateCreated = new DateOnly(2024, 07, 24),
                            DueDate = new DateOnly(2024, 07, 25),
                            IsCompleted = false,
                            TaskItemId = 1
                        }
                    }
                },
                new TaskItem
                {
                    TaskItemId = 2,
                    TaskItemName = "Test Task Item Service",
                    TaskItemDescription = "It is a unit test task no 2",
                    DateCreated = new DateOnly(2024, 07, 26),
                    DueDate = new DateOnly(2024, 07, 31),
                    IsCompleted = false,
                    SubTasks = new List<SubTask>()
                }
            };

            _mockContext = new Mock<TaskContext>();
            _mockContext.Setup(c => c.TaskItems).ReturnsDbSet(_testTaskItems);

            _mockLogger = new Mock<ILogger<TaskService>>();
            _taskService = new TaskService(_mockContext.Object, _mockLogger.Object);
        }

        
        [Fact]
        public void GetAllTaskItems_ShouldReturnAllItems()
        {
            // Act
            var result = _taskService.GetAllTaskItems();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Test Task Item", result[0].TaskItemName);
            Assert.Equal("Test Task Item Service", result[1].TaskItemName);
        }

        [Fact]
        public void GetTaskItem_ExistingId_ShouldReturnTaskItem()
        {
            // Arrange
            int existingId = 1;

            // Act
            var (taskItem, message) = _taskService.GetTaskItem(existingId);

            // Assert
            Assert.NotNull(taskItem);
            Assert.Equal(existingId, taskItem.TaskItemId);
            Assert.Equal("Test Task Item", taskItem.TaskItemName);
            Assert.Contains($"Successfully retrieved TaskItem with ID {existingId}", message);
        }

      [Fact]

public void UpdateTaskItem_WithExistingId()
{
    // Arrange
    var taskItemId = 3;
    var taskItem = new TaskItem
    {
        TaskItemId = taskItemId,
        TaskItemName = "Test Task Item--3",
        TaskItemDescription = "It is a unit test task",
        DateCreated = new DateOnly(2024, 07, 26),
        DueDate = new DateOnly(2024, 07, 28),
        IsCompleted = false
    };

    // Add the new task item to the test data
    _testTaskItems.Add(taskItem);

    // Debug: Print out the contents of _testTaskItems
    Console.WriteLine($"Number of items in _testTaskItems: {_testTaskItems.Count}");
    foreach (var item in _testTaskItems)
    {
        Console.WriteLine($"Item ID: {item.TaskItemId}, Name: {item.TaskItemName}");
    }

    // Setup mock context to handle SaveChangesAsync
    _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

    // Setup mock context to handle Entry
    _mockContext.Setup(c => c.Entry(It.IsAny<TaskItem>()))
                .Callback<TaskItem>(t => t.IsCompleted = true) // This simulates setting the state
                .Returns((TaskItem t) => null); // Return null as we don't need the actual EntityEntry

    // Debug: Verify mock setups
    Console.WriteLine("Mock context setups completed");

    // Act
    Console.WriteLine("Calling UpdateTask method");
    var (result, message) = _taskService.UpdateTask(taskItemId, taskItem);

    // Debug output
    Console.WriteLine($"Result: {result}");
    Console.WriteLine($"Message: {message}");

    // Assert
    Assert.True(result, $"Expected result to be true, but got false. Message: {message}");

    // Verify that SaveChangesAsync was called
    _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    // Verify that the logger was called
    _mockLogger.Verify(
        x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"TaskItem with id:{taskItemId} successfully updated")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
        Times.Once);
}
    }
}