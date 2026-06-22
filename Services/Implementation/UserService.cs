using TaskManagementSystem.Repositories.Interface;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services.Interface;
using TaskManagementSystem.DTO.Request;
using TaskManagementSystem.DTO.Response;
namespace TaskManagementSystem.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IEnumerable<UserResponseDto> GetAllUsers()
        {
            return _userRepository.GetAll().Select(MapToResponseDto);
        }
        // FIXED: Changed from IEnumerable to a single nullable object
        public UserResponseDto? GetUserById(int userId)
        {
            var user = _userRepository.GetUserById(userId);

            if (user == null)
            {
                return null;
            }

            return MapToResponseDto(user);
        }

        public UserWithTasksDto? GetTasksByUserId(int userId)
        {
            var user = _userRepository.GetTasksByUserId(userId);

            if (user == null)
            {
                return null;
            }
            //return user;
            return MapToResponseDto(user);
        }

        public bool CreateUser(CreateUserDto dto, out UserResponseDto? user, out string errorMessage)
        {
            user = null;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(dto.UserName))
            {
                errorMessage = "UserName is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.UserEmail))
            {
                errorMessage = "UserEmail is required.";
                return false;
            }

            var newUser = new User
            {
                UserName = dto.UserName.Trim(),
                UserEmail = dto.UserEmail.Trim(),
            };

            var createdUser = _userRepository.Add(newUser);
            user = MapToResponseDto(createdUser);
            return true;
        }

        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                userEmail = user.UserEmail
            };
        }

        // FUNCTION 1: Handles the outer User details and main payload container
        private static UserWithTasksDto MapToResponseDto(UserWithTasks userWithTasks)
        {
            return new UserWithTasksDto
            {
                UserId = userWithTasks.UserProfile.UserId,
                UserName = userWithTasks.UserProfile.UserName,
                Email = userWithTasks.UserProfile.UserEmail,

                // FIXED: Changed TaskItemResponseDto to MapToTaskItemResponseDto method reference
                Tasks = userWithTasks.Tasks.Select(MapToTaskItemResponseDto).ToList()
            };
        }

        // Split Mapper 2: Maps individual TaskItem domain objects to TaskItemResponseDto
        private static TaskItemResponseDto MapToTaskItemResponseDto(TaskItem task)
        {
            return new TaskItemResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                CreatedDate = task.CreatedDate,
                UserId= task.UserId,
                UserName= task.UserName

            };
        }
    }
}
