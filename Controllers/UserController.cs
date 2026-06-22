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
       
        // GET /api/user — returns all users
        [HttpGet]
        public ActionResult<ApiResponse<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            var users = _userService.GetAllUsers();

            if (users == null)
            {
                var validationErrors = new List<string>
                {
                        "No user exist ",
                        "user table empty"
                };
            }
            // Wrap the user list inside your generic ApiResponse wrapper
            var response = ApiResponse<IEnumerable<UserResponseDto>>.SuccessResponse("Users retrieved successfully.", users);

            return Ok(response);
            //return new JsonResult(response);
        }

        // GET /api/user — returns all tasks
        [HttpGet("{userId}")]
        public ActionResult<ApiResponse<IEnumerable<UserResponseDto>>>GetUserById(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                // Define the specific error list for your JSON format
                var validationErrors = new List<string>
                {
                        $"User with ID {userId} not found.",
                        "user not exist"
                };
                var errorResponse = ApiResponse<UserResponseDto>.FailureResponse($"User with ID {userId} not found.", validationErrors);
                return NotFound(errorResponse);
            }

            var response = ApiResponse<UserResponseDto>.SuccessResponse("User profile retrieved.",user);
            return Ok(response);
        }

        // GET /api/users/{userId}/tasks
        // FIXED: Route explicitly expanded to match required /api/users/{id}/tasks path
        [HttpGet("{userId:int}/tasks")]
        public ActionResult<ApiResponse<IEnumerable<UserWithTasksDto>>> GetTasksByUserId(int userId)
        {
            var userWithTasks = _userService.GetTasksByUserId(userId);
            if (userWithTasks == null)
            {
                // Define the specific error list for your JSON format
                var validationErrors = new List<string>
                {
                        $"User {userId} not found.",
                        "user not exist"
                };
                var errorResponse = ApiResponse<UserWithTasksDto>.FailureResponse($"User with ID {userId} not found.",validationErrors);
                return NotFound(errorResponse);
            }

            var response = ApiResponse<UserWithTasksDto>.SuccessResponse("User tasks retrieved.", userWithTasks);
            return Ok(response);
        }

        // POST /api/user — creates a new user.
        [HttpPost]
        [Consumes("multipart/form-data")] // Tells Swagger to render individual text boxes
        public ActionResult<ApiResponse<UserResponseDto>> CreateUser([FromForm] CreateUserDto dto)
        {
            if (!_userService.CreateUser(dto, out var user, out var errorMessage))
            {
                var errorResponse = ApiResponse<UserResponseDto>.FailureResponse(errorMessage ?? "Failed to create user.");
                return BadRequest(errorResponse);
            }

            var response = ApiResponse<UserResponseDto>.SuccessResponse("User created successfully.", user!);
            return Ok(response);
        }
    }
}
