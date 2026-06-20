using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagementSystem.DTO;
using TaskManagementSystem.Repositories.Implementation;
using TaskManagementSystem.Repositories.Interface;
using TaskManagementSystem.Services.Implementation;
using TaskManagementSystem.Services.Interface;

namespace TaskManagementSystem.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;

        // Manual wiring with `new` so students can see how layers connect.
        // ASP.NET Core still supplies IConfiguration automatically.
        public TaskController(IConfiguration configuration)
        {
            ITaskRepository taskRepository = new TaskRepository(configuration);
            _taskService = new TaskService(taskRepository);
        }
        // GET /api/task — returns all tasks
        [HttpGet]
        public ActionResult<IEnumerable<UserResponseDto>> GetAllTasks()
        {
            var tasks = _taskService.GetAllTasks();
            return Ok(tasks);
        }
        // GET /api/task — returns all tasks
        [HttpGet("{taskId}")]
        public ActionResult<IEnumerable<TaskItemResponseDto>> GetTaskById(int taskId)
        {
            var task = _taskService.GetTaskById(taskId);
            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {taskId} not found." });
            }
            return Ok(task);
        }

        // GET /api/task/search/{taskName}
        [HttpGet("search/{taskName}")] // Changed route prefix to prevent ambiguitys
        public ActionResult<IEnumerable<TaskItemResponseDto>> GetTaskByName(string taskName)
        {
            var task = _taskService.GetTaskByName(taskName);
            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {taskName} not found." });
            }
            return Ok(task);
        }

        // POST /api/task — creates a new user.
        [HttpPost]
        [Consumes("multipart/form-data")] // Tells Swagger to render individual text boxes
        public ActionResult<TaskItemResponseDto> CreateTask([FromForm] CreateTaskItemDto dto)
        {
            if (!_taskService.CreateTask(dto, out var createdTask, out var errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            // Fixed: References the correct out variable 'createdTask' and routes to 'GetTaskById'
            return CreatedAtAction(nameof(GetTaskById), new { taskId = createdTask!.TaskId }, createdTask);

        }

        // PUT /api/task/{taskId}
        [HttpPut("{taskId:int}")] // Added route parameter
        [Consumes("multipart/form-data")] // Tells Swagger to render individual text boxes
        public ActionResult<TaskItemResponseDto> UpdateTask(int TaskId,[FromForm] UpdateTaskItemDto dto)
        {
            if (!_taskService.UpdateTask(TaskId,dto, out var updatedTask, out var errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            return CreatedAtAction(nameof(GetTaskById), new { taskId = updatedTask!.TaskId }, updatedTask);

        }


        // Patch /api/task — creates a new user.
        [HttpPatch]
        [Consumes("multipart/form-data")] // Tells Swagger to render individual text boxes
        public ActionResult<TaskItemResponseDto> UpdateTaskStatus([FromForm] ChangeStatusDto dto)
        {
            if (!_taskService.UpdateTaskStatus(dto.TaskId, dto, out var updatedTask, out var errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            return CreatedAtAction(nameof(GetTaskById), new { taskId = updatedTask!.TaskId }, updatedTask);

        }

        // Delete /api/task — creates a new user.
        [HttpDelete]
        [Consumes("multipart/form-data")] // Tells Swagger to render individual text boxes
        public ActionResult<TaskItemResponseDto> DeleteTask(int TaskId,[FromForm] TaskItemResponseDto dto)
        {
            if (!_taskService.DeleteTask(TaskId,out dto,  out string errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            return NoContent();



        }

    }
}
