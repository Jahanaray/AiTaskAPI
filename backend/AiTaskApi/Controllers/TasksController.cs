using Microsoft.AspNetCore.Mvc;
using AiTaskApi.Services;
using AiTaskApi.Shared.DTOs;
using AiTaskApi.Common;
using Microsoft.AspNetCore.Authorization;


namespace AiTaskApi.Controllers
{

    [Authorize]
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _service;

        public TasksController(TaskService service)
        {
            _service = service;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<IActionResult> GetAll(
    bool? isDone,
    string? title,
    int page = 1,
    int pageSize = 10)
        {
            var result = await _service.GetAll(isDone, title, page, pageSize);
            return Ok(ApiResponseHelper.Success(result));
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetById(id);

            if (result == null)
                return NotFound(ApiResponseHelper.Fail<TaskReadDto>("Task not found"));

            return Ok(ApiResponseHelper.Success(result));
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<IActionResult> Create(TaskCreateDto dto)
        {
            var result = await _service.Create(dto);
            return Ok(ApiResponseHelper.Success(result, "Task created"));
        }

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskCreateDto dto)
        {
            var updated = await _service.Update(id, dto);

            if (!updated)
                return NotFound(ApiResponseHelper.Fail<bool>("Task not found"));

            return Ok(ApiResponseHelper.Success(true, "Task updated"));
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.Delete(id);

            if (!deleted)
                return NotFound(ApiResponseHelper.Fail<bool>("Task not found"));

            return Ok(ApiResponseHelper.Success(true, "Task deleted"));
        }



    }
}