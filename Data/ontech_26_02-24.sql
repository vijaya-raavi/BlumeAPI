--
-- PostgreSQL database dump
--

-- Dumped from database version 16.1
-- Dumped by pg_dump version 16.1

-- Started on 2024-02-26 12:27:30

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 4 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: pg_database_owner
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO pg_database_owner;

--
-- TOC entry 5003 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--

COMMENT ON SCHEMA public IS 'standard public schema';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 223 (class 1259 OID 25125)
-- Name: ohd_company; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_company (
    id bigint NOT NULL,
    name character varying(55) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_company OWNER TO postgres;

--
-- TOC entry 5004 (class 0 OID 0)
-- Dependencies: 223
-- Name: TABLE ohd_company; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_company IS 'Lists all companies';


--
-- TOC entry 231 (class 1259 OID 25352)
-- Name: ohd_company_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_company ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_company_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 222 (class 1259 OID 25113)
-- Name: ohd_configuration; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_configuration (
    id bigint NOT NULL,
    name character varying(55) NOT NULL,
    value character varying(55) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_configuration OWNER TO postgres;

--
-- TOC entry 5005 (class 0 OID 0)
-- Dependencies: 222
-- Name: TABLE ohd_configuration; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_configuration IS 'lists the all configurations related to application ';


--
-- TOC entry 232 (class 1259 OID 25353)
-- Name: ohd_configuration_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_configuration ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_configuration_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 221 (class 1259 OID 25096)
-- Name: ohd_document; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_document (
    id bigint NOT NULL,
    document_type smallint NOT NULL,
    url character varying(500) NOT NULL,
    title character varying(100) NOT NULL,
    extension character varying(10) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_document OWNER TO postgres;

--
-- TOC entry 5006 (class 0 OID 0)
-- Dependencies: 221
-- Name: TABLE ohd_document; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_document IS 'all documents';


--
-- TOC entry 233 (class 1259 OID 25354)
-- Name: ohd_document_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_document ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_document_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 220 (class 1259 OID 25084)
-- Name: ohd_document_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_document_type (
    id smallint NOT NULL,
    name character varying(55) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_document_type OWNER TO postgres;

--
-- TOC entry 5007 (class 0 OID 0)
-- Dependencies: 220
-- Name: TABLE ohd_document_type; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_document_type IS 'allowed documents to upload';


--
-- TOC entry 234 (class 1259 OID 25355)
-- Name: ohd_document_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_document_type ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_document_type_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 218 (class 1259 OID 25056)
-- Name: ohd_enum_communications; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_enum_communications (
    id smallint NOT NULL,
    name character varying NOT NULL,
    display_name character varying NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_enum_communications OWNER TO postgres;

--
-- TOC entry 5008 (class 0 OID 0)
-- Dependencies: 218
-- Name: TABLE ohd_enum_communications; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_communications IS 'list the communication types';


--
-- TOC entry 235 (class 1259 OID 25356)
-- Name: ohd_enum_communications_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_enum_communications ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_enum_communications_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 216 (class 1259 OID 25028)
-- Name: ohd_enum_meter_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_enum_meter_type (
    id smallint NOT NULL,
    name character varying(55) NOT NULL,
    display_value character varying(55) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_enum_meter_type OWNER TO postgres;

--
-- TOC entry 5009 (class 0 OID 0)
-- Dependencies: 216
-- Name: TABLE ohd_enum_meter_type; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_meter_type IS 'lists the meter types';


--
-- TOC entry 236 (class 1259 OID 25357)
-- Name: ohd_enum_meter_type_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_enum_meter_type ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."ohd_enum_meter_type_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 215 (class 1259 OID 25019)
-- Name: ohd_enum_status; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_enum_status (
    id smallint NOT NULL,
    name character varying(55) NOT NULL,
    display_value character varying(55) NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_enum_status OWNER TO postgres;

--
-- TOC entry 5010 (class 0 OID 0)
-- Dependencies: 215
-- Name: TABLE ohd_enum_status; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_status IS 'Lists all possible status values for all tables';


--
-- TOC entry 247 (class 1259 OID 33565)
-- Name: ohd_enum_status_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_enum_status ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_enum_status_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 217 (class 1259 OID 25042)
-- Name: ohd_enum_title; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_enum_title (
    id smallint NOT NULL,
    name character varying(55) NOT NULL,
    display_value character varying(55) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_enum_title OWNER TO postgres;

--
-- TOC entry 5011 (class 0 OID 0)
-- Dependencies: 217
-- Name: TABLE ohd_enum_title; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_title IS 'Lists all possible name titles';


--
-- TOC entry 237 (class 1259 OID 25359)
-- Name: ohd_enum_title_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_enum_title ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_enum_title_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 219 (class 1259 OID 25072)
-- Name: ohd_enum_user_relation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_enum_user_relation (
    id smallint NOT NULL,
    name character varying(55) NOT NULL,
    display_name character varying(55) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_enum_user_relation OWNER TO postgres;

--
-- TOC entry 5012 (class 0 OID 0)
-- Dependencies: 219
-- Name: TABLE ohd_enum_user_relation; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_user_relation IS 'lists the relation of user with property';


--
-- TOC entry 248 (class 1259 OID 33581)
-- Name: ohd_enum_user_relation_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_enum_user_relation ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_enum_user_relation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 230 (class 1259 OID 25323)
-- Name: ohd_meter; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_meter (
    id bigint NOT NULL,
    property_id bigint NOT NULL,
    meter_number character varying NOT NULL,
    meter_type_id smallint NOT NULL,
    meter_alias character varying NOT NULL,
    daily_target_consumption integer NOT NULL,
    tax_number bigint NOT NULL,
    contract_proof_document bigint,
    contract_end_date timestamp without time zone NOT NULL,
    address_line_1 character varying(100),
    address_line_2 character varying(100),
    city character varying(55),
    state character varying(55),
    country character varying(55),
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_meter OWNER TO postgres;

--
-- TOC entry 238 (class 1259 OID 25361)
-- Name: ohd_meter_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_meter ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_meter_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 224 (class 1259 OID 25137)
-- Name: ohd_mobile_country_code; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_mobile_country_code (
    id bigint NOT NULL,
    code character varying(5) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_mobile_country_code OWNER TO postgres;

--
-- TOC entry 5013 (class 0 OID 0)
-- Dependencies: 224
-- Name: TABLE ohd_mobile_country_code; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_mobile_country_code IS 'Lists all possible mobile country codes';


--
-- TOC entry 239 (class 1259 OID 25362)
-- Name: ohd_mobile_country_code_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_mobile_country_code ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_mobile_country_code_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 245 (class 1259 OID 25373)
-- Name: ohd_otp; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_otp (
    "Id" bigint NOT NULL,
    "Otp" character varying(10) NOT NULL,
    "Email" character varying(100) NOT NULL,
    "StatusId" smallint,
    created_at timestamp without time zone,
    modified_at timestamp without time zone,
    "CountryCodeId" bigint,
    "companyId" bigint,
    "MobileNumber" character varying(15) NOT NULL
);


ALTER TABLE public.ohd_otp OWNER TO postgres;

--
-- TOC entry 246 (class 1259 OID 25398)
-- Name: ohd_otp_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_otp ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."ohd_otp_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 228 (class 1259 OID 25223)
-- Name: ohd_property; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_property (
    id bigint NOT NULL,
    name character varying(55) NOT NULL,
    unit_number character varying(55) NOT NULL,
    owner_id bigint NOT NULL,
    company_id bigint NOT NULL,
    status_id smallint NOT NULL,
    address_lattitude real,
    address_longitude real,
    address_line_1 character varying(100) NOT NULL,
    address_line_2 character varying(100) NOT NULL,
    city character varying(55) NOT NULL,
    state character varying(55) NOT NULL,
    country character varying(55) NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_property OWNER TO postgres;

--
-- TOC entry 240 (class 1259 OID 25363)
-- Name: ohd_property_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_property ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_property_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 229 (class 1259 OID 25297)
-- Name: ohd_property_user_relation; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_property_user_relation (
    id bigint NOT NULL,
    user_id bigint NOT NULL,
    property_id bigint NOT NULL,
    user_relation_id smallint NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_property_user_relation OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 25364)
-- Name: ohd_property_user_relation_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_property_user_relation ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_property_user_relation_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 227 (class 1259 OID 25169)
-- Name: ohd_user; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_user (
    id bigint NOT NULL,
    title smallint,
    first_name character varying(55),
    last_name character varying(55),
    mobile_country_code smallint,
    company_id bigint NOT NULL,
    mobile character varying(20) NOT NULL,
    email character varying(100) NOT NULL,
    password character varying(500) NOT NULL,
    role_id smallint NOT NULL,
    address_line_1 character varying(100),
    address_line_2 character varying(100),
    city character varying(55),
    state character varying(55),
    country character varying(55),
    communication_setting integer,
    status_id smallint DEFAULT 1 NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    address_lattitude character varying(200)[],
    address_longitude character varying(200)[],
    proof_document_id bigint,
    profile_url character varying
);


ALTER TABLE public.ohd_user OWNER TO postgres;

--
-- TOC entry 5014 (class 0 OID 0)
-- Dependencies: 227
-- Name: TABLE ohd_user; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_user IS 'User';


--
-- TOC entry 5015 (class 0 OID 0)
-- Dependencies: 227
-- Name: COLUMN ohd_user.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.id IS 'User ID';


--
-- TOC entry 5016 (class 0 OID 0)
-- Dependencies: 227
-- Name: COLUMN ohd_user.mobile; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.mobile IS 'Mobile';


--
-- TOC entry 5017 (class 0 OID 0)
-- Dependencies: 227
-- Name: COLUMN ohd_user.email; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.email IS 'Email';


--
-- TOC entry 5018 (class 0 OID 0)
-- Dependencies: 227
-- Name: COLUMN ohd_user.password; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.password IS 'Password';


--
-- TOC entry 5019 (class 0 OID 0)
-- Dependencies: 227
-- Name: COLUMN ohd_user.role_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.role_id IS 'Role ID';


--
-- TOC entry 5020 (class 0 OID 0)
-- Dependencies: 227
-- Name: COLUMN ohd_user.status_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.status_id IS 'Status';


--
-- TOC entry 5021 (class 0 OID 0)
-- Dependencies: 227
-- Name: COLUMN ohd_user.created_at; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.created_at IS 'Created At';


--
-- TOC entry 5022 (class 0 OID 0)
-- Dependencies: 227
-- Name: COLUMN ohd_user.modified_at; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.modified_at IS 'Modified At';


--
-- TOC entry 225 (class 1259 OID 25149)
-- Name: ohd_user_communication_setting; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_user_communication_setting (
    id bigint NOT NULL,
    user_id bigint NOT NULL,
    communication_id smallint NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_user_communication_setting OWNER TO postgres;

--
-- TOC entry 5023 (class 0 OID 0)
-- Dependencies: 225
-- Name: TABLE ohd_user_communication_setting; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_user_communication_setting IS 'notification setting as per user';


--
-- TOC entry 243 (class 1259 OID 25366)
-- Name: ohd_user_communication_setting_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_user_communication_setting ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_user_communication_setting_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 242 (class 1259 OID 25365)
-- Name: ohd_user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_user ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_user_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 226 (class 1259 OID 25157)
-- Name: ohd_user_role_master; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_user_role_master (
    id smallint NOT NULL,
    name character varying(50) NOT NULL,
    description text,
    status_id smallint DEFAULT 1 NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.ohd_user_role_master OWNER TO postgres;

--
-- TOC entry 5024 (class 0 OID 0)
-- Dependencies: 226
-- Name: TABLE ohd_user_role_master; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_user_role_master IS 'User Role Master';


--
-- TOC entry 5025 (class 0 OID 0)
-- Dependencies: 226
-- Name: COLUMN ohd_user_role_master.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.id IS 'Role ID';


--
-- TOC entry 5026 (class 0 OID 0)
-- Dependencies: 226
-- Name: COLUMN ohd_user_role_master.name; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.name IS 'Role Name';


--
-- TOC entry 5027 (class 0 OID 0)
-- Dependencies: 226
-- Name: COLUMN ohd_user_role_master.description; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.description IS 'Role Description';


--
-- TOC entry 5028 (class 0 OID 0)
-- Dependencies: 226
-- Name: COLUMN ohd_user_role_master.status_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.status_id IS 'Status';


--
-- TOC entry 5029 (class 0 OID 0)
-- Dependencies: 226
-- Name: COLUMN ohd_user_role_master.created_at; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.created_at IS 'Created At';


--
-- TOC entry 5030 (class 0 OID 0)
-- Dependencies: 226
-- Name: COLUMN ohd_user_role_master.modified_at; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.modified_at IS 'Modified At';


--
-- TOC entry 244 (class 1259 OID 25367)
-- Name: ohd_user_role_master_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_user_role_master ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_user_role_master_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 4972 (class 0 OID 25125)
-- Dependencies: 223
-- Data for Name: ohd_company; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_company OVERRIDING SYSTEM VALUE VALUES (1, 'Ontec', 1, NULL, NULL);


--
-- TOC entry 4971 (class 0 OID 25113)
-- Dependencies: 222
-- Data for Name: ohd_configuration; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 4970 (class 0 OID 25096)
-- Dependencies: 221
-- Data for Name: ohd_document; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (1, 1, 'https://bill1', 'Water bill', 'pdf', 1, NULL, NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (2, 2, 'https://bill2', 'Electrical bill', 'pdf', 1, NULL, NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (3, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\6677.png', '6677', '.png', 1, '2024-01-29 19:45:36.780267', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (4, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\78569.png', '78569', '.png', 1, '2024-01-29 19:52:31.175053', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (8, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\78900.png', '78900', '.png', 1, '2024-01-29 20:25:27.4227', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (9, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\23465.png', '23465', '.png', 1, '2024-01-29 20:27:35.581302', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (10, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\1245336.png', '1245336', '.png', 1, '2024-01-29 20:28:25.143009', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (11, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\456.png', '456', '.png', 1, '2024-01-29 20:29:26.43599', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (12, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\12345.png', '12345', '.png', 1, '2024-01-29 20:32:00.023777', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (13, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\4.png', '4', '.png', 1, '2024-01-29 20:37:56.872342', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (14, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\4.png', '4', '.png', 1, '2024-01-29 20:38:27.769733', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (15, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\3456.png', '3456', '.png', 1, '2024-01-29 20:42:31.846711', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (16, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\23.png', '23', '.png', 1, '2024-01-29 20:43:55.648634', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (17, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\785.png', '785', '.png', 1, '2024-01-30 20:03:21.646234', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (18, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\ontec_ERD.pgerd.png.png', 'ontec_ERD.pgerd.png', '.png', 1, '2024-02-25 18:44:31.680405', NULL);
INSERT INTO public.ohd_document OVERRIDING SYSTEM VALUE VALUES (19, 1, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\Screenshot (2).png.png', 'Screenshot (2).png', '.png', 1, '2024-02-25 19:21:54.508561', NULL);


--
-- TOC entry 4969 (class 0 OID 25084)
-- Dependencies: 220
-- Data for Name: ohd_document_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_document_type OVERRIDING SYSTEM VALUE VALUES (2, 'Electrical bill', 1, NULL, NULL);
INSERT INTO public.ohd_document_type OVERRIDING SYSTEM VALUE VALUES (1, 'Contract proof', 1, NULL, NULL);


--
-- TOC entry 4967 (class 0 OID 25056)
-- Dependencies: 218
-- Data for Name: ohd_enum_communications; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_communications OVERRIDING SYSTEM VALUE VALUES (1, 'Mobile', 'On Mobile', 1, NULL, NULL);
INSERT INTO public.ohd_enum_communications OVERRIDING SYSTEM VALUE VALUES (2, 'Email', 'Email', 1, NULL, NULL);


--
-- TOC entry 4965 (class 0 OID 25028)
-- Dependencies: 216
-- Data for Name: ohd_enum_meter_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_meter_type OVERRIDING SYSTEM VALUE VALUES (1, 'Gas', 'Gas', 1, NULL, NULL);
INSERT INTO public.ohd_enum_meter_type OVERRIDING SYSTEM VALUE VALUES (2, 'Water', 'Water', 1, NULL, NULL);
INSERT INTO public.ohd_enum_meter_type OVERRIDING SYSTEM VALUE VALUES (3, 'Electricity', 'Electricity', 1, NULL, NULL);


--
-- TOC entry 4964 (class 0 OID 25019)
-- Dependencies: 215
-- Data for Name: ohd_enum_status; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_status OVERRIDING SYSTEM VALUE VALUES (1, 'Active', 'Active', NULL, NULL);
INSERT INTO public.ohd_enum_status OVERRIDING SYSTEM VALUE VALUES (2, 'InComplete', 'Waiting for complete', NULL, NULL);
INSERT INTO public.ohd_enum_status OVERRIDING SYSTEM VALUE VALUES (3, 'NotVerified', 'Otp not verified', NULL, NULL);
INSERT INTO public.ohd_enum_status OVERRIDING SYSTEM VALUE VALUES (4, 'Pending', 'Pending', NULL, NULL);
INSERT INTO public.ohd_enum_status OVERRIDING SYSTEM VALUE VALUES (5, 'InActive', 'In active', NULL, NULL);
INSERT INTO public.ohd_enum_status OVERRIDING SYSTEM VALUE VALUES (6, 'Rejected', 'Rejected', NULL, NULL);


--
-- TOC entry 4966 (class 0 OID 25042)
-- Dependencies: 217
-- Data for Name: ohd_enum_title; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_title OVERRIDING SYSTEM VALUE VALUES (2, 'Mr', 'Mr', 1, NULL, NULL);
INSERT INTO public.ohd_enum_title OVERRIDING SYSTEM VALUE VALUES (3, 'Mrs', 'Mrs', 1, NULL, NULL);
INSERT INTO public.ohd_enum_title OVERRIDING SYSTEM VALUE VALUES (4, 'Ms', 'Ms', 1, NULL, NULL);


--
-- TOC entry 4968 (class 0 OID 25072)
-- Dependencies: 219
-- Data for Name: ohd_enum_user_relation; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_user_relation OVERRIDING SYSTEM VALUE VALUES (1, 'Tenant', 'Tenant', 1, NULL, NULL);
INSERT INTO public.ohd_enum_user_relation OVERRIDING SYSTEM VALUE VALUES (2, 'Associate', 'Associate', 1, NULL, NULL);


--
-- TOC entry 4979 (class 0 OID 25323)
-- Dependencies: 230
-- Data for Name: ohd_meter; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_meter OVERRIDING SYSTEM VALUE VALUES (2, 5, '128456', 2, 'testMeter', 60, 123, 1, '2025-01-01 00:00:00', NULL, NULL, NULL, NULL, NULL, 4, '2024-01-27 00:00:00', NULL);
INSERT INTO public.ohd_meter OVERRIDING SYSTEM VALUE VALUES (4, 5, '789456', 2, 'test', 7845, 124563, 1, '2025-01-27 15:31:10.568', NULL, NULL, NULL, NULL, NULL, 1, '2024-01-27 15:32:38.237923', '2024-01-27 16:39:49.796235');
INSERT INTO public.ohd_meter OVERRIDING SYSTEM VALUE VALUES (5, 5, '1233', 1, 'MeterAlias', 45, 45, 14, '2025-01-01 00:00:00', NULL, NULL, NULL, NULL, NULL, 4, '2021-01-01 00:00:00', NULL);
INSERT INTO public.ohd_meter OVERRIDING SYSTEM VALUE VALUES (6, 5, '785', 1, 'koju', 75, 555, 17, '2025-01-01 00:00:00', NULL, NULL, NULL, NULL, NULL, 4, '2024-01-30 20:03:35.227766', NULL);
INSERT INTO public.ohd_meter OVERRIDING SYSTEM VALUE VALUES (1, 5, '123456', 1, 'test type', 123, 123, 1, '2025-12-31 00:00:00', 'Pune', 'pune', 'pune', 'Maharashtra', 'India', 5, NULL, '2024-02-14 19:56:10.028601');


--
-- TOC entry 4973 (class 0 OID 25137)
-- Dependencies: 224
-- Data for Name: ohd_mobile_country_code; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_mobile_country_code OVERRIDING SYSTEM VALUE VALUES (1, '91', 1, NULL, NULL);


--
-- TOC entry 4994 (class 0 OID 25373)
-- Dependencies: 245
-- Data for Name: ohd_otp; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_otp OVERRIDING SYSTEM VALUE VALUES (6, '123456', 's@h.k', 4, '2024-01-03 21:48:32.556959', '2024-01-03 21:48:32.556965', 1, 1, '9011619932');
INSERT INTO public.ohd_otp OVERRIDING SYSTEM VALUE VALUES (7, '123456', 'shital@gmail.com', 4, '2024-01-04 18:52:28.690725', '2024-01-04 18:52:28.695052', 1, 1, '9552109391');
INSERT INTO public.ohd_otp OVERRIDING SYSTEM VALUE VALUES (8, '123456', 'shital.p@gmail.com', 4, '2024-01-23 19:06:34.964729', '2024-01-23 19:06:34.964801', 1, 1, '8956237423');
INSERT INTO public.ohd_otp OVERRIDING SYSTEM VALUE VALUES (9, '123456', 'shital.k@outlook.com', 4, '2024-01-23 19:23:55.058856', '2024-01-23 19:23:55.058899', 1, 1, '8975488213');
INSERT INTO public.ohd_otp OVERRIDING SYSTEM VALUE VALUES (10, '123456', 'pChetan@gmail.com', 4, '2024-01-23 20:58:37.202861', '2024-01-23 20:58:37.202961', 1, 1, '2255889922');


--
-- TOC entry 4977 (class 0 OID 25223)
-- Dependencies: 228
-- Data for Name: ohd_property; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_property OVERRIDING SYSTEM VALUE VALUES (6, 'Office', '1228', 3, 1, 1, NULL, NULL, 'Hinjewadi', 'Hinjewadi', 'Pune', 'Maharashtra', 'India', NULL, NULL);
INSERT INTO public.ohd_property OVERRIDING SYSTEM VALUE VALUES (8, 'Home2', '9865', 3, 1, 1, NULL, NULL, 'pune', 'pune', 'pune', 'pune', 'India', '2024-01-04 14:21:13', '2024-01-04 14:21:13');
INSERT INTO public.ohd_property OVERRIDING SYSTEM VALUE VALUES (15, 'Home3', '5689', 3, 1, 1, NULL, NULL, 'pune', 'pune', 'pune', 'pune', 'pune', '2024-01-04 20:29:08.282926', '2024-01-04 20:13:19.289');
INSERT INTO public.ohd_property OVERRIDING SYSTEM VALUE VALUES (5, 'Home234', '9324', 3, 1, 1, NULL, NULL, 'Pune', 'Pune', 'Chinchwad', 'Maharashtra', 'India', NULL, '2024-01-30 22:21:05.255794');
INSERT INTO public.ohd_property OVERRIDING SYSTEM VALUE VALUES (16, 'Home4', '5699', 6, 1, 1, NULL, NULL, 'pune', 'pune', 'pune', 'pune', 'pune', '2024-01-04 20:30:12.675129', '2024-01-04 20:13:19.289');
INSERT INTO public.ohd_property OVERRIDING SYSTEM VALUE VALUES (19, 'Home6', '5589', 6, 1, 1, NULL, NULL, 'pune', 'pune', 'pune', 'pune', 'pune', '2024-01-04 20:43:51.358881', '2024-01-04 20:13:19.289');
INSERT INTO public.ohd_property OVERRIDING SYSTEM VALUE VALUES (23, 'Home3', '5669', 6, 1, 1, NULL, NULL, 'pune', 'pune', 'pune', 'pune', 'pune', '2024-01-04 20:58:20.366899', '2024-01-04 20:13:19.289');


--
-- TOC entry 4978 (class 0 OID 25297)
-- Dependencies: 229
-- Data for Name: ohd_property_user_relation; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_property_user_relation OVERRIDING SYSTEM VALUE VALUES (1, 6, 6, 1, 5, NULL, '2024-02-14 19:59:31.923568');
INSERT INTO public.ohd_property_user_relation OVERRIDING SYSTEM VALUE VALUES (4, 8, 19, 1, 1, '2024-02-16 19:45:35.574994', NULL);
INSERT INTO public.ohd_property_user_relation OVERRIDING SYSTEM VALUE VALUES (2, 3, 16, 2, 1, NULL, NULL);


--
-- TOC entry 4976 (class 0 OID 25169)
-- Dependencies: 227
-- Data for Name: ohd_user; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_user OVERRIDING SYSTEM VALUE VALUES (5, NULL, NULL, NULL, 1, 1, '9552109391', 'shital@gmail.com', 'GctGUspVCKk4FaEszJDFUyHgJlLCdhCp4bnAHLkdTf0=', 1, NULL, NULL, NULL, NULL, NULL, NULL, 4, '2024-01-04 18:57:21.942163', '2024-01-04 18:57:22.597512', NULL, NULL, NULL, NULL);
INSERT INTO public.ohd_user OVERRIDING SYSTEM VALUE VALUES (6, 2, 'Test', 'Test', 1, 1, '8956237423', 'shital.p@gmail.com', 'GctGUspVCKk4FaEszJDFUyHgJlLCdhCp4bnAHLkdTf0=', 1, NULL, NULL, NULL, NULL, NULL, NULL, 4, '2024-01-23 19:13:00.822835', '2024-01-23 19:13:00.822836', NULL, NULL, NULL, NULL);
INSERT INTO public.ohd_user OVERRIDING SYSTEM VALUE VALUES (7, 2, 'TestAssociate', 'TestAssociate', 1, 1, '2255889922', 'pChetan@gmail.com', 'GctGUspVCKk4FaEszJDFUyHgJlLCdhCp4bnAHLkdTf0=', 1, NULL, NULL, NULL, NULL, NULL, NULL, 4, '2024-01-23 20:59:38.025608', '2024-01-23 20:59:38.025609', NULL, NULL, NULL, NULL);
INSERT INTO public.ohd_user OVERRIDING SYSTEM VALUE VALUES (8, 3, 'kiran', 'patil', NULL, 1, '8956231245', 'Shitalk@gmail.com', 'Admin@123', 4, NULL, NULL, NULL, NULL, NULL, NULL, 4, '2024-02-16 19:45:35.551298', '2024-02-16 19:45:35.551725', NULL, NULL, NULL, NULL);
INSERT INTO public.ohd_user OVERRIDING SYSTEM VALUE VALUES (3, 2, 'Chetan', 'Patil', 1, 1, '1234567890', 'shital.patil47@gmail.com', 'GctGUspVCKk4FaEszJDFUyHgJlLCdhCp4bnAHLkdTf0=', 1, 'India', 'Pune', 'Pune', 'Maharashtra', 'India', 1, 1, '2023-12-29 17:48:20.288358', '2024-02-25 19:22:01.314617', NULL, NULL, 19, 'D:\Megsys\Projects\Ontec\sourcecode\ontec_api\Ontec.WebUI\wwwroot\Uploads\3.png');


--
-- TOC entry 4974 (class 0 OID 25149)
-- Dependencies: 225
-- Data for Name: ohd_user_communication_setting; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_user_communication_setting OVERRIDING SYSTEM VALUE VALUES (3, 0, 1, 1, '2024-01-30 21:25:19.126341', NULL);
INSERT INTO public.ohd_user_communication_setting OVERRIDING SYSTEM VALUE VALUES (4, 0, 2, 1, '2024-01-30 21:25:28.133935', NULL);
INSERT INTO public.ohd_user_communication_setting OVERRIDING SYSTEM VALUE VALUES (1, 3, 1, 1, NULL, '2024-02-25 19:22:01.311948');
INSERT INTO public.ohd_user_communication_setting OVERRIDING SYSTEM VALUE VALUES (2, 3, 2, 1, NULL, '2024-02-25 19:22:01.313276');


--
-- TOC entry 4975 (class 0 OID 25157)
-- Dependencies: 226
-- Data for Name: ohd_user_role_master; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_user_role_master OVERRIDING SYSTEM VALUE VALUES (1, 'Customer', 'Customer', 1, '2023-12-29 17:32:05.52404', '2023-12-29 17:32:05.52404');
INSERT INTO public.ohd_user_role_master OVERRIDING SYSTEM VALUE VALUES (2, 'Admin', 'Admin', 1, '2024-01-24 21:21:33.329777', '2024-01-24 21:21:33.329777');
INSERT INTO public.ohd_user_role_master OVERRIDING SYSTEM VALUE VALUES (3, 'SuperAdmin', 'SuperAdmin', 1, '2024-01-24 21:21:49.842682', '2024-01-24 21:21:49.842682');
INSERT INTO public.ohd_user_role_master OVERRIDING SYSTEM VALUE VALUES (4, 'Temporary', 'Temporary User as associate or tenant', 1, '2024-02-14 20:03:44.314464', '2024-02-14 20:03:44.314464');


--
-- TOC entry 5031 (class 0 OID 0)
-- Dependencies: 231
-- Name: ohd_company_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_company_id_seq', 1, true);


--
-- TOC entry 5032 (class 0 OID 0)
-- Dependencies: 232
-- Name: ohd_configuration_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_configuration_id_seq', 1, false);


--
-- TOC entry 5033 (class 0 OID 0)
-- Dependencies: 233
-- Name: ohd_document_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_document_id_seq', 19, true);


--
-- TOC entry 5034 (class 0 OID 0)
-- Dependencies: 234
-- Name: ohd_document_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_document_type_id_seq', 2, true);


--
-- TOC entry 5035 (class 0 OID 0)
-- Dependencies: 235
-- Name: ohd_enum_communications_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_enum_communications_id_seq', 2, true);


--
-- TOC entry 5036 (class 0 OID 0)
-- Dependencies: 236
-- Name: ohd_enum_meter_type_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ohd_enum_meter_type_Id_seq"', 3, true);


--
-- TOC entry 5037 (class 0 OID 0)
-- Dependencies: 247
-- Name: ohd_enum_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_enum_status_id_seq', 1, false);


--
-- TOC entry 5038 (class 0 OID 0)
-- Dependencies: 237
-- Name: ohd_enum_title_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_enum_title_id_seq', 4, true);


--
-- TOC entry 5039 (class 0 OID 0)
-- Dependencies: 248
-- Name: ohd_enum_user_relation_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_enum_user_relation_id_seq', 1, false);


--
-- TOC entry 5040 (class 0 OID 0)
-- Dependencies: 238
-- Name: ohd_meter_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_meter_id_seq', 6, true);


--
-- TOC entry 5041 (class 0 OID 0)
-- Dependencies: 239
-- Name: ohd_mobile_country_code_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_mobile_country_code_id_seq', 1, true);


--
-- TOC entry 5042 (class 0 OID 0)
-- Dependencies: 246
-- Name: ohd_otp_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ohd_otp_Id_seq"', 10, true);


--
-- TOC entry 5043 (class 0 OID 0)
-- Dependencies: 240
-- Name: ohd_property_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_property_id_seq', 25, true);


--
-- TOC entry 5044 (class 0 OID 0)
-- Dependencies: 241
-- Name: ohd_property_user_relation_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_property_user_relation_id_seq', 4, true);


--
-- TOC entry 5045 (class 0 OID 0)
-- Dependencies: 243
-- Name: ohd_user_communication_setting_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_user_communication_setting_id_seq', 4, true);


--
-- TOC entry 5046 (class 0 OID 0)
-- Dependencies: 242
-- Name: ohd_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_user_id_seq', 8, true);


--
-- TOC entry 5047 (class 0 OID 0)
-- Dependencies: 244
-- Name: ohd_user_role_master_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_user_role_master_id_seq', 4, true);


--
-- TOC entry 4789 (class 2606 OID 25379)
-- Name: ohd_otp ohd_Otp_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_otp
    ADD CONSTRAINT "ohd_Otp_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4759 (class 2606 OID 25131)
-- Name: ohd_company ohd_company_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_company
    ADD CONSTRAINT ohd_company_name_key UNIQUE (name);


--
-- TOC entry 4761 (class 2606 OID 25129)
-- Name: ohd_company ohd_company_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_company
    ADD CONSTRAINT ohd_company_pkey PRIMARY KEY (id);


--
-- TOC entry 4755 (class 2606 OID 25119)
-- Name: ohd_configuration ohd_configuration_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_configuration
    ADD CONSTRAINT ohd_configuration_name_key UNIQUE (name);


--
-- TOC entry 4757 (class 2606 OID 25117)
-- Name: ohd_configuration ohd_configuration_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_configuration
    ADD CONSTRAINT ohd_configuration_pkey PRIMARY KEY (id);


--
-- TOC entry 4753 (class 2606 OID 25102)
-- Name: ohd_document ohd_document_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document
    ADD CONSTRAINT ohd_document_pkey PRIMARY KEY (id);


--
-- TOC entry 4749 (class 2606 OID 25090)
-- Name: ohd_document_type ohd_document_type_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document_type
    ADD CONSTRAINT ohd_document_type_name_key UNIQUE (name);


--
-- TOC entry 4751 (class 2606 OID 25088)
-- Name: ohd_document_type ohd_document_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document_type
    ADD CONSTRAINT ohd_document_type_pkey PRIMARY KEY (id);


--
-- TOC entry 4739 (class 2606 OID 25066)
-- Name: ohd_enum_communications ohd_enum_communications_display_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_communications
    ADD CONSTRAINT ohd_enum_communications_display_name_key UNIQUE (display_name);


--
-- TOC entry 4741 (class 2606 OID 25064)
-- Name: ohd_enum_communications ohd_enum_communications_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_communications
    ADD CONSTRAINT ohd_enum_communications_name_key UNIQUE (name);


--
-- TOC entry 4743 (class 2606 OID 25062)
-- Name: ohd_enum_communications ohd_enum_communications_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_communications
    ADD CONSTRAINT ohd_enum_communications_pkey PRIMARY KEY (id);


--
-- TOC entry 4727 (class 2606 OID 25036)
-- Name: ohd_enum_meter_type ohd_enum_meter_type_display_value_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_meter_type
    ADD CONSTRAINT ohd_enum_meter_type_display_value_key UNIQUE (display_value);


--
-- TOC entry 4729 (class 2606 OID 25034)
-- Name: ohd_enum_meter_type ohd_enum_meter_type_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_meter_type
    ADD CONSTRAINT ohd_enum_meter_type_name_key UNIQUE (name);


--
-- TOC entry 4731 (class 2606 OID 25032)
-- Name: ohd_enum_meter_type ohd_enum_meter_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_meter_type
    ADD CONSTRAINT ohd_enum_meter_type_pkey PRIMARY KEY (id);


--
-- TOC entry 4721 (class 2606 OID 25027)
-- Name: ohd_enum_status ohd_enum_status_display_value_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_status
    ADD CONSTRAINT ohd_enum_status_display_value_key UNIQUE (display_value);


--
-- TOC entry 4723 (class 2606 OID 25025)
-- Name: ohd_enum_status ohd_enum_status_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_status
    ADD CONSTRAINT ohd_enum_status_name_key UNIQUE (name);


--
-- TOC entry 4725 (class 2606 OID 25023)
-- Name: ohd_enum_status ohd_enum_status_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_status
    ADD CONSTRAINT ohd_enum_status_pkey PRIMARY KEY (id);


--
-- TOC entry 4733 (class 2606 OID 25050)
-- Name: ohd_enum_title ohd_enum_title_display_value_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_title
    ADD CONSTRAINT ohd_enum_title_display_value_key UNIQUE (display_value);


--
-- TOC entry 4735 (class 2606 OID 25048)
-- Name: ohd_enum_title ohd_enum_title_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_title
    ADD CONSTRAINT ohd_enum_title_name_key UNIQUE (name);


--
-- TOC entry 4737 (class 2606 OID 25046)
-- Name: ohd_enum_title ohd_enum_title_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_title
    ADD CONSTRAINT ohd_enum_title_pkey PRIMARY KEY (id);


--
-- TOC entry 4745 (class 2606 OID 25078)
-- Name: ohd_enum_user_relation ohd_enum_user_relation_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_user_relation
    ADD CONSTRAINT ohd_enum_user_relation_name_key UNIQUE (name);


--
-- TOC entry 4747 (class 2606 OID 25076)
-- Name: ohd_enum_user_relation ohd_enum_user_relation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_user_relation
    ADD CONSTRAINT ohd_enum_user_relation_pkey PRIMARY KEY (id);


--
-- TOC entry 4785 (class 2606 OID 25331)
-- Name: ohd_meter ohd_meter_meter_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT ohd_meter_meter_number_key UNIQUE (meter_number);


--
-- TOC entry 4787 (class 2606 OID 25329)
-- Name: ohd_meter ohd_meter_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT ohd_meter_pkey PRIMARY KEY (id);


--
-- TOC entry 4763 (class 2606 OID 25143)
-- Name: ohd_mobile_country_code ohd_mobile_country_code_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_mobile_country_code
    ADD CONSTRAINT ohd_mobile_country_code_code_key UNIQUE (code);


--
-- TOC entry 4765 (class 2606 OID 25141)
-- Name: ohd_mobile_country_code ohd_mobile_country_code_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_mobile_country_code
    ADD CONSTRAINT ohd_mobile_country_code_pkey PRIMARY KEY (id);


--
-- TOC entry 4779 (class 2606 OID 25229)
-- Name: ohd_property ohd_property_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT ohd_property_pkey PRIMARY KEY (id);


--
-- TOC entry 4781 (class 2606 OID 33574)
-- Name: ohd_property ohd_property_unit_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT ohd_property_unit_number_key UNIQUE (unit_number);


--
-- TOC entry 4783 (class 2606 OID 25301)
-- Name: ohd_property_user_relation ohd_property_user_relation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT ohd_property_user_relation_pkey PRIMARY KEY (id);


--
-- TOC entry 4767 (class 2606 OID 25153)
-- Name: ohd_user_communication_setting ohd_user_communication_setting_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_communication_setting
    ADD CONSTRAINT ohd_user_communication_setting_pkey PRIMARY KEY (id);


--
-- TOC entry 4773 (class 2606 OID 25182)
-- Name: ohd_user ohd_user_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT ohd_user_email_key UNIQUE (email);


--
-- TOC entry 4775 (class 2606 OID 25180)
-- Name: ohd_user ohd_user_mobile_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT ohd_user_mobile_key UNIQUE (mobile);


--
-- TOC entry 4777 (class 2606 OID 25178)
-- Name: ohd_user ohd_user_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT ohd_user_pkey PRIMARY KEY (id);


--
-- TOC entry 4769 (class 2606 OID 25168)
-- Name: ohd_user_role_master ohd_user_role_master_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_role_master
    ADD CONSTRAINT ohd_user_role_master_name_key UNIQUE (name);


--
-- TOC entry 4771 (class 2606 OID 25166)
-- Name: ohd_user_role_master ohd_user_role_master_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_role_master
    ADD CONSTRAINT ohd_user_role_master_pkey PRIMARY KEY (id);


--
-- TOC entry 4800 (class 2606 OID 25368)
-- Name: ohd_user fk_communication_setting_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_communication_setting_id FOREIGN KEY (communication_setting) REFERENCES public.ohd_enum_communications(id) NOT VALID;


--
-- TOC entry 4801 (class 2606 OID 25208)
-- Name: ohd_user fk_company_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_company_id FOREIGN KEY (company_id) REFERENCES public.ohd_company(id);


--
-- TOC entry 4818 (class 2606 OID 25399)
-- Name: ohd_otp fk_company_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_otp
    ADD CONSTRAINT fk_company_id FOREIGN KEY ("companyId") REFERENCES public.ohd_company(id) NOT VALID;


--
-- TOC entry 4807 (class 2606 OID 25411)
-- Name: ohd_property fk_company_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT fk_company_id FOREIGN KEY (company_id) REFERENCES public.ohd_company(id) NOT VALID;


--
-- TOC entry 4814 (class 2606 OID 25332)
-- Name: ohd_meter fk_contract_proof_document_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT fk_contract_proof_document_id FOREIGN KEY (contract_proof_document) REFERENCES public.ohd_document(id);


--
-- TOC entry 4819 (class 2606 OID 25393)
-- Name: ohd_otp fk_countryCode_Id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_otp
    ADD CONSTRAINT "fk_countryCode_Id" FOREIGN KEY ("CountryCodeId") REFERENCES public.ohd_mobile_country_code(id) NOT VALID;


--
-- TOC entry 4795 (class 2606 OID 25103)
-- Name: ohd_document fk_document_type_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document
    ADD CONSTRAINT fk_document_type_id FOREIGN KEY (document_type) REFERENCES public.ohd_document_type(id);


--
-- TOC entry 4815 (class 2606 OID 25347)
-- Name: ohd_meter fk_meter_type_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT fk_meter_type_id FOREIGN KEY (meter_type_id) REFERENCES public.ohd_enum_meter_type(id);


--
-- TOC entry 4802 (class 2606 OID 25198)
-- Name: ohd_user fk_mobile_code_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_mobile_code_id FOREIGN KEY (mobile_country_code) REFERENCES public.ohd_mobile_country_code(id);


--
-- TOC entry 4808 (class 2606 OID 25237)
-- Name: ohd_property fk_owner_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT fk_owner_id FOREIGN KEY (owner_id) REFERENCES public.ohd_user(id);


--
-- TOC entry 4803 (class 2606 OID 41766)
-- Name: ohd_user fk_proofdocument_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_proofdocument_id FOREIGN KEY (proof_document_id) REFERENCES public.ohd_document(id) NOT VALID;


--
-- TOC entry 4810 (class 2606 OID 25307)
-- Name: ohd_property_user_relation fk_property_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT fk_property_id FOREIGN KEY (property_id) REFERENCES public.ohd_property(id);


--
-- TOC entry 4816 (class 2606 OID 25342)
-- Name: ohd_meter fk_property_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT fk_property_id FOREIGN KEY (property_id) REFERENCES public.ohd_property(id);


--
-- TOC entry 4804 (class 2606 OID 25183)
-- Name: ohd_user fk_role_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_role_id FOREIGN KEY (role_id) REFERENCES public.ohd_user_role_master(id);


--
-- TOC entry 4817 (class 2606 OID 25337)
-- Name: ohd_meter fk_status; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT fk_status FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4790 (class 2606 OID 25037)
-- Name: ohd_enum_meter_type fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_meter_type
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4791 (class 2606 OID 25051)
-- Name: ohd_enum_title fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_title
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4792 (class 2606 OID 25067)
-- Name: ohd_enum_communications fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_communications
    ADD CONSTRAINT fk_status_id FOREIGN KEY (id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4793 (class 2606 OID 25079)
-- Name: ohd_enum_user_relation fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_user_relation
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4794 (class 2606 OID 25091)
-- Name: ohd_document_type fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document_type
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4796 (class 2606 OID 25108)
-- Name: ohd_document fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4797 (class 2606 OID 25120)
-- Name: ohd_configuration fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_configuration
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4798 (class 2606 OID 25132)
-- Name: ohd_company fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_company
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4799 (class 2606 OID 25144)
-- Name: ohd_mobile_country_code fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_mobile_country_code
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4805 (class 2606 OID 25188)
-- Name: ohd_user fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4809 (class 2606 OID 25232)
-- Name: ohd_property fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4811 (class 2606 OID 25317)
-- Name: ohd_property_user_relation fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4820 (class 2606 OID 25384)
-- Name: ohd_otp fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_otp
    ADD CONSTRAINT fk_status_id FOREIGN KEY ("StatusId") REFERENCES public.ohd_enum_status(id) NOT VALID;


--
-- TOC entry 4806 (class 2606 OID 25193)
-- Name: ohd_user fk_title_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_title_id FOREIGN KEY (title) REFERENCES public.ohd_enum_title(id);


--
-- TOC entry 4812 (class 2606 OID 25302)
-- Name: ohd_property_user_relation fk_user_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT fk_user_id FOREIGN KEY (user_id) REFERENCES public.ohd_user(id);


--
-- TOC entry 4813 (class 2606 OID 25312)
-- Name: ohd_property_user_relation fk_user_relation_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT fk_user_relation_id FOREIGN KEY (user_relation_id) REFERENCES public.ohd_enum_user_relation(id);


-- Completed on 2024-02-26 12:27:30

--
-- PostgreSQL database dump complete
--

