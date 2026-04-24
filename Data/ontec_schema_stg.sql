--
-- PostgreSQL database dump
--

-- Dumped from database version 16.1
-- Dumped by pg_dump version 16.1

-- Started on 2024-06-24 08:10:45

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
-- TOC entry 5098 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--

COMMENT ON SCHEMA public IS 'standard public schema';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 215 (class 1259 OID 41010)
-- Name: ohd_application_log; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_application_log (
    id bigint NOT NULL,
    request character varying,
    error character varying,
    method character varying
);


ALTER TABLE public.ohd_application_log OWNER TO postgres;

--
-- TOC entry 216 (class 1259 OID 41015)
-- Name: ohd_associate_user_settings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_associate_user_settings (
    id bigint NOT NULL,
    property_user_id bigint,
    allow_top_up smallint,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_associate_user_settings OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 41018)
-- Name: ohd_associate_user_settings_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_associate_user_settings ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_associate_user_settings_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 218 (class 1259 OID 41019)
-- Name: ohd_bank_accounts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_bank_accounts (
    id bigint NOT NULL,
    account_name character varying,
    account_number character varying,
    branch_code character varying,
    status_id bigint,
    created_at timestamp without time zone,
    modified_at timestamp without time zone,
    bank_name character varying
);


ALTER TABLE public.ohd_bank_accounts OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 41024)
-- Name: ohd_bank_accounts_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_bank_accounts ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_bank_accounts_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 220 (class 1259 OID 41025)
-- Name: ohd_company; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_company (
    id bigint NOT NULL,
    name character varying(55) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone,
    firstname character varying(100),
    lastname character varying(100),
    address character varying(150),
    country character varying(100),
    state character varying(100),
    mobile character varying(15),
    email character varying(150),
    city character varying(100),
    zip character varying(50),
    description character varying(200),
    state_id bigint,
    country_id bigint,
    companylogourl character varying(5000),
    faviconlogourl character varying,
    smalllogourl character varying
);


ALTER TABLE public.ohd_company OWNER TO postgres;

--
-- TOC entry 5099 (class 0 OID 0)
-- Dependencies: 220
-- Name: TABLE ohd_company; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_company IS 'Lists all companies';


--
-- TOC entry 221 (class 1259 OID 41030)
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
-- TOC entry 222 (class 1259 OID 41031)
-- Name: ohd_configuration; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_configuration (
    id bigint NOT NULL,
    name character varying(55) NOT NULL,
    value character varying(55) NOT NULL,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone,
    created_by bigint,
    modified_by bigint,
    display_name character varying(55),
    description character varying(250),
    note character varying,
    configuration_type character varying,
    is_editable boolean,
    unit character varying,
    company_id bigint
);


ALTER TABLE public.ohd_configuration OWNER TO postgres;

--
-- TOC entry 5100 (class 0 OID 0)
-- Dependencies: 222
-- Name: TABLE ohd_configuration; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_configuration IS 'lists the all configurations related to application ';


--
-- TOC entry 223 (class 1259 OID 41036)
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
-- TOC entry 224 (class 1259 OID 41037)
-- Name: ohd_country; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_country (
    id bigint NOT NULL,
    name character varying(100),
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_country OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 41040)
-- Name: ohd_country_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_country ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_country_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 226 (class 1259 OID 41041)
-- Name: ohd_country_master; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_country_master (
    id bigint NOT NULL,
    name character varying NOT NULL,
    status_id smallint
);


ALTER TABLE public.ohd_country_master OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 41046)
-- Name: ohd_country_master_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_country_master ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_country_master_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 228 (class 1259 OID 41047)
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
-- TOC entry 5101 (class 0 OID 0)
-- Dependencies: 228
-- Name: TABLE ohd_document; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_document IS 'all documents';


--
-- TOC entry 229 (class 1259 OID 41052)
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
-- TOC entry 230 (class 1259 OID 41053)
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
-- TOC entry 5102 (class 0 OID 0)
-- Dependencies: 230
-- Name: TABLE ohd_document_type; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_document_type IS 'allowed documents to upload';


--
-- TOC entry 231 (class 1259 OID 41056)
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
-- TOC entry 232 (class 1259 OID 41057)
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
-- TOC entry 5103 (class 0 OID 0)
-- Dependencies: 232
-- Name: TABLE ohd_enum_communications; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_communications IS 'list the communication types';


--
-- TOC entry 233 (class 1259 OID 41062)
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
-- TOC entry 234 (class 1259 OID 41063)
-- Name: ohd_enum_meter_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_enum_meter_type (
    id smallint NOT NULL,
    name character varying(55),
    display_value character varying(55),
    status_id smallint,
    created_at timestamp without time zone,
    modified_at timestamp without time zone,
    unitofmeasure character varying(100),
    meterreadingtype character varying(100),
    priority smallint,
    meterreadingtypeid character varying(50),
    mindailytarget double precision,
    maxdailytarget double precision
);


ALTER TABLE public.ohd_enum_meter_type OWNER TO postgres;

--
-- TOC entry 5104 (class 0 OID 0)
-- Dependencies: 234
-- Name: TABLE ohd_enum_meter_type; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_meter_type IS 'lists the meter types';


--
-- TOC entry 235 (class 1259 OID 41066)
-- Name: ohd_enum_meter_type_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_enum_meter_type ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_enum_meter_type_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 236 (class 1259 OID 41067)
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
-- TOC entry 5105 (class 0 OID 0)
-- Dependencies: 236
-- Name: TABLE ohd_enum_status; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_status IS 'Lists all possible status values for all tables';


--
-- TOC entry 237 (class 1259 OID 41070)
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
-- TOC entry 238 (class 1259 OID 41071)
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
-- TOC entry 5106 (class 0 OID 0)
-- Dependencies: 238
-- Name: TABLE ohd_enum_title; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_title IS 'Lists all possible name titles';


--
-- TOC entry 239 (class 1259 OID 41074)
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
-- TOC entry 240 (class 1259 OID 41075)
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
-- TOC entry 5107 (class 0 OID 0)
-- Dependencies: 240
-- Name: TABLE ohd_enum_user_relation; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_enum_user_relation IS 'lists the relation of user with property';


--
-- TOC entry 241 (class 1259 OID 41078)
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
-- TOC entry 242 (class 1259 OID 41079)
-- Name: ohd_manage_permission; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_manage_permission (
    id bigint NOT NULL,
    role_id bigint,
    user_id bigint,
    setting_type_id bigint,
    created_at timestamp without time zone,
    modified_at timestamp without time zone,
    canadd smallint,
    canedit smallint,
    canview smallint,
    candelete smallint
);


ALTER TABLE public.ohd_manage_permission OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 41082)
-- Name: ohd_manage_permission_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_manage_permission ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_manage_permission_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 244 (class 1259 OID 41083)
-- Name: ohd_meter; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_meter (
    id bigint NOT NULL,
    property_id bigint NOT NULL,
    meter_number character varying NOT NULL,
    meter_type_id smallint,
    meter_alias character varying,
    daily_target_consumption numeric,
    contract_proof_document bigint,
    contract_end_date timestamp without time zone,
    address_line_1 character varying(100),
    address_line_2 character varying(100),
    city character varying(55),
    state character varying(55),
    country character varying(55),
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone,
    meter_master_typeid bigint,
    comments character varying,
    tax_number character varying,
    eft_number character varying
);


ALTER TABLE public.ohd_meter OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 41088)
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
-- TOC entry 246 (class 1259 OID 41089)
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
-- TOC entry 5108 (class 0 OID 0)
-- Dependencies: 246
-- Name: TABLE ohd_mobile_country_code; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_mobile_country_code IS 'Lists all possible mobile country codes';


--
-- TOC entry 247 (class 1259 OID 41092)
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
-- TOC entry 248 (class 1259 OID 41093)
-- Name: ohd_notification_type_master; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_notification_type_master (
    id bigint NOT NULL,
    notification_type character varying,
    status_id smallint,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_notification_type_master OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 41098)
-- Name: ohd_notification_type_master_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_notification_type_master ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_notification_type_master_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 250 (class 1259 OID 41099)
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
-- TOC entry 251 (class 1259 OID 41102)
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
-- TOC entry 252 (class 1259 OID 41103)
-- Name: ohd_payment_methods; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_payment_methods (
    id bigint NOT NULL,
    name character varying,
    display_name character varying,
    slug character varying,
    percentage double precision,
    discount double precision,
    status_id smallint,
    create_at timestamp without time zone,
    modified_at timestamp without time zone,
    modifed_by bigint,
    ipaymethod bigint
);


ALTER TABLE public.ohd_payment_methods OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 41108)
-- Name: ohd_payment_methods_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_payment_methods ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_payment_methods_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 254 (class 1259 OID 41109)
-- Name: ohd_payment_status; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_payment_status (
    id smallint NOT NULL,
    name character varying(55) NOT NULL,
    display_value character varying(55) NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_payment_status OWNER TO postgres;

--
-- TOC entry 5109 (class 0 OID 0)
-- Dependencies: 254
-- Name: TABLE ohd_payment_status; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_payment_status IS 'Lists all possible status values for all tables';


--
-- TOC entry 255 (class 1259 OID 41112)
-- Name: ohd_payment_status_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_payment_status ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_payment_status_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 256 (class 1259 OID 41113)
-- Name: ohd_property; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_property (
    id bigint NOT NULL,
    name character varying NOT NULL,
    unit_number character varying NOT NULL,
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
    modified_at timestamp without time zone,
    customer_agreement_id character varying
);


ALTER TABLE public.ohd_property OWNER TO postgres;

--
-- TOC entry 257 (class 1259 OID 41118)
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
-- TOC entry 258 (class 1259 OID 41119)
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
-- TOC entry 259 (class 1259 OID 41122)
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
-- TOC entry 260 (class 1259 OID 41123)
-- Name: ohd_request_reject_reason; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_request_reject_reason (
    id bigint NOT NULL,
    reason character varying NOT NULL,
    status_id smallint,
    created_at time without time zone,
    modified_at time without time zone,
    rejectionreasonfor smallint
);


ALTER TABLE public.ohd_request_reject_reason OWNER TO postgres;

--
-- TOC entry 261 (class 1259 OID 41128)
-- Name: ohd_request_reject_reason_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_request_reject_reason ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_request_reject_reason_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 262 (class 1259 OID 41129)
-- Name: ohd_setting_type; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_setting_type (
    id bigint NOT NULL,
    settingtype character varying(50),
    createdat time without time zone,
    display_name character varying,
    priority smallint
);


ALTER TABLE public.ohd_setting_type OWNER TO postgres;

--
-- TOC entry 263 (class 1259 OID 41132)
-- Name: ohd_setting_type_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_setting_type ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."ohd_setting_type_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 264 (class 1259 OID 41133)
-- Name: ohd_state; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_state (
    id bigint NOT NULL,
    name character varying(100),
    country_id bigint,
    status_id smallint NOT NULL,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_state OWNER TO postgres;

--
-- TOC entry 265 (class 1259 OID 41136)
-- Name: ohd_state_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_state ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_state_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 266 (class 1259 OID 41137)
-- Name: ohd_state_master; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_state_master (
    "Id" bigint NOT NULL,
    name character varying,
    status_id smallint
);


ALTER TABLE public.ohd_state_master OWNER TO postgres;

--
-- TOC entry 267 (class 1259 OID 41142)
-- Name: ohd_state_master_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_state_master ALTER COLUMN "Id" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."ohd_state_master_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 268 (class 1259 OID 41143)
-- Name: ohd_top_up_transactions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_top_up_transactions (
    id bigint NOT NULL,
    transaction_id character varying NOT NULL,
    amount double precision,
    user_id bigint,
    use_wallet smallint,
    meter_id bigint,
    transaction_fee double precision,
    recharge_amount double precision,
    created_at timestamp without time zone,
    modified_at timestamp without time zone,
    flag smallint,
    pf_response character varying,
    payment_method_id bigint,
    ipaymethod_id bigint,
    topup_status character varying,
    vend_response character varying,
    trial_vend_response character varying,
    std_token character varying,
    net_up_transaction character varying,
    wallet_amount_used double precision,
    final_amount_to_pay double precision,
    debt_amount double precision
);


ALTER TABLE public.ohd_top_up_transactions OWNER TO postgres;

--
-- TOC entry 269 (class 1259 OID 41148)
-- Name: ohd_top_up_transactions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_top_up_transactions ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_top_up_transactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 270 (class 1259 OID 41149)
-- Name: ohd_transaction_type_master; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_transaction_type_master (
    id bigint NOT NULL,
    transaction_type character varying,
    status_id smallint,
    created_at time without time zone,
    last_modified_at time without time zone
);


ALTER TABLE public.ohd_transaction_type_master OWNER TO postgres;

--
-- TOC entry 271 (class 1259 OID 41154)
-- Name: ohd_transaction_type_master_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_transaction_type_master ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_transaction_type_master_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 272 (class 1259 OID 41155)
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
    address_line_1 character varying,
    address_line_2 character varying,
    city character varying,
    state character varying,
    country character varying,
    communication_setting integer,
    status_id smallint DEFAULT 1 NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    modified_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    address_lattitude character varying(200)[],
    address_longitude character varying(200)[],
    proof_document_id bigint,
    profile_url character varying,
    failed_to_validate bigint DEFAULT 0 NOT NULL,
    failedtovalidate_modified_at timestamp with time zone,
    isblocked boolean DEFAULT false NOT NULL,
    last_login_date timestamp with time zone,
    comments character varying,
    tax_number character varying,
    terms_accepted boolean
);


ALTER TABLE public.ohd_user OWNER TO postgres;

--
-- TOC entry 5110 (class 0 OID 0)
-- Dependencies: 272
-- Name: TABLE ohd_user; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_user IS 'User';


--
-- TOC entry 5111 (class 0 OID 0)
-- Dependencies: 272
-- Name: COLUMN ohd_user.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.id IS 'User ID';


--
-- TOC entry 5112 (class 0 OID 0)
-- Dependencies: 272
-- Name: COLUMN ohd_user.mobile; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.mobile IS 'Mobile';


--
-- TOC entry 5113 (class 0 OID 0)
-- Dependencies: 272
-- Name: COLUMN ohd_user.email; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.email IS 'Email';


--
-- TOC entry 5114 (class 0 OID 0)
-- Dependencies: 272
-- Name: COLUMN ohd_user.password; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.password IS 'Password';


--
-- TOC entry 5115 (class 0 OID 0)
-- Dependencies: 272
-- Name: COLUMN ohd_user.role_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.role_id IS 'Role ID';


--
-- TOC entry 5116 (class 0 OID 0)
-- Dependencies: 272
-- Name: COLUMN ohd_user.status_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.status_id IS 'Status';


--
-- TOC entry 5117 (class 0 OID 0)
-- Dependencies: 272
-- Name: COLUMN ohd_user.created_at; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.created_at IS 'Created At';


--
-- TOC entry 5118 (class 0 OID 0)
-- Dependencies: 272
-- Name: COLUMN ohd_user.modified_at; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user.modified_at IS 'Modified At';


--
-- TOC entry 273 (class 1259 OID 41165)
-- Name: ohd_user_cards; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_user_cards (
    id bigint NOT NULL,
    user_id bigint,
    card_holder_name character varying,
    card_number character varying,
    valid_thru character varying,
    status_id smallint,
    created_at time without time zone,
    modified_at time without time zone,
    use_default boolean,
    masked_card_number character varying
);


ALTER TABLE public.ohd_user_cards OWNER TO postgres;

--
-- TOC entry 274 (class 1259 OID 41170)
-- Name: ohd_user_cards_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_user_cards ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_user_cards_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 275 (class 1259 OID 41171)
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
-- TOC entry 5119 (class 0 OID 0)
-- Dependencies: 275
-- Name: TABLE ohd_user_communication_setting; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_user_communication_setting IS 'notification setting as per user';


--
-- TOC entry 276 (class 1259 OID 41174)
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
-- TOC entry 277 (class 1259 OID 41175)
-- Name: ohd_user_device_token; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_user_device_token (
    id bigint NOT NULL,
    user_id bigint,
    device_token character varying,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_user_device_token OWNER TO postgres;

--
-- TOC entry 278 (class 1259 OID 41180)
-- Name: ohd_user_device_token_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_user_device_token ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_user_device_token_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 279 (class 1259 OID 41181)
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
-- TOC entry 280 (class 1259 OID 41182)
-- Name: ohd_user_notifications; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_user_notifications (
    id bigint NOT NULL,
    user_id bigint,
    title character varying,
    description character varying,
    notification_type bigint,
    status_id smallint,
    created_at timestamp without time zone,
    modified_at timestamp without time zone
);


ALTER TABLE public.ohd_user_notifications OWNER TO postgres;

--
-- TOC entry 281 (class 1259 OID 41187)
-- Name: ohd_user_notifications_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_user_notifications ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_user_notifications_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 282 (class 1259 OID 41188)
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
-- TOC entry 5120 (class 0 OID 0)
-- Dependencies: 282
-- Name: TABLE ohd_user_role_master; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON TABLE public.ohd_user_role_master IS 'User Role Master';


--
-- TOC entry 5121 (class 0 OID 0)
-- Dependencies: 282
-- Name: COLUMN ohd_user_role_master.id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.id IS 'Role ID';


--
-- TOC entry 5122 (class 0 OID 0)
-- Dependencies: 282
-- Name: COLUMN ohd_user_role_master.name; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.name IS 'Role Name';


--
-- TOC entry 5123 (class 0 OID 0)
-- Dependencies: 282
-- Name: COLUMN ohd_user_role_master.description; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.description IS 'Role Description';


--
-- TOC entry 5124 (class 0 OID 0)
-- Dependencies: 282
-- Name: COLUMN ohd_user_role_master.status_id; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.status_id IS 'Status';


--
-- TOC entry 5125 (class 0 OID 0)
-- Dependencies: 282
-- Name: COLUMN ohd_user_role_master.created_at; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.created_at IS 'Created At';


--
-- TOC entry 5126 (class 0 OID 0)
-- Dependencies: 282
-- Name: COLUMN ohd_user_role_master.modified_at; Type: COMMENT; Schema: public; Owner: postgres
--

COMMENT ON COLUMN public.ohd_user_role_master.modified_at IS 'Modified At';


--
-- TOC entry 283 (class 1259 OID 41196)
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
-- TOC entry 284 (class 1259 OID 41197)
-- Name: ohd_user_wallet; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_user_wallet (
    id bigint NOT NULL,
    user_id bigint,
    balance double precision,
    status_id smallint,
    created_at timestamp without time zone,
    last_modified_at timestamp without time zone
);


ALTER TABLE public.ohd_user_wallet OWNER TO postgres;

--
-- TOC entry 285 (class 1259 OID 41200)
-- Name: ohd_user_wallet_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_user_wallet ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_user_wallet_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 286 (class 1259 OID 41201)
-- Name: ohd_wallet_transaction; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.ohd_wallet_transaction (
    id bigint NOT NULL,
    wallet_id bigint,
    transaction_amount double precision,
    transaction_type_id bigint,
    updated_balance double precision,
    transaction_remark character varying,
    transaction_date timestamp without time zone
);


ALTER TABLE public.ohd_wallet_transaction OWNER TO postgres;

--
-- TOC entry 287 (class 1259 OID 41206)
-- Name: ohd_wallet_transaction_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.ohd_wallet_transaction ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.ohd_wallet_transaction_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 288 (class 1259 OID 41207)
-- Name: user_notification_connections; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_notification_connections (
    id bigint NOT NULL,
    user_id bigint,
    connection_id character varying,
    log_on timestamp without time zone
);


ALTER TABLE public.user_notification_connections OWNER TO postgres;

--
-- TOC entry 289 (class 1259 OID 41212)
-- Name: user_notification_connections_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.user_notification_connections ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.user_notification_connections_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 4857 (class 2606 OID 41214)
-- Name: ohd_otp ohd_Otp_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_otp
    ADD CONSTRAINT "ohd_Otp_pkey" PRIMARY KEY ("Id");


--
-- TOC entry 4875 (class 2606 OID 41216)
-- Name: ohd_setting_type ohd_SettingType_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_setting_type
    ADD CONSTRAINT "ohd_SettingType_pkey" PRIMARY KEY (id);


--
-- TOC entry 4797 (class 2606 OID 41218)
-- Name: ohd_application_log ohd_application_log_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_application_log
    ADD CONSTRAINT ohd_application_log_pkey PRIMARY KEY (id);


--
-- TOC entry 4799 (class 2606 OID 41220)
-- Name: ohd_associate_user_settings ohd_associate_user_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_associate_user_settings
    ADD CONSTRAINT ohd_associate_user_settings_pkey PRIMARY KEY (id);


--
-- TOC entry 4801 (class 2606 OID 41222)
-- Name: ohd_bank_accounts ohd_bank_accounts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_bank_accounts
    ADD CONSTRAINT ohd_bank_accounts_pkey PRIMARY KEY (id);


--
-- TOC entry 4803 (class 2606 OID 41224)
-- Name: ohd_company ohd_company_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_company
    ADD CONSTRAINT ohd_company_name_key UNIQUE (name);


--
-- TOC entry 4805 (class 2606 OID 41226)
-- Name: ohd_company ohd_company_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_company
    ADD CONSTRAINT ohd_company_pkey PRIMARY KEY (id);


--
-- TOC entry 4807 (class 2606 OID 41228)
-- Name: ohd_configuration ohd_configuration_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_configuration
    ADD CONSTRAINT ohd_configuration_name_key UNIQUE (name);


--
-- TOC entry 4809 (class 2606 OID 41230)
-- Name: ohd_configuration ohd_configuration_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_configuration
    ADD CONSTRAINT ohd_configuration_pkey PRIMARY KEY (id);


--
-- TOC entry 4813 (class 2606 OID 41232)
-- Name: ohd_country_master ohd_country_master_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_country_master
    ADD CONSTRAINT ohd_country_master_pkey PRIMARY KEY (id);


--
-- TOC entry 4811 (class 2606 OID 41234)
-- Name: ohd_country ohd_country_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_country
    ADD CONSTRAINT ohd_country_pkey PRIMARY KEY (id);


--
-- TOC entry 4877 (class 2606 OID 41236)
-- Name: ohd_state ohd_cstate_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_state
    ADD CONSTRAINT ohd_cstate_pkey PRIMARY KEY (id);


--
-- TOC entry 4815 (class 2606 OID 41238)
-- Name: ohd_document ohd_document_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document
    ADD CONSTRAINT ohd_document_pkey PRIMARY KEY (id);


--
-- TOC entry 4817 (class 2606 OID 41240)
-- Name: ohd_document_type ohd_document_type_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document_type
    ADD CONSTRAINT ohd_document_type_name_key UNIQUE (name);


--
-- TOC entry 4819 (class 2606 OID 41242)
-- Name: ohd_document_type ohd_document_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document_type
    ADD CONSTRAINT ohd_document_type_pkey PRIMARY KEY (id);


--
-- TOC entry 4821 (class 2606 OID 41244)
-- Name: ohd_enum_communications ohd_enum_communications_display_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_communications
    ADD CONSTRAINT ohd_enum_communications_display_name_key UNIQUE (display_name);


--
-- TOC entry 4823 (class 2606 OID 41246)
-- Name: ohd_enum_communications ohd_enum_communications_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_communications
    ADD CONSTRAINT ohd_enum_communications_name_key UNIQUE (name);


--
-- TOC entry 4825 (class 2606 OID 41248)
-- Name: ohd_enum_communications ohd_enum_communications_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_communications
    ADD CONSTRAINT ohd_enum_communications_pkey PRIMARY KEY (id);


--
-- TOC entry 4827 (class 2606 OID 41250)
-- Name: ohd_enum_meter_type ohd_enum_meter_type_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_meter_type
    ADD CONSTRAINT ohd_enum_meter_type_name_key UNIQUE (name);


--
-- TOC entry 4829 (class 2606 OID 41252)
-- Name: ohd_enum_meter_type ohd_enum_meter_type_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_meter_type
    ADD CONSTRAINT ohd_enum_meter_type_pkey PRIMARY KEY (id);


--
-- TOC entry 4831 (class 2606 OID 41254)
-- Name: ohd_enum_status ohd_enum_status_display_value_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_status
    ADD CONSTRAINT ohd_enum_status_display_value_key UNIQUE (display_value);


--
-- TOC entry 4833 (class 2606 OID 41256)
-- Name: ohd_enum_status ohd_enum_status_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_status
    ADD CONSTRAINT ohd_enum_status_name_key UNIQUE (name);


--
-- TOC entry 4835 (class 2606 OID 41258)
-- Name: ohd_enum_status ohd_enum_status_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_status
    ADD CONSTRAINT ohd_enum_status_pkey PRIMARY KEY (id);


--
-- TOC entry 4837 (class 2606 OID 41260)
-- Name: ohd_enum_title ohd_enum_title_display_value_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_title
    ADD CONSTRAINT ohd_enum_title_display_value_key UNIQUE (display_value);


--
-- TOC entry 4839 (class 2606 OID 41262)
-- Name: ohd_enum_title ohd_enum_title_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_title
    ADD CONSTRAINT ohd_enum_title_name_key UNIQUE (name);


--
-- TOC entry 4841 (class 2606 OID 41264)
-- Name: ohd_enum_title ohd_enum_title_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_title
    ADD CONSTRAINT ohd_enum_title_pkey PRIMARY KEY (id);


--
-- TOC entry 4843 (class 2606 OID 41266)
-- Name: ohd_enum_user_relation ohd_enum_user_relation_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_user_relation
    ADD CONSTRAINT ohd_enum_user_relation_name_key UNIQUE (name);


--
-- TOC entry 4845 (class 2606 OID 41268)
-- Name: ohd_enum_user_relation ohd_enum_user_relation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_user_relation
    ADD CONSTRAINT ohd_enum_user_relation_pkey PRIMARY KEY (id);


--
-- TOC entry 4847 (class 2606 OID 41270)
-- Name: ohd_manage_permission ohd_manage_permission_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_manage_permission
    ADD CONSTRAINT ohd_manage_permission_pkey PRIMARY KEY (id);


--
-- TOC entry 4849 (class 2606 OID 41272)
-- Name: ohd_meter ohd_meter_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT ohd_meter_pkey PRIMARY KEY (id);


--
-- TOC entry 4851 (class 2606 OID 41274)
-- Name: ohd_mobile_country_code ohd_mobile_country_code_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_mobile_country_code
    ADD CONSTRAINT ohd_mobile_country_code_code_key UNIQUE (code);


--
-- TOC entry 4853 (class 2606 OID 41276)
-- Name: ohd_mobile_country_code ohd_mobile_country_code_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_mobile_country_code
    ADD CONSTRAINT ohd_mobile_country_code_pkey PRIMARY KEY (id);


--
-- TOC entry 4855 (class 2606 OID 41278)
-- Name: ohd_notification_type_master ohd_notification_type_master_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_notification_type_master
    ADD CONSTRAINT ohd_notification_type_master_pkey PRIMARY KEY (id);


--
-- TOC entry 4859 (class 2606 OID 41280)
-- Name: ohd_payment_methods ohd_payment_methods_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_payment_methods
    ADD CONSTRAINT ohd_payment_methods_pkey PRIMARY KEY (id);


--
-- TOC entry 4861 (class 2606 OID 41282)
-- Name: ohd_payment_status ohd_payment_status_display_value_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_payment_status
    ADD CONSTRAINT ohd_payment_status_display_value_key UNIQUE (display_value);


--
-- TOC entry 4863 (class 2606 OID 41284)
-- Name: ohd_payment_status ohd_payment_status_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_payment_status
    ADD CONSTRAINT ohd_payment_status_name_key UNIQUE (name);


--
-- TOC entry 4865 (class 2606 OID 41286)
-- Name: ohd_payment_status ohd_payment_status_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_payment_status
    ADD CONSTRAINT ohd_payment_status_pkey PRIMARY KEY (id);


--
-- TOC entry 4867 (class 2606 OID 41288)
-- Name: ohd_property ohd_property_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT ohd_property_pkey PRIMARY KEY (id);


--
-- TOC entry 4869 (class 2606 OID 41290)
-- Name: ohd_property ohd_property_unit_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT ohd_property_unit_number_key UNIQUE (unit_number);


--
-- TOC entry 4871 (class 2606 OID 41292)
-- Name: ohd_property_user_relation ohd_property_user_relation_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT ohd_property_user_relation_pkey PRIMARY KEY (id);


--
-- TOC entry 4873 (class 2606 OID 41294)
-- Name: ohd_request_reject_reason ohd_request_reject_reason_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_request_reject_reason
    ADD CONSTRAINT ohd_request_reject_reason_pkey PRIMARY KEY (id);


--
-- TOC entry 4879 (class 2606 OID 41296)
-- Name: ohd_state_master ohd_state_master_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_state_master
    ADD CONSTRAINT ohd_state_master_pkey PRIMARY KEY ("Id");


--
-- TOC entry 4881 (class 2606 OID 41298)
-- Name: ohd_top_up_transactions ohd_top_up_transactions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_top_up_transactions
    ADD CONSTRAINT ohd_top_up_transactions_pkey PRIMARY KEY (id);


--
-- TOC entry 4883 (class 2606 OID 41300)
-- Name: ohd_transaction_type_master ohd_transaction_type_master_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_transaction_type_master
    ADD CONSTRAINT ohd_transaction_type_master_pkey PRIMARY KEY (id);


--
-- TOC entry 4891 (class 2606 OID 41302)
-- Name: ohd_user_cards ohd_user_cards_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_cards
    ADD CONSTRAINT ohd_user_cards_pkey PRIMARY KEY (id);


--
-- TOC entry 4893 (class 2606 OID 41304)
-- Name: ohd_user_communication_setting ohd_user_communication_setting_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_communication_setting
    ADD CONSTRAINT ohd_user_communication_setting_pkey PRIMARY KEY (id);


--
-- TOC entry 4895 (class 2606 OID 41306)
-- Name: ohd_user_device_token ohd_user_device_token_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_device_token
    ADD CONSTRAINT ohd_user_device_token_pkey PRIMARY KEY (id);


--
-- TOC entry 4885 (class 2606 OID 41308)
-- Name: ohd_user ohd_user_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT ohd_user_email_key UNIQUE (email);


--
-- TOC entry 4887 (class 2606 OID 41310)
-- Name: ohd_user ohd_user_mobile_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT ohd_user_mobile_key UNIQUE (mobile);


--
-- TOC entry 4897 (class 2606 OID 41312)
-- Name: ohd_user_notifications ohd_user_notifications_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_notifications
    ADD CONSTRAINT ohd_user_notifications_pkey PRIMARY KEY (id);


--
-- TOC entry 4889 (class 2606 OID 41314)
-- Name: ohd_user ohd_user_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT ohd_user_pkey PRIMARY KEY (id);


--
-- TOC entry 4899 (class 2606 OID 41316)
-- Name: ohd_user_role_master ohd_user_role_master_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_role_master
    ADD CONSTRAINT ohd_user_role_master_name_key UNIQUE (name);


--
-- TOC entry 4901 (class 2606 OID 41318)
-- Name: ohd_user_role_master ohd_user_role_master_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_role_master
    ADD CONSTRAINT ohd_user_role_master_pkey PRIMARY KEY (id);


--
-- TOC entry 4903 (class 2606 OID 41320)
-- Name: ohd_user_wallet ohd_user_wallet_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_wallet
    ADD CONSTRAINT ohd_user_wallet_pkey PRIMARY KEY (id);


--
-- TOC entry 4905 (class 2606 OID 41322)
-- Name: ohd_wallet_transaction ohd_wallet_transaction_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_wallet_transaction
    ADD CONSTRAINT ohd_wallet_transaction_pkey PRIMARY KEY (id);


--
-- TOC entry 4937 (class 2606 OID 41323)
-- Name: ohd_state_master ek_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_state_master
    ADD CONSTRAINT ek_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4938 (class 2606 OID 41328)
-- Name: ohd_user fk_communication_setting_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_communication_setting_id FOREIGN KEY (communication_setting) REFERENCES public.ohd_enum_communications(id) NOT VALID;


--
-- TOC entry 4939 (class 2606 OID 41333)
-- Name: ohd_user fk_company_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_company_id FOREIGN KEY (company_id) REFERENCES public.ohd_company(id);


--
-- TOC entry 4925 (class 2606 OID 41338)
-- Name: ohd_otp fk_company_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_otp
    ADD CONSTRAINT fk_company_id FOREIGN KEY ("companyId") REFERENCES public.ohd_company(id) NOT VALID;


--
-- TOC entry 4928 (class 2606 OID 41343)
-- Name: ohd_property fk_company_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT fk_company_id FOREIGN KEY (company_id) REFERENCES public.ohd_company(id) NOT VALID;


--
-- TOC entry 4919 (class 2606 OID 41348)
-- Name: ohd_meter fk_contract_proof_document_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT fk_contract_proof_document_id FOREIGN KEY (contract_proof_document) REFERENCES public.ohd_document(id);


--
-- TOC entry 4926 (class 2606 OID 41353)
-- Name: ohd_otp fk_countryCode_Id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_otp
    ADD CONSTRAINT "fk_countryCode_Id" FOREIGN KEY ("CountryCodeId") REFERENCES public.ohd_mobile_country_code(id) NOT VALID;


--
-- TOC entry 4907 (class 2606 OID 41358)
-- Name: ohd_company fk_country_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_company
    ADD CONSTRAINT fk_country_id FOREIGN KEY (country_id) REFERENCES public.ohd_country_master(id) NOT VALID;


--
-- TOC entry 4935 (class 2606 OID 41363)
-- Name: ohd_state fk_country_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_state
    ADD CONSTRAINT fk_country_id FOREIGN KEY (country_id) REFERENCES public.ohd_country(id);


--
-- TOC entry 4912 (class 2606 OID 41368)
-- Name: ohd_document fk_document_type_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document
    ADD CONSTRAINT fk_document_type_id FOREIGN KEY (document_type) REFERENCES public.ohd_document_type(id);


--
-- TOC entry 4920 (class 2606 OID 41373)
-- Name: ohd_meter fk_meter_type_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT fk_meter_type_id FOREIGN KEY (meter_type_id) REFERENCES public.ohd_enum_meter_type(id);


--
-- TOC entry 4940 (class 2606 OID 41378)
-- Name: ohd_user fk_mobile_code_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_mobile_code_id FOREIGN KEY (mobile_country_code) REFERENCES public.ohd_mobile_country_code(id);


--
-- TOC entry 4946 (class 2606 OID 41383)
-- Name: ohd_user_notifications fk_notificatioin_user_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_notifications
    ADD CONSTRAINT fk_notificatioin_user_id FOREIGN KEY (user_id) REFERENCES public.ohd_user(id);


--
-- TOC entry 4924 (class 2606 OID 41388)
-- Name: ohd_notification_type_master fk_notification_status; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_notification_type_master
    ADD CONSTRAINT fk_notification_status FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4929 (class 2606 OID 41393)
-- Name: ohd_property fk_owner_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT fk_owner_id FOREIGN KEY (owner_id) REFERENCES public.ohd_user(id);


--
-- TOC entry 4941 (class 2606 OID 41398)
-- Name: ohd_user fk_proof_document_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_proof_document_id FOREIGN KEY (proof_document_id) REFERENCES public.ohd_document(id) NOT VALID;


--
-- TOC entry 4931 (class 2606 OID 41403)
-- Name: ohd_property_user_relation fk_property_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT fk_property_id FOREIGN KEY (property_id) REFERENCES public.ohd_property(id);


--
-- TOC entry 4921 (class 2606 OID 41408)
-- Name: ohd_meter fk_property_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT fk_property_id FOREIGN KEY (property_id) REFERENCES public.ohd_property(id);


--
-- TOC entry 4906 (class 2606 OID 41413)
-- Name: ohd_associate_user_settings fk_property_user_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_associate_user_settings
    ADD CONSTRAINT fk_property_user_id FOREIGN KEY (property_user_id) REFERENCES public.ohd_property_user_relation(id) NOT VALID;


--
-- TOC entry 4942 (class 2606 OID 41418)
-- Name: ohd_user fk_role_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_role_id FOREIGN KEY (role_id) REFERENCES public.ohd_user_role_master(id);


--
-- TOC entry 4908 (class 2606 OID 41423)
-- Name: ohd_company fk_state_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_company
    ADD CONSTRAINT fk_state_id FOREIGN KEY (state_id) REFERENCES public.ohd_enum_status(id) NOT VALID;


--
-- TOC entry 4922 (class 2606 OID 41428)
-- Name: ohd_meter fk_status; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_meter
    ADD CONSTRAINT fk_status FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4916 (class 2606 OID 41433)
-- Name: ohd_enum_meter_type fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_meter_type
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4917 (class 2606 OID 41438)
-- Name: ohd_enum_title fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_title
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4915 (class 2606 OID 41443)
-- Name: ohd_enum_communications fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_communications
    ADD CONSTRAINT fk_status_id FOREIGN KEY (id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4918 (class 2606 OID 41448)
-- Name: ohd_enum_user_relation fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_enum_user_relation
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4914 (class 2606 OID 41453)
-- Name: ohd_document_type fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document_type
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4913 (class 2606 OID 41458)
-- Name: ohd_document fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_document
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4910 (class 2606 OID 41463)
-- Name: ohd_configuration fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_configuration
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4909 (class 2606 OID 41468)
-- Name: ohd_company fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_company
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4923 (class 2606 OID 41473)
-- Name: ohd_mobile_country_code fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_mobile_country_code
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4943 (class 2606 OID 41478)
-- Name: ohd_user fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4930 (class 2606 OID 41483)
-- Name: ohd_property fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4932 (class 2606 OID 41488)
-- Name: ohd_property_user_relation fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4927 (class 2606 OID 41493)
-- Name: ohd_otp fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_otp
    ADD CONSTRAINT fk_status_id FOREIGN KEY ("StatusId") REFERENCES public.ohd_enum_status(id) NOT VALID;


--
-- TOC entry 4911 (class 2606 OID 41498)
-- Name: ohd_country_master fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_country_master
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4936 (class 2606 OID 41503)
-- Name: ohd_state fk_status_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_state
    ADD CONSTRAINT fk_status_id FOREIGN KEY (status_id) REFERENCES public.ohd_enum_status(id);


--
-- TOC entry 4944 (class 2606 OID 41508)
-- Name: ohd_user fk_title_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user
    ADD CONSTRAINT fk_title_id FOREIGN KEY (title) REFERENCES public.ohd_enum_title(id);


--
-- TOC entry 4948 (class 2606 OID 41513)
-- Name: ohd_wallet_transaction fk_transaction_type_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_wallet_transaction
    ADD CONSTRAINT fk_transaction_type_id FOREIGN KEY (transaction_type_id) REFERENCES public.ohd_transaction_type_master(id);


--
-- TOC entry 4947 (class 2606 OID 41518)
-- Name: ohd_user_wallet fk_user_balance_userid; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_wallet
    ADD CONSTRAINT fk_user_balance_userid FOREIGN KEY (user_id) REFERENCES public.ohd_user(id);


--
-- TOC entry 4945 (class 2606 OID 41523)
-- Name: ohd_user_cards fk_user_card_user_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_user_cards
    ADD CONSTRAINT fk_user_card_user_id FOREIGN KEY (user_id) REFERENCES public.ohd_user(id) NOT VALID;


--
-- TOC entry 4933 (class 2606 OID 41528)
-- Name: ohd_property_user_relation fk_user_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT fk_user_id FOREIGN KEY (user_id) REFERENCES public.ohd_user(id);


--
-- TOC entry 4934 (class 2606 OID 41533)
-- Name: ohd_property_user_relation fk_user_relation_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_property_user_relation
    ADD CONSTRAINT fk_user_relation_id FOREIGN KEY (user_relation_id) REFERENCES public.ohd_enum_user_relation(id);


--
-- TOC entry 4949 (class 2606 OID 41538)
-- Name: ohd_wallet_transaction fk_wallet_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.ohd_wallet_transaction
    ADD CONSTRAINT fk_wallet_id FOREIGN KEY (wallet_id) REFERENCES public.ohd_user_wallet(id);


-- Completed on 2024-06-24 08:10:45

--
-- PostgreSQL database dump complete
--

