using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;
using System.Threading.Tasks;
using TaskManagementSystem.DTO.Request;
using TaskManagementSystem.DTO.Response;
using TaskManagementSystem.Repositories.Implementation;
using TaskManagementSystem.Repositories.Interface;
using TaskManagementSystem.Services.Implementation;
using TaskManagementSystem.Services.Interface;
using static Microsoft.Data.SqlClient.Internal.SqlClientEventSource;

namespace TaskManagementSystem.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
    
        public TaskController(ITaskService taskService)
        {
           _taskService = taskService;
        }
        // GET /api/task — returns all tasks
        [HttpGet]
        public ActionResult<ApiResponse<IEnumerable<TaskItemResponseDto>>> GetAllTasks()
        {

            var tasks = _taskService.GetAllTasks();

            if (tasks == null)
            {
                var validationErrors = new List<string>
                {
                        "No tasks exist ",
                        "task table empty"
                };
                var errorResponse = ApiResponse<IEnumerable<TaskItemResponseDto>>.FailureResponse("Retrieval failed.", validationErrors);
                return NotFound(errorResponse);
            }
            // Wrap the user list inside your generic ApiResponse wrapper
            var response = ApiResponse<IEnumerable<TaskItemResponseDto>>.SuccessResponse("Tasks retrieved successfully.", tasks);

            return Ok(response);
        }
        // GET /api/task — returns all tasks
        [HttpGet("{taskId}")]
        public ActionResult<ApiResponse<IEnumerable<TaskItemResponseDto>>> GetTaskById(int taskId)
        {
            var task = _taskService.GetTaskById(taskId);
            if (task == null)
            {
                var validationErrors = new List<string>
                {
                        "The requested task ID does not exist.",
                        "Please check the ID and try again."
                };
                var errorResponse = ApiResponse<IEnumerable<TaskItemResponseDto>>.FailureResponse($"Task with Id {taskId} not found.", validationErrors);
                return NotFound(errorResponse);
              
            }
            // FIX: Wrapped single task instead of IEnumerable
            var response = ApiResponse<TaskItemResponseDto>.SuccessResponse("Task retrieved successfully.", task);
            return Ok(response);
        }

        // GET /api/task/search/{taskName}
        [HttpGet("taskName")] // Changed route prefix to prevent ambiguitys
        public ActionResult<ApiResponse<IEnumerable<TaskItemResponseDto>>> GetTaskByName([FromQuery] string ? taskName=null)
        {
            var task = _taskService.GetTaskByName(taskName);
            if (task == null)
            {
                var validationErrors = new List<string>
                {
                        "Task name matching not found ",
                        "Enter correct Task name pattern"
                };
                var errorResponse = ApiResponse<IEnumerable<TaskItemResponseDto>>.FailureResponse($"Task with Name {taskName} not found.", validationErrors);
                return NotFound(errorResponse);
               
            }
            // Wrap the user list inside your generic ApiResponse wrapper
            var response = ApiResponse<IEnumerable<TaskItemResponseDto>>.SuccessResponse("Tasks retrieved successfully.", task);
            return Ok(response);
        }

       

        // POST /api/task — creates a new user.
        [HttpPost]
        [Consumes("multipart/form-data")] // Tells Swagger to render individual text boxes
        public ActionResult<ApiResponse<TaskItemResponseDto>> CreateTask([FromForm] CreateTaskItemDto dto)
        {
            if (!_taskService.CreateTask(dto, out var createdTask, out var errorMessage))
            {
                var errorResponse = ApiResponse<TaskItemResponseDto>.FailureResponse(errorMessage ?? "Failed to create task.");
                return BadRequest(errorResponse);
            }

            var response = ApiResponse<TaskItemResponseDto>.SuccessResponse("Task created successfully.", createdTask!);
            return Ok(response);

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
            if (!_taskService.UpdateTaskStatus(dto.TaskId,dto, out var changedTask, out var errorMessage))
            {
                var errorResponse = ApiResponse<TaskItemResponseDto>.FailureResponse(errorMessage ?? "Failed to change status of task.");
                return BadRequest(errorResponse);
            }

            var response = ApiResponse<TaskItemResponseDto>.SuccessResponse("Task status changed successfully.", changedTask!);
            return Ok(response);

        }
        
        // 1. Fixed Route Path: Added "{taskId:int}" route parameter to match standard DELETE endpoints
        [HttpDelete("{taskId:int}")]
        public ActionResult<TaskItemResponseDto> DeleteTask(int taskId)
        {
            if (!_taskService.DeleteTask(taskId, out var deletedTask, out var errorMessage))
            {
                var errorResponse = ApiResponse<TaskItemResponseDto>.FailureResponse(errorMessage ?? "Failed to delete task.");
                return BadRequest(errorResponse);
            }

            var response = ApiResponse<TaskItemResponseDto>.SuccessResponse("Task deleted successfully.", deletedTask!);
            return Ok(response);
                       
        }

    }
}
