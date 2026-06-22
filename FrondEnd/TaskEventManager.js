class TaskEventManager {
    constructor(taskService) {
        this.taskService = taskService;
        this.isEditing = false;
        this.initDomElements();
    }

    initDomElements() {
        this.taskForm = document.getElementById('taskForm');
        this.taskIdInput = document.getElementById('taskId');
        this.titleInput = document.getElementById('title');
        this.descriptionInput = document.getElementById('description');
        this.userInput = document.getElementById('assignedUserId');
        this.taskListContainer = document.getElementById('taskList');
        this.formTitle = document.getElementById('formTitle');
        this.btnCancelEdit = document.getElementById('btnCancelEdit');
        
        this.searchNameInput = document.getElementById('searchName');
        this.btnSearch = document.getElementById('btnSearch');
        this.btnClearSearch = document.getElementById('btnClearSearch');
    }

    setupEventListeners() {
        // Form Submission (Create / Update)
        this.taskForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            await this.handleFormSubmit();
        });

        // Cancel Edit Mode
        this.btnCancelEdit.addEventListener('click', () => this.resetForm());

        // Search Features
        this.btnSearch.addEventListener('click', async () => await this.handleSearch());
        this.btnClearSearch.addEventListener('click', async () => {
            this.searchNameInput.value = '';
            await this.loadAllTasks();
        });
    }

    async loadAllTasks() {
        try {
            const tasks = await this.taskService.getAllTasks();
            this.renderTasks(tasks);
        } catch (error) {
            alert(error.message);
        }
    }

    renderTasks(tasks) {
        this.taskListContainer.innerHTML = '';
        if(!tasks || tasks.length === 0) {
            this.taskListContainer.innerHTML = '<p>No tasks found.</p>';
            return;
        }

        // Standard handles iteration regardless if the backend sent a single object or list
        const taskArray = Array.isArray(tasks) ? tasks : [tasks];

        taskArray.forEach(task => {
            const div = document.createElement('div');
            div.className = 'task-item';
            div.innerHTML = `
                <h3>${task.title || 'No Title'}</h3>
                <p>${task.description || ''}</p>
                <small>ID: ${task.taskId} | Status: <strong>${task.status || 'Pending'}</strong></small>
                <div class="task-actions">
                    <button onclick="eventManager.editTaskMode(${task.taskId}, '${task.title}', '${task.description}', ${task.assignedUserId})">Edit</button>
                    <button class="btn-patch" onclick="eventManager.toggleStatus(${task.taskId}, '${task.status}')">Toggle Status</button>
                    <button class="btn-delete" onclick="eventManager.handleDeleteTask(${task.taskId})">Delete</button>
                </div>
            `;
            this.taskListContainer.appendChild(div);
        });
    }

    async handleFormSubmit() {
        const taskDto = {
            title: this.titleInput.value,
            description: this.descriptionInput.value,
            assignedUserId: this.userInput.value
        };

        try {
            if (this.isEditing) {
                const id = this.taskIdInput.value;
                await this.taskService.updateTask(id, taskDto);
            } else {
                await this.taskService.createTask(taskDto);
            }
            this.resetForm();
            await this.loadAllTasks();
        } catch (error) {
            alert(error.message);
        }
    }

    editTaskMode(id, title, description, userId) {
        this.isEditing = true;
        this.formTitle.innerText = "Edit Task";
        this.taskIdInput.value = id;
        this.titleInput.value = title;
        this.descriptionInput.value = description;
        this.userInput.value = userId;
        this.btnCancelEdit.style.display = 'inline-block';
    }

    async toggleStatus(id, currentStatus) {
        const nextStatus = currentStatus === 'Completed' ? 'Pending' : 'Completed';
        try {
            await this.taskService.updateTaskStatus(id, nextStatus);
            await this.loadAllTasks();
        } catch (error) {
            alert(error.message);
        }
    }

    async handleDeleteTask(id) {
        if (!confirm('Are you sure you want to delete this task?')) return;
        try {
            await this.taskService.deleteTask(id);
            await this.loadAllTasks();
        } catch (error) {
            alert(error.message);
        }
    }

    async handleSearch() {
        const query = this.searchNameInput.value.trim();
        if (!query) return;
        try {
            const results = await this.taskService.getTaskByName(query);
            this.renderTasks(results);
        } catch (error) {
            this.taskListContainer.innerHTML = `<p style="color:red;">${error.message}</p>`;
        }
    }

    resetForm() 
    {
        this.isEditing = false;
        this.formTitle.innerText = "Create Task";
        this.taskForm.reset();
        this.taskIdInput.value = '';
        this.btnCancelEdit.style.display = 'none';
    }
}
