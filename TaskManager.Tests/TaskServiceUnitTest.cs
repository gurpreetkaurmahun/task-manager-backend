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
                    TaskItemName = "Test Task Item 1",
                    TaskItemDescription = "It is a unit test task",
                    DateCreated = new DateOnly(2024, 07, 23),
                    DueDate = new DateOnly(2024, 07, 28),
                    IsCompleted = true,
                    SubTasks = new List<SubTask>
                    {
                        new SubTask
                        {
                            SubTaskId = 1,
                            SubTaskName = "SubTask 1",
                            SubTaskDescription = "SubDescription 1",
                            DateCreated = new DateOnly(2024, 07, 24),
                            DueDate = new DateOnly(2024, 07, 25),
                            IsCompleted = true,
                            TaskItemId = 1
                        }
                    }
                },
                new TaskItem
                {
                    TaskItemId = 2,
                    TaskItemName = "Test Task Item 2",
                    TaskItemDescription = "It is a unit test task no 2",
                    DateCreated = new DateOnly(2024, 07, 26),
                    DueDate = new DateOnly(2024, 07, 31),
                    IsCompleted = false,
                    SubTasks = new List<SubTask>()
                },
                new TaskItem
                {
                    TaskItemId = 3,
                    TaskItemName = "Test Task Item 3",
                    TaskItemDescription = "It is a unit test task",
                    DateCreated = new DateOnly(2024, 07, 23),
                    DueDate = new DateOnly(2024, 07, 28),
                    IsCompleted = true,
                    SubTasks = new List<SubTask>
                    {
                        new SubTask
                        {
                            SubTaskId = 2,
                            SubTaskName = "SubTask 2",
                            SubTaskDescription = "SubDescription 1",
                            DateCreated = new DateOnly(2024, 07, 24),
                            DueDate = new DateOnly(2024, 07, 25),
                            IsCompleted = true,
                            TaskItemId = 3
                        }
                    }
                },
            };

            _mockContext = new Mock<TaskContext>();
            _mockContext.Setup(c => c.TaskItems).ReturnsDbSet(_testTaskItems);

            _mockLogger = new Mock<ILogger<TaskService>>();
            _taskService = new TaskService(_mockContext.Object, _mockLogger.Object);
        }

        
        [Fact]
        public void GetAllTaskItems_ShouldReturnAllItems()
        {
            
            var result = _taskService.GetAllTaskItems();

            
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("Test Task Item 1", result[0].TaskItemName);
            Assert.Equal("Test Task Item 2", result[1].TaskItemName);
        }

        [Fact]
        public void GetTaskItem_ExistingId_ShouldReturnTaskItem()
        {
            
            int existingId = 1;

            
            var (taskItem, message) = _taskService.GetTaskItem(existingId);

            
            Assert.NotNull(taskItem);
            Assert.Equal(existingId, taskItem.TaskItemId);
            Assert.Equal("Test Task Item 1", taskItem.TaskItemName);
            Assert.Contains($"Successfully retrieved TaskItem with id: {existingId}", message);
        }

        [Fact]
        public void AddTask_ValidTaskItem_ShouldAddTaskItemAndReturnSuccessMessage(){
            
            var newTaskItem = new TaskItem
            {
                TaskItemId = 3,
                TaskItemName = "New Task Item",
                TaskItemDescription = "New task description",
                DateCreated = new DateOnly(2024, 07, 24),
                DueDate = new DateOnly(2024, 07, 30),
                IsCompleted = false,
                SubTasks = new List<SubTask>()
            };

            // Mock the DbSet and context
            var mockTaskItems = new Mock<DbSet<TaskItem>>();
            _mockContext.Setup(c => c.TaskItems).ReturnsDbSet(new List<TaskItem>());


            
            var (taskItem, message) = _taskService.AddTask(newTaskItem);

            
            Assert.NotNull(taskItem);
            Assert.Equal(newTaskItem.TaskItemId, taskItem.TaskItemId);
            Assert.Contains($"TaskItem with name {newTaskItem.TaskItemName} added successfully", message);
            _mockContext.Verify(c => c.TaskItems.Add(It.IsAny<TaskItem>()), Times.Once);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Fact]
        public void AddTask_InvalidDates_ShouldReturnValidationErrorMessage()
        {
            
            var newTaskItem = new TaskItem
            {
                TaskItemId = 3,
                TaskItemName = "New Task Item",
                TaskItemDescription = "New task description",
                DateCreated = new DateOnly(2024, 07, 24),
                DueDate = new DateOnly(2023, 07, 23), // Invalid due date (earlier than creation date)
                IsCompleted = false,
                SubTasks = new List<SubTask>()
            };

            
            var (taskItem, message) = _taskService.AddTask(newTaskItem);

           
            Assert.Null(taskItem);
            Assert.Contains("Date validation failed", message);
            _mockContext.Verify(c => c.TaskItems.Add(It.IsAny<TaskItem>()), Times.Never);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [Fact]
        public void AddTask_EmptyTaskItemDescription_ShouldReturnValidationErrorMessage()
        {
            
            var newTaskItem = new TaskItem
            {
                TaskItemId = 3,
                TaskItemName = "New Task Item",
                TaskItemDescription = "",
                DateCreated = new DateOnly(2024, 07, 24),
                DueDate = new DateOnly(2024, 07, 30),
                IsCompleted = false,
                SubTasks = new List<SubTask>()
            };


           
            var (taskItem, message) = _taskService.AddTask(newTaskItem);

           
            Assert.Null(taskItem);
            Assert.Contains("Validation failed. TaskItem description cannot be left empty.", message);
            _mockContext.Verify(c => c.TaskItems.Add(It.IsAny<TaskItem>()), Times.Never);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [Fact]
        public void DeleteTask_ExistingId_ShouldDeleteTaskItemAndReturnSuccessMessage()
        {
           
            int existingId = 3;

            
            var (result, message) = _taskService.DeleteTask(existingId);

           
            Assert.True(result);
            Assert.Contains($"TaskItem with id: {existingId} deleted sucessfully", message);
            _mockContext.Verify(c => c.TaskItems.Remove(It.IsAny<TaskItem>()), Times.Once);
            _mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }


        [Fact]
        public void DeleteTask_NonExistingId_ShouldReturnErrorMessage()
        {
            // Act
            var (result, message) = _taskService.DeleteTask(999);

            // Assert
            Assert.False(result);
            Assert.Contains("TaskItem with id: 999 not found", message);
            _mockContext.Verify(c => c.TaskItems.Remove(It.IsAny<TaskItem>()), Times.Never);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [Fact]
        public void DeleteTask_NotCompletedTaskItem_ShouldReturnErrorMessage()
        {
            // Arrange
            int notCompletedId = 2;

            // Act
            var (result, message) = _taskService.DeleteTask(notCompletedId);

            // Assert
            Assert.False(result);
            Assert.Contains($"TaskItem with id: {notCompletedId} cannot be deleted as it has one or more pending subtasks.", message);
            _mockContext.Verify(c => c.TaskItems.Remove(It.IsAny<TaskItem>()), Times.Never);
            _mockContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [Fact]
        public void UpdateTask_TaskItemDoesNotExist_ShouldReturnFalseAndErrorMessage()
        {
            // Arrange
            int nonExistingId = 999;
            var taskItem = new TaskItem
            {
                TaskItemId = 999,
                TaskItemName = "Non-Existing Task",
                TaskItemDescription = "This task does not exist",
                DateCreated = new DateOnly(2024, 07, 20),
                DueDate = new DateOnly(2024, 07, 30),
                IsCompleted = false
            };

            // Act
            var (result, message) = _taskService.UpdateTask(nonExistingId, taskItem);

            // Assert
            Assert.False(result);
            Assert.Contains($"TaskItem with id: {nonExistingId} does not exist", message);
        }

           [Fact]
        public void UpdateTask_ProvidedIdDoesNotMatchTaskItemId_ShouldReturnFalseAndErrorMessage()
        {
            // Arrange
            int existingId = 1;
            var taskItem = new TaskItem
            {
                TaskItemId = 2, // Different TaskItemId
                TaskItemName = "Test Task Item",
                TaskItemDescription = "It is a unit test task",
                DateCreated = new DateOnly(2024, 07, 23),
                DueDate = new DateOnly(2024, 07, 28),
                IsCompleted = false
            };

            // Act
            var (result, message) = _taskService.UpdateTask(existingId, taskItem);

            // Assert
            Assert.False(result);
            Assert.Contains("Invalid request as provided id does not match with TaskItemId", message);
        }

      
    }
}