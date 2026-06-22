class TaskService {
    constructor(baseUrl) {
        this.baseUrl = `${baseUrl}/task`;
    }

    // GET /api/task
    async getAllTasks()
     {
        try
        {
            const response = await fetch(this.baseUrl);
            
            if (!response.ok)
            {
                const errData = await response.json().catch(() => ({}));
                console.error(`[Task Service Error] Failed to retrieve tasks list. 
                               Status: ${response.status}.
                               Details: ${errData.message || 'No details provided.'}`);
                return null;
            }
            
            return await response.json();
        }
        catch (error)
        {
            console.error("[Network Error] Cannot connect to the server. Please check if the backend API is running.");
            return null;
        }
    }

    // GET /api/task/{taskId}
    async getTaskById(taskId)
     {
        try {
            const response = await fetch(`${this.baseUrl}/${taskId}`);
            
            if (!response.ok) {
                const errData = await response.json().catch(() => ({}));
                console.error(`[Task Service Error] Failed to retrieve task (ID: ${taskId}). Status: ${response.status}. Details: ${errData.message || 'Task not found.'}`);
                return null;
            }
            
            return await response.json();
        } catch (error) {
            console.error(`[Network Error] Cannot connect to the server to fetch task ID ${taskId}.`);
            return null;
        }
    }

    // GET /api/task/taskName?taskName=XYZ
    async getTaskByName(name) 
    {
        try 
        {
            const response = await fetch(`${this.baseUrl}/taskName?taskName=${encodeURIComponent(name)}`);
            
            if (!response.ok) {
                const errData = await response.json().catch(() => ({}));
                console.error(`[Task Service Error] Task search failed for "${name}". Status: ${response.status}. Details: ${errData.message || 'Lookup failed.'}`);
                return null;
            }
            
            return await response.json();
        } catch (error) {
            console.error(`[Network Error] Cannot connect to the server to look up task name "${name}".`);
            return null;
        }
    }

    // POST /api/task (multipart/form-data)
    async createTask(taskDto) 
    {
        try
        {
            const formData = new FormData();
            formData.append('Title', taskDto.title);
            formData.append('Description', taskDto.description);
            formData.append('AssignedUserId', taskDto.assignedUserId);

            const response = await fetch(this.baseUrl, {
                method: 'POST',
                body: formData 
            });

            if (!response.ok) {
                const errData = await response.json().catch(() => ({}));
                console.error(`[Task Service Error] Creation failed. Status: ${response.status}. Reason: ${errData.message || 'Invalid task data provided.'}`);
                return null;
            }

            return await response.json();
        }
        catch (error)
        {
            console.error("[Network Error] Cannot connect to the server to create a new task.");
            return null;
        }
    }

    // PUT /api/task/{taskId} (multipart/form-data)
    async updateTask(taskId, taskDto)
    {
        try
        {
            const formData = new FormData();
            formData.append('Title', taskDto.title);
            formData.append('Description', taskDto.description);
            formData.append('AssignedUserId', taskDto.assignedUserId);

            const response = await fetch(`${this.baseUrl}/${taskId}`, {
                method: 'PUT',
                body: formData
            });

            if (!response.ok) {
                const errData = await response.json().catch(() => ({}));
                console.error(`[Task Service Error] Update failed for task (ID: ${taskId}). Status: ${response.status}. Reason: ${errData.message || 'Could not update task data.'}`);
                return null;
            }

            return await response.json();
        }
        catch (error)
        {
            console.error(`[Network Error] Cannot connect to the server to update task ID ${taskId}.`);
            return null;
        }
    }

    // PATCH /api/task (multipart/form-data)
    async updateTaskStatus(taskId, newStatus)
    {
        try
        {
            const formData = new FormData();
            formData.append('TaskId', taskId);
            formData.append('Status', newStatus);

            const response = await fetch(this.baseUrl, {
                method: 'PATCH',
                body: formData
            });

            if (!response.ok) {
                const errData = await response.json().catch(() => ({}));
                console.error(`[Task Service Error] Status transition failed for task (ID: ${taskId}). Status: ${response.status}. Reason: ${errData.message || 'Could not update status.'}`);
                return null;
            }

            return await response.json();
        } 
        catch (error)
        {
            console.error(`[Network Error] Cannot connect to the server to update status for task ID ${taskId}.`);
            return null;
        }
    }

    // DELETE /api/task/{taskId}
    async deleteTask(taskId)
    {
        try
        {
            const response = await fetch(`${this.baseUrl}/${taskId}`, {
                method: 'DELETE'
            });

            if (!response.ok) 
            {
                const errData = await response.json().catch(() => ({}));
                console.error(`[Task Service Error] Deletion failed for task (ID: ${taskId}). Status: ${response.status}. Reason: ${errData.message || 'Could not delete task.'}`);
                return null;
            }

            return await response.json();
        } catch (error) {
            console.error(`[Network Error] Cannot connect to the server to delete task ID ${taskId}.`);
            return null;
        }
    }
}
