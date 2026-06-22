using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TaskManagementSystem.DTO.Request;
using TaskManagementSystem.DTO.Response;
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
            IUserRepository userRepository = new UserRepository(configuration);
            _taskService = new TaskService(taskRepository,userRepository);
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
        [HttpGet("taskName")] // Changed route prefix to prevent ambiguitys
        public ActionResult<IEnumerable<TaskItemResponseDto>> GetTaskByName([FromQuery] string ? taskName=null)
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
        [HttpPut("{taskId:int}")] // updates an entire task resource
        [Consumes("multipart/form-data")] // Tells Swagger to render individual text boxes
        public ActionResult<TaskItemResponseDto> UpdateTask(int taskId,[FromForm] UpdateTaskItemDto dto)
        {
            if (!_taskService.UpdateTask(taskId,dto, out var updatedTask, out var errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

               return Ok(updatedTask); 

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
        
        // 1. Fixed Route Path: Added "{taskId:int}" route parameter to match standard DELETE endpoints
        [HttpDelete("{taskId:int}")]
        public ActionResult<TaskItemResponseDto> DeleteTask(int taskId)
        {
            // 2. Clear Variable Passing: Let the 'out' parameter initialize 'deletedTask' directly
            if (!_taskService.DeleteTask(taskId, out var deletedTask, out var errorMessage))
            {
                // 3. Conditional Error Handling: Return 404 if not found, 400 for bad input
                if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(new { message = errorMessage });
                }
                return BadRequest(new { message = errorMessage });
            }

            // 4. Exact Return Matching: Successfully returns HTTP 200 OK along with the deleted object data
            return Ok(deletedTask);
        }

    }
}
