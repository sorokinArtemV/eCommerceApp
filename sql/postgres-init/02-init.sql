CREATE TABLE public."Users"
(
    "user_id" uuid NOT NULL,
    "person_name" character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "email" character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "password" character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "gender" character varying(15) COLLATE pg_catalog."default",
    CONSTRAINT "Users_pkey" PRIMARY KEY ("user_id")
);

INSERT INTO "Users" (user_id, person_name, email, password, gender)
VALUES
('c32f8b42-60e6-4c02-90a7-9143ab37189f', 'Polina', 'polina@example.com', 'password123', 'Female'),
('8ff22c7d-18c7-4ef0-a0ac-988ecb2ac7f5', 'Artem', 'artem@example.com', 'password123', 'Male');