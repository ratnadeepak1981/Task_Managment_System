class UserService {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    // GET /api/user
    async getAllUsers()
    {
        try {
            const response = await fetch(`${this.baseUrl}/user`);

            if (!response.ok)
            {
                const errData = await response.json().catch(() => ({}));
                console.error(`[User Service Error] Failed to retrieve users list. Status: ${response.status}. Details: ${errData.message || 'No additional details provided.'}`);
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

    // GET /api/user/{userId}
    async getUserById(userId) {
        try
        {
            const response = await fetch(`${this.baseUrl}/user/${userId}`);

            if (!response.ok) {
                const errData = await response.json().catch(() => ({}));
                console.error(`[User Service Error] Failed to retrieve user (ID: ${userId}). Status: ${response.status}. Details: ${errData.message || 'User not found.'}`);
                return null;
            }

            return await response.json();
        }
        catch (error)
        {
            console.error(`[Network Error] Cannot connect to the server to fetch user ID ${userId}.`);
            return null;
        }
    }

    // GET /api/user/{userId}/tasks
    async getTasksByUserId(userId) {
        try
        {
            const response = await fetch(`${this.baseUrl}/user/${userId}/tasks`);

            if (!response.ok)
            {
                const errData = await response.json().catch(() => ({}));
                console.error(`[User Service Error] Failed to retrieve tasks for user (ID: ${userId}). Status: ${response.status}. Details: ${errData.message || 'Tasks or user not found.'}`);
                return null;
            }

            return await response.json();
        }
        catch (error)
        {
            console.error(`[Network Error] Cannot connect to the server to fetch tasks for user ID ${userId}.`);
            return null;
        }
    }

    // POST /api/user (multipart/form-data)
    async createUser(createUserDto) {
        try
        {
            const formData = new FormData();

            Object.keys(createUserDto).forEach(key => {
                formData.append(key, createUserDto[key]);
            });

            const response = await fetch(`${this.baseUrl}/user`,
            {
                method: 'POST',
                body: formData
            });

            if (!response.ok)
            {
                const errData = await response.json().catch(() => ({}));
                console.error(`[User Service Error] Registration failed. Status: ${response.status}. Reason: ${errData.message || 'Invalid registration data provided.'}`);
                return null;
            }

            return await response.json();
        }
        catch (error)
        {
            console.error("[Network Error] Cannot connect to the server to create a new user.");
            return null;
        }
    }
}
