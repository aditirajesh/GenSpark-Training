--Phase 1: Table Creation 
--1)Students 
CREATE TABLE Students(
	student_id SERIAL PRIMARY KEY,
	student_name TEXT,
	email TEXT,
	contact BIGINT
);

--2)Courses 
CREATE TABLE Courses(
	course_id SERIAL PRIMARY KEY,
	course_name TEXT,
	category TEXT,
	duration_days INT
);

--3)Trainers
CREATE TABLE Trainers(
	trainer_id SERIAL PRIMARY KEY,
	trainer_name TEXT,
	expertise TEXT
);

--4)Enrollments
CREATE TABLE Enrollments(
	enrollment_id SERIAL PRIMARY KEY,
	student_id INT REFERENCES Students(student_id),
	course_id INT REFERENCES Courses(course_id),
	enroll_date TIMESTAMP
);

--5)Assignments
CREATE TABLE Assignments(
	course_id INT REFERENCES Courses(course_id),
	trainer_id INT REFERENCES Trainers(trainer_id),
	PRIMARY KEY (course_id, trainer_id)
);

--6)Certificates
CREATE TABLE Certificates(
	certificate_id SERIAL PRIMARY KEY,
	enrollment_id INT REFERENCES Enrollments(enrollment_id),
	issue_date TIMESTAMP,
	serial_no INT
);

--Phase 2: Inserts and Indexes 
--1)Insert into student
INSERT INTO Students (student_name, email, contact) VALUES
('Alice Johnson', 'alice@example.com', 1234567890),
('Bob Smith', 'bob@example.com', 1987654321),
('Charlie Brown', 'charlie@example.com', 1122334455),
('Diana Prince', 'diana@example.com', 2233445566),
('Ethan Hunt', 'ethan@example.com', 3344556677);

--2)Insert into Courses
INSERT INTO Courses (course_name, category, duration_days) VALUES
('Python Programming', 'Software Development', 30),
('Data Science Basics', 'Data Science', 45),
('Web Development', 'Software Development', 60),
('Machine Learning', 'AI/ML', 50),
('Project Management', 'Business', 20);

--3)Insert into Trainers
INSERT INTO Trainers (trainer_name, expertise) VALUES
('John Doe', 'Python, Web Development'),
('Sarah Lee', 'Data Science'),
('Mark Spencer', 'Machine Learning'),
('Nina Gomez', 'Project Management'),
('Tom Hardy', 'Software Engineering');

--4)Insert into Assignments
INSERT INTO Assignments (course_id, trainer_id) VALUES
(1, 1),
(2, 2),
(3, 1),
(4, 3),
(5, 4),
(2, 3), 
(3, 5); 

--5)Insert into Enrollments
INSERT INTO Enrollments (student_id, course_id, enroll_date) VALUES
(1, 1, '2024-01-15 10:00:00'),
(2, 1, '2024-01-16 11:30:00'),
(3, 2, '2024-01-20 09:00:00'),
(4, 3, '2024-01-25 14:00:00'),
(5, 4, '2024-02-01 13:00:00'),
(1, 5, '2024-02-03 15:00:00'),
(2, 3, '2024-02-10 12:30:00'),
(3, 5, '2024-02-12 10:45:00'),
(4, 1, '2024-02-15 16:20:00'),
(5, 2, '2024-02-18 17:00:00');

--6)Insert into Certificates
INSERT INTO Certificates (enrollment_id, issue_date, serial_no) VALUES
(1, '2024-03-01 10:00:00', 1001),
(3, '2024-03-03 11:00:00', 1002),
(5, '2024-03-05 12:00:00', 1003),
(7, '2024-03-08 13:00:00', 1004),
(9, '2024-03-10 14:00:00', 1005);

--7)Index on student id
CREATE INDEX idx_studentid ON Students(student_id);

--8)Index on student email
CREATE INDEX idx_studentemail ON Students(email);

--9)Index on course id
CREATE INDEX idx_courseid ON Courses(course_id);

--Phase 3: SQL Joins 

--1)List students and the courses they enrolled in
SELECT s.student_id,s.student_name,c.course_id,c.course_name
from students s 
INNER JOIN enrollments e 
ON s.student_id = e.student_id
INNER JOIN courses c 
ON e.course_id = c.course_id;

--2)Show students who received certificates with trainer names
SELECT 
  s.student_id,
  s.student_name,
  cer.certificate_id,
  t.trainer_name
FROM students s
JOIN enrollments e ON s.student_id = e.student_id
JOIN certificates cer ON e.enrollment_id = cer.enrollment_id
JOIN (
    SELECT DISTINCT ON (course_id) course_id, trainer_id
    FROM assignments
    ORDER BY course_id, trainer_id 
) a ON e.course_id = a.course_id
JOIN trainers t ON a.trainer_id = t.trainer_id;

--3)Count number of students per course 
SELECT c.course_id,c.course_name, COUNT(e.student_id) as StudentCount
FROM courses c
LEFT JOIN enrollments e
ON c.course_id = e.course_id
GROUP BY c.course_id,c.course_name;

--Phase 4: Functions and Stored Procedures
--1)Function: Return a list of students who completed the given course and received certificates.

CREATE OR REPLACE FUNCTION get_certified_students(f_courseid INT)
RETURNS TABLE (student_id INT, student_name TEXT)
LANGUAGE plpgsql
AS $$
BEGIN 
	return QUERY 
	SELECT s.student_id,s.student_name
	from students s
	INNER JOIN enrollments e
	ON s.student_id = e.student_id
	INNER JOIN certificates cer
	ON e.enrollment_id = cer.enrollment_id
	WHERE e.course_id = f_courseid;
END;
$$;

SELECT * FROM get_certified_students(1);

--2)Procedure: Inserts into `enrollments` and conditionally adds a certificate if completed (simulate with status flag).
CREATE OR REPLACE PROCEDURE sp_enroll_student(p_studentid INT, p_courseid INT, p_enrolldate TIMESTAMP, statusflag INT)
LANGUAGE plpgsql 
AS $$
DECLARE 
	p_enrollmentid INT;
BEGIN 
	IF p_studentid IN (SELECT student_id from Students) AND p_courseid IN (SELECT course_id from Courses) THEN
		INSERT INTO Enrollments(student_id,course_id,enroll_date) VALUES (p_studentid,p_courseid,p_enrolldate);
		p_enrollmentid := (SELECT max(enrollment_id) from Enrollments);
		RAISE NOTICE 'Student id: % ENROLLED INTO Course: % ',p_studentid,p_courseid;
		IF statusflag = 1 THEN 
			INSERT INTO Certificates(enrollment_id,issue_date) VALUES(p_enrollmentid,NOW());
			RAISE NOTICE 'Student id: % ISSUED CERTIFICATE FOR Course: %',p_studentid,p_courseid;
			
		END IF;
	ELSE 
		RAISE NOTICE 'please enter valid student or course id.';
	END IF;
END;
$$;

CALL sp_enroll_student(1,5,'2025-03-03',1);
select * from enrollments;
select * from certificates;

CALL sp_enroll_student(2,4,'2025-04-01',0);
select * from enrollments;

--Phase 5: Cursors 
CREATE OR REPLACE PROCEDURE sp_nocertificates(p_courseid INT)
LANGUAGE plpgsql
AS $$
DECLARE 
	s_name TEXT;
	s_email TEXT;
	cur_getstudent CURSOR FOR (
		SELECT  s.student_name as studentname, s.email as email
		FROM students s 
		JOIN enrollments e 
		ON s.student_id = e.student_id
		LEFT JOIN certificates cer
		ON e.enrollment_id = cer.enrollment_id
		WHERE cer.certificate_id IS NULL AND e.course_id = p_courseid
	);
BEGIN 
	open cur_getstudent;
	loop
		FETCH NEXT FROM cur_getstudent INTO s_name,s_email;
		EXIT WHEN NOT FOUND;

		RAISE NOTICE 'Student Name: %, Student Email: %',s_name,s_email;
	end loop;
	close cur_getstudent;
END;
$$;

CALL sp_nocertificates(1);

--Phase 6:Security and Roles
/* 1)Create a readonly user
pg_ctl -D "C:/Program Files/PostgreSQL/17/data" -o "-p 5433" -l "C:/Program Files/PostgreSQL/17/data/logfile.log" start
psql -p 5433 -d postgres -U postgres
create role readonly login password 'rol123';
grant connect on database postgres to readonly;
GRANT SELECT ON Students, Courses, Certificates TO readonly;

seeing if the select works:
 select * from students;
 student_id | student_name  |        email        |  contact
------------+---------------+---------------------+------------
          1 | Alice Johnson | alice@example.com   | 1234567890
          2 | Bob Smith     | bob@example.com     | 1987654321
          3 | Charlie Brown | charlie@example.com | 1122334455
          4 | Diana Prince  | diana@example.com   | 2233445566
          5 | Ethan Hunt    | ethan@example.com   | 3344556677

postgres=> select * from trainers;
ERROR:  permission denied for table trainers

2)Create data_entry_user role
psql -p 5433 -d postgres -U postgres
create role data_user_entry login password 'datuser123';
GRANT connect on database postgres to readonly;
GRANT INSERT ON Students,Enrollments to data_user_entry;
GRANT USAGE, SELECT ON SEQUENCE students_student_id_seq to data_user_entry;
 
Testing insert:
insert into Students(student_name,email,contact) VALUES('Aditi Rajesh','aditi@example.com',894729101);
INSERT 0 1

insert on certificates:
postgres=> insert into Certificates VALUES(10,5,NOW(),1010);
ERROR:  permission denied for table certificates
*/


--Phase 7: Transactions and Atomicity 
-- Enroll student, issue certificate, fail if certificate generation fails

CREATE OR REPLACE PROCEDURE sp_enrollandissue(p_studentid int, p_courseid int, p_enrolldate TIMESTAMP, statusflag INT)
LANGUAGE plpgsql 
AS $$
DECLARE 
	p_enrollmentid INT;
BEGIN 
	BEGIN 
		IF p_studentid IN (SELECT student_id from Students) AND p_courseid IN (SELECT course_id from Courses) THEN 
			INSERT INTO Enrollments(student_id,course_id,enroll_date) VALUES(p_studentid,p_courseid,p_enrolldate);
			RAISE NOTICE 'Student ID:% ENROLLED INTO Course ID:%',p_studentid,p_courseid;
			p_enrollmentid := (SELECT max(enrollment_id) from Enrollments);

			IF statusflag = 1 THEN 
				INSERT INTO Certificates(enrollment_id,issue_date) VALUES(p_enrollmentid,NOW());
				RAISE NOTICE 'Student ID:% ISSUED CERTIFICATE FOR COURSE ID:%',p_studentid,p_courseid;
			ELSE 
				RAISE NOTICE 'Certificate generation has failed. Rolling back...';
				ROLLBACK;

			END IF;	
		ELSE 
			RAISE NOTICE 'Invalid Student or Course ID. Try again';
			ROLLBACK;
		END IF;
	END;
END;
$$;

CALL sp_enrollandissue(2,5,'2025-02-02',1) --enrollment successful, no rollback
select * from enrollments e 
inner join certificates c
on e.enrollment_id = c.enrollment_id;

CALL sp_enrollandissue(3,2,'2025-02-01',0) --enrollment successful, but certificate gen failed. entire transaction rolled back. 

