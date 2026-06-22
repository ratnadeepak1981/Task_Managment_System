using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DTO.Request;
using TaskManagementSystem.DTO.Response;
using TaskManagementSystem.Models;
using TaskManagementSystem.Repositories.Implementation;
using TaskManagementSystem.Repositories.Interface;
using TaskManagementSystem.Services.Implementation;
using TaskManagementSystem.Services.Interface;
namespace TaskManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        // Manual wiring with `new` so students can see how layers connect.
        // ASP.NET Core still supplies IConfiguration automatically.
        public UserController(IConfiguration configuration)
        {
            IUserRepository userRepository = new UserRepository(configuration);
            _userService = new UserService(userRepository);
        }

        // GET /api/user — returns all books with category names.
        [HttpGet]
        public ActionResult<IEnumerable<UserResponseDto>> GetAllUsers()
        {
            var users = _userService.GetAllUsers();
            var response = ApiResponse<IEnumerable<UserResponseDto>>.SuccessResponse(users, "Users retrieved successfully.");
            return Ok(response);
        }

        // GET /api/user — returns all tasks
        [HttpGet("{userId}")]
        public ActionResult<IEnumerable<UserResponseDto>> GetUserById(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                var errorResponse = ApiResponse<UserResponseDto>.FailureResponse($"User with ID {userId} not found.");
                return NotFound(errorResponse);
            }

            var response = ApiResponse<UserResponseDto>.SuccessResponse(user, "User profile retrieved.");
            return Ok(response);
        }

        // GET /api/users/{userId}/tasks
        // FIXED: Route explicitly expanded to match required /api/users/{id}/tasks path
        [HttpGet("{userId:int}/tasks")]
        public ActionResult<IEnumerable<UserWithTasksDto>> GetTasksByUserId(int userId)
        {
            var userWithTasks = _userService.GetTasksByUserId(userId);
            if (userWithTasks == null)
            {
                var errorResponse = ApiResponse<UserWithTasksDto>.FailureResponse($"User with ID {userId} not found.");
                return NotFound(errorResponse);
            }

            var response = ApiResponse<UserWithTasksDto>.SuccessResponse(userWithTasks, "User tasks retrieved.");
            return Ok(response);
        }

        // POST /api/user — creates a new user.
        [HttpPost]
        [Consumes("multipart/form-data")] // Tells Swagger to render individual text boxes
        public ActionResult<UserResponseDto> CreateUser([FromForm] CreateUserDto dto)
        {
            if (!_userService.CreateUser(dto, out var user, out var errorMessage))
            {
                var errorResponse = ApiResponse<UserResponseDto>.FailureResponse(errorMessage ?? "Failed to create user.");
                return BadRequest(errorResponse);
            }

            var response = ApiResponse<UserResponseDto>.SuccessResponse(user!, "User created successfully.");
            return Ok(response);
        }
    }
}
