DROP TABLE IF EXISTS test_pgsqltype;
CREATE TABLE test_pgsqltype
(
    sysid uuid,
    bizid bigint DEFAULT nextval('bizid_test_pgsqltype_seq') NOT NULL,
    arraytype character varying(1024)[],
    jsontype json,
    PRIMARY KEY (sysid)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

create sequence bizid_test_pgsqltype_seq;