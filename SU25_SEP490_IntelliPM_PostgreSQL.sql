-- -- Tạo database SU25_SEP490_IntelliPM
-- CREATE DATABASE SU25_SEP490_IntelliPM;

-- -- Sử dụng database SU25_SEP490_IntelliPM
-- \connect SU25_SEP490_IntelliPM;


-- 1. account
CREATE TABLE account (
    id SERIAL PRIMARY KEY,
    username VARCHAR(255) NOT NULL UNIQUE,
    full_name VARCHAR(255) NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    role VARCHAR(50) NULL,
    position VARCHAR(50) NULL,
    phone VARCHAR(50) NULL,
    gender VARCHAR(20) NULL,
    google_id VARCHAR(255) NULL UNIQUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    address TEXT NULL
);
-- Thêm cột picture vào bảng account, cho phép NULL
ALTER TABLE account
ADD COLUMN picture VARCHAR(255) NULL;


-- Create the RefreshToken table
CREATE TABLE refresh_token (
    refresh_token_id SERIAL PRIMARY KEY,
    expired_at TIMESTAMP NOT NULL,
    token VARCHAR(255) NOT NULL,
    account_id INTEGER NOT NULL,
    CONSTRAINT fk_refresh_token_account FOREIGN KEY (account_id) REFERENCES account(id) ON DELETE CASCADE
);


-- 2. project
CREATE TABLE project (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    budget DECIMAL(15, 2) NULL,
    project_type VARCHAR(50) NOT NULL,
    created_by INT NOT NULL,
    start_date TIMESTAMP NULL,
    end_date TIMESTAMP NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (created_by) REFERENCES account(id)
);

-- 3. epic
CREATE TABLE epic (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    start_date TIMESTAMP NULL,
    end_date TIMESTAMP NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 4. sprint
CREATE TABLE sprint (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    goal TEXT NULL,
    start_date TIMESTAMP NULL,
    end_date TIMESTAMP NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 5. milestone
CREATE TABLE milestone (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    start_date TIMESTAMP NULL,
    end_date TIMESTAMP NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 6. task
CREATE TABLE tasks (
    id SERIAL PRIMARY KEY,
    reporter_id INT NOT NULL,
    project_id INT NOT NULL,
    epic_id INT NULL, -- Allow NULL for Agile/Waterfall flexibility
    sprint_id INT NULL, -- Allow NULL for Agile/Waterfall flexibility
    milestone_id INT NULL, -- Allow NULL for Agile/Waterfall flexibility
    type VARCHAR(50) NULL,
    manual_input BOOLEAN NOT NULL DEFAULT FALSE,
    generation_ai_input BOOLEAN NOT NULL DEFAULT FALSE,
    title VARCHAR(255) NOT NULL,
    description TEXT NULL,
    planned_start_date TIMESTAMP NULL,
    planned_end_date TIMESTAMP NULL,
    duration VARCHAR(100) NULL,
    actual_start_date TIMESTAMP NULL,
    actual_end_date TIMESTAMP NULL,
    percent_complete DECIMAL(5, 2) NULL,
    planned_hours DECIMAL(8, 2) NULL,
    actual_hours DECIMAL(8, 2) NULL,
    planned_cost DECIMAL(15, 2) NULL,
    planned_resource_cost DECIMAL(15, 2) NULL,
    actual_cost DECIMAL(15, 2) NULL,
    actual_resource_cost DECIMAL(15, 2) NULL,
    remaining_hours DECIMAL(8, 2) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    priority VARCHAR(50) NULL,
    evaluate VARCHAR(50) NULL,
    status VARCHAR(50) NULL,
    FOREIGN KEY (reporter_id) REFERENCES account(id),
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (epic_id) REFERENCES epic(id),
    FOREIGN KEY (sprint_id) REFERENCES sprint(id),
    FOREIGN KEY (milestone_id) REFERENCES milestone(id)
);

-- 7. task_assignment
CREATE TABLE task_assignment (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    account_id INT NOT NULL,
    status VARCHAR(50) NULL,
    assigned_at TIMESTAMP NULL,
    completed_at TIMESTAMP NULL,
    hourly_rate DECIMAL(10, 2) NULL,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 8. task_check_list
CREATE TABLE task_check_list (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    status VARCHAR(50) NULL,
    manual_input BOOLEAN NOT NULL DEFAULT FALSE,
    generation_ai_input BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 9. task_comment
CREATE TABLE task_comment (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    user_id INT NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (user_id) REFERENCES account(id)
);

-- 10. task_dependency
CREATE TABLE task_dependency (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    linked_from INT NOT NULL,
    linked_to INT NOT NULL,
    type VARCHAR(50) NULL,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (linked_from) REFERENCES tasks(id),
    FOREIGN KEY (linked_to) REFERENCES tasks(id)
);

-- 11. task_file
CREATE TABLE task_file (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    url_file VARCHAR(1024) NOT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 12. task_status_log
CREATE TABLE task_status_log (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    status VARCHAR(50) NOT NULL,
    changed_by INT NOT NULL,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (changed_by) REFERENCES account(id)
);

-- 13. document
CREATE TABLE document (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    task_id INT NULL,
    title VARCHAR(255) NOT NULL,
    type VARCHAR(100) NULL,
    template TEXT NULL,
    content TEXT NULL,
    file_url VARCHAR(1024) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_by INT NOT NULL,
    updated_by INT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (created_by) REFERENCES account(id),
    FOREIGN KEY (updated_by) REFERENCES account(id)
);

-- 14. document_permission
CREATE TABLE document_permission (
    id SERIAL PRIMARY KEY,
    document_id INT NOT NULL,
    account_id INT NOT NULL,
    permission_type VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (document_id) REFERENCES document(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 15. project_member
CREATE TABLE project_member (
    id SERIAL PRIMARY KEY,
    account_id INT NOT NULL,
    project_id INT NOT NULL,
    joined_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (account_id) REFERENCES account(id),
    FOREIGN KEY (project_id) REFERENCES project(id),
    UNIQUE (account_id, project_id)
);

-- 16. project_position
CREATE TABLE project_position (
    id SERIAL PRIMARY KEY,
    project_member_id INT NOT NULL,
    position VARCHAR(100) NOT NULL,
    assigned_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_member_id) REFERENCES project_member(id)
);

-- 17. notification
CREATE TABLE notification (
    id SERIAL PRIMARY KEY,
    created_by INT NOT NULL,
    type VARCHAR(100) NOT NULL,
    priority VARCHAR(50) NOT NULL,
    message TEXT NOT NULL,
    related_entity_type VARCHAR(100) NULL,
    related_entity_id INT NULL,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by) REFERENCES account(id)
);

-- 18. recipient_notification
CREATE TABLE recipient_notification (
    id SERIAL PRIMARY KEY,
    account_id INT NOT NULL,
    notification_id INT NOT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (account_id) REFERENCES account(id),
    FOREIGN KEY (notification_id) REFERENCES notification(id)
);

-- 19. meeting
CREATE TABLE meeting (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    meeting_topic VARCHAR(255) NOT NULL,
    meeting_date TIMESTAMP NOT NULL,
    meeting_url VARCHAR(1024) NULL,
    status VARCHAR(50) NULL,
    start_time TIMESTAMP NULL,
    end_time TIMESTAMP NULL,
    attendees INT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 20. meeting_document (1:1 with meeting)
CREATE TABLE meeting_document (
    meeting_id INT PRIMARY KEY, -- Primary key to enforce 1:1
    title VARCHAR(255) NOT NULL,
    description TEXT NULL,
    file_url VARCHAR(1024) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    account_id INT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 21. meeting_log
CREATE TABLE meeting_log (
    id SERIAL PRIMARY KEY,
    meeting_id INT NOT NULL,
    account_id INT NOT NULL,
    action TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 22. meeting_participant
CREATE TABLE meeting_participant (
    id SERIAL PRIMARY KEY,
    meeting_id INT NOT NULL,
    account_id INT NOT NULL,
    role VARCHAR(100) NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (account_id) REFERENCES account(id),
    UNIQUE (meeting_id, account_id)
);

-- 23. meeting_transcript (1:1 with meeting)
CREATE TABLE meeting_transcript (
    meeting_id INT PRIMARY KEY, -- Primary key to enforce 1:1
    transcript_text TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id)
);

-- 24. meeting_summary (1:1 with meeting_transcript)
CREATE TABLE meeting_summary (
    meeting_transcript_id INT PRIMARY KEY, -- Primary key to enforce 1:1
    summary_text TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_transcript_id) REFERENCES meeting_transcript(meeting_id)
);

-- 25. milestone_feedback
CREATE TABLE milestone_feedback (
    id SERIAL PRIMARY KEY,
    meeting_id INT NOT NULL,
    account_id INT NOT NULL,
    feedback_text TEXT NOT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 26. risk (1:1 with account through responsible_id)
CREATE TABLE risk (
    id SERIAL PRIMARY KEY,
    responsible_id INT NOT NULL,
    project_id INT NOT NULL,
    task_id INT NULL,
    risk_scope VARCHAR(255) NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT NULL,
    status VARCHAR(50) NULL,
    type VARCHAR(100) NULL,
    generated_by VARCHAR(100) NULL,
    probability VARCHAR(50) NULL,
    impact_level VARCHAR(50) NULL,
    severity_level VARCHAR(50) NULL,
    is_approved BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (responsible_id) REFERENCES account(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 27. risk_solution
CREATE TABLE risk_solution (
    id SERIAL PRIMARY KEY,
    risk_id INT NOT NULL,
    mitigation_plan TEXT NULL,
    contingency_plan TEXT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (risk_id) REFERENCES risk(id)
);

-- 28. change_request
CREATE TABLE change_request (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    requested_by INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (requested_by) REFERENCES account(id)
);

-- 29. project_recommendation
CREATE TABLE project_recommendation (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    task_id INT NOT NULL,
    type VARCHAR(100) NOT NULL,
    recommendation TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 30. label
CREATE TABLE label (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    color_code VARCHAR(7) NULL,
    description TEXT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 31. task_label
CREATE TABLE task_label (
    id SERIAL PRIMARY KEY,
    label_id INT NOT NULL,
    task_id INT NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (label_id) REFERENCES label(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    UNIQUE (label_id, task_id)
);

-- 32. requirement
CREATE TABLE requirement (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    type VARCHAR(100) NULL,
    description TEXT NULL,
    priority VARCHAR(50) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 33. project_metric
CREATE TABLE project_metric (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    calculated_by VARCHAR(50) NOT NULL,
    is_approved BOOLEAN NOT NULL DEFAULT FALSE,
    planned_value DECIMAL(15, 2) NULL,
    earned_value DECIMAL(15, 2) NULL,
    actual_cost DECIMAL(15, 2) NULL,
    spi DECIMAL(15, 2) NULL,
    cpi DECIMAL(15, 2) NULL,
    delay_days INT NULL,
    budget_overrun DECIMAL(15, 2) NULL,
    projected_finish_date TIMESTAMP NULL,
    projected_total_cost DECIMAL(15, 2) NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 34. system_configuration
CREATE TABLE system_configuration (
    id SERIAL PRIMARY KEY,
    config_key VARCHAR(255) NOT NULL UNIQUE,
    value_config TEXT NULL,
    min_value VARCHAR(255) NULL,
    max_value VARCHAR(255) NULL,
    estimate_value VARCHAR(255) NULL,
    description TEXT NULL,
    note TEXT NULL,
    effected_from TIMESTAMP NULL,
    effected_to TIMESTAMP NULL
);

-- 35. dynamic_category
CREATE TABLE dynamic_category (
    id SERIAL PRIMARY KEY,
    category_group VARCHAR(100) NOT NULL,
    code VARCHAR(100) NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    order_index INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (category_group, code)
);








------------------------------------------------------------------------


-- Insert dữ liệu mẫu vào bảng account
INSERT INTO account (username, full_name, email, password, role, position, phone, gender, google_id, status, address)
VALUES 
    ('user1', 'Nguyen Van A', 'user1@example.com', 'pass123', 'Admin', 'Manager', '0901234567', 'Male', 'google1', 'Active', 'Ha Noi'),
    ('user2', 'Tran Thi B', 'user2@example.com', 'pass456', 'User', 'Developer', '0907654321', 'Female', 'google2', 'Active', 'Ho Chi Minh'),
    ('user3', 'Le Van C', 'user3@example.com', 'pass789', 'User', 'Tester', '0912345678', 'Male', NULL, 'Inactive', 'Da Nang'),
    ('user4', 'Pham Thi D', 'user4@example.com', 'pass101', 'User', 'Designer', '0923456789', 'Female', 'google4', 'Active', 'Can Tho'),
    ('user5', 'Hoang Van E', 'user5@example.com', 'pass112', 'Admin', 'Project Lead', '0934567890', 'Male', 'google5', 'Active', 'Hai Phong');

	-- Insert sample data into RefreshToken table
INSERT INTO refresh_token (expired_at, token, account_id)
VALUES 
    ('2025-06-01 00:00:00', 'token_1_abc123', 1),
    ('2025-06-02 00:00:00', 'token_2_def456', 2),
    ('2025-06-03 00:00:00', 'token_3_ghi789', 3),
    ('2025-06-04 00:00:00', 'token_4_jkl012', 4),
    ('2025-06-05 00:00:00', 'token_5_mno345', 5);


-- Insert dữ liệu mẫu vào bảng project
INSERT INTO project (name, description, budget, project_type, created_by, start_date, end_date, status)
VALUES 
    ('Project A', 'Development project', 1000000.00, 'Software', 1, '2025-06-01', '2025-12-01', 'In Progress'),
    ('Project B', 'Marketing campaign', 500000.00, 'Marketing', 2, '2025-07-01', '2025-09-01', 'Planning'),
    ('Project C', 'Research project', 750000.00, 'Research', 3, '2025-08-01', '2025-11-01', 'On Hold'),
    ('Project D', 'UI/UX Design', 300000.00, 'Design', 4, '2025-09-01', '2025-10-01', 'Completed'),
    ('Project E', 'Testing project', 400000.00, 'Testing', 5, '2025-10-01', '2025-12-01', 'In Progress');

-- Insert dữ liệu mẫu vào bảng epic
INSERT INTO epic (project_id, name, description, start_date, end_date, status)
VALUES 
    (1, 'Epic 1', 'Core features', '2025-06-01', '2025-08-01', 'In Progress'),
    (1, 'Epic 2', 'Additional features', '2025-08-01', '2025-10-01', 'Planning'),
    (2, 'Epic 3', 'Campaign setup', '2025-07-01', '2025-08-01', 'Completed'),
    (3, 'Epic 4', 'Research phase', '2025-08-01', '2025-09-01', 'On Hold'),
    (4, 'Epic 5', 'Design phase', '2025-09-01', '2025-09-15', 'Completed'),
    (5, 'Epic 6', 'Testing phase', '2025-10-01', '2025-11-01', 'In Progress'); -- Thêm epic cho project_id 5

-- Insert dữ liệu mẫu vào bảng sprint
INSERT INTO sprint (project_id, name, goal, start_date, end_date, status)
VALUES 
    (1, 'Sprint 1', 'Implement login', '2025-06-01', '2025-06-15', 'Completed'),
    (1, 'Sprint 2', 'Add dashboard', '2025-06-16', '2025-06-30', 'In Progress'),
    (2, 'Sprint 3', 'Launch campaign', '2025-07-01', '2025-07-15', 'Planning'),
    (3, 'Sprint 4', 'Initial research', '2025-08-01', '2025-08-15', 'On Hold'),
    (4, 'Sprint 5', 'Design UI', '2025-09-01', '2025-09-10', 'Completed'),
    (5, 'Sprint 6', 'Test automation', '2025-10-01', '2025-10-15', 'In Progress'); -- Thêm sprint cho project_id 5

-- Insert dữ liệu mẫu vào bảng milestone
INSERT INTO milestone (project_id, name, description, start_date, end_date, status)
VALUES 
    (1, 'Milestone 1', 'Beta release', '2025-07-01', '2025-07-15', 'In Progress'),
    (2, 'Milestone 2', 'Campaign launch', '2025-07-15', '2025-07-20', 'Planning'),
    (3, 'Milestone 3', 'Research complete', '2025-09-01', '2025-09-15', 'On Hold'),
    (4, 'Milestone 4', 'UI review', '2025-09-10', '2025-09-12', 'Completed'),
    (5, 'Milestone 5', 'Test plan', '2025-10-15', '2025-10-20', 'In Progress');

-- Insert dữ liệu mẫu vào bảng task
INSERT INTO tasks (reporter_id, project_id, epic_id, sprint_id, milestone_id, type, manual_input, generation_ai_input, title, description, planned_start_date, planned_end_date, duration, actual_start_date, actual_end_date, percent_complete, planned_hours, actual_hours, planned_cost, planned_resource_cost, actual_cost, actual_resource_cost, remaining_hours, priority, evaluate, status)
VALUES 
    (1, 1, 1, 1, 1, 'Development', FALSE, FALSE, 'Task 1', 'Build login page', '2025-06-01', '2025-06-05', '5 days', '2025-06-01', '2025-06-04', 100.00, 40.00, 38.00, 5000.00, 4000.00, 4800.00, 3800.00, 0.00, 'High', 'Good', 'Completed'),
    (2, 1, 1, 2, 1, 'Development', FALSE, TRUE, 'Task 2', 'Add dashboard', '2025-06-16', '2025-06-20', '4 days', '2025-06-16', NULL, 50.00, 32.00, 16.00, 4000.00, 3000.00, 2000.00, 1500.00, 16.00, 'Medium', NULL, 'In Progress'),
    (3, 2, 3, 3, 2, 'Marketing', TRUE, FALSE, 'Task 3', 'Setup ads', '2025-07-01', '2025-07-05', '4 days', '2025-07-01', '2025-07-04', 100.00, 24.00, 22.00, 3000.00, 2500.00, 2800.00, 2300.00, 0.00, 'Low', 'Excellent', 'Completed'),
    (4, 3, 4, 4, 3, 'Research', FALSE, FALSE, 'Task 4', 'Gather data', '2025-08-01', '2025-08-10', '9 days', '2025-08-01', NULL, 30.00, 72.00, 20.00, 9000.00, 8000.00, 2500.00, 2000.00, 52.00, 'High', NULL, 'On Hold'),
    (5, 4, 5, 5, 4, 'Design', TRUE, TRUE, 'Task 5', 'Design UI', '2025-09-01', '2025-09-05', '4 days', '2025-09-01', '2025-09-03', 100.00, 32.00, 30.00, 4000.00, 3500.00, 3800.00, 3300.00, 0.00, 'Medium', 'Good', 'Completed'),
    (5, 5, 6, 6, 5, 'Testing', FALSE, TRUE, 'Task 6', 'Setup automation tests', '2025-10-01', '2025-10-05', '4 days', '2025-10-01', NULL, 50.00, 32.00, 16.00, 4000.00, 3500.00, 2000.00, 1800.00, 16.00, 'Medium', NULL, 'In Progress'); -- Thêm task cho project_id 5

-- Insert dữ liệu mẫu vào bảng task_assignment
INSERT INTO task_assignment (task_id, account_id, status, assigned_at, completed_at, hourly_rate)
VALUES 
    (1, 1, 'Assigned', '2025-06-01', '2025-06-04', 50.00),
    (2, 2, 'In Progress', '2025-06-16', NULL, 45.00),
    (3, 3, 'Completed', '2025-07-01', '2025-07-04', 40.00),
    (4, 4, 'On Hold', '2025-08-01', NULL, 55.00),
    (5, 5, 'Completed', '2025-09-01', '2025-09-03', 60.00),
    (6, 5, 'In Progress', '2025-10-01', NULL, 60.00); -- Thêm assignment cho task 6

-- Insert dữ liệu mẫu vào bảng task_check_list
INSERT INTO task_check_list (task_id, title, status, manual_input, generation_ai_input)
VALUES 
    (1, 'Check login UI', 'Completed', FALSE, FALSE),
    (2, 'Test dashboard', 'In Progress', TRUE, FALSE),
    (3, 'Verify ads', 'Completed', FALSE, TRUE),
    (4, 'Review data', 'On Hold', FALSE, FALSE),
    (5, 'Approve design', 'Completed', TRUE, TRUE),
    (6, 'Setup test scripts', 'In Progress', FALSE, TRUE); -- Thêm checklist cho task 6

-- Insert dữ liệu mẫu vào bảng task_comment
INSERT INTO task_comment (task_id, user_id, content)
VALUES 
    (1, 1, 'Login page looks good'),
    (2, 2, 'Need more charts on dashboard'),
    (3, 3, 'Ads are live now'),
    (4, 4, 'Data collection delayed'),
    (5, 5, 'Design approved by client'),
    (6, 5, 'Automation tests in progress'); -- Thêm comment cho task 6

-- Insert dữ liệu mẫu vào bảng task_dependency
INSERT INTO task_dependency (task_id, linked_from, linked_to, type)
VALUES 
    (1, 1, 2, 'Finish-Start'),
    (2, 2, 3, 'Finish-Start'),
    (3, 3, 4, 'Start-Start'),
    (4, 4, 5, 'Finish-Start'),
    (5, 5, 1, 'Start-Finish'),
    (6, 5, 6, 'Finish-Start'); -- Thêm dependency cho task 6

-- Insert dữ liệu mẫu vào bảng task_file
INSERT INTO task_file (task_id, title, url_file, status)
VALUES 
    (1, 'login_design.pdf', 'http://example.com/login.pdf', 'Uploaded'),
    (2, 'dashboard_mockup.png', 'http://example.com/dashboard.png', 'In Review'),
    (3, 'ad_campaign.doc', 'http://example.com/ad.doc', 'Approved'),
    (4, 'research_data.xlsx', 'http://example.com/data.xlsx', 'Pending'),
    (5, 'ui_design.jpg', 'http://example.com/ui.jpg', 'Approved'),
    (6, 'test_scripts.zip', 'http://example.com/tests.zip', 'Uploaded'); -- Thêm file cho task 6

-- Insert dữ liệu mẫu vào bảng task_status_log
INSERT INTO task_status_log (task_id, status, changed_by)
VALUES 
    (1, 'Completed', 1),
    (2, 'In Progress', 2),
    (3, 'Completed', 3),
    (4, 'On Hold', 4),
    (5, 'Completed', 5),
    (6, 'In Progress', 5); -- Thêm log cho task 6

-- Insert dữ liệu mẫu vào bảng document
INSERT INTO document (project_id, task_id, title, type, template, content, file_url, is_active, created_by, updated_by)
VALUES 
    (1, 1, 'Project Plan', 'Plan', 'Template A', 'Project plan details', 'http://example.com/plan.pdf', TRUE, 1, 1),
    (2, 3, 'Campaign Brief', 'Brief', 'Template B', 'Campaign details', 'http://example.com/brief.pdf', TRUE, 2, 2),
    (3, 4, 'Research Report', 'Report', 'Template C', 'Research findings', 'http://example.com/report.pdf', TRUE, 3, NULL),
    (4, 5, 'Design Spec', 'Spec', 'Template D', 'Design specifications', 'http://example.com/spec.pdf', TRUE, 4, 4),
    (5, 6, 'Test Strategy', 'Strategy', 'Template E', 'Test strategy', 'http://example.com/strategy.pdf', TRUE, 5, 5); -- Sửa task_id từ NULL thành 6

-- Insert dữ liệu mẫu vào bảng document_permission
INSERT INTO document_permission (document_id, account_id, permission_type)
VALUES 
    (1, 1, 'Read'),
    (2, 2, 'Write'),
    (3, 3, 'Read'),
    (4, 4, 'Write'),
    (5, 5, 'Read');

-- Insert dữ liệu mẫu vào bảng project_member
INSERT INTO project_member (account_id, project_id)
VALUES 
    (1, 1),
    (2, 1),
    (3, 2),
    (4, 3),
    (5, 4),
    (5, 5); -- Thêm member cho project_id 5

-- Insert dữ liệu mẫu vào bảng project_position
INSERT INTO project_position (project_member_id, position)
VALUES 
    (1, 'Project Manager'),
    (2, 'Developer'),
    (3, 'Marketing Lead'),
    (4, 'Researcher'),
    (5, 'Designer'),
    (6, 'Tester'); -- Thêm position cho member của project_id 5

-- Insert dữ liệu mẫu vào bảng notification
INSERT INTO notification (created_by, type, priority, message, related_entity_type, related_entity_id, is_read)
VALUES 
    (1, 'Task Update', 'High', 'Task 1 completed', 'Task', 1, FALSE),
    (2, 'Project Update', 'Medium', 'Project B started', 'Project', 2, FALSE),
    (3, 'Meeting Reminder', 'Low', 'Meeting tomorrow', 'Meeting', 1, TRUE),
    (4, 'Risk Alert', 'High', 'Risk identified', 'Risk', 1, FALSE),
    (5, 'Document Update', 'Medium', 'New document added', 'Document', 1, FALSE);

-- Insert dữ liệu mẫu vào bảng recipient_notification
INSERT INTO recipient_notification (account_id, notification_id, status)
VALUES 
    (1, 1, 'Received'),
    (2, 2, 'Received'),
    (3, 3, 'Read'),
    (4, 4, 'Received'),
    (5, 5, 'Received');

-- Insert dữ liệu mẫu vào bảng meeting
INSERT INTO meeting (project_id, meeting_topic, meeting_date, meeting_url, status, start_time, end_time, attendees)
VALUES 
    (1, 'Planning Meeting', '2025-06-05 10:00:00', 'http://meet.example.com/1', 'Scheduled', '2025-06-05 10:00:00', '2025-06-05 11:00:00', 5),
    (2, 'Campaign Review', '2025-07-10 14:00:00', 'http://meet.example.com/2', 'Completed', '2025-07-10 14:00:00', '2025-07-10 15:00:00', 3),
    (3, 'Research Sync', '2025-08-15 09:00:00', 'http://meet.example.com/3', 'Cancelled', '2025-08-15 09:00:00', '2025-08-15 10:00:00', 2),
    (4, 'Design Review', '2025-09-12 13:00:00', 'http://meet.example.com/4', 'Completed', '2025-09-12 13:00:00', '2025-09-12 14:00:00', 4),
    (5, 'Test Planning', '2025-10-20 11:00:00', 'http://meet.example.com/5', 'Scheduled', '2025-10-20 11:00:00', '2025-10-20 12:00:00', 3);

-- Insert dữ liệu mẫu vào bảng meeting_document (1:1 with meeting)
INSERT INTO meeting_document (meeting_id, title, description, file_url, is_active, account_id)
VALUES 
    (1, 'Meeting Agenda', 'Agenda for planning', 'http://example.com/agenda1.pdf', TRUE, 1),
    (2, 'Review Notes', 'Notes from campaign review', 'http://example.com/notes2.pdf', TRUE, 2),
    (3, 'Sync Notes', 'Notes from research sync', 'http://example.com/notes3.pdf', TRUE, 3),
    (4, 'Design Feedback', 'Feedback on design', 'http://example.com/feedback4.pdf', TRUE, 4),
    (5, 'Test Plan', 'Plan for testing', 'http://example.com/plan5.pdf', TRUE, 5);

-- Insert dữ liệu mẫu vào bảng meeting_log
INSERT INTO meeting_log (meeting_id, account_id, action)
VALUES 
    (1, 1, 'Started meeting'),
    (2, 2, 'Reviewed campaign'),
    (3, 3, 'Cancelled meeting'),
    (4, 4, 'Provided feedback'),
    (5, 5, 'Planned tests');

-- Insert dữ liệu mẫu vào bảng meeting_participant
INSERT INTO meeting_participant (meeting_id, account_id, role, status)
VALUES 
    (1, 1, 'Chair', 'Attended'),
    (1, 2, 'Member', 'Attended'),
    (2, 3, 'Presenter', 'Attended'),
    (3, 4, 'Member', 'Absent'),
    (4, 5, 'Chair', 'Attended');

-- Insert dữ liệu mẫu vào bảng meeting_transcript (1:1 with meeting)
INSERT INTO meeting_transcript (meeting_id, transcript_text)
VALUES 
    (1, 'Discussion on project planning'),
    (2, 'Review of campaign progress'),
    (3, 'Cancellation due to lack of data'),
    (4, 'Feedback on UI design'),
    (5, 'Planning for test cases');

-- Insert dữ liệu mẫu vào bảng meeting_summary (1:1 with meeting_transcript)
INSERT INTO meeting_summary (meeting_transcript_id, summary_text)
VALUES 
    (1, 'Planned next steps for project'),
    (2, 'Campaign on track'),
    (3, 'Meeting rescheduled'),
    (4, 'Design approved with changes'),
    (5, 'Test plan finalized');

-- Insert dữ liệu mẫu vào bảng milestone_feedback
INSERT INTO milestone_feedback (meeting_id, account_id, feedback_text, status)
VALUES 
    (1, 1, 'Good progress', 'Reviewed'),
    (2, 2, 'Needs more effort', 'Pending'),
    (3, 3, 'Delayed', 'Reviewed'),
    (4, 4, 'Excellent work', 'Approved'),
    (5, 5, 'On schedule', 'Pending');

-- Insert dữ liệu mẫu vào bảng risk
INSERT INTO risk (responsible_id, project_id, task_id, risk_scope, title, description, status, type, generated_by, probability, impact_level, severity_level, is_approved)
VALUES 
    (1, 1, 1, 'Schedule', 'Delay Risk', 'Possible delay in delivery', 'Open', 'Schedule', 'AI', 'Medium', 'High', 'Moderate', FALSE),
    (2, 2, 3, 'Budget', 'Cost Overrun', 'Budget might exceed', 'Open', 'Financial', 'Manual', 'Low', 'Medium', 'Low', FALSE),
    (3, 3, 4, 'Resource', 'Staff Shortage', 'Lack of resources', 'Closed', 'Resource', 'AI', 'High', 'High', 'High', TRUE),
    (4, 4, 5, 'Quality', 'Bug Risk', 'Potential bugs', 'Open', 'Quality', 'Manual', 'Medium', 'Low', 'Low', FALSE),
    (5, 5, 6, 'Scope', 'Scope Creep', 'Expanding scope', 'Open', 'Scope', 'AI', 'Low', 'Medium', 'Moderate', FALSE); -- Sửa task_id từ NULL thành 6

-- Insert dữ liệu mẫu vào bảng risk_solution
INSERT INTO risk_solution (risk_id, mitigation_plan, contingency_plan)
VALUES 
    (1, 'Add more resources', 'Hire contractors'),
    (2, 'Reduce scope', 'Seek additional funding'),
    (3, 'Reallocate staff', 'Outsource tasks'),
    (4, 'Increase testing', 'Rollback changes'),
    (5, 'Freeze scope', 'Prioritize core features');

-- Insert dữ liệu mẫu vào bảng change_request
INSERT INTO change_request (project_id, requested_by, title, description, status)
VALUES 
    (1, 1, 'Add Feature', 'Add new login feature', 'Pending'),
    (2, 2, 'Change Budget', 'Increase budget by 10%', 'Approved'),
    (3, 3, 'Extend Deadline', 'Extend by 1 month', 'Rejected'),
    (4, 4, 'Update Design', 'Revise UI colors', 'Pending'),
    (5, 5, 'Add Test Case', 'Include edge cases', 'Approved');

-- Insert dữ liệu mẫu vào bảng project_recommendation (Sửa lỗi: thay NULL bằng 6)
INSERT INTO project_recommendation (project_id, task_id, type, recommendation)
VALUES 
    (1, 1, 'Performance', 'Optimize database queries'),
    (2, 3, 'Marketing', 'Increase ad spend'),
    (3, 4, 'Research', 'Use additional sources'),
    (4, 5, 'Design', 'Improve color contrast'),
    (5, 6, 'Testing', 'Add automation tests'); -- Sửa task_id từ NULL thành 6

-- Insert dữ liệu mẫu vào bảng label
INSERT INTO label (project_id, name, color_code, description, status)
VALUES 
    (1, 'Bug', '#FF0000', 'Critical issues', 'ACTIVE'),
    (2, 'Enhancement', '#00FF00', 'New features', 'ACTIVE'),
    (3, 'Task', '#0000FF', 'General tasks', 'ACTIVE'),
    (4, 'Review', '#FFFF00', 'For review', 'ACTIVE'),
    (5, 'Done', '#00FFFF', 'Completed tasks', 'ACTIVE');

-- Insert dữ liệu mẫu vào bảng task_label
INSERT INTO task_label (label_id, task_id)
VALUES 
    (1, 1),
    (2, 2),
    (3, 3),
    (4, 4),
    (5, 5),
    (5, 6); -- Thêm label cho task 6

-- Insert dữ liệu mẫu vào bảng requirement
INSERT INTO requirement (project_id, title, type, description, priority)
VALUES 
    (1, 'User Login', 'Functional', 'User must log in', 'High'),
    (2, 'Ad Campaign', 'Non-functional', 'Ads must be visible', 'Medium'),
    (3, 'Data Collection', 'Functional', 'Gather research data', 'High'),
    (4, 'UI Layout', 'Non-functional', 'Responsive design', 'Medium'),
    (5, 'Test Coverage', 'Functional', 'Cover all cases', 'Low');

-- Insert dữ liệu mẫu vào bảng project_metric
INSERT INTO project_metric (project_id, calculated_by, is_approved, planned_value, earned_value, actual_cost, spi, cpi, delay_days, budget_overrun, projected_finish_date, projected_total_cost)
VALUES 
    (1, 'user1', FALSE, 100000.00, 80000.00, 75000.00, 0.90, 0.95, 5, 5000.00, '2025-12-10', 105000.00),
    (2, 'user2', TRUE, 50000.00, 45000.00, 48000.00, 0.85, 0.90, 3, 3000.00, '2025-09-10', 53000.00),
    (3, 'user3', FALSE, 75000.00, 60000.00, 65000.00, 0.80, 0.88, 10, 7000.00, '2025-11-15', 82000.00),
    (4, 'user4', TRUE, 30000.00, 28000.00, 29000.00, 0.95, 0.97, 2, 1000.00, '2025-10-05', 31000.00),
    (5, 'user5', FALSE, 40000.00, 35000.00, 36000.00, 0.90, 0.92, 4, 2000.00, '2025-12-10', 42000.00);

-- Insert dữ liệu mẫu vào bảng system_configuration (cấu hình tham số hệ thống)
INSERT INTO system_configuration (config_key, value_config, min_value, max_value, estimate_value, description, note, effected_from, effected_to)
VALUES 
    ('min_budget', '1000.00', '0.00', '1000000.00', '50000.00', 'Minimum project budget', 'Must be non-negative', '2025-01-01', '2025-12-31'),
    ('max_users', '100', '1', '1000', '500', 'Maximum users per project', 'Adjust based on scale', '2025-01-01', '2025-12-31'),
    ('work_hours_day', '8', '4', '12', '8', 'Working hours per day', 'Standard work hours', '2025-01-01', '2025-12-31'),
    ('max_delay_days', '10', '0', '30', '15', 'Maximum allowable delay', 'Monitor closely', '2025-01-01', '2025-12-31'),
    ('budget_threshold', '50000.00', '0.00', '1000000.00', '75000.00', 'Budget threshold for approval', 'Requires review', '2025-01-01', '2025-12-31');

INSERT INTO dynamic_category (category_group, code, name, description, order_index)
VALUES 
    -- Existing categories with updated naming conventions
    -- project_type (SW, MK, RS)
    ('project_type', 'SW', 'SOFTWARE', 'Software projects', 1),
    ('project_type', 'MK', 'MARKETING', 'Marketing projects', 2),
    ('project_type', 'RS', 'RESEARCH', 'Research projects', 3),

    -- status (IP, PL, CM) with DELETED added
    ('status', 'IP', 'IN PROGRESS', 'Projects, epics, sprints, milestones, tasks in progress', 1),
    ('status', 'PL', 'PLANNING', 'Projects, epics, sprints, milestones, tasks in planning', 2),
    ('status', 'CM', 'COMPLETED', 'Projects, epics, sprints, milestones, tasks completed', 3),
    ('status', 'DL', 'DELETED', 'Deleted projects, epics, sprints, milestones, tasks', 4),

    -- role (PM, DEV) - Assuming this is part of account_role, updated to match your request
    ('account_role', 'PM', 'PROJECT MANAGER', 'Project manager role', 1),
    ('account_role', 'TL', 'TEAM LEADER', 'Team leader role', 2),
    ('account_role', 'TM', 'TEAM MEMBER', 'Team member role', 3),
    ('account_role', 'SH', 'STAKEHOLDER', 'Stakeholder role', 4),

    -- account_status
    ('account_status', 'AC', 'ACTIVE', 'Active user account', 1),
    ('account_status', 'IN', 'INACTIVE', 'Inactive user account', 2),
    ('account_status', 'VR', 'VERIFIED', 'Verified user account', 3),
    ('account_status', 'UV', 'UNVERIFIED', 'Unverified user account', 4),
    ('account_status', 'BN', 'BANNED', 'Banned user account', 5),
    ('account_status', 'DL', 'DELETED', 'Deleted user account', 6),

    -- account_position
    ('account_position', 'BA', 'BUSINESS ANALYST', 'Business analyst position', 1),
    ('account_position', 'BE', 'BACKEND DEVELOPER', 'Backend developer position', 2),
    ('account_position', 'FE', 'FRONTEND DEVELOPER', 'Frontend developer position', 3),
    ('account_position', 'TS', 'TESTER', 'Tester position', 4),
    ('account_position', 'PM', 'PROJECT MANAGER', 'Project manager position', 5),
    ('account_position', 'DS', 'DESIGNER', 'Designer position', 6),

    -- status for task_assignment
    ('task_assignment_status', 'AS', 'ASSIGNED', 'Task assigned to user', 1),
    ('task_assignment_status', 'IP', 'IN PROGRESS', 'Task in progress', 2),
    ('task_assignment_status', 'CM', 'COMPLETED', 'Task completed', 3),
    ('task_assignment_status', 'OH', 'ON HOLD', 'Task on hold', 4),
    ('task_assignment_status', 'DL', 'DELETED', 'Deleted task assignment', 5),

    -- status for task_check_list
    ('task_check_list_status', 'IP', 'IN PROGRESS', 'Checklist item in progress', 1),
    ('task_check_list_status', 'CM', 'COMPLETED', 'Checklist item completed', 2),
    ('task_check_list_status', 'OH', 'ON HOLD', 'Checklist item on hold', 3),
    ('task_check_list_status', 'DL', 'DELETED', 'Deleted checklist item', 4),

    -- status for task_file
    ('task_file_status', 'UP', 'UPLOADED', 'File uploaded', 1),
    ('task_file_status', 'IR', 'IN REVIEW', 'File under review', 2),
    ('task_file_status', 'AP', 'APPROVED', 'File approved', 3),
    ('task_file_status', 'PD', 'PENDING', 'File pending', 4),
    ('task_file_status', 'DL', 'DELETED', 'Deleted file', 5),

    -- status for task_status_log
    ('task_status_log_status', 'IP', 'IN PROGRESS', 'Task in progress', 1),
    ('task_status_log_status', 'CM', 'COMPLETED', 'Task completed', 2),
    ('task_status_log_status', 'OH', 'ON HOLD', 'Task on hold', 3),
    ('task_status_log_status', 'DL', 'DELETED', 'Deleted task', 4),

    -- type for task (updated to STORY, TASK, BUG)
    ('task_type', 'ST', 'STORY', 'User story tasks', 1),
    ('task_type', 'TS', 'TASK', 'General task', 2),
    ('task_type', 'BG', 'BUG', 'Bug fix tasks', 3),

    -- type for document
    ('document_type', 'PL', 'PLAN', 'Project plan', 1),
    ('document_type', 'BR', 'BRIEF', 'Project brief', 2),
    ('document_type', 'RP', 'REPORT', 'Project report', 3),
    ('document_type', 'SP', 'SPEC', 'Project specification', 4),
    ('document_type', 'ST', 'STRATEGY', 'Project strategy', 5),

    -- permission_type for document_permission
    ('permission_type', 'RD', 'READ', 'Read permission', 1),
    ('permission_type', 'WR', 'WRITE', 'Write permission', 2),

    -- type for notification
    ('notification_type', 'TU', 'TASK UPDATE', 'Task update notification', 1),
    ('notification_type', 'PU', 'PROJECT UPDATE', 'Project update notification', 2),
    ('notification_type', 'MR', 'MEETING REMINDER', 'Meeting reminder notification', 3),
    ('notification_type', 'RA', 'RISK ALERT', 'Risk alert notification', 4),
    ('notification_type', 'DU', 'DOCUMENT UPDATE', 'Document update notification', 5),

    -- status for recipient_notification
    ('recipient_notification_status', 'RC', 'RECEIVED', 'Notification received', 1),
    ('recipient_notification_status', 'RD', 'READ', 'Notification read', 2),
    ('recipient_notification_status', 'DL', 'DELETED', 'Deleted notification', 3),

    -- status for meeting
    ('meeting_status', 'SC', 'SCHEDULED', 'Meeting scheduled', 1),
    ('meeting_status', 'CM', 'COMPLETED', 'Meeting completed', 2),
    ('meeting_status', 'CN', 'CANCELLED', 'Meeting cancelled', 3),
    ('meeting_status', 'DL', 'DELETED', 'Deleted meeting', 4),

    -- status for meeting_participant
    ('meeting_participant_status', 'AT', 'ATTENDED', 'Participant attended', 1),
    ('meeting_participant_status', 'AB', 'ABSENT', 'Participant absent', 2),
    ('meeting_participant_status', 'DL', 'DELETED', 'Deleted participant', 3),

    -- status for milestone_feedback
    ('milestone_feedback_status', 'RV', 'REVIEWED', 'Feedback reviewed', 1),
    ('milestone_feedback_status', 'PD', 'PENDING', 'Feedback pending', 2),
    ('milestone_feedback_status', 'AP', 'APPROVED', 'Feedback approved', 3),
    ('milestone_feedback_status', 'DL', 'DELETED', 'Deleted feedback', 4),

    -- status for risk
    ('risk_status', 'OP', 'OPEN', 'Risk open', 1),
    ('risk_status', 'CL', 'CLOSED', 'Risk closed', 2),
    ('risk_status', 'DL', 'DELETED', 'Deleted risk', 3),

    -- type for risk
    ('risk_type', 'SC', 'SCHEDULE', 'Schedule risk', 1),
    ('risk_type', 'FN', 'FINANCIAL', 'Financial risk', 2),
    ('risk_type', 'RS', 'RESOURCE', 'Resource risk', 3),
    ('risk_type', 'QL', 'QUALITY', 'Quality risk', 4),
    ('risk_type', 'SP', 'SCOPE', 'Scope risk', 5),

    -- status for change_request
    ('change_request_status', 'PD', 'PENDING', 'Change request pending', 1),
    ('change_request_status', 'AP', 'APPROVED', 'Change request approved', 2),
    ('change_request_status', 'RJ', 'REJECTED', 'Change request rejected', 3),
    ('change_request_status', 'DL', 'DELETED', 'Deleted change request', 4),

    -- type for project_recommendation
    ('recommendation_type', 'PF', 'PERFORMANCE', 'Performance recommendation', 1),
    ('recommendation_type', 'MK', 'MARKETING', 'Marketing recommendation', 2),
    ('recommendation_type', 'RS', 'RESEARCH', 'Research recommendation', 3),
    ('recommendation_type', 'DS', 'DESIGN', 'Design recommendation', 4),
    ('recommendation_type', 'TS', 'TESTING', 'Testing recommendation', 5),

    -- status for label
    ('label_status', 'AC', 'ACTIVE', 'Active label', 1),
    ('label_status', 'DL', 'DELETED', 'Deleted label', 2),

    -- type for requirement
    ('requirement_type', 'FN', 'FUNCTIONAL', 'Functional requirement', 1),
    ('requirement_type', 'NF', 'NON-FUNCTIONAL', 'Non-functional requirement', 2);


-------------------------------------------------------------------------------
-- IntelliPM-- :)
