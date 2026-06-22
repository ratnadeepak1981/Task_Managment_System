// Composition root linking runtime parameters
const URL = "http://localhost:5108/api"; // Adjust your ASP.NET Core port here

const taskService = new TaskService(URL);
const userService = new UserService(URL);

// Exposed globally to enable programmatic HTML inline execution 'onclick="eventManager..."'
window.eventManager = new TaskEventManager(taskService);

// Start App
document.addEventListener('DOMContentLoaded', () => 
{
    window.eventManager.setupEventListeners();
    window.eventManager.loadAllTasks();
});
