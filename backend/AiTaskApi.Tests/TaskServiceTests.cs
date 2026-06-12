using AiTaskApi.Data;
using AiTaskApi.Shared.DTOs;
using AiTaskApi.Models;
using AiTaskApi.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;

public class TaskServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // IMPORTANT: isolate each test
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Create_Task_Should_Add_Task()
    {
        // Arrange
        var context = GetDbContext();
        var service = new TaskService(context);

        var taskDto = new TaskCreateDto
        {
            Title = "Test Task",
            Description = "Test Desc"
        };

        // Act
        var result = await service.Create(taskDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Task", result.Title);
        Assert.Contains(context.Tasks, t => t.Title == "Test Task");
    }

    [Fact]
    public async Task GetAll_Should_Return_Tasks()
    {
        // Arrange
        var context = GetDbContext();
        context.Tasks.Add(new TaskItem { Title = "Task 1", Description = "Desc 1" });
        context.Tasks.Add(new TaskItem { Title = "Task 2", Description = "Desc 2" });
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        // Act
        var result = await service.GetAll(null, null, 1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task Delete_Task_Should_Remove_Task()
    {
        // Arrange
        var context = GetDbContext();

        var task = new TaskItem { Title = "To Delete", Description = "Desc" };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        // Act
        var result = await service.Delete(task.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(context.Tasks);
    }

    [Fact]
    public async Task Update_Task_Should_Change_Data()
    {
        // Arrange
        var context = GetDbContext();

        var task = new TaskItem { Title = "Old", Description = "Old Desc" };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        var service = new TaskService(context);

        var updateDto = new TaskCreateDto
        {
            Title = "New Title",
            Description = "New Desc"
        };

        // Act
        var result = await service.Update(task.Id, updateDto);

        // Assert
        Assert.True(result);
        Assert.Equal("New Title", context.Tasks.First().Title);
    }


    [Fact]
    public async Task Delete_NonExistingTask_Should_ReturnFalse()
    {
        // Arrange
        var context = GetDbContext();
        var service = new TaskService(context);

        // Act
        var result = await service.Delete(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Update_NonExistingTask_Should_ReturnFalse()
    {
        // Arrange
        var context = GetDbContext();
        var service = new TaskService(context);

        var dto = new TaskCreateDto
        {
            Title = "New",
            Description = "New"
        };

        // Act
        var result = await service.Update(999, dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Create_Task_WithEmptyTitle_ShouldStillFailOrHandle()
    {
        // Arrange
        var context = GetDbContext();
        var service = new TaskService(context);

        var dto = new TaskCreateDto
        {
            Title = "",
            Description = "Test"
        };

        // Act
        var result = await service.Create(dto);

        // Assert (depends on your logic)
        Assert.NotNull(result); // or Assert.Throws if you enforce validation
    }

    //[Fact]
    //public async Task CreateTask_From_AI_Should_Add_Task()
    //{
    //    var fakeAi = new FakeAiService();
    //    var context = GetDbContext();
    //    var service = new TaskService(context);

    //    await service.CreateFromAi("study math");

    //    Assert.Single(context.Tasks);
    //}


}