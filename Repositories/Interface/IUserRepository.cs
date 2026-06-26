using TaskManagementSystem.Models;
using TaskManagementSystem.DTO;
namespace TaskManagementSystem.Repositories.Interface
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User? GetUserById(int userId);
        UserWithTasks? GetTasksByUserId(int userId);
        User Add(User user);
        public bool GetUserByEmail(string userEmail);
    }
}
