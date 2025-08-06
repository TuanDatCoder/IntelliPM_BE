-- Database schema for SU25_SEP490_IntelliPM with TIMESTAMPTZ and standardized data

-- Create database
-- CREATE DATABASE SU25_SEP490_IntelliPM;

-- Connect to database

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
    picture VARCHAR(255) NULL,
    date_of_birth DATE NULL
);

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
    project_key VARCHAR(10) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    budget DECIMAL(15, 2) NULL,
    project_type VARCHAR(50) NOT NULL,
    created_by INT NOT NULL,
    start_date TIMESTAMPTZ NULL,
    end_date TIMESTAMPTZ NULL,
    icon_url TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (created_by) REFERENCES account(id)
);


-- 4. project_member
CREATE TABLE project_member (
    id SERIAL PRIMARY KEY,
    account_id INT NOT NULL,
    project_id INT NOT NULL,
    joined_at TIMESTAMPTZ NULL,
    invited_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    hourly_rate DECIMAL(10, 2) NULL,
    working_hours_per_day INT NULL DEFAULT 8, 
    FOREIGN KEY (account_id) REFERENCES account(id),
    FOREIGN KEY (project_id) REFERENCES project(id),
    UNIQUE (account_id, project_id)
);

--ALTER TABLE project_member ADD COLUMN working_hours_per_day INT DEFAULT 8;

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

ALTER TABLE sprint
ADD COLUMN planned_start_date TIMESTAMPTZ NULL;

ALTER TABLE sprint
ADD COLUMN planned_end_date TIMESTAMPTZ NULL;



-- 6. epic
CREATE TABLE epic (
    id VARCHAR(255) PRIMARY KEY,
    project_id INT NOT NULL,
	reporter_id INT NULL,
	assigned_by INT NULL,
    sprint_id INT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    start_date TIMESTAMPTZ NULL,
    end_date TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (project_id) REFERENCES project(id),
	FOREIGN KEY (assigned_by) REFERENCES account(id),
    FOREIGN KEY (reporter_id) REFERENCES account(id) ON DELETE SET NULL,
    FOREIGN KEY (sprint_id) REFERENCES sprint(id)
);

-- 7. epic_comment
CREATE TABLE epic_comment (
    id SERIAL PRIMARY KEY,
    epic_id VARCHAR(255) NOT NULL,
    account_id INT NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (epic_id) REFERENCES epic(id),
    FOREIGN KEY (account_id) REFERENCES account(id) 
);
-- 8. epic_file
CREATE TABLE epic_file (
    id SERIAL PRIMARY KEY,
    epic_id VARCHAR(255) NOT NULL,
    title VARCHAR(255) NOT NULL,
    url_file VARCHAR(1024) NOT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (epic_id) REFERENCES epic(id)
);

-- 9. milestone
CREATE TABLE milestone (
    id SERIAL PRIMARY KEY,
    key VARCHAR(255) NOT NULL UNIQUE,
    project_id INT NOT NULL,
    sprint_id INT NULL,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    start_date TIMESTAMPTZ NULL,
    end_date TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NULL,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (sprint_id) REFERENCES sprint(id)
);

-- 10. milestone _comment
CREATE TABLE milestone_comment (
    id SERIAL PRIMARY KEY,
    milestone_id INT NOT NULL,
    account_id INT NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (milestone_id) REFERENCES milestone(id),
    FOREIGN KEY (account_id) REFERENCES account(id) 
);

-- 11. tasks
CREATE TABLE tasks (
    id VARCHAR(255) PRIMARY KEY,
    reporter_id INT NOT NULL,
    project_id INT NOT NULL,
    epic_id VARCHAR(255) NULL,
    sprint_id INT NULL,
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
    FOREIGN KEY (sprint_id) REFERENCES sprint(id)
);

-- 12. task_assignment
CREATE TABLE task_assignment (
    id SERIAL PRIMARY KEY,
    task_id VARCHAR(255) NOT NULL,
    account_id INT NOT NULL,
    status VARCHAR(50) NULL,
    assigned_at TIMESTAMPTZ NULL,
    completed_at TIMESTAMPTZ NULL,
    planned_hours DECIMAL(8, 2) NULL,
    actual_hours DECIMAL(8, 2) NULL,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 13. subtask
CREATE TABLE subtask (
    id VARCHAR(255) PRIMARY KEY,
    task_id VARCHAR(255) NOT NULL,
    assigned_by INT NULL,
	reporter_id INT NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT NULL,
    status VARCHAR(50) NULL,
	start_date TIMESTAMPTZ NULL,
    end_date TIMESTAMPTZ NULL,
    manual_input BOOLEAN NOT NULL DEFAULT FALSE,
    generation_ai_input BOOLEAN NOT NULL DEFAULT FALSE,
    priority VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sprint_id INT NULL,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (assigned_by) REFERENCES account(id),
    FOREIGN KEY (sprint_id) REFERENCES sprint(id),
	FOREIGN KEY (reporter_id) REFERENCES account(id)
);

-- Thêm các trường còn thiếu cho bảng subtask (tất cả đều cho phép NULL)
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS planned_start_date TIMESTAMPTZ NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS planned_end_date TIMESTAMPTZ NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS duration VARCHAR(100) NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS actual_start_date TIMESTAMPTZ NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS actual_end_date TIMESTAMPTZ NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS percent_complete DECIMAL(5, 2) NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS planned_hours DECIMAL(8, 2) NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS actual_hours DECIMAL(8, 2) NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS remaining_hours DECIMAL(8, 2) NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS planned_cost DECIMAL(15, 2) NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS planned_resource_cost DECIMAL(15, 2) NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS actual_cost DECIMAL(15, 2) NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS actual_resource_cost DECIMAL(15, 2) NULL;
ALTER TABLE subtask ADD COLUMN IF NOT EXISTS evaluate VARCHAR(50) NULL;





-- 14. subtask_file
CREATE TABLE subtask_file (
    id SERIAL PRIMARY KEY,
    subtask_id VARCHAR(255) NOT NULL,
    title VARCHAR(255) NOT NULL,
    url_file VARCHAR(1024) NOT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (subtask_id) REFERENCES subtask(id)
);

-- 15. subtask_comment
CREATE TABLE subtask_comment (
    id SERIAL PRIMARY KEY,
    subtask_id VARCHAR(255) NOT NULL,
    account_id INT NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (subtask_id) REFERENCES subtask(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 16. task_comment
CREATE TABLE task_comment (
    id SERIAL PRIMARY KEY,
    task_id VARCHAR(255) NOT NULL,
    account_id INT NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 16. task_dependency
-- CREATE TABLE task_dependency (
--     id SERIAL PRIMARY KEY,
--     milestone_id INT NULL,
--     task_id VARCHAR(255) NULL,
--     linked_from VARCHAR(255) NOT NULL,
--     linked_to VARCHAR(255) NOT NULL,
--     type VARCHAR(50) NULL,
--     FOREIGN KEY (milestone_id) REFERENCES milestone(id),
--     FOREIGN KEY (task_id) REFERENCES tasks(id),
--     FOREIGN KEY (linked_from) REFERENCES tasks(id),
--     FOREIGN KEY (linked_to) REFERENCES tasks(id)
-- );

-- 17. task_dependency
CREATE TABLE task_dependency (
    id SERIAL PRIMARY KEY,
    from_type VARCHAR(50) NOT NULL,    
    linked_from VARCHAR(255) NOT NULL, 
    to_type VARCHAR(50) NOT NULL,      
    linked_to VARCHAR(255) NOT NULL,
    type VARCHAR(50) NOT NULL         
);


-- 18. task_file
CREATE TABLE task_file (
    id SERIAL PRIMARY KEY,
    task_id VARCHAR(255) NOT NULL,
    title VARCHAR(255) NOT NULL,
    url_file VARCHAR(1024) NOT NULL,
    status VARCHAR(50) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 19. document
CREATE TABLE document (
    id SERIAL PRIMARY KEY,
    project_id INTEGER NOT NULL,
    task_id VARCHAR(255),
    epic_id VARCHAR(255),
    subtask_id VARCHAR(255),
    title VARCHAR(255) NOT NULL,
    type VARCHAR(100),
    template TEXT,
    content TEXT,
    file_url VARCHAR(1024),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    status VARCHAR(30) DEFAULT 'Draft',
    created_by INTEGER NOT NULL,
    updated_by INTEGER,
    approver_id INTEGER,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    visibility VARCHAR(20),

    CONSTRAINT fk_document_project FOREIGN KEY (project_id)
        REFERENCES public.project(id) ON DELETE NO ACTION ON UPDATE NO ACTION,

    CONSTRAINT fk_document_task FOREIGN KEY (task_id)
        REFERENCES public.tasks(id) ON DELETE NO ACTION ON UPDATE NO ACTION,

    CONSTRAINT fk_document_epic FOREIGN KEY (epic_id)
        REFERENCES public.epic(id) ON DELETE NO ACTION ON UPDATE NO ACTION,

    CONSTRAINT fk_document_subtask FOREIGN KEY (subtask_id)
        REFERENCES public.subtask(id) ON DELETE NO ACTION ON UPDATE NO ACTION,

    CONSTRAINT fk_document_created_by FOREIGN KEY (created_by)
        REFERENCES public.account(id) ON DELETE NO ACTION ON UPDATE NO ACTION,

    CONSTRAINT fk_document_updated_by FOREIGN KEY (updated_by)
        REFERENCES public.account(id) ON DELETE NO ACTION ON UPDATE NO ACTION,

    CONSTRAINT fk_document_approver_id FOREIGN KEY (approver_id)
        REFERENCES public.account(id) ON DELETE NO ACTION ON UPDATE NO ACTION
);

CREATE TABLE document_comment (
    id SERIAL PRIMARY KEY,
    document_id INTEGER NOT NULL,
    author_id INTEGER NOT NULL,
    content TEXT NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,

  
    CONSTRAINT fk_document FOREIGN KEY (document_id)
        REFERENCES document (id) ON DELETE CASCADE,

    CONSTRAINT fk_author FOREIGN KEY (author_id)
        REFERENCES account (id) ON DELETE CASCADE
);


CREATE TABLE document_export_file (
    id SERIAL PRIMARY KEY,
    document_id INTEGER NOT NULL,
    exported_file_url VARCHAR(1000) NOT NULL,
    exported_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    exported_by INTEGER NOT NULL,

    
    CONSTRAINT fk_document_export FOREIGN KEY (document_id)
        REFERENCES document(id) ON DELETE CASCADE,

    CONSTRAINT fk_exported_by FOREIGN KEY (exported_by)
        REFERENCES account(id) ON DELETE SET NULL
);

-- 20. document_permission
CREATE TABLE document_permission (
    id SERIAL PRIMARY KEY,
    document_id INT NOT NULL,
    account_id INT NOT NULL,
    permission_type VARCHAR(50) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (document_id) REFERENCES document(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 21. project_position
CREATE TABLE project_position (
    id SERIAL PRIMARY KEY,
    project_member_id INT NOT NULL,
    position VARCHAR(100) NOT NULL,
    assigned_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_member_id) REFERENCES project_member(id)
);

-- 22. notification
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

-- 23. recipient_notification
CREATE TABLE recipient_notification (
    id SERIAL PRIMARY KEY,
    account_id INT NOT NULL,
    notification_id INT NOT NULL,
    status VARCHAR(50) NULL,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (account_id) REFERENCES account(id),
    FOREIGN KEY (notification_id) REFERENCES notification(id)
);

-- 24. meeting
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

-- 25. meeting_document
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

-- 26. meeting_log
CREATE TABLE meeting_log (
    id SERIAL PRIMARY KEY,
    meeting_id INT NOT NULL,
    account_id INT NOT NULL,
    action TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 27. meeting_participant
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

-- 28. meeting_transcript
CREATE TABLE meeting_transcript (
    meeting_id INT PRIMARY KEY,
    transcript_text TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id)
);

-- 29. meeting_summary
CREATE TABLE meeting_summary (
    meeting_transcript_id INT PRIMARY KEY,
    summary_text TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_transcript_id) REFERENCES meeting_transcript(meeting_id)
);

-- 30. milestone_feedback
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

-- 31. risk
CREATE TABLE risk (
    id SERIAL PRIMARY KEY,
    risk_key VARCHAR(20) NOT NULL UNIQUE,
    responsible_id INT NULL,
    created_by INT NOT NULL,
    project_id INT NOT NULL,
    task_id VARCHAR(255) NULL,
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
    due_date TIMESTAMPTZ NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (responsible_id) REFERENCES account(id),
    FOREIGN KEY (created_by) REFERENCES account(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 32. risk_solution
CREATE TABLE risk_solution (
    id SERIAL PRIMARY KEY,
    risk_id INT NOT NULL,
    mitigation_plan TEXT NULL,
    contingency_plan TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (risk_id) REFERENCES risk(id)
);

-- 33. risk_file
CREATE TABLE risk_file (
    id SERIAL PRIMARY KEY,
    risk_id INT NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_url VARCHAR(1024) NOT NULL, 
    uploaded_by INT NOT NULL,
    uploaded_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (risk_id) REFERENCES risk(id),
    FOREIGN KEY (uploaded_by) REFERENCES account(id)
);

-- 34. risk_comment
CREATE TABLE risk_comment (
    id SERIAL PRIMARY KEY,
    risk_id INT NOT NULL,
    account_id INT NOT NULL,
    comment TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (risk_id) REFERENCES risk(id),
    FOREIGN KEY (account_id) REFERENCES account(id)
);

-- 35. change_request
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

-- 36. project_recommendation
CREATE TABLE project_recommendation (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    task_id VARCHAR(255) NULL,
    type VARCHAR(100) NOT NULL,
    recommendation TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id)
);

-- 37. label
CREATE TABLE label (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    color_code VARCHAR(10) NULL,
    description TEXT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'ACTIVE',
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 38. work_item_label
CREATE TABLE work_item_label (
    id SERIAL PRIMARY KEY,
    label_id INT NOT NULL,
    task_id VARCHAR(255) NULL,     
    epic_id VARCHAR(255) NULL,      
    subtask_id VARCHAR(255) NULL,   
    is_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (label_id) REFERENCES label(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (epic_id) REFERENCES epic(id),
    FOREIGN KEY (subtask_id) REFERENCES subtask(id),
    UNIQUE (label_id, task_id, epic_id, subtask_id) 
);

-- 39. work_log
CREATE TABLE work_log (
    id SERIAL PRIMARY KEY,
    task_id VARCHAR(255) NULL,     
    subtask_id VARCHAR(255) NULL,      
    log_date TIMESTAMPTZ NOT NULL,   
    hours DECIMAL(8, 2) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (subtask_id) REFERENCES subtask(id)
);

-- 40. requirement
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

-- 41. project_metric
CREATE TABLE project_metric (
    id SERIAL PRIMARY KEY,
    project_id INT NOT NULL,
    calculated_by VARCHAR(50) NOT NULL,
    is_approved BOOLEAN NOT NULL DEFAULT FALSE,
    planned_value DECIMAL(15, 2) NULL,
    earned_value DECIMAL(15, 2) NULL,
    actual_cost DECIMAL(15, 2) NULL,
    schedule_performance_index DECIMAL(15, 2) NULL,
    cost_performance_index DECIMAL(15, 2) NULL,
    budget_at_completion DECIMAL(15, 2) NULL,
    duration_at_completion DECIMAL(15, 2) NULL,
    cost_variance DECIMAL(15, 2) NULL,
    schedule_variance DECIMAL(15, 2) NULL,
    estimate_at_completion DECIMAL(15, 2) NULL,
    estimate_to_complete DECIMAL(15, 2) NULL,
    variance_at_completion DECIMAL(15, 2) NULL,
    estimate_duration_at_completion DECIMAL(15, 2) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (project_id) REFERENCES project(id)
);

-- 42. system_configuration
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

-- 43. dynamic_category
CREATE TABLE dynamic_category (
    id SERIAL PRIMARY KEY,
    category_group VARCHAR(100) NOT NULL,
    name VARCHAR(255) NOT NULL,
    label VARCHAR(255) NOT NULL,
    description TEXT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    order_index INT NOT NULL DEFAULT 0,
    icon_link TEXT NULL,
    color VARCHAR(10) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (category_group, name)
);

-- 44. activity_log
CREATE TABLE activity_log (
    id SERIAL PRIMARY KEY,
    project_id INT NULL,
    task_id VARCHAR(255) NULL,
    subtask_id VARCHAR(255) NULL,
    related_entity_type VARCHAR(100) NOT NULL,
    related_entity_id VARCHAR(255) NULL,
    action_type VARCHAR(100) NOT NULL,
    field_changed VARCHAR(100) NULL,
    old_value TEXT NULL,
    new_value TEXT NULL,
    message TEXT NULL,
    created_by INT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (created_by) REFERENCES account(id),
    FOREIGN KEY (task_id) REFERENCES tasks(id),
    FOREIGN KEY (subtask_id) REFERENCES subtask(id)
);

-- 46. meeting_reschedule_request
CREATE TABLE meeting_reschedule_request (
    id SERIAL PRIMARY KEY,
    meeting_id INT NOT NULL,
    requester_id INT NOT NULL,
    requested_date TIMESTAMPTZ NOT NULL,
    reason TEXT NULL,
    status VARCHAR(50) NOT NULL,
    pm_id INT NULL,
    pm_proposed_date TIMESTAMPTZ NULL,
    pm_note TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id),
    FOREIGN KEY (requester_id) REFERENCES account(id),
    FOREIGN KEY (pm_id) REFERENCES account(id)
);
--------------------------------------------------------------

-- Insert sample data into account
INSERT INTO account (username, full_name, email, password, role, position, phone, gender, google_id, status, address, picture)
VALUES 
    ('admin', 'Nguyen Van Admin', 'admin@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'ADMIN', 'ADMIN', '0901234567', 'MALE', 'googleID1', 'VERIFIED', 'Ha Noi', 'https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/male.png?alt=media&token=6f3a8425-e611-4f17-b690-08fd7b465219'),
    ('teamLeader', 'Tuan Dat Leader', 'datdqtse171685@fpt.edu.vn', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_LEADER', 'TEAM_LEADER', '0901234567', 'MALE', 'google1', 'VERIFIED', 'Ha Noi', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751518375/z6546708478561_a4d16f85e1617d0f8cfa1524f44e51f6_thpbov.jpg'),
    ('client', 'Nguyen Van KH', 'client@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'CLIENT', 'CLIENT', '0901234567', 'MALE', 'google2', 'VERIFIED', 'Ha Noi', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751556759/z5320227395399_018_cdb0a3792325321f269d1d504775d361_eikshl.jpg'),
    ('projectManager', 'Tran Thi B', 'tuandatdq03@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'PROJECT_MANAGER', 'PROJECT_MANAGER', '0907654321', 'MALE', 'google3', 'VERIFIED', 'Ho Chi Minh', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751518452/z5320227395399_017_a673556c6dc7718c178654828bdbba1d_qtszvc.jpg'),
    ('teamMemberFE', 'Le Van C', 'tuandatdinhquoc@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'FRONTEND_DEVELOPER', '0912345678', 'MALE', NULL, 'VERIFIED', 'Da Nang', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751554127/z6652659612030_ed08b7914d40502375a8825b8f3f8abb_ikelle.jpg'),
    ('teamMemberFE2', 'Pham Van D', 'dinhhoangdat789@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'FRONTEND_DEVELOPER', '0923456789', 'FEMALE', 'google4', 'VERIFIED', 'Can Tho', 'https://i.pravatar.cc/40?img=28'),
    ('teamMemberFE3', 'Hoang Van E', 'teamMemberfe3@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'FRONTEND_DEVELOPER', '0934567890', 'MALE', 'google5', 'VERIFIED', 'Hai Phong', 'https://i.pravatar.cc/40?img=56'),
    ('teamMemberBE', 'Hoang Van Dat', 'teamMemberbe@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BACKEND_DEVELOPER', '0934567890', 'MALE', 'google6', 'VERIFIED', 'Hai Phong', 'https://i.pravatar.cc/40?img=57'),
    ('teamMemberBE2', 'Hoang Van E', 'teamMemberbe2@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BACKEND_DEVELOPER', '0934567890', 'MALE', 'google7', 'VERIFIED', 'Hai Phong', 'https://i.pravatar.cc/40?img=47'),
    ('teamMemberBE3', 'Hoang Van P', 'teamMemberbe3@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BACKEND_DEVELOPER', '0934567890', 'MALE', 'google8', 'VERIFIED', 'Hai Phong', 'https://i.pravatar.cc/40?img=5'),
    ('teamMemberBE4', 'Hoang Van R', 'teamMemberbe4@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BACKEND_DEVELOPER', '0934567890', 'MALE', 'google9', 'VERIFIED', 'Hai Phong', 'https://i.pravatar.cc/40?img=2'),
    ('teamMemberTS', 'Le Van Q', 'teammemberts@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'TESTER', '0912345678', 'MALE', NULL, 'VERIFIED', 'Da Nang', 'https://i.pravatar.cc/40?img=7'),
    ('teamMemberTS2', 'Pham Van W', 'teamMemberts2@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'TESTER', '0923456789', 'MALE', 'google10', 'VERIFIED', 'Can Tho', 'https://i.pravatar.cc/40?img=8'),
    ('teamMemberTS3', 'Hoang Van T', 'teamMemberts3@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'TESTER', '0934567890', 'MALE', 'google11', 'VERIFIED', 'Hai Phong', 'https://i.pravatar.cc/40?img=9'),
    ('teamMemberBA', 'Hoang Van U', 'teamMemberba@gmail.com', '8776f108e247ab1e2b323042c049c266407c81fbad41bde1e8dfc1bb66fd267e', 'TEAM_MEMBER', 'BUSINESS_ANALYST', '0934567890', 'MALE', 'google12', 'VERIFIED', 'Hai Phong', 'https://i.pravatar.cc/40?img=10'),
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

-- Insert sample data into project (gán project_key + icon_url)
INSERT INTO project (project_key, name, description, budget, project_type, created_by, start_date, end_date, icon_url, status)
VALUES 
    ('PROJA', 'Project A', 'Development project', 1000000.00, 'WEB_APPLICATION', 1, '2025-06-01 00:00:00+00', '2025-12-01 00:00:00+00', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517835/iconProject2_qawavf.svg', 'IN_PROGRESS'),
    ('PROJB', 'Project B', 'Marketing campaign', 500000.00, 'WEB_APPLICATION', 2, '2025-07-01 00:00:00+00', '2025-09-01 00:00:00+00', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517982/iconProject3_c2v4bp.svg', 'PLANNING'),
    ('PROJC', 'Project C', 'Research project', 750000.00, 'WEB_APPLICATION', 3, '2025-08-01 00:00:00+00', '2025-11-01 00:00:00+00', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751518033/iconProject4_lfhkgf.svg', 'ON_HOLD'),
    ('PROJD', 'Project D', 'UI/UX Design', 300000.00, 'WEB_APPLICATION', 4, '2025-09-01 00:00:00+00', '2025-10-01 00:00:00+00', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517675/iconProject1_fkaeja.svg', 'COMPLETED'),
    ('PROJE', 'Project E', 'Testing project', 400000.00, 'WEB_APPLICATION', 5, '2025-10-01 00:00:00+00', '2025-12-01 00:00:00+00', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517835/iconProject2_qawavf.svg', 'IN_REVIEW');

-- Insert sample data into project_member
INSERT INTO project_member (account_id, project_id, joined_at, invited_at, status, hourly_rate)
VALUES 
    (1, 1, '2025-06-01 00:00:00+07', '2025-05-25 00:00:00+07', 'IN_PROGRESS', 50.00),
    (2, 1, '2025-06-01 00:00:00+07', '2025-05-26 00:00:00+07', 'IN_PROGRESS', 45.00),
    (3, 2, '2025-07-01 00:00:00+07', '2025-06-20 00:00:00+07', 'IN_PROGRESS', 40.00),
    (4, 3, '2025-08-01 00:00:00+07', '2025-07-15 00:00:00+07', 'DONE', 55.00),
    (5, 4, '2025-09-01 00:00:00+07', '2025-08-20 00:00:00+07', 'IN_PROGRESS', 60.00),
    (5, 5, '2025-10-01 00:00:00+07', '2025-09-15 00:00:00+07', 'DONE', 60.00);


-- Insert sample data into epic (sử dụng project_key-số_thứ_tự)
INSERT INTO epic (id, project_id, name, description, start_date, end_date, status,reporter_id)
VALUES 
    ('PROJA-1', 1, 'Epic 1', 'Core features', '2025-06-01 00:00:00+00', '2025-08-01 00:00:00+00', 'IN_PROGRESS',1),
    ('PROJA-2', 1, 'Epic 2', 'Additional features', '2025-08-01 00:00:00+00', '2025-10-01 00:00:00+00', 'TO_DO',2),
    ('PROJB-1', 2, 'Epic 3', 'Campaign setup', '2025-07-01 00:00:00+00', '2025-08-01 00:00:00+00', 'DONE',3),
    ('PROJC-1', 3, 'Epic 4', 'Research phase', '2025-08-01 00:00:00+00', '2025-09-01 00:00:00+00', 'IN_PROGRESS',4),
    ('PROJD-1', 4, 'Epic 5', 'Design phase', '2025-09-01 00:00:00+00', '2025-09-15 00:00:00+00', 'DONE',5),
    ('PROJE-1', 5, 'Epic 6', 'Testing phase', '2025-10-01 00:00:00+00', '2025-11-01 00:00:00+00', 'TO_DO',5);

-- Insert sample data into epic_comment
INSERT INTO epic_comment (epic_id, account_id, content, created_at)
VALUES 
    ('PROJA-1', 1, 'Great progress on Epic 1! Looking forward to the next update.', '2025-06-15 10:00:00+07'),
	('PROJA-1', 1, 'Great progress on Epic 1! Looking forward to the next update.2', '2025-06-15 10:00:00+07'),
    ('PROJA-2', 2, 'Can we discuss the timeline for Epic 2 in the next meeting?', '2025-06-16 14:30:00+07'),
    ('PROJB-1', 3, 'Epic 3 is complete! Please review the deliverables.', '2025-06-20 09:15:00+07'),
    ('PROJC-1', 4, 'Need more resources for Epic 4. Please assign additional team members.', '2025-06-22 11:00:00+07'),
    ('PROJD-1', 5, 'Excellent work on Epic 5! Let’s plan the next steps.', '2025-06-23 15:45:00+07'),
	('PROJD-1', 5, 'Excellent work on Epic 5! Let’s plan the next steps. 2', '2025-06-23 15:45:00+07'),
	('PROJE-1', 5, 'Excellent work on Epic 5! Let’s plan the next steps. ', '2025-06-23 15:45:00+07');

INSERT INTO epic_file (epic_id, title, url_file, status)
VALUES 
    ('PROJA-1', 'validation_rules.pdf', 'http://example.com/validation.pdf', 'UPLOADED'),
    ('PROJA-2', 'chart_design.png', 'http://example.com/chart.png', 'IN_REVIEW'),
    ('PROJB-1', 'ad_placement.doc', 'http://example.com/ad_placement.doc', 'APPROVED'),
    ('PROJC-1', 'data_review.xlsx', 'http://example.com/data_review.xlsx', 'PENDING'),
    ('PROJD-1', 'ui_feedback.jpg', 'http://example.com/ui_feedback.jpg', 'APPROVED'),
    ('PROJE-1', 'test_scripts.zip', 'http://example.com/test_scripts.zip', 'UPLOADED');


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
INSERT INTO milestone (project_id, key, name, description, start_date, end_date, status)
VALUES 
    (1, 'PROJA-M1', 'Milestone 1', 'Beta release', '2025-07-01 00:00:00+00', '2025-07-15 00:00:00+00', 'IN_PROGRESS'),
    (2, 'PROJB-M2', 'Milestone 2', 'Campaign launch', '2025-07-15 00:00:00+00', '2025-07-20 00:00:00+00', 'PLANNING'),
    (3, 'PROJC-M3', 'Milestone 3', 'Research complete', '2025-09-01 00:00:00+00', '2025-09-15 00:00:00+00', 'ON_HOLD'),
    (4, 'PROJD-M4', 'Milestone 4', 'UI review', '2025-09-10 00:00:00+00', '2025-09-12 00:00:00+00', 'COMPLETED'),
    (5, 'PROJE-M5', 'Milestone 5', 'Test plan', '2025-10-15 00:00:00+00', '2025-10-20 00:00:00+00', 'IN_PROGRESS');


-- Insert sample data into tasks (sử dụng project_key-số_thứ_tự)
INSERT INTO tasks (id, reporter_id, project_id, epic_id, sprint_id, type, manual_input, generation_ai_input, title, description, planned_start_date, planned_end_date, duration, actual_start_date, actual_end_date, percent_complete, planned_hours, actual_hours, planned_cost, planned_resource_cost, actual_cost, actual_resource_cost, remaining_hours, priority, evaluate, status)
VALUES 
    ('PROJA-3', 1, 1, 'PROJA-1', 1, 'STORY', FALSE, FALSE, 'Build login page', 'Build login page', '2025-06-01 00:00:00+00', '2025-06-05 00:00:00+00', '5 days', '2025-06-01 00:00:00+00', '2025-06-04 00:00:00+00', 100.00, 40.00, 38.00, 5000.00, 4000.00, 4800.00, 3800.00, 0.00, 'HIGH', 'Good', 'IN_PROGRESS'),
    ('PROJA-4', 2, 1, 'PROJA-1', 2, 'STORY', FALSE, TRUE, 'Add dashboard', 'Add dashboard', '2025-06-16 00:00:00+00', '2025-06-20 00:00:00+00', '4 days', '2025-06-16 00:00:00+00', NULL, 50.00, 32.00, 16.00, 4000.00, 3000.00, 2000.00, 1500.00, 16.00, 'MEDIUM', NULL, 'IN_PROGRESS'),
	('PROJA-5', 1, 1, 'PROJA-1', 1, 'STORY', FALSE, FALSE, 'Build login page', 'Build login page', '2025-06-01 00:00:00+00', '2025-06-05 00:00:00+00', '5 days', '2025-06-01 00:00:00+00', '2025-06-04 00:00:00+00', 100.00, 40.00, 38.00, 5000.00, 4000.00, 4800.00, 3800.00, 0.00, 'HIGH', 'Good', 'IN_PROGRESS'),
    ('PROJA-6', 2, 1, 'PROJA-1', 2, 'STORY', FALSE, TRUE, 'Add dashboard', 'Add dashboard', '2025-06-16 00:00:00+00', '2025-06-20 00:00:00+00', '4 days', '2025-06-16 00:00:00+00', NULL, 50.00, 32.00, 16.00, 4000.00, 3000.00, 2000.00, 1500.00, 16.00, 'MEDIUM', NULL, 'IN_PROGRESS'),
    ('PROJB-2', 3, 2, 'PROJB-1', 3, 'TASK', TRUE, FALSE, 'Setup ads', 'Setup ads', '2025-07-01 00:00:00+00', '2025-07-05 00:00:00+00', '4 days', '2025-07-01 00:00:00+00', '2025-07-04 00:00:00+00', 100.00, 24.00, 22.00, 3000.00, 2500.00, 2800.00, 2300.00, 0.00, 'LOW', 'Excellent', 'IN_PROGRESS'),
    ('PROJC-2', 4, 3, 'PROJC-1', 4, 'TASK', FALSE, FALSE, 'Gather data', 'Gather data', '2025-08-01 00:00:00+00', '2025-08-10 00:00:00+00', '9 days', '2025-08-01 00:00:00+00', NULL, 30.00, 72.00, 20.00, 9000.00, 8000.00, 2500.00, 2000.00, 52.00, 'HIGH', NULL, 'IN_PROGRESS'),
    ('PROJD-2', 5, 4, 'PROJD-1', 5,'STORY', TRUE, TRUE, 'Design UI', 'Design UI', '2025-09-01 00:00:00+00', '2025-09-05 00:00:00+00', '4 days', '2025-09-01 00:00:00+00', '2025-09-03 00:00:00+00', 100.00, 32.00, 30.00, 4000.00, 3500.00, 3800.00, 3300.00, 0.00, 'MEDIUM', 'Good', 'IN_PROGRESS'),
    ('PROJE-2', 5, 5, 'PROJE-1', 6, 'TASK', FALSE, TRUE, 'Setup automation tests', 'Setup automation tests', '2025-10-01 00:00:00+00', '2025-10-05 00:00:00+00', '4 days', '2025-10-01 00:00:00+00', NULL, 50.00, 32.00, 16.00, 4000.00, 3500.00, 2000.00, 1800.00, 16.00, 'MEDIUM', NULL, 'IN_PROGRESS');



-- Insert sample data into task_assignment (cập nhật với project_member_id, planned_hours, actual_hours)
INSERT INTO task_assignment (task_id, account_id, status, assigned_at, completed_at, planned_hours, actual_hours)
VALUES 
    ('PROJA-3', 1, 'IN_PROGRESS', '2025-06-01 00:00:00+07', '2025-06-04 00:00:00+07', 40.00, 38.00),
    ('PROJA-4', 2, 'IN_PROGRESS', '2025-06-16 00:00:00+07', NULL, 32.00, 16.00),
	('PROJA-3', 5, 'IN_PROGRESS', '2025-06-01 00:00:00+07', '2025-06-04 00:00:00+07', 40.00, 38.00),
    ('PROJA-4', 6, 'IN_PROGRESS', '2025-06-16 00:00:00+07', NULL, 32.00, 16.00),
    ('PROJB-2', 3, 'IN_PROGRESS', '2025-07-01 00:00:00+07', '2025-07-04 00:00:00+07', 24.00, 22.00),
    ('PROJC-2', 4, 'IN_PROGRESS', '2025-08-01 00:00:00+07', NULL, 72.00, 20.00),
    ('PROJD-2', 5, 'IN_PROGRESS', '2025-09-01 00:00:00+07', '2025-09-03 00:00:00+07', 32.00, 30.00),
    ('PROJE-2', 6, 'IN_PROGRESS', '2025-10-01 00:00:00+07', NULL, 32.00, 16.00);


-- Insert sample data into task_comment
INSERT INTO task_comment (task_id, account_id, content)
VALUES 
    ('PROJA-3', 1, 'Login page looks good'),
    ('PROJA-4', 2, 'Need more charts on dashboard'),
    ('PROJB-2', 3, 'Ads are live now'),
    ('PROJC-2', 4, 'Data collection delayed'),
    ('PROJD-2', 5, 'Design approved by client'),
    ('PROJE-2', 5, 'Automation tests in progress');

-- -- Insert sample data into task_dependency
-- INSERT INTO task_dependency (task_id, linked_from, linked_to, type)
-- VALUES 
--     ('PROJA-3', 'PROJA-3', 'PROJA-4', 'FINISH_START'),
--     ('PROJA-4', 'PROJA-4', 'PROJB-2', 'FINISH_START'),
--     ('PROJB-2', 'PROJB-2', 'PROJC-2', 'START_START'),
--     ('PROJC-2', 'PROJC-2', 'PROJD-2', 'FINISH_START'),
--     ('PROJD-2', 'PROJD-2', 'PROJA-4', 'START_FINISH'),
--     ('PROJE-2', 'PROJD-2', 'PROJE-2', 'FINISH_START');

-- Insert sample data into task_dependency
INSERT INTO task_dependency (from_type, linked_from, to_type, linked_to, type)
VALUES 
    ('task',     'PROJA-3', 'task',     'PROJA-4', 'FINISH_START'),
    ('task',     'PROJA-4', 'task',     'PROJB-2', 'FINISH_START'),
    ('task',     'PROJB-2', 'task',     'PROJC-2', 'START_START'),
    ('task',     'PROJC-2', 'task',     'PROJD-2', 'FINISH_START'),
    ('task',     'PROJD-2', 'task',     'PROJA-4', 'START_FINISH'),
    ('task',     'PROJD-2', 'task',     'PROJE-2', 'FINISH_START');


-- Insert sample data into task_file
INSERT INTO task_file (task_id, title, url_file, status)
VALUES 
    ('PROJA-3', 'login_design.pdf', 'http://example.com/login.pdf', 'UPLOADED'),
    ('PROJA-4', 'dashboard_mockup.png', 'http://example.com/dashboard.png', 'IN_REVIEW'),
    ('PROJB-2', 'ad_campaign.doc', 'http://example.com/ad.doc', 'APPROVED'),
    ('PROJC-2', 'research_data.xlsx', 'http://example.com/data.xlsx', 'PENDING'),
    ('PROJD-2', 'ui_design.jpg', 'http://example.com/ui.jpg', 'APPROVED'),
    ('PROJE-2', 'test_scripts.zip', 'http://example.com/tests.zip', 'UPLOADED');

-- Insert sample data into subtask (id based on project_id)
INSERT INTO subtask (id, task_id, assigned_by, title, description, status, manual_input, generation_ai_input)
VALUES 
    ('PROJA-7', 'PROJA-3', 1, 'Subtask 1 - Login Validation', 'Validate login credentials', 'IN_PROGRESS', FALSE, FALSE),
    ('PROJA-8', 'PROJA-4', 2, 'Subtask 2 - Chart Implementation', 'Implement charts on dashboard', 'IN_PROGRESS', TRUE, FALSE),
	('PROJA-9', 'PROJA-3', 1, 'Subtask 3 - Login Validation', 'Validate login credentials MORE', 'IN_PROGRESS', FALSE, FALSE),
    ('PROJA-10', 'PROJA-4', 2, 'Subtask 4 - Chart Implementation', 'Implement charts on dashboard MORE', 'IN_PROGRESS', TRUE, FALSE),
	('PROJA-11', 'PROJA-4', 1, 'Subtask 5 - Login Validation', 'Validate login credentials MORE', 'IN_PROGRESS', FALSE, FALSE),
    ('PROJA-12', 'PROJA-6', 2, 'Subtask 6 - Chart Implementation', 'Implement charts on dashboard MORE', 'IN_PROGRESS', TRUE, FALSE),
    ('PROJB-3', 'PROJB-2', 3, 'Subtask 3 - Ad Placement', 'Place ads on website', 'IN_PROGRESS', FALSE, TRUE),
    ('PROJC-3', 'PROJC-2', 4, 'Subtask 4 - Data Review', 'Review collected data', 'IN_PROGRESS', FALSE, FALSE),
    ('PROJD-3', 'PROJD-2', 5, 'Subtask 5 - UI Feedback', 'Gather feedback on UI', 'IN_PROGRESS', TRUE, TRUE),
    ('PROJE-3', 'PROJE-2', 5, 'Subtask 6 - Test Script Setup', 'Setup initial test scripts', 'IN_PROGRESS', FALSE, TRUE);

-- Insert sample data into subtask_file
INSERT INTO subtask_file (subtask_id, title, url_file, status)
VALUES 
    ('PROJA-7', 'validation_rules.pdf', 'http://example.com/validation.pdf', 'UPLOADED'),
    ('PROJA-8', 'chart_design.png', 'http://example.com/chart.png', 'IN_REVIEW'),
    ('PROJB-3', 'ad_placement.doc', 'http://example.com/ad_placement.doc', 'APPROVED'),
    ('PROJC-3', 'data_review.xlsx', 'http://example.com/data_review.xlsx', 'PENDING'),
    ('PROJD-3', 'ui_feedback.jpg', 'http://example.com/ui_feedback.jpg', 'APPROVED'),
    ('PROJE-3', 'test_scripts.zip', 'http://example.com/test_scripts.zip', 'UPLOADED');

-- Insert sample data into subtask_comment
INSERT INTO subtask_comment (subtask_id, account_id, content)
VALUES 
    ('PROJA-7', 1, 'Validation logic implemented'),
    ('PROJA-8', 2, 'Charts need more data points'),
    ('PROJB-3', 3, 'Ads placed successfully'),
    ('PROJC-3', 4, 'Data review ongoing'),
    ('PROJD-3', 5, 'Feedback collected from client'),
    ('PROJE-3', 5, 'Test scripts in progress');


-- Insert sample data into document
INSERT INTO document (project_id, task_id, title, type, template, content, file_url, is_active, created_by, updated_by)
VALUES 
    (1, 'PROJA-3', 'Project Plan', 'PLAN', 'Template_A', 'Project plan details', 'http://example.com/plan.pdf', TRUE, 1, 1),
    (2, 'PROJB-2', 'Campaign Brief', 'BRIEF', 'Template_B', 'Campaign details', 'http://example.com/brief.pdf', TRUE, 2, 2),
    (3, 'PROJC-2', 'Research Report', 'REPORT', 'Template_C', 'Research findings', 'http://example.com/report.pdf', TRUE, 3, NULL),
    (4, 'PROJD-2', 'Design Spec', 'SPEC', 'Template_D', 'Design specifications', 'http://example.com/spec.pdf', TRUE, 4, 4),
    (5, 'PROJE-2', 'Test Strategy', 'STRATEGY', 'Template_E', 'Test strategy', 'http://example.com/strategy.pdf', TRUE, 5, 5);

-- Insert sample data into document_permission
INSERT INTO document_permission (document_id, account_id, permission_type)
VALUES 
    (1, 1, 'READ'),
    (2, 2, 'WRITE'),
    (3, 3, 'READ'),
    (4, 4, 'WRITE'),
    (5, 5, 'READ');


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
    (1, 'TASK_UPDATE', 'HIGH', 'Task PROJA-4 completed', 'TASK', 1, FALSE),
    (2, 'PROJECT_UPDATE', 'MEDIUM', 'Project B started', 'PROJECT', 2, FALSE),
    (3, 'MEETING_REMINDER', 'LOW', 'Meeting tomorrow', 'MEETING', 1, TRUE),
    (4, 'RISK_ALERT', 'HIGH', 'Risk identified', 'RISK', 1, FALSE),
    (5, 'DOCUMENT_UPDATE', 'MEDIUM', 'New document added', 'DOCUMENT', 1, FALSE);

-- Insert sample data into recipient_notification
INSERT INTO recipient_notification (account_id, notification_id, status,is_read )
VALUES 
    (1, 1, 'RECEIVED', TRUE),
    (2, 2, 'RECEIVED', TRUE),
    (3, 3, 'READ', TRUE),
    (4, 4, 'RECEIVED', TRUE),
    (5, 5, 'RECEIVED', TRUE);

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
-- Insert sample data into risk with the 'due_date' column
INSERT INTO risk (risk_key, responsible_id, created_by, project_id, task_id, risk_scope, title, description, status, type, generated_by, probability, impact_level, severity_level, is_approved, due_date)
VALUES 
    ('RISK-001', 1, 1, 1, 'PROJA-3', 'SCHEDULE', 'Delay Risk', 'Possible delay in delivery', 'OPEN', 'SCHEDULE', 'AI', 'Medium', 'High', 'Moderate', FALSE, '2025-07-20 00:00:00+00'),
    ('RISK-002', 2, 2, 2, 'PROJB-2', 'BUDGET', 'Cost Overrun', 'Budget might exceed', 'OPEN', 'FINANCIAL', 'Manual', 'Low', 'Medium', 'Low', FALSE, '2025-07-25 00:00:00+00'),
    ('RISK-003', 3, 3, 3, 'PROJC-2', 'RESOURCE', 'Staff Shortage', 'Lack of resources', 'CLOSED', 'RESOURCE', 'AI', 'High', 'High', 'High', TRUE, '2025-09-10 00:00:00+00'),
    ('RISK-004', 4, 4, 4, 'PROJD-2', 'QUALITY', 'Bug Risk', 'Potential bugs', 'OPEN', 'QUALITY', 'Manual', 'Medium', 'Low', 'Low', FALSE, '2025-09-15 00:00:00+00'),
    ('RISK-005', 5, 5, 5, 'PROJE-2', 'SCOPE', 'Scope Creep', 'Expanding scope', 'OPEN', 'SCOPE', 'AI', 'Low', 'Medium', 'Moderate', FALSE, '2025-10-25 00:00:00+00');

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
    (1, 'PROJA-3', 'PERFORMANCE', 'Optimize database queries'),
    (2, 'PROJB-2', 'MARKETING', 'Increase ad spend'),
    (3, 'PROJC-2', 'RESEARCH', 'Use additional sources'),
    (4, 'PROJD-2', 'DESIGN', 'Improve color contrast'),
    (5, 'PROJE-2', 'TESTING', 'Add automation tests');

-- Insert sample data into label
INSERT INTO label (project_id, name, color_code, description, status)
VALUES 
    (1, 'BUG', '#FF0000', 'Critical issues', 'ACTIVE'),
    (2, 'ENHANCEMENT', '#00FF00', 'New features', 'ACTIVE'),
    (3, 'TASK', '#0000FF', 'General tasks', 'ACTIVE'),
    (4, 'REVIEW', '#FFFF00', 'For review', 'ACTIVE'),
    (5, 'DONE', '#00FFFF', 'Completed tasks', 'ACTIVE');

-- Insert sample data into task_label
INSERT INTO work_item_label (label_id, task_id)
VALUES 
    (1, 'PROJA-3'),
    (2, 'PROJA-4'),
    (3, 'PROJB-2'),
    (4, 'PROJC-2'),
    (5, 'PROJD-2'),
    (5, 'PROJE-2');

-- Insert sample data into requirement
INSERT INTO requirement (project_id, title, type, description, priority)
VALUES 
    (1, 'User Login', 'FUNCTIONAL', 'User must log in', 'HIGH'),
    (2, 'Ad Campaign', 'NON_FUNCTIONAL', 'Ads must be visible', 'MEDIUM'),
    (3, 'Data Collection', 'FUNCTIONAL', 'Gather research data', 'HIGH'),
    (4, 'UI Layout', 'NON_FUNCTIONAL', 'Responsive design', 'MEDIUM'),
    (5, 'Test Coverage', 'FUNCTIONAL', 'Cover all cases', 'LOW');

-- Insert sample data into project_metric
INSERT INTO project_metric (
    project_id, calculated_by, is_approved,
    planned_value, earned_value, actual_cost,
    schedule_performance_index, cost_performance_index,
    budget_at_completion, duration_at_completion,
    cost_variance, schedule_variance,
    estimate_at_completion, estimate_to_complete,
    variance_at_completion, estimate_duration_at_completion
) VALUES
    (1, 'user1', FALSE, 100000.00, 80000.00, 75000.00, 0.90, 0.95, 110000.00, 120.00, 5000.00, -20000.00, 105000.00, 25000.00, -5000.00, 125.00),
    (2, 'user2', TRUE, 50000.00, 45000.00, 48000.00, 0.85, 0.90, 55000.00, 90.00, 2000.00, -5000.00, 53000.00, 8000.00, -3000.00, 95.00),
    (3, 'user3', FALSE, 75000.00, 60000.00, 65000.00, 0.80, 0.88, 80000.00, 110.00, -5000.00, -15000.00, 82000.00, 22000.00, -2000.00, 115.00),
    (4, 'user4', TRUE, 30000.00, 28000.00, 29000.00, 0.95, 0.97, 31000.00, 60.00, -1000.00, -2000.00, 31000.00, 3000.00, -1000.00, 62.00),
    (5, 'user5', FALSE, 40000.00, 35000.00, 36000.00, 0.90, 0.92, 42000.00, 75.00, -1000.00, -5000.00, 42000.00, 6000.00, -2000.00, 78.00);

	-- Insert sample data into system_configuration
INSERT INTO system_configuration (config_key, value_config, min_value, max_value, estimate_value, description, note, effected_from, effected_to)
VALUES 
    ('max_sprint_duration', '30', '7', '60', '14', 'Maximum duration of a sprint in days', 'Adjust based on team capacity', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00'),
    ('min_sprint_tasks', '5', '1', '20', '10', 'Minimum number of tasks per sprint', 'Ensure productivity', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00'),
    ('max_tasks_per_user', '10', '1', '50', '15', 'Maximum tasks assigned per user', 'Monitor workload', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00'),
    ('priority_threshold', 'HIGH', 'LOW', 'HIGHEST', 'MEDIUM', 'Threshold for priority alerts', 'Trigger notifications', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00'),
    ('overtime_hours', '2', '0', '4', '2', 'Maximum overtime hours per day', 'Ensure compliance', '2025-01-01 00:00:00+00', '2025-12-31 00:00:00+00');


-- Insert sample data into meeting_reschedule_request
INSERT INTO meeting_reschedule_request (meeting_id, requester_id, requested_date, reason, status, pm_id, pm_proposed_date, pm_note)
VALUES 
    (1, 2, '2025-06-26 14:00:00+07', 'Conflict with another meeting', 'PENDING', 1, NULL, NULL),
    (2, 3, '2025-07-11 15:00:00+07', 'Need more preparation time', 'APPROVED', 2, '2025-07-11 15:00:00+07', 'Approved with new time'),
    (3, 4, '2025-08-16 10:00:00+07', 'Team unavailable', 'REJECTED', 3, NULL, 'Reschedule not needed'),
    (4, 5, '2025-09-13 14:00:00+07', 'Client requested delay', 'PENDING', 4, NULL, NULL),
    (5, 1, '2025-10-21 12:00:00+07', 'Technical issues', 'APPROVED', 5, '2025-10-21 12:00:00+07', 'Adjusted for technical setup');

-- Insert sample data into dynamic_category (thêm #c97cf4 vào một số mục)
INSERT INTO dynamic_category (category_group, name, label, description, order_index, icon_link, color)
VALUES 
    ('project_type', 'WEB_APPLICATION', 'Web Application', 'Web application development projects for IT companies', 1, 'https://example.com/icons/web-app.png', '#00FF00'),
    ('project_type', 'MOBILE_APPLICATION', 'Mobile Application', 'Mobile application development projects for IT companies', 2, 'https://example.com/icons/mobile-app.png', '#FF0000'),
    ('project_type', 'ENTERPRISE_SOFTWARE', 'Enterprise Software', 'Enterprise software development projects for IT companies', 3, NULL, '#c97cf4'), -- Thêm #c97cf4 vào đây
    ('project_type', 'GAME_DEVELOPMENT', 'Game Development', 'Game development projects for IT companies', 4, NULL, NULL),
    ('project_status', 'PLANNING', 'Planning', 'Project is in planning stage', 1, 'https://example.com/icons/planning.png', '#0000FF'),
    ('project_status', 'IN_PROGRESS', 'In Progress', 'Project is actively in progress', 2, NULL, NULL),
    ('project_status', 'ON_HOLD', 'On Hold', 'Project is temporarily paused', 3, NULL, NULL),
    ('project_status', 'IN_REVIEW', 'In Review', 'Project is being reviewed', 4, NULL, NULL),
    ('project_status', 'COMPLETED', 'Completed', 'Project has been successfully completed', 5, NULL, NULL),
    ('project_status', 'CANCELLED', 'Cancelled', 'Project was cancelled', 6, NULL, '#b2da73'),
	('requirement_type', 'FUNCTIONAL', 'Functional', 'Functional', 1, NULL, NULL),
    ('requirement_type', 'NON_FUNCTIONAL', 'Non Functional', 'Non Functional', 2, NULL, NULL),
	('requirement_priority', 'HIGHEST', 'Highest', 'Highest priority requirement', 1, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517097/highest_new_ys492q.svg', '#d04437'),
    ('requirement_priority', 'HIGH', 'High', 'High priority requirement', 2, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517070/high_new_urjrrl.svg', '#f15c75'),
    ('requirement_priority', 'MEDIUM', 'Medium', 'Medium priority requirement', 3, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517013/medium_new_iobtik.svg', '#f79232'),
    ('requirement_priority', 'LOW', 'Low', 'Low priority requirement', 4, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751516829/low_new_uldiwt.svg', '#707070'),
    ('requirement_priority', 'LOWEST', 'Lowest', 'Lowest priority requirement', 5, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751516920/lowest_new_q8dvr8.svg', '#999'),
    ('epic_status', 'TO_DO', 'To Do', 'Epic not started yet', 1, 'https://example.com/icons/to-do.png', '##dddee1'),
    ('epic_status', 'IN_PROGRESS', 'In Progress', 'Epic is in progress', 2, NULL, '#87b1e1'),
    ('epic_status', 'DONE', 'Done', 'Epic has been completed', 3, NULL, '#b2da73'),
    ('sprint_status', 'FUTURE', 'Future', 'Sprint not started yet', 1, NULL, NULL),
    ('sprint_status', 'ACTIVE', 'Active', 'Sprint in progress or planning phase', 2, NULL, NULL),
    ('sprint_status', 'COMPLETED', 'Completed', 'Sprint successfully completed', 3, NULL, NULL),
    ('account_role', 'PROJECT_MANAGER', 'Project Manager', 'Project manager role', 1, 'https://example.com/icons/pm.png', '#800080'),
    ('account_role', 'TEAM_LEADER', 'Team Leader', 'Team leader role', 2, NULL, NULL),
    ('account_role', 'TEAM_MEMBER', 'Team Member', 'Team member role', 3, NULL, NULL),
    ('account_role', 'CLIENT', 'Client', 'Client role', 4, NULL, NULL),
    ('account_role', 'ADMIN', 'Admin', 'Admin role', 5, NULL, NULL),
    ('account_status', 'ACTIVE', 'Active', 'Active user account', 1, NULL, NULL),
    ('account_status', 'INACTIVE', 'Inactive', 'Inactive user account', 2, NULL, NULL),
    ('account_status', 'VERIFIED', 'Verified', 'Verified user account', 3, NULL, NULL),
    ('account_status', 'UNVERIFIED', 'Unverified', 'Unverified user account', 4, NULL, NULL),
    ('account_status', 'BANNED', 'Banned', 'Banned user account', 5, NULL, NULL),
    ('account_status', 'DELETED', 'Deleted', 'Deleted user account', 6, NULL, NULL),
    ('account_position', 'BUSINESS_ANALYST', 'Business Analyst', 'Business analyst position', 1, NULL, NULL),
    ('account_position', 'BACKEND_DEVELOPER', 'Backend Developer', 'Backend developer position', 2, NULL, NULL),
    ('account_position', 'FRONTEND_DEVELOPER', 'Frontend Developer', 'Frontend developer position', 3, NULL, NULL),
    ('account_position', 'TESTER', 'Tester', 'Tester position', 4, NULL, NULL),
    ('account_position', 'PROJECT_MANAGER', 'Project Manager', 'Project manager position', 5, NULL, NULL),
    ('account_position', 'DESIGNER', 'Designer', 'Designer position', 6, NULL, '#c97cf4'), -- Thêm #c97cf4 vào đây
    ('account_position', 'TEAM_LEADER', 'Team Leader', 'Team leader position', 7, NULL, NULL),
    ('account_position', 'CLIENT', 'Client', 'Client position', 8, NULL, NULL),
    ('account_position', 'ADMIN', 'Admin', 'Admin position', 9, NULL, NULL),
    ('task_assignment_status', 'ASSIGNED', 'Assigned', 'Task assigned to user', 1, NULL, NULL),
    ('task_assignment_status', 'IN_PROGRESS', 'In Progress', 'User is actively working on the task', 2, NULL, NULL),
    ('task_assignment_status', 'BLOCKED', 'Blocked', 'User is blocked and cannot proceed', 3, NULL, NULL),
    ('task_assignment_status', 'COMPLETED', 'Completed', 'User has completed their assigned part', 4, NULL, NULL),
    ('task_assignment_status', 'UNASSIGNED', 'Unassigned', 'User is unassigned or removed from task', 5, NULL, NULL),
    ('task_assignment_status', 'DELETED', 'Deleted', 'Task assignment record is deleted', 6, NULL, NULL),
	('subtask_status', 'TO_DO', 'To Do', 'subtask_status to do', 1, NULL, '##dddee1'),
    ('subtask_status', 'IN_PROGRESS', 'In Progress', 'Checklist item in progress', 2, NULL,  '#87b1e1'),
    ('subtask_status', 'DONE', 'Done', 'Checklist item completed', 3, NULL, '#b2da73'),
    ('task_file_status', 'UPLOADED', 'Uploaded', 'File uploaded', 1, NULL, NULL),
    ('task_file_status', 'IN_REVIEW', 'In Review', 'File under review', 2, NULL, NULL),
    ('task_file_status', 'APPROVED', 'Approved', 'File approved', 3, NULL, NULL),
    ('task_file_status', 'PENDING', 'Pending', 'File pending', 4, NULL, NULL),
    ('task_file_status', 'DELETED', 'Deleted', 'Deleted file', 5, NULL, NULL),
    ('subtask_file_status', 'UPLOADED', 'Uploaded', 'File uploaded', 1, NULL, NULL),
    ('subtask_file_status', 'IN_REVIEW', 'In Review', 'File under review', 2, NULL, NULL),
    ('subtask_file_status', 'APPROVED', 'Approved', 'File approved', 3, NULL, NULL),
    ('subtask_file_status', 'PENDING', 'Pending', 'File pending', 4, NULL, NULL),
    ('subtask_file_status', 'DELETED', 'Deleted', 'Deleted file', 5, NULL, NULL),
    ('task_status', 'TO_DO', 'To Do', 'Task to do', 1, NULL, '##dddee1'),
    ('task_status', 'IN_PROGRESS', 'In Progress', 'Task in progress', 2, NULL, '#87b1e1'),
    ('task_status', 'DONE', 'Done', 'Task completed', 3, NULL, '#b2da73'),
    ('task_type', 'STORY', 'Story', 'User story tasks', 1, 'https://drive.google.com/file/d/1aCfATSVY-FdeeTNLoJl3o3k49NtP9lUg/view?usp=drive_link', NULL),
    ('task_type', 'TASK', 'Task', 'General task', 2, 'https://drive.google.com/file/d/1ebm-P9XekWL5vOYc2ErwFk5jT-dSkGSD/view?usp=drive_link', NULL),
    ('task_type', 'BUG', 'Bug', 'Bug fix tasks', 3, 'https://drive.google.com/file/d/1b7WqqObZEqSAhFa8QOQN3hLAQqkS99vS/view?usp=drive_link', NULL),
    ('document_type', 'PLAN', 'Plan', 'Project plan', 1, NULL, NULL),
    ('document_type', 'BRIEF', 'Brief', 'Project brief', 2, NULL, NULL),
    ('document_type', 'REPORT', 'Report', 'Project report', 3, NULL, NULL),
    ('document_type', 'SPEC', 'Specification', 'Project specification', 4, NULL, NULL),
    ('document_type', 'STRATEGY', 'Strategy', 'Project strategy', 5, NULL, NULL),
    ('permission_type', 'READ', 'Read', 'Read permission', 1, NULL, NULL),
    ('permission_type', 'WRITE', 'Write', 'Write permission', 2, NULL, NULL),
    ('notification_type', 'TASK_UPDATE', 'Task Update', 'Task update notification', 1, NULL, NULL),
    ('notification_type', 'PROJECT_UPDATE', 'Project Update', 'Project update notification', 2, NULL, NULL),
    ('notification_type', 'MEETING_REMINDER', 'Meeting Reminder', 'Meeting reminder notification', 3, NULL, NULL),
    ('notification_type', 'RISK_ALERT', 'Risk Alert', 'Risk alert notification', 4, NULL, NULL),
    ('notification_type', 'DOCUMENT_UPDATE', 'Document Update', 'Document update notification', 5, NULL, NULL),
    ('recipient_notification_status', 'RECEIVED', 'Received', 'Notification received', 1, NULL, NULL),
    ('recipient_notification_status', 'READ', 'Read', 'Notification read', 2, NULL, NULL),
    ('recipient_notification_status', 'DELETED', 'Deleted', 'Deleted notification', 3, NULL, NULL),
    ('meeting_status', 'SCHEDULED', 'Scheduled', 'Meeting scheduled', 1, NULL, NULL),
    ('meeting_status', 'COMPLETED', 'Completed', 'Meeting completed', 2, NULL, NULL),
    ('meeting_status', 'CANCELLED', 'Cancelled', 'Meeting cancelled', 3, NULL, NULL),
    ('meeting_status', 'DELETED', 'Deleted', 'Deleted meeting', 4, NULL, NULL),
    ('meeting_participant_status', 'ATTENDED', 'Attended', 'Participant attended', 1, NULL, NULL),
    ('meeting_participant_status', 'ABSENT', 'Absent', 'Participant absent', 2, NULL, NULL),
    ('meeting_participant_status', 'DELETED', 'Deleted', 'Deleted participant', 3, NULL, NULL),
    ('milestone_feedback_status', 'REVIEWED', 'Reviewed', 'Feedback reviewed', 1, NULL, NULL),
    ('milestone_feedback_status', 'PENDING', 'Pending', 'Feedback pending', 2, NULL, NULL),
    ('milestone_feedback_status', 'APPROVED', 'Approved', 'Feedback approved', 3, NULL, NULL),
    ('milestone_feedback_status', 'DELETED', 'Deleted', 'Deleted feedback', 4, NULL, NULL),
    ('risk_status', 'OPEN', 'Open', 'Risk open', 1, NULL, NULL),
    ('risk_status', 'CLOSED', 'Closed', 'Risk closed', 2, NULL, NULL),
    ('risk_status', 'DELETED', 'Deleted', 'Deleted risk', 3, NULL, NULL),
    ('risk_status', 'MITIGATED', 'Mitigated', 'Risk has been mitigated', 4, NULL, NULL),
    ('risk_type', 'SCHEDULE', 'Schedule', 'Schedule risk', 1, NULL, NULL),
    ('risk_type', 'FINANCIAL', 'Financial', 'Financial risk', 2, NULL, NULL),
    ('risk_type', 'RESOURCE', 'Resource', 'Resource risk', 3, NULL, NULL),
    ('risk_type', 'QUALITY', 'Quality', 'Quality risk', 4, NULL, NULL),
    ('risk_type', 'SCOPE', 'Scope', 'Scope risk', 5, NULL, NULL),
    ('risk_type', 'TECHNICAL', 'Technical', 'Technical risk', 6, NULL, NULL),
    ('risk_type', 'SECURITY', 'Security', 'Security risk', 7, NULL, NULL),
    ('change_request_status', 'PENDING', 'Pending', 'Change request pending', 1, NULL, NULL),
    ('change_request_status', 'APPROVED', 'Approved', 'Change request approved', 2, NULL, NULL),
    ('change_request_status', 'REJECTED', 'Rejected', 'Change request rejected', 3, NULL, NULL),
    ('change_request_status', 'DELETED', 'Deleted', 'Deleted change request', 4, NULL, NULL),
    ('recommendation_type', 'PERFORMANCE', 'Performance', 'Performance recommendation', 1, NULL, NULL),
    ('recommendation_type', 'MARKETING', 'Marketing', 'Marketing recommendation', 2, NULL, NULL),
    ('recommendation_type', 'RESEARCH', 'Research', 'Research recommendation', 3, NULL, NULL),
    ('recommendation_type', 'DESIGN', 'Design', 'Design recommendation', 4, NULL, NULL),
    ('recommendation_type', 'TESTING', 'Testing', 'Testing recommendation', 5, NULL, NULL),
    ('label_status', 'ACTIVE', 'Active', 'Active label', 1, NULL, NULL),
    ('label_status', 'DELETED', 'Deleted', 'Deleted label', 2, NULL, NULL),
    ('project_member_status', 'CREATED', 'Created', 'Project member newly created', 1, NULL, NULL),
    ('project_member_status', 'INVITED', 'Invited', 'Project member invited to join', 2, NULL, NULL),
    ('project_member_status', 'ACTIVE', 'Active', 'Project member active in project', 3, NULL, NULL),
    ('project_member_status', 'BANNED', 'Banned', 'Project member banned, no access', 4, NULL, NULL),
    ('project_member_status', 'DELETED', 'Deleted', 'Project member deleted from project', 5, NULL, NULL),
	('task_priority', 'HIGHEST', 'Highest', 'Highest priority requirement', 1, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517097/highest_new_ys492q.svg', '#d04437'),
    ('task_priority', 'HIGH', 'High', 'High priority requirement', 2, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517070/high_new_urjrrl.svg', '#f15c75'),
    ('task_priority', 'MEDIUM', 'Medium', 'Medium priority requirement', 3, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751517013/medium_new_iobtik.svg', '#f79232'),
    ('task_priority', 'LOW', 'Low', 'Low priority requirement', 4, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751516829/low_new_uldiwt.svg', '#707070'),
    ('task_priority', 'LOWEST', 'Lowest', 'Lowest priority requirement', 5, 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751516920/lowest_new_q8dvr8.svg', '#999'),
    ('subtask_priority', 'HIGHEST', 'Highest', 'Highest priority subtask', 1, NULL, NULL),
    ('subtask_priority', 'HIGH', 'High', 'High priority subtask', 2, NULL, NULL),
    ('subtask_priority', 'MEDIUM', 'Medium', 'Medium priority subtask', 3, NULL, NULL),
    ('subtask_priority', 'LOW', 'Low', 'Low priority subtask', 4, NULL, NULL),
    ('subtask_priority', 'LOWEST', 'Lowest', 'Lowest priority subtask', 5, NULL, NULL),
    ('milestone_status', 'PLANNING', 'Planning', 'Milestone is in planning stage', 1, NULL, NULL),
    ('milestone_status', 'IN_PROGRESS', 'In Progress', 'Milestone is currently being worked on', 2, NULL, NULL),
    ('milestone_status', 'AWAITING_REVIEW', 'Awaiting Client Review', 'Milestone completed and pending client approval', 3, NULL, NULL),
    ('milestone_status', 'APPROVED', 'Approved by Client', 'Milestone has been reviewed and approved by the client', 4, NULL, NULL),
    ('milestone_status', 'REJECTED', 'Rejected by Client', 'Milestone was reviewed and rejected by the client', 5, NULL, NULL),
    ('milestone_status', 'ON_HOLD', 'On Hold', 'Milestone is temporarily paused', 6, NULL, NULL),
    ('milestone_status', 'CANCELLED', 'Cancelled', 'Milestone has been cancelled', 7, NULL, NULL);
    ('task-dependency_type', 'FINISH_START', 'Finish-to-Start', 'Task must finish before next task starts', 1, NULL, NULL),
    ('task-dependency_type', 'START_START', 'Start-to-Start', 'Task must start before next task starts', 2, NULL, NULL),
    ('task-dependency_type', 'FINISH_FINISH', 'Finish-to-Finish', 'Task must finish before next task finishes', 3, NULL, NULL),
    ('task-dependency_type', 'START_FINISH', 'Start-to-Finish', 'Task must start before next task finishes', 4, NULL, NULL),
    ('activity_log_action_type', 'UPDATE', 'Update', 'Record update action', 1, NULL, NULL),
    ('activity_log_action_type', 'DELETE', 'Delete', 'Record deletion action', 2, NULL, NULL),
    ('activity_log_action_type', 'STATUS_CHANGE', 'Status Change', 'Record status change action', 3, NULL, NULL),
    ('activity_log_action_type', 'COMMENT', 'Comment', 'Record comment action', 4, NULL, NULL),
    ('activity_log_related_entity_type', 'TASK', 'Task', 'Task related entity', 1, NULL, NULL),
    ('activity_log_related_entity_type', 'PROJECT', 'Project', 'Project related entity', 2, NULL, NULL),
    ('activity_log_related_entity_type', 'COMMENT', 'Comment', 'Comment related entity', 3, NULL, NULL),
    ('activity_log_related_entity_type', 'FILE', 'File', 'File related entity', 4, NULL, NULL),
    ('activity_log_related_entity_type', 'NOTIFICATION', 'Notification', 'Notification related entity', 5, NULL, NULL),
    ('risk_scope', 'PROJECT', 'Project', 'Risk that affects the whole project', 1, NULL, '#2f54eb'),
    ('risk_scope', 'TASK', 'Task', 'Risk that affects a specific task', 2, NULL, '#faad14');

-------  INTELLIPM DB ---------
	-- Update 16/06/2025


	-- Update 19/06/2025
-- Insert sample data for the project "Online Flower Shop"
INSERT INTO project (project_key, name, description, budget, project_type, created_by, start_date, end_date, icon_url, status)
VALUES 
    ('FLOWER', 'Online Flower Shop', 'Develop an e-commerce website for selling fresh flowers online, featuring product categories (occasion flowers, bouquets, potted plants), a shopping cart, online payment, and order management. The website will have a user-friendly interface, integrated promotion system, and support fast delivery within 24 hours.', 1500000.00, 'WEB_APPLICATION', 1, '2025-06-19 00:00:00+07', '2025-12-19 00:00:00+07', 'https://res.cloudinary.com/didnsp4p0/image/upload/v1751518181/iconProject5_xejcga.svg', 'IN_PROGRESS');

-- Insert sample data for project_member (at least 10 members)
INSERT INTO project_member (account_id, project_id, joined_at, invited_at, status)
VALUES 
    (1, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-10 00:00:00+07', 'IN_PROGRESS'), -- Admin
    (2, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-11 00:00:00+07', 'IN_PROGRESS'), -- Team Leader
    (4, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-12 00:00:00+07', 'IN_PROGRESS'), -- Project Manager
    (5, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-13 00:00:00+07', 'IN_PROGRESS'), -- Frontend Developer
    (6, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-14 00:00:00+07', 'IN_PROGRESS'), -- Frontend Developer
    (7, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-15 00:00:00+07', 'IN_PROGRESS'), -- Frontend Developer
    (8, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-16 00:00:00+07', 'IN_PROGRESS'), -- Backend Developer
    (9, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-17 00:00:00+07', 'IN_PROGRESS'), -- Backend Developer
    (12, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-18 00:00:00+07', 'IN_PROGRESS'), -- Tester
    (19, (SELECT id FROM project WHERE project_key = 'FLOWER'), '2025-06-19 00:00:00+07', '2025-06-18 00:00:00+07', 'IN_PROGRESS'); -- Designer

-- Insert sample data for project_position
INSERT INTO project_position (project_member_id, position, assigned_at)
VALUES 
    ((SELECT id FROM project_member WHERE account_id = 1 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'ADMIN', '2025-06-19 00:00:00+07'),
    ((SELECT id FROM project_member WHERE account_id = 2 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'TEAM_LEADER', '2024-06-19 00:00:00+07'),
    ((SELECT id FROM project_member WHERE account_id = 4 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'PROJECT_MANAGER', '2025-06-19 00:00:00+07'),
    ((SELECT id FROM project_member WHERE account_id = 5 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'FRONTEND_DEVELOPER', '2025-06-19 00:00:00+07'),
    ((SELECT id FROM project_member WHERE account_id = 6 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'FRONTEND_DEVELOPER', '2025-06-19 00:00:00+07'),
    ((SELECT id FROM project_member WHERE account_id = 7 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'FRONTEND_DEVELOPER', '2025-06-19 00:00:00+07'),
    ((SELECT id FROM project_member WHERE account_id = 8 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'BACKEND_DEVELOPER', '2025-06-19 00:00:00+07'),
    ((SELECT id FROM project_member WHERE account_id = 9 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'BACKEND_DEVELOPER', '2025-06-19 00:00:00+07'),
    ((SELECT id FROM project_member WHERE account_id = 12 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'TESTER', '2025-06-19 00:00:00+07'),
    ((SELECT id FROM project_member WHERE account_id = 19 AND project_id = (SELECT id FROM project WHERE project_key = 'FLOWER')), 'DESIGNER', '2025-06-19 00:00:00+07');

-- Insert sample data for requirement
INSERT INTO requirement (project_id, title, type, description, priority)
VALUES 
    ((SELECT id FROM project WHERE project_key = 'FLOWER'), 'User Registration', 'FUNCTIONAL', 'Users can register an account using email and password.', 'HIGH'),
    ((SELECT id FROM project WHERE project_key = 'FLOWER'), 'Product Catalog', 'FUNCTIONAL', 'Display a product catalog including occasion flowers, bouquets, and potted plants with images and prices.', 'HIGH'),
    ((SELECT id FROM project WHERE project_key = 'FLOWER'), 'Shopping Cart', 'FUNCTIONAL', 'Allow users to add products to a shopping cart and edit quantities.', 'MEDIUM'),
    ((SELECT id FROM project WHERE project_key = 'FLOWER'), 'Payment Integration', 'NON_FUNCTIONAL', 'Integrate online payment via MoMo and credit cards with SSL security.', 'HIGH'),
    ((SELECT id FROM project WHERE project_key = 'FLOWER'), 'Order Management', 'FUNCTIONAL', 'Provide an order management system for admins and track delivery status.', 'MEDIUM');

	------------------------------------------------------------------------------------------
	-- Insert sample data for the project "Online Course Platform"
-- Insert project
INSERT INTO project (project_key, name, description, budget, project_type, created_by, start_date, end_date, icon_url,status)
VALUES 
(
    'COURSE',
    'Online Course Platform',
    'Develop a web-based platform for managing and delivering online courses. The system supports course creation, student enrollment, video lectures, quizzes, grading, and progress tracking. It will have role-based access for Admin, Instructor, and Student, and support responsive UI.',
    1200000.00,
    'WEB_APPLICATION',
    1,
    '2025-06-20 00:00:00+07',
    '2025-12-20 00:00:00+07',
	'https://res.cloudinary.com/didnsp4p0/image/upload/v1751518212/iconProject6_nd1hhy.svg',
    'IN_PROGRESS'
);

-- Insert project members
INSERT INTO project_member (account_id, project_id, joined_at, invited_at, status)
VALUES 
(1, (SELECT id FROM project WHERE project_key = 'COURSE'), '2025-06-20', '2025-06-10', 'IN_PROGRESS'),
(2, (SELECT id FROM project WHERE project_key = 'COURSE'), '2025-06-20', '2025-06-11', 'IN_PROGRESS'),
(3, (SELECT id FROM project WHERE project_key = 'COURSE'), '2025-06-20', '2025-06-12', 'IN_PROGRESS'),
(4, (SELECT id FROM project WHERE project_key = 'COURSE'), '2025-06-20', '2025-06-13', 'IN_PROGRESS'),
(5, (SELECT id FROM project WHERE project_key = 'COURSE'), '2025-06-20', '2025-06-14', 'IN_PROGRESS'),
(6, (SELECT id FROM project WHERE project_key = 'COURSE'), '2025-06-20', '2025-06-15', 'IN_PROGRESS'),
(7, (SELECT id FROM project WHERE project_key = 'COURSE'), '2025-06-20', '2025-06-16', 'IN_PROGRESS'),
(8, (SELECT id FROM project WHERE project_key = 'COURSE'), '2025-06-20', '2025-06-17', 'IN_PROGRESS'),
(9, (SELECT id FROM project WHERE project_key = 'COURSE'), '2025-06-20', '2025-06-18', 'IN_PROGRESS');

-- Insert project positions
INSERT INTO project_position (project_member_id, position, assigned_at)
VALUES 
((SELECT id FROM project_member WHERE account_id = 1 AND project_id = (SELECT id FROM project WHERE project_key = 'COURSE')), 'ADMIN', '2025-06-20'),
((SELECT id FROM project_member WHERE account_id = 2 AND project_id = (SELECT id FROM project WHERE project_key = 'COURSE')), 'TEAM_LEADER', '2025-06-20'),
((SELECT id FROM project_member WHERE account_id = 3 AND project_id = (SELECT id FROM project WHERE project_key = 'COURSE')), 'PROJECT_MANAGER', '2025-06-20'),
((SELECT id FROM project_member WHERE account_id = 4 AND project_id = (SELECT id FROM project WHERE project_key = 'COURSE')), 'FRONTEND_DEVELOPER', '2025-06-20'),
((SELECT id FROM project_member WHERE account_id = 5 AND project_id = (SELECT id FROM project WHERE project_key = 'COURSE')), 'FRONTEND_DEVELOPER', '2025-06-20'),
((SELECT id FROM project_member WHERE account_id = 6 AND project_id = (SELECT id FROM project WHERE project_key = 'COURSE')), 'BACKEND_DEVELOPER', '2025-06-20'),
((SELECT id FROM project_member WHERE account_id = 7 AND project_id = (SELECT id FROM project WHERE project_key = 'COURSE')), 'BACKEND_DEVELOPER', '2025-06-20'),
((SELECT id FROM project_member WHERE account_id = 8 AND project_id = (SELECT id FROM project WHERE project_key = 'COURSE')), 'QA_TESTER', '2025-06-20'),
((SELECT id FROM project_member WHERE account_id = 9 AND project_id = (SELECT id FROM project WHERE project_key = 'COURSE')), 'UX_UI_DESIGNER', '2025-06-20');

-- Insert project requirements
INSERT INTO requirement (project_id, title, type, description, priority)
VALUES 
((SELECT id FROM project WHERE project_key = 'COURSE'), 'User Registration and Login', 'FUNCTIONAL', 'Users can register and log in with email/password or Google.', 'HIGH'),
((SELECT id FROM project WHERE project_key = 'COURSE'), 'Course Creation by Instructors', 'FUNCTIONAL', 'Instructors can create and manage their own courses.', 'HIGH'),
((SELECT id FROM project WHERE project_key = 'COURSE'), 'Video Lecture Streaming', 'NON_FUNCTIONAL', 'Stream high-quality videos hosted on cloud storage.', 'MEDIUM'),
((SELECT id FROM project WHERE project_key = 'COURSE'), 'Quiz and Grading System', 'FUNCTIONAL', 'Students can take quizzes and receive scores instantly.', 'HIGH'),
((SELECT id FROM project WHERE project_key = 'COURSE'), 'Progress Tracking', 'FUNCTIONAL', 'Students and instructors can view progress reports.', 'MEDIUM');

-- Check 20/07/2025