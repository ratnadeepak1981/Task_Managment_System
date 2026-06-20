using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DTO;
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
            return Ok(users);
        }

        // GET /api/user — returns all tasks
        [HttpGet("{userId}")]
        public ActionResult<IEnumerable<UserResponseDto>> GetUserById(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {userId} not found." });
            }
            return Ok(user);
        }

        // GET /api/users/{userId}/tasks
        // FIXED: Route explicitly expanded to match required /api/users/{id}/tasks path
        [HttpGet("{userId:int}/tasks")]
        public ActionResult<IEnumerable<UserWithTasksDto>> GetTasksByUserId(int userId)
        {
            var user = _userService.GetTasksByUserId(userId);
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {userId} not found." });
            }
            return Ok(user);
        }

        // POST /api/user — creates a new user.
        [HttpPost]
        [Consumes("multipart/form-data")] // Tells Swagger to render individual text boxes
        public ActionResult<UserResponseDto> CreateUser([FromForm] CreateUserDto dto)
        {
            if (!_userService.CreateUser(dto, out var user, out var errorMessage))
            {
                return BadRequest(new { message = errorMessage });
            }

            return CreatedAtAction(nameof(GetAllUsers), new { id = user!.UserId }, user);

            // FIXED: Changed GetAllUsers to GetUserById, and matched the parameter name 'userId'
            //return CreatedAtAction(nameof(GetUserById), new { userId = user!.UserId }, user);
        }
    }
}
