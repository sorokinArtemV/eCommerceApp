CREATE TABLE public."Users"
(
    "user_id" uuid NOT NULL,
    "person_name" character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "email" character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "password" character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "gender" character varying(15) COLLATE pg_catalog."default",
    CONSTRAINT "Users_pkey" PRIMARY KEY ("user_id")
);