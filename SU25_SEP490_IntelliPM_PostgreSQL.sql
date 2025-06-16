-- Database schema for SU25_SEP490_IntelliPM with TIMESTAMPTZ and standardized data

-- Create database
-- CREATE DATABASE SU25_SEP490_IntelliPM;

-- -- Connect to database
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
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    address TEXT NULL,
    picture VARCHAR(255) NULL
);

ALTER TABLE account
ADD COLUMN date_of_birth DATE NULL;

-- 2. refresh_token
CREATE TABLE refresh_token (
    refresh_token_id SERIAL PRIMARY KEY,
    expired_at TIMESTAMPTZ NOT NULL,
    token VARCHAR(255) NOT NULL,
    account_id INTEGER NOT NULL,
    CONSTRAINT fk_refresh_token_account FOREIGN KEY (account_id) REFERENCES account(id) ON DELETE CASCADE
);

-- 3. project
CREATE TABLE project (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    budget DECIMAL(15, 2) NULL,
    project_type VARCHAR(50) NOT NULL,
    created_by INT NOT NULL,
    start_date TIMESTAMPTZ NULL,
    end_date TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (created_by) REFERENCES account(id)
);

-- 4. epic
CREATE TABLE epic (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    start_date TIMESTAMPTZ NULL,
    end_date TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 5. sprint
CREATE TABLE sprint (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    goal TEXT NULL,
    start_date TIMESTAMPTZ NULL,
    end_date TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 6. milestone
CREATE TABLE milestone (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    start_date TIMESTAMPTZ NULL,
    end_date TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 7. tasks
CREATE TABLE tasks (
    id SERIAL PRIMARY KEY,
    reporter_id INT NOT NULL,
    project_id INT NOT NULL,
    epic_id INT NULL,
    sprint_id INT NULL,
    milestone_id INT NULL,
    type VARCHAR(50) NULL,
    manual_input BOOLEAN NOT NULL DEFAULT FALSE,
    generation_ai_input BOOLEAN NOT NULL DEFAULT FALSE,
    title VARCHAR(255) NOT NULL,
    description TEXT NULL,
    planned_start_date TIMESTAMPTZ NULL,
    planned_end_date TIMESTAMPTZ NULL,
    duration VARCHAR(100) NULL,
    actual_start_date TIMESTAMPTZ NULL,
    actual_end_date TIMESTAMPTZ NULL,
    percent_complete DECIMAL(5, 2) NULL,
    planned_hours DECIMAL(8, 2) NULL,
    actual_hours DECIMAL(8, 2) NULL,
    planned_cost DECIMAL(15, 2) NULL,
    planned_resource_cost DECIMAL(15, 2) NULL,
    actual_cost DECIMAL(15, 2) NULL,
    actual_resource_cost DECIMAL(15, 2) NULL,
    remaining_hours DECIMAL(8, 2) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    priority VARCHAR(50) NULL,
    evaluate VARCHAR(50) NULL,
    status VARCHAR(50) NULL,
    FOREIGN KEY (reporter_id) REFERENCES account(id),
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (epic_id) REFERENCES epic(id),
    FOREIGN KEY (sprint_id) REFERENCES sprint(id),
    FOREIGN KEY (milestone_id) REFERENCES milestone(id)
);

-- 8. task_assignment
CREATE TABLE task_assignment (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    account_id INT NOT NULL,
    status VARCHAR(50) NULL,
    assigned_at TIMESTAMPTZ NULL,
    completed_at TIMESTAMPTZ NULL,
    hourly_rate DECIMAL(10, 2) NULL,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 9. task_check_list
CREATE TABLE task_check_list (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    title VARCHAR(255) NOT NULL,
	description TEXT NULL,
    status VARCHAR(50) NULL,
    manual_input BOOLEAN NOT NULL DEFAULT FALSE,
    generation_ai_input BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 10. task_comment
CREATE TABLE task_comment (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    user_id INT NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (user_id) REFERENCES account(id)
);

-- 11. task_dependency
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

-- 12. task_file
CREATE TABLE task_file (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    url_file VARCHAR(1024) NOT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 13. task_status_log
CREATE TABLE task_status_log (
    id SERIAL PRIMARY KEY,
    task_id INT NOT NULL,
    status VARCHAR(50) NOT NULL,
    changed_by INT NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (changed_by) REFERENCES account(id)
);

-- 14. document
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
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (created_by) REFERENCES account(id),
    FOREIGN KEY (updated_by) REFERENCES account(id)
);

-- 15. document_permission
CREATE TABLE document_permission (
    id SERIAL PRIMARY KEY,
    document_id INT NOT NULL,
    account_id INT NOT NULL,
    permission_type VARCHAR(50) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (document_id) REFERENCES document(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 16. project_member
CREATE TABLE project_member (
    id SERIAL PRIMARY KEY,
    account_id INT NOT NULL,
    project_id INT NOT NULL,
    joined_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    invited_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL, 
    FOREIGN KEY (account_id) REFERENCES account(id),
    FOREIGN KEY (project_id) REFERENCES project(id),
    UNIQUE (account_id, project_id)
);

-- 17. project_position
CREATE TABLE project_position (
    id SERIAL PRIMARY KEY,
    project_member_id INT NOT NULL,
    position VARCHAR(100) NOT NULL,
    assigned_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_member_id) REFERENCES project_member(id)
);

-- 18. notification
CREATE TABLE notification (
    id SERIAL PRIMARY KEY,
    created_by INT NOT NULL,
    type VARCHAR(100) NOT NULL,
    priority VARCHAR(50) NOT NULL,
    message TEXT NOT NULL,
    related_entity_type VARCHAR(100) NULL,
    related_entity_id INT NULL,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by) REFERENCES account(id)
);

-- 19. recipient_notification
CREATE TABLE recipient_notification (
    id SERIAL PRIMARY KEY,
    account_id INT NOT NULL,
    notification_id INT NOT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (account_id) REFERENCES account(id),
    FOREIGN KEY (notification_id) REFERENCES notification(id)
);

-- 20. meeting
CREATE TABLE meeting (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    meeting_topic VARCHAR(255) NOT NULL,
    meeting_date TIMESTAMPTZ NOT NULL,
    meeting_url VARCHAR(1024) NULL,
    status VARCHAR(50) NULL,
    start_time TIMESTAMPTZ NULL,
    end_time TIMESTAMPTZ NULL,
    attendees INT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 21. meeting_document
CREATE TABLE meeting_document (
    meeting_id INT PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT NULL,
    file_url VARCHAR(1024) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    account_id INT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 22. meeting_log
CREATE TABLE meeting_log (
    id SERIAL PRIMARY KEY,
    meeting_id INT NOT NULL,
    account_id INT NOT NULL,
    action TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 23. meeting_participant
CREATE TABLE meeting_participant (
    id SERIAL PRIMARY KEY,
    meeting_id INT NOT NULL,
    account_id INT NOT NULL,
    role VARCHAR(100) NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (account_id) REFERENCES account(id),
    UNIQUE (meeting_id, account_id)
);

-- 24. meeting_transcript
CREATE TABLE meeting_transcript (
    meeting_id INT PRIMARY KEY,
    transcript_text TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id)
);

-- 25. meeting_summary
CREATE TABLE meeting_summary (
    meeting_transcript_id INT PRIMARY KEY,
    summary_text TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_transcript_id) REFERENCES meeting_transcript(meeting_id)
);

-- 26. milestone_feedback
CREATE TABLE milestone_feedback (
    id SERIAL PRIMARY KEY,
    meeting_id INT NOT NULL,
    account_id INT NOT NULL,
    feedback_text TEXT NOT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 27. risk
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
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (responsible_id) REFERENCES account(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 28. risk_solution
CREATE TABLE risk_solution (
    id SERIAL PRIMARY KEY,
    risk_id INT NOT NULL,
    mitigation_plan TEXT NULL,
    contingency_plan TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (risk_id) REFERENCES risk(id)
);

-- 29. change_request
CREATE TABLE change_request (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    requested_by INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (requested_by) REFERENCES account(id)
);

-- 30. project_recommendation
CREATE TABLE project_recommendation (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    task_id INT NOT NULL,
    type VARCHAR(100) NOT NULL,
    recommendation TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 31. label
CREATE TABLE label (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    color_code VARCHAR(7) NULL,
    description TEXT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 32. task_label
CREATE TABLE task_label (
    id SERIAL PRIMARY KEY,
    label_id INT NOT NULL,
    task_id INT NOT NULL,
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (label_id) REFERENCES label(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    UNIQUE (label_id, task_id)
);

-- 33. requirement
CREATE TABLE requirement (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    title VARCHAR(255) NOT NULL,
    type VARCHAR(100) NULL,
    description TEXT NULL,
    priority VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 34. project_metric
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
    projected_finish_date TIMESTAMPTZ NULL,
    projected_total_cost DECIMAL(15, 2) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 35. system_configuration
CREATE TABLE system_configuration (
    id SERIAL PRIMARY KEY,
    config_key VARCHAR(255) NOT NULL UNIQUE,
    value_config TEXT NULL,
    min_value VARCHAR(255) NULL,
    max_value VARCHAR(255) NULL,
    estimate_value VARCHAR(255) NULL,
    description TEXT NULL,
    note TEXT NULL,
    effected_from TIMESTAMPTZ NULL,
    effected_to TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 36. dynamic_category
CREATE TABLE dynamic_category (
    id SERIAL PRIMARY KEY,
    category_group VARCHAR(100) NOT NULL,
    code VARCHAR(100) NOT NULL,
    name VARCHAR(255) NOT NULL,
	label VARCHAR(255) NOT NULL,  
    description TEXT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    order_index INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (category_group, name) 
);

--------------------------------------------------------------




--------------------------------------------------------------

-- Insert sample data into account
INSERT INTO account (username, full_name, email, password, role, position, phone, gender, google_id, status, address, picture)
VALUES 
	('admin', 'Nguyen Van Admin', 'admin@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'ADMIN', 'ADMIN', '0901234567', 'MALE', 'googleID1', 'VERIFIED', 'Ha Noi', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamLeader', 'Nguyen Van A', 'teamleader@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_LEADER', 'TEAM_LEADER', '0901234567', 'MALE', 'google1', 'VERIFIED', 'Ha Noi', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('client', 'Nguyen Van KH', 'client@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'CLIENT', 'CLIENT', '0901234567', 'MALE', 'google2', 'VERIFIED', 'Ha Noi', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('projectManager', 'Tran Thi B', 'projectmanager@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'PROJECT_MANAGER', 'PROJECT_MANAGER', '0907654321', 'MALE', 'google3', 'VERIFIED', 'Ho Chi Minh', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberFE', 'Le Van C', 'teammemberfe@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'FRONTEND_DEVELOPER', '0912345678', 'MALE', NULL, 'VERIFIED', 'Da Nang', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberFE2', 'Pham Van D', 'teamMemberfe2@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'FRONTEND_DEVELOPER', '0923456789', 'MALE', 'google4', 'VERIFIED', 'Can Tho', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberFE3', 'Hoang Van E', 'teamMemberfe3@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'FRONTEND_DEVELOPER', '0934567890', 'MALE', 'google5', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberBE', 'Hoang Van Dat', 'teamMemberbe@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BACKEND_DEVELOPER', '0934567890', 'MALE', 'google6', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberBE2', 'Hoang Van E', 'teamMemberbe2@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BACKEND_DEVELOPER', '0934567890', 'MALE', 'google7', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberBE3', 'Hoang Van P', 'teamMemberbe3@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BACKEND_DEVELOPER', '0934567890', 'MALE', 'google8', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberBE4', 'Hoang Van R', 'teamMemberbe4@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BACKEND_DEVELOPER', '0934567890', 'MALE', 'google9', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberTS', 'Le Van Q', 'teammemberts@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'TESTER', '0912345678', 'MALE', NULL, 'VERIFIED', 'Da Nang', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberTS2', 'Pham Van W', 'teamMemberts2@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'TESTER', '0923456789', 'MALE', 'google10', 'VERIFIED', 'Can Tho', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberTS3', 'Hoang Van T', 'teamMemberts3@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'TESTER', '0934567890', 'MALE', 'google11', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberBA', 'Hoang Van U', 'teamMemberba@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BUSINESS_ANALYST', '0934567890', 'MALE', 'google12', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberBA2', 'Hoang Van L', 'teamMemberba2@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BUSINESS_ANALYST', '0934567890', 'MALE', 'google13', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberBA3', 'Hoang Van K', 'teamMemberba3@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BUSINESS_ANALYST', '0934567890', 'MALE', 'google14', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberBA4', 'Hoang Van H', 'teamMemberBA@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BACKEND_DEVELOPER', '0934567890', 'MALE', 'google15', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberDS', 'Le Van Q', 'teammemberds@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'DESIGNER', '0912345678', 'MALE', NULL, 'VERIFIED', 'Da Nang', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberDS2', 'Pham Van W', 'teamMemberds2@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'DESIGNER', '0923456789', 'MALE', 'google16', 'VERIFIED', 'Can Tho', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamMemberDS3', 'Hoang Van T', 'teamMemberds3@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'DESIGNER', '0934567890', 'MALE', 'google17', 'VERIFIED', 'Hai Phong', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219');

-- Insert sample data into refresh_token
INSERT INTO refresh_token (expired_at, token, account_id)
VALUES 
    ('2025-06-01 00:00:00+00', 'token_1_abc123', 1),
    ('2025-06-02 00:00:00+00', 'token_2_def456', 2),
    ('2025-06-03 00:00:00+00', 'token_3_ghi789', 3),
    ('2025-06-04 00:00:00+00', 'token_4_jkl012', 4),
    ('2025-06-05 00:00:00+00', 'token_5_mno345', 5);

-- Insert sample data into project
INSERT INTO project (name, description, budget, project_type, created_by, start_date, end_date, status)
VALUES 
    ('Project A', 'Development project', 1000000.00, 'WEB_APPLICATION', 1, '2025-06-01 00:00:00+00', '2025-12-01 00:00:00+00', 'IN_PROGRESS'),
    ('Project B', 'Marketing campaign', 500000.00, 'WEB_APPLICATION', 2, '2025-07-01 00:00:00+00', '2025-09-01 00:00:00+00', 'PLANNING'),
    ('Project C', 'Research project', 750000.00, 'WEB_APPLICATION', 3, '2025-08-01 00:00:00+00', '2025-11-01 00:00:00+00', 'ON_HOLD'),
    ('Project D', 'UI/UX Design', 300000.00, 'WEB_APPLICATION', 4, '2025-09-01 00:00:00+00', '2025-10-01 00:00:00+00', 'COMPLETED'),
    ('Project E', 'Testing project', 400000.00, 'WEB_APPLICATION', 5, '2025-10-01 00:00:00+00', '2025-12-01 00:00:00+00', 'IN_REVIEW');



-- Insert sample data into epic
INSERT INTO epic (project_id, name, description, start_date, end_date, status)
VALUES 
    (1, 'Epic 1', 'Core features', '2025-06-01 00:00:00+00', '2025-08-01 00:00:00+00', 'IN_PROGRESS'),
    (1, 'Epic 2', 'Additional features', '2025-08-01 00:00:00+00', '2025-10-01 00:00:00+00', 'TODO'),
    (2, 'Epic 3', 'Campaign setup', '2025-07-01 00:00:00+00', '2025-08-01 00:00:00+00', 'DONE'),
    (3, 'Epic 4', 'Research phase', '2025-08-01 00:00:00+00', '2025-09-01 00:00:00+00', 'IN_PROGRESS'),
    (4, 'Epic 5', 'Design phase', '2025-09-01 00:00:00+00', '2025-09-15 00:00:00+00', 'DONE'),
    (5, 'Epic 6', 'Testing phase', '2025-10-01 00:00:00+00', '2025-11-01 00:00:00+00', 'TODO');


-- Insert sample data into sprint
INSERT INTO sprint (project_id, name, goal, start_date, end_date, status)
VALUES 
    (1, 'Sprint 1', 'Implement login', '2025-06-01 00:00:00+00', '2025-06-15 00:00:00+00', 'COMPLETED'),
    (1, 'Sprint 2', 'Add dashboard', '2025-06-16 00:00:00+00', '2025-06-30 00:00:00+00', 'IN_PROGRESS'),
    (2, 'Sprint 3', 'Launch campaign', '2025-07-01 00:00:00+00', '2025-07-15 00:00:00+00', 'PLANNING'),
    (3, 'Sprint 4', 'Initial research', '2025-08-01 00:00:00+00', '2025-08-15 00:00:00+00', 'ON_HOLD'),
    (4, 'Sprint 5', 'Design UI', '2025-09-01 00:00:00+00', '2025-09-10 00:00:00+00', 'COMPLETED'),
    (5, 'Sprint 6', 'Test automation', '2025-10-01 00:00:00+00', '2025-10-15 00:00:00+00', 'IN_PROGRESS');

-- Insert sample data into milestone
INSERT INTO milestone (project_id, name, description, start_date, end_date, status)
VALUES 
    (1, 'Milestone 1', 'Beta release', '2025-07-01 00:00:00+00', '2025-07-15 00:00:00+00', 'IN_PROGRESS'),
    (2, 'Milestone 2', 'Campaign launch', '2025-07-15 00:00:00+00', '2025-07-20 00:00:00+00', 'PLANNING'),
    (3, 'Milestone 3', 'Research complete', '2025-09-01 00:00:00+00', '2025-09-15 00:00:00+00', 'ON_HOLD'),
    (4, 'Milestone 4', 'UI review', '2025-09-10 00:00:00+00', '2025-09-12 00:00:00+00', 'COMPLETED'),
    (5, 'Milestone 5', 'Test plan', '2025-10-15 00:00:00+00', '2025-10-20 00:00:00+00', 'IN_PROGRESS');

-- Insert sample data into tasks
INSERT INTO tasks (reporter_id, project_id, epic_id, sprint_id, milestone_id, type, manual_input, generation_ai_input, title, description, planned_start_date, planned_end_date, duration, actual_start_date, actual_end_date, percent_complete, planned_hours, actual_hours, planned_cost, planned_resource_cost, actual_cost, actual_resource_cost, remaining_hours, priority, evaluate, status)
VALUES 
    (1, 1, 1, 1, 1, 'STORY', FALSE, FALSE, 'Task 1', 'Build login page', '2025-06-01 00:00:00+00', '2025-06-05 00:00:00+00', '5 days', '2025-06-01 00:00:00+00', '2025-06-04 00:00:00+00', 100.00, 40.00, 38.00, 5000.00, 4000.00, 4800.00, 3800.00, 0.00, 'HIGH', 'Good', 'IN_PROGRESS'),
    (2, 1, 1, 2, 1, 'STORY', FALSE, TRUE, 'Task 2', 'Add dashboard', '2025-06-16 00:00:00+00', '2025-06-20 00:00:00+00', '4 days', '2025-06-16 00:00:00+00', NULL, 50.00, 32.00, 16.00, 4000.00, 3000.00, 2000.00, 1500.00, 16.00, 'MEDIUM', NULL, 'IN_PROGRESS'),
    (3, 2, 3, 3, 2, 'TASK', TRUE, FALSE, 'Task 3', 'Setup ads', '2025-07-01 00:00:00+00', '2025-07-05 00:00:00+00', '4 days', '2025-07-01 00:00:00+00', '2025-07-04 00:00:00+00', 100.00, 24.00, 22.00, 3000.00, 2500.00, 2800.00, 2300.00, 0.00, 'LOW', 'Excellent', 'IN_PROGRESS'),
    (4, 3, 4, 4, 3, 'TASK', FALSE, FALSE, 'Task 4', 'Gather data', '2025-08-01 00:00:00+00', '2025-08-10 00:00:00+00', '9 days', '2025-08-01 00:00:00+00', NULL, 30.00, 72.00, 20.00, 9000.00, 8000.00, 2500.00, 2000.00, 52.00, 'HIGH', NULL, 'IN_PROGRESS'),
    (5, 4, 5, 5, 4, 'STORY', TRUE, TRUE, 'Task 5', 'Design UI', '2025-09-01 00:00:00+00', '2025-09-05 00:00:00+00', '4 days', '2025-09-01 00:00:00+00', '2025-09-03 00:00:00+00', 100.00, 32.00, 30.00, 4000.00, 3500.00, 3800.00, 3300.00, 0.00, 'MEDIUM', 'Good', 'IN_PROGRESS'),
    (5, 5, 6, 6, 5, 'TASK', FALSE, TRUE, 'Task 6', 'Setup automation tests', '2025-10-01 00:00:00+00', '2025-10-05 00:00:00+00', '4 days', '2025-10-01 00:00:00+00', NULL, 50.00, 32.00, 16.00, 4000.00, 3500.00, 2000.00, 1800.00, 16.00, 'MEDIUM', NULL, 'IN_PROGRESS');

-- Insert sample data into task_assignment
INSERT INTO task_assignment (task_id, account_id, status, assigned_at, completed_at, hourly_rate)
VALUES 
    (1, 1, 'IN_PROGRESS', '2025-06-01 00:00:00+00', '2025-06-04 00:00:00+00', 50.00),
    (2, 2, 'IN_PROGRESS', '2025-06-16 00:00:00+00', NULL, 45.00),
    (3, 3, 'IN_PROGRESS', '2025-07-01 00:00:00+00', '2025-07-04 00:00:00+00', 40.00),
    (4, 4, 'IN_PROGRESS', '2025-08-01 00:00:00+00', NULL, 55.00),
    (5, 5, 'IN_PROGRESS', '2025-09-01 00:00:00+00', '2025-09-03 00:00:00+00', 60.00),
    (6, 5, 'IN_PROGRESS', '2025-10-01 00:00:00+00', NULL, 60.00);

-- Insert sample data into task_check_list
INSERT INTO task_check_list (task_id, title, description, status, manual_input, generation_ai_input)
VALUES 
    (1, 'Check login UI', 'Ensure login page layout is responsive and user-friendly', 'IN_PROGRESS', FALSE, FALSE),
    (2, 'Test dashboard', 'Verify all widgets and data visualization on the dashboard', 'IN_PROGRESS', TRUE, FALSE),
    (3, 'Verify ads', 'Confirm ad placements and tracking functionality', 'IN_PROGRESS', FALSE, TRUE),
    (4, 'Review data', 'Check data integrity and consistency in the report', 'IN_PROGRESS', FALSE, FALSE),
    (5, 'Approve design', 'Review UI/UX design for final approval', 'IN_PROGRESS', TRUE, TRUE),
    (6, 'Setup test scripts', 'Configure automated test scripts for regression testing', 'IN_PROGRESS', FALSE, TRUE);

-- Insert sample data into task_comment
INSERT INTO task_comment (task_id, user_id, content)
VALUES 
    (1, 1, 'Login page looks good'),
    (2, 2, 'Need more charts on dashboard'),
    (3, 3, 'Ads are live now'),
    (4, 4, 'Data collection delayed'),
    (5, 5, 'Design approved by client'),
    (6, 5, 'Automation tests in progress');

-- Insert sample data into task_dependency
INSERT INTO task_dependency (task_id, linked_from, linked_to, type)
VALUES 
    (1, 1, 2, 'FINISH_START'),
    (2, 2, 3, 'FINISH_START'),
    (3, 3, 4, 'START_START'),
    (4, 4, 5, 'FINISH_START'),
    (5, 5, 1, 'START_FINISH'),
    (6, 5, 6, 'FINISH_START');

-- Insert sample data into task_file
INSERT INTO task_file (task_id, title, url_file, status)
VALUES 
    (1, 'login_design.pdf', 'http://example.com/login.pdf', 'UPLOADED'),
    (2, 'dashboard_mockup.png', 'http://example.com/dashboard.png', 'IN_REVIEW'),
    (3, 'ad_campaign.doc', 'http://example.com/ad.doc', 'APPROVED'),
    (4, 'research_data.xlsx', 'http://example.com/data.xlsx', 'PENDING'),
    (5, 'ui_design.jpg', 'http://example.com/ui.jpg', 'APPROVED'),
    (6, 'test_scripts.zip', 'http://example.com/tests.zip', 'UPLOADED');

-- Insert sample data into task_status_log
INSERT INTO task_status_log (task_id, status, changed_by)
VALUES 
    (1, 'IN_PROGRESS', 1),
    (2, 'IN_PROGRESS', 2),
    (3, 'IN_PROGRESS', 3),
    (4, 'IN_PROGRESS', 4),
    (5, 'IN_PROGRESS', 5),
    (6, 'IN_PROGRESS', 5);

-- Insert sample data into document
INSERT INTO document (project_id, task_id, title, type, template, content, file_url, is_active, created_by, updated_by)
VALUES 
    (1, 1, 'Project Plan', 'PLAN', 'Template_A', 'Project plan details', 'http://example.com/plan.pdf', TRUE, 1, 1),
    (2, 3, 'Campaign Brief', 'BRIEF', 'Template_B', 'Campaign details', 'http://example.com/brief.pdf', TRUE, 2, 2),
    (3, 4, 'Research Report', 'REPORT', 'Template_C', 'Research findings', 'http://example.com/report.pdf', TRUE, 3, NULL),
    (4, 5, 'Design Spec', 'SPEC', 'Template_D', 'Design specifications', 'http://example.com/spec.pdf', TRUE, 4, 4),
    (5, 6, 'Test Strategy', 'STRATEGY', 'Template_E', 'Test strategy', 'http://example.com/strategy.pdf', TRUE, 5, 5);

-- Insert sample data into document_permission
INSERT INTO document_permission (document_id, account_id, permission_type)
VALUES 
    (1, 1, 'READ'),
    (2, 2, 'WRITE'),
    (3, 3, 'READ'),
    (4, 4, 'WRITE'),
    (5, 5, 'READ');

-- Insert sample data into project_member
INSERT INTO project_member (account_id, project_id, joined_at, invited_at, status)
VALUES 
    (1, 1, '2025-06-01 00:00:00+00', '2025-05-25 00:00:00+00', 'IN_PROGRESS'),
    (2, 1, '2025-06-01 00:00:00+00', '2025-05-26 00:00:00+00', 'IN_PROGRESS'),
    (3, 2, '2025-07-01 00:00:00+00', '2025-06-20 00:00:00+00', 'IN_PROGRESS'),
    (4, 3, '2025-08-01 00:00:00+00', '2025-07-15 00:00:00+00', 'DONE'),
    (5, 4, '2025-09-01 00:00:00+00', '2025-08-20 00:00:00+00', 'IN_PROGRESS'),
    (5, 5, '2025-10-01 00:00:00+00', '2025-09-15 00:00:00+00', 'DONE');

-- Insert sample data into project_position
INSERT INTO project_position (project_member_id, position, assigned_at)
VALUES 
    (1, 'PROJECT_MANAGER', '2025-06-01 00:00:00+00'),
    (2, 'BACKEND_DEVELOPER', '2025-06-01 00:00:00+00'),
    (3, 'TEAM_MEMBER', '2025-07-01 00:00:00+00'),
    (4, 'BUSINESS_ANALYST', '2025-08-01 00:00:00+00'),
    (5, 'DESIGNER', '2025-09-01 00:00:00+00'),
    (6, 'TESTER', '2025-10-01 00:00:00+00');

-- Insert sample data into notification
INSERT INTO notification (created_by, type, priority, message, related_entity_type, related_entity_id, is_read)
VALUES 
    (1, 'TASK_UPDATE', 'HIGH', 'Task 1 completed', 'TASK', 1, FALSE),
    (2, 'PROJECT_UPDATE', 'MEDIUM', 'Project B started', 'PROJECT', 2, FALSE),
    (3, 'MEETING_REMINDER', 'LOW', 'Meeting tomorrow', 'MEETING', 1, TRUE),
    (4, 'RISK_ALERT', 'HIGH', 'Risk identified', 'RISK', 1, FALSE),
    (5, 'DOCUMENT_UPDATE', 'MEDIUM', 'New document added', 'DOCUMENT', 1, FALSE);

-- Insert sample data into recipient_notification
INSERT INTO recipient_notification (account_id, notification_id, status)
VALUES 
    (1, 1, 'RECEIVED'),
    (2, 2, 'RECEIVED'),
    (3, 3, 'READ'),
    (4, 4, 'RECEIVED'),
    (5, 5, 'RECEIVED');

-- Insert sample data into meeting
INSERT INTO meeting (project_id, meeting_topic, meeting_date, meeting_url, status, start_time, end_time, attendees)
VALUES 
    (1, 'Planning Meeting', '2025-06-05 10:00:00+00', 'http://meet.example.com/1', 'SCHEDULED', '2025-06-05 10:00:00+00', '2025-06-05 11:00:00+00', 5),
    (2, 'Campaign Review', '2025-07-10 14:00:00+00', 'http://meet.example.com/2', 'COMPLETED', '2025-07-10 14:00:00+00', '2025-07-10 15:00:00+00', 3),
    (3, 'Research Sync', '2025-08-15 09:00:00+00', 'http://meet.example.com/3', 'CANCELLED', '2025-08-15 09:00:00+00', '2025-08-15 10:00:00+00', 2),
    (4, 'Design Review', '2025-09-12 13:00:00+00', 'http://meet.example.com/4', 'COMPLETED', '2025-09-12 13:00:00+00', '2025-09-12 14:00:00+00', 4),
    (5, 'Test Planning', '2025-10-20 11:00:00+00', 'http://meet.example.com/5', 'SCHEDULED', '2025-10-20 11:00:00+00', '2025-10-20 12:00:00+00', 3);

-- Insert sample data into meeting_document
INSERT INTO meeting_document (meeting_id, title, description, file_url, is_active, account_id)
VALUES 
    (1, 'Meeting Agenda', 'Agenda for planning', 'http://example.com/agenda1.pdf', TRUE, 1),
    (2, 'Review Notes', 'Notes from campaign review', 'http://example.com/notes2.pdf', TRUE, 2),
    (3, 'Sync Notes', 'Notes from research sync', 'http://example.com/notes3.pdf', TRUE, 3),
    (4, 'Design Feedback', 'Feedback on design', 'http://example.com/feedback4.pdf', TRUE, 4),
    (5, 'Test Plan', 'Plan for testing', 'http://example.com/plan5.pdf', TRUE, 5);

-- Insert sample data into meeting_log
INSERT INTO meeting_log (meeting_id, account_id, action)
VALUES 
    (1, 1, 'Started meeting'),
    (2, 2, 'Reviewed campaign'),
    (3, 3, 'Cancelled meeting'),
    (4, 4, 'Provided feedback'),
    (5, 5, 'Planned tests');

-- Insert sample data into meeting_participant
INSERT INTO meeting_participant (meeting_id, account_id, role, status)
VALUES 
    (1, 1, 'PROJECT_MANAGER', 'ATTENDED'),
    (1, 2, 'TEAM_MEMBER', 'ATTENDED'),
    (2, 3, 'TEAM_MEMBER', 'ATTENDED'),
    (3, 4, 'TEAM_MEMBER', 'ABSENT'),
    (4, 5, 'PROJECT_MANAGER', 'ATTENDED');

-- Insert sample data into meeting_transcript
INSERT INTO meeting_transcript (meeting_id, transcript_text)
VALUES 
    (1, 'Discussion on project planning'),
    (2, 'Review of campaign progress'),
    (3, 'Cancellation due to lack of data'),
    (4, 'Feedback on UI design'),
    (5, 'Planning for test cases');

-- Insert sample data into meeting_summary
INSERT INTO meeting_summary (meeting_transcript_id, summary_text)
VALUES 
    (1, 'Planned next steps for project'),
    (2, 'Campaign on track'),
    (3, 'Meeting rescheduled'),
    (4, 'Design approved with changes'),
    (5, 'Test plan finalized');

-- Insert sample data into milestone_feedback
INSERT INTO milestone_feedback (meeting_id, account_id, feedback_text, status)
VALUES 
    (1, 1, 'Good progress', 'REVIEWED'),
    (2, 2, 'Needs more effort', 'PENDING'),
    (3, 3, 'Delayed', 'REVIEWED'),
    (4, 4, 'Excellent work', 'APPROVED'),
    (5, 5, 'On schedule', 'PENDING');

-- Insert sample data into risk
INSERT INTO risk (responsible_id, project_id, task_id, risk_scope, title, description, status, type, generated_by, probability, impact_level, severity_level, is_approved)
VALUES 
    (1, 1, 1, 'SCHEDULE', 'Delay Risk', 'Possible delay in delivery', 'OPEN', 'SCHEDULE', 'AI', 'Medium', 'High', 'Moderate', FALSE),
    (2, 2, 3, 'BUDGET', 'Cost Overrun', 'Budget might exceed', 'OPEN', 'FINANCIAL', 'Manual', 'Low', 'Medium', 'Low', FALSE),
    (3, 3, 4, 'RESOURCE', 'Staff Shortage', 'Lack of resources', 'CLOSED', 'RESOURCE', 'AI', 'High', 'High', 'High', TRUE),
    (4, 4, 5, 'QUALITY', 'Bug Risk', 'Potential bugs', 'OPEN', 'QUALITY', 'Manual', 'Medium', 'Low', 'Low', FALSE),
    (5, 5, 6, 'SCOPE', 'Scope Creep', 'Expanding scope', 'OPEN', 'SCOPE', 'AI', 'Low', 'Medium', 'Moderate', FALSE);

-- Insert sample data into risk_solution
INSERT INTO risk_solution (risk_id, mitigation_plan, contingency_plan)
VALUES 
    (1, 'Add more resources', 'Hire contractors'),
    (2, 'Reduce scope', 'Seek additional funding'),
    (3, 'Reallocate staff', 'Outsource tasks'),
    (4, 'Increase testing', 'Rollback changes'),
    (5, 'Freeze scope', 'Prioritize core features');

-- Insert sample data into change_request
INSERT INTO change_request (project_id, requested_by, title, description, status)
VALUES 
    (1, 1, 'Add Feature', 'Add new login feature', 'PENDING'),
    (2, 2, 'Change Budget', 'Increase budget by 10%', 'APPROVED'),
    (3, 3, 'Extend Deadline', 'Extend by 1 month', 'REJECTED'),
    (4, 4, 'Update Design', 'Revise UI colors', 'PENDING'),
    (5, 5, 'Add Test Case', 'Include edge cases', 'APPROVED');

-- Insert sample data into project_recommendation
INSERT INTO project_recommendation (project_id, task_id, type, recommendation)
VALUES 
    (1, 1, 'PERFORMANCE', 'Optimize database queries'),
    (2, 3, 'MARKETING', 'Increase ad spend'),
    (3, 4, 'RESEARCH', 'Use additional sources'),
    (4, 5, 'DESIGN', 'Improve color contrast'),
    (5, 6, 'TESTING', 'Add automation tests');

-- Insert sample data into label
INSERT INTO label (project_id, name, color_code, description, status)
VALUES 
    (1, 'BUG', '#FF0000', 'Critical issues', 'ACTIVE'),
    (2, 'ENHANCEMENT', '#00FF00', 'New features', 'ACTIVE'),
    (3, 'TASK', '#0000FF', 'General tasks', 'ACTIVE'),
    (4, 'REVIEW', '#FFFF00', 'For review', 'ACTIVE'),
    (5, 'DONE', '#00FFFF', 'Completed tasks', 'ACTIVE');

-- Insert sample data into task_label
INSERT INTO task_label (label_id, task_id)
VALUES 
    (1, 1),
    (2, 2),
    (3, 3),
    (4, 4),
    (5, 5),
    (5, 6);

-- Insert sample data into requirement
INSERT INTO requirement (project_id, title, type, description, priority)
VALUES 
    (1, 'User Login', 'FUNCTIONAL', 'User must log in', 'HIGH'),
    (2, 'Ad Campaign', 'NON_FUNCTIONAL', 'Ads must be visible', 'MEDIUM'),
    (3, 'Data Collection', 'FUNCTIONAL', 'Gather research data', 'HIGH'),
    (4, 'UI Layout', 'NON_FUNCTIONAL', 'Responsive design', 'MEDIUM'),
    (5, 'Test Coverage', 'FUNCTIONAL', 'Cover all cases', 'LOW');

-- Insert sample data into project_metric
INSERT INTO project_metric (project_id, calculated_by, is_approved, planned_value, earned_value, actual_cost, spi, cpi, delay_days, budget_overrun, projected_finish_date, projected_total_cost)
VALUES 
    (1, 'user1', FALSE, 100000.00, 80000.00, 75000.00, 0.90, 0.95, 5, 5000.00, '2025-12-10 00:00:00+00', 105000.00),
    (2, 'user2', TRUE, 50000.00, 45000.00, 48000.00, 0.85, 0.90, 3, 3000.00, '2025-09-10 00:00:00+00', 53000.00),
    (3, 'user3', FALSE, 75000.00, 60000.00, 65000.00, 0.80, 0.88, 10, 7000.00, '2025-11-15 00:00:00+00', 82000.00),
    (4, 'user4', TRUE, 30000.00, 28000.00, 29000.00, 0.95, 0.97, 2, 1000.00, '2025-10-05 00:00:00+00', 31000.00),
    (5, 'user5', FALSE, 40000.00, 35000.00, 36000.00, 0.90, 0.92, 4, 2000.00, '2025-12-10 00:00:00+00', 42000.00);

-- Insert sample data into system_configuration
INSERT INTO system_configuration (config_key, value_config, min_value, max_value, estimate_value, description, note, effected_from, effected_to)
VALUES 
    ('max_sprint_duration', '30', '7', '60', '14', 'Maximum duration of a sprint in days', 'Adjust based on team capacity', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00'),
    ('min_sprint_tasks', '5', '1', '20', '10', 'Minimum number of tasks per sprint', 'Ensure productivity', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00'),
    ('max_tasks_per_user', '10', '1', '50', '15', 'Maximum tasks assigned per user', 'Monitor workload', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00'),
    ('priority_threshold', 'HIGH', 'LOW', 'HIGHEST', 'MEDIUM', 'Threshold for priority alerts', 'Trigger notifications', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00'),
    ('overtime_hours', '2', '0', '4', '2', 'Maximum overtime hours per day', 'Ensure compliance', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00');


INSERT INTO dynamic_category (category_group, name, label, description, order_index)
VALUES 
    ('project_type', 'WEB_APPLICATION', 'Web Application', 'Web application development projects for IT companies', 1),
    ('project_type', 'MOBILE_APPLICATION', 'Mobile Application', 'Mobile application development projects for IT companies', 2),
    ('project_type', 'ENTERPRISE_SOFTWARE', 'Enterprise Software', 'Enterprise software development projects for IT companies', 3),
    ('project_type', 'GAME_DEVELOPMENT', 'Game Development', 'Game development projects for IT companies', 4),
    ('project_status', 'PLANNING', 'Planning', 'Project is in planning stage', 1),
    ('project_status', 'IN_PROGRESS', 'In Progress', 'Project is actively in progress', 2),
    ('project_status', 'ON_HOLD', 'On Hold', 'Project is temporarily paused', 3),
    ('project_status', 'IN_REVIEW', 'In Review', 'Project is being reviewed', 4),
    ('project_status', 'COMPLETED', 'Completed', 'Project has been successfully completed', 5),
    ('project_status', 'CANCELLED', 'Cancelled', 'Project was cancelled', 6),
    ('epic_status', 'TODO', 'To Do', 'Epic not started yet', 1),
    ('epic_status', 'IN_PROGRESS', 'In Progress', 'Epic is in progress', 2),
    ('epic_status', 'DONE', 'Done', 'Epic has been completed', 3),
    ('sprint_status', 'FUTURE', 'Future', 'Sprint not started yet', 1),
    ('sprint_status', 'ACTIVE', 'Active', 'Sprint in progress or planning phase', 2),
    ('sprint_status', 'COMPLETED', 'Completed', 'Sprint successfully completed', 3),
    ('account_role', 'PROJECT_MANAGER', 'Project Manager', 'Project manager role', 1),
    ('account_role', 'TEAM_LEADER', 'Team Leader', 'Team leader role', 2),
    ('account_role', 'TEAM_MEMBER', 'Team Member', 'Team member role', 3),
    ('account_role', 'CLIENT', 'Client', 'Client role', 4),
    ('account_role', 'ADMIN', 'Admin', 'Admin role', 5),
    ('account_status', 'ACTIVE', 'Active', 'Active user account', 1),
    ('account_status', 'INACTIVE', 'Inactive', 'Inactive user account', 2),
    ('account_status', 'VERIFIED', 'Verified', 'Verified user account', 3),
    ('account_status', 'UNVERIFIED', 'Unverified', 'Unverified user account', 4),
    ('account_status', 'BANNED', 'Banned', 'Banned user account', 5),
    ('account_status', 'DELETED', 'Deleted', 'Deleted user account', 6),
    ('account_position', 'BUSINESS_ANALYST', 'Business Analyst', 'Business analyst position', 1),
    ('account_position', 'BACKEND_DEVELOPER', 'Backend Developer', 'Backend developer position', 2),
    ('account_position', 'FRONTEND_DEVELOPER', 'Frontend Developer', 'Frontend developer position', 3),
    ('account_position', 'TESTER', 'Tester', 'Tester position', 4),
    ('account_position', 'PROJECT_MANAGER', 'Project Manager', 'Project manager position', 5),
    ('account_position', 'DESIGNER', 'Designer', 'Designer position', 6),
    ('account_position', 'TEAM_LEADER', 'Team Leader', 'Team leader position', 7),
    ('account_position', 'CLIENT', 'Client', 'Client position', 8),
    ('account_position', 'ADMIN', 'Admin', 'Admin position', 9),
    ('task_assignment_status', 'ASSIGNED', 'Assigned', 'Task assigned to user', 1),
    ('task_assignment_status', 'IN_PROGRESS', 'In Progress', 'User is actively working on the task', 2),
    ('task_assignment_status', 'BLOCKED', 'Blocked', 'User is blocked and cannot proceed', 3),
    ('task_assignment_status', 'COMPLETED', 'Completed', 'User has completed their assigned part', 4),
    ('task_assignment_status', 'UNASSIGNED', 'Unassigned', 'User is unassigned or removed from task', 5),
    ('task_assignment_status', 'DELETED', 'Deleted', 'Task assignment record is deleted', 6),
    ('task_check_list_status', 'TODO', 'To Do', 'Checklist item to do', 1),
    ('task_check_list_status', 'IN_PROGRESS', 'In Progress', 'Checklist item in progress', 2),
    ('task_check_list_status', 'DONE', 'Done', 'Checklist item completed', 3),
    ('task_file_status', 'UPLOADED', 'Uploaded', 'File uploaded', 1),
    ('task_file_status', 'IN_REVIEW', 'In Review', 'File under review', 2),
    ('task_file_status', 'APPROVED', 'Approved', 'File approved', 3),
    ('task_file_status', 'PENDING', 'Pending', 'File pending', 4),
    ('task_file_status', 'DELETED', 'Deleted', 'Deleted file', 5),
    ('task_status', 'TODO', 'To Do', 'Task to do', 1),
    ('task_status', 'IN_PROGRESS', 'In Progress', 'Task in progress', 2),
    ('task_status', 'DONE', 'Done', 'Task completed', 3),
    ('log_status', 'TODO', 'To Do', 'Task to do', 1),
    ('log_status', 'IN_PROGRESS', 'In Progress', 'Task in progress', 2),
    ('log_status', 'DONE', 'Done', 'Task completed', 3),
    ('task_type', 'STORY', 'Story', 'User story tasks', 1),
    ('task_type', 'TASK', 'Task', 'General task', 2),
    ('task_type', 'BUG', 'Bug', 'Bug fix tasks', 3),
    ('document_type', 'PLAN', 'Plan', 'Project plan', 1),
    ('document_type', 'BRIEF', 'Brief', 'Project brief', 2),
    ('document_type', 'REPORT', 'Report', 'Project report', 3),
    ('document_type', 'SPEC', 'Specification', 'Project specification', 4),
    ('document_type', 'STRATEGY', 'Strategy', 'Project strategy', 5),
    ('permission_type', 'READ', 'Read', 'Read permission', 1),
    ('permission_type', 'WRITE', 'Write', 'Write permission', 2),
    ('notification_type', 'TASK_UPDATE', 'Task Update', 'Task update notification', 1),
    ('notification_type', 'PROJECT_UPDATE', 'Project Update', 'Project update notification', 2),
    ('notification_type', 'MEETING_REMINDER', 'Meeting Reminder', 'Meeting reminder notification', 3),
    ('notification_type', 'RISK_ALERT', 'Risk Alert', 'Risk alert notification', 4),
    ('notification_type', 'DOCUMENT_UPDATE', 'Document Update', 'Document update notification', 5),
    ('recipient_notification_status', 'RECEIVED', 'Received', 'Notification received', 1),
    ('recipient_notification_status', 'READ', 'Read', 'Notification read', 2),
    ('recipient_notification_status', 'DELETED', 'Deleted', 'Deleted notification', 3),
    ('meeting_status', 'SCHEDULED', 'Scheduled', 'Meeting scheduled', 1),
    ('meeting_status', 'COMPLETED', 'Completed', 'Meeting completed', 2),
    ('meeting_status', 'CANCELLED', 'Cancelled', 'Meeting cancelled', 3),
    ('meeting_status', 'DELETED', 'Deleted', 'Deleted meeting', 4),
    ('meeting_participant_status', 'ATTENDED', 'Attended', 'Participant attended', 1),
    ('meeting_participant_status', 'ABSENT', 'Absent', 'Participant absent', 2),
    ('meeting_participant_status', 'DELETED', 'Deleted', 'Deleted participant', 3),
    ('milestone_feedback_status', 'REVIEWED', 'Reviewed', 'Feedback reviewed', 1),
    ('milestone_feedback_status', 'PENDING', 'Pending', 'Feedback pending', 2),
    ('milestone_feedback_status', 'APPROVED', 'Approved', 'Feedback approved', 3),
    ('milestone_feedback_status', 'DELETED', 'Deleted', 'Deleted feedback', 4),
    ('risk_status', 'OPEN', 'Open', 'Risk open', 1),
    ('risk_status', 'CLOSED', 'Closed', 'Risk closed', 2),
    ('risk_status', 'DELETED', 'Deleted', 'Deleted risk', 3),
    ('risk_type', 'SCHEDULE', 'Schedule', 'Schedule risk', 1),
    ('risk_type', 'FINANCIAL', 'Financial', 'Financial risk', 2),
    ('risk_type', 'RESOURCE', 'Resource', 'Resource risk', 3),
    ('risk_type', 'QUALITY', 'Quality', 'Quality risk', 4),
    ('risk_type', 'SCOPE', 'Scope', 'Scope risk', 5),
    ('change_request_status', 'PENDING', 'Pending', 'Change request pending', 1),
    ('change_request_status', 'APPROVED', 'Approved', 'Change request approved', 2),
    ('change_request_status', 'REJECTED', 'Rejected', 'Change request rejected', 3),
    ('change_request_status', 'DELETED', 'Deleted', 'Deleted change request', 4),
    ('recommendation_type', 'PERFORMANCE', 'Performance', 'Performance recommendation', 1),
    ('recommendation_type', 'MARKETING', 'Marketing', 'Marketing recommendation', 2),
    ('recommendation_type', 'RESEARCH', 'Research', 'Research recommendation', 3),
    ('recommendation_type', 'DESIGN', 'Design', 'Design recommendation', 4),
    ('recommendation_type', 'TESTING', 'Testing', 'Testing recommendation', 5),
    ('label_status', 'ACTIVE', 'Active', 'Active label', 1),
    ('label_status', 'DELETED', 'Deleted', 'Deleted label', 2),
    ('requirement_type', 'FUNCTIONAL', 'Functional', 'Functional requirement', 1),
    ('requirement_type', 'NON_FUNCTIONAL', 'Non-Functional', 'Non-functional requirement', 2),
    ('project_member_status', 'CREATED', 'Created', 'Project member newly created', 1),
    ('project_member_status', 'INVITED', 'Invited', 'Project member invited to join', 2),
    ('project_member_status', 'ACTIVE', 'Active', 'Project member active in project', 3),
    ('project_member_status', 'BANNED', 'Banned', 'Project member banned, no access', 4),
    ('project_member_status', 'DELETED', 'Deleted', 'Project member deleted from project', 5),
    ('task_priority', 'HIGHEST', 'Highest', 'Highest priority task', 1),
    ('task_priority', 'HIGH', 'High', 'High priority task', 2),
    ('task_priority', 'MEDIUM', 'Medium', 'Medium priority task', 3),
    ('task_priority', 'LOW', 'Low', 'Low priority task', 4),
    ('task_priority', 'LOWEST', 'Lowest', 'Lowest priority task', 5),
	('milestone_status', 'PLANNING', 'Planning', 'Milestone is in planning stage', 1),
    ('milestone_status', 'IN_PROGRESS', 'In Progress', 'Milestone is in progress', 2),
    ('milestone_status', 'COMPLETED', 'Completed', 'Milestone has been completed', 3),
    ('milestone_status', 'ON_HOLD', 'On Hold', 'Milestone is temporarily paused', 4),
    ('milestone_status', 'CANCELLED', 'Cancelled', 'Milestone was cancelled', 5),
	('dependency_type', 'FINISH_START', 'Finish-to-Start', 'Task must finish before next task starts', 1),
    ('dependency_type', 'START_START', 'Start-to-Start', 'Task must start before next task starts', 2),
    ('dependency_type', 'FINISH_FINISH', 'Finish-to-Finish', 'Task must finish before next task finishes', 3),
    ('dependency_type', 'START_FINISH', 'Start-to-Finish', 'Task must start before next task finishes', 4);


	-------  INTELLIPM DB ---------
	-- Update 16/06/2025
