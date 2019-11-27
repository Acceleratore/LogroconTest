CREATE TABLE logrocon.Officer
(
    ID serial NOT NULL,
    FirstName character varying COLLATE pg_catalog."default" NOT NULL,
    Surname character varying COLLATE pg_catalog."default",
    Patronymic character varying COLLATE pg_catalog."default",
    BirthDate date,
    CONSTRAINT Officer_pkey PRIMARY KEY (ID)
);

ALTER TABLE logrocon.Officer
    OWNER to postgres;

GRANT ALL ON TABLE logrocon.Officer TO postgres WITH GRANT OPTION;



CREATE TABLE logrocon.Posts
(
    ID serial NOT NULL,
    NamePost character varying(200) NOT NULL,
    Grade smallint,
    PRIMARY KEY (ID)
);

ALTER TABLE logrocon.Posts OWNER to postgres;

GRANT ALL ON TABLE logrocon.Posts TO postgres WITH GRANT OPTION;


CREATE TABLE logrocon.officer_to_posts
(
    id_officer integer NOT NULL,
    id_post integer NOT NULL,
    CONSTRAINT "IND_OFFICER_POSTS_UNIQ" UNIQUE (id_officer, id_post)
,
    CONSTRAINT "FK_officer_id" FOREIGN KEY (id_officer)
        REFERENCES logrocon.officer (id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
);

ALTER TABLE logrocon.Officer_to_posts
    OWNER to postgres;

GRANT ALL ON TABLE logrocon.Officer_to_posts TO postgres WITH GRANT OPTION;