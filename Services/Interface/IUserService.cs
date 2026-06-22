using TaskManagementSystem.DTO.Request;
using TaskManagementSystem.DTO.Response;
using TaskManagementSystem.Models;
namespace TaskManagementSystem.Services.Interface
{
     public interface IUserService
    {
        // 1. Lightweight list for overview pages
        IEnumerable<UserResponseDto> GetAllUsers();

        UserResponseDto? GetUserById(int userId);
        // 2. Heavy, deep fetch for a specific user's detail panel
        // UserWithTasksDto? GetUserByIdWithTasks(int userId);

        UserWithTasksDto? GetTasksByUserId(int userId);

        // 3. Creation usually returns the basic profile
        bool CreateUser(CreateUserDto dto, out UserResponseDto? user, out string errorMessage);
    }
}
