--
-- PostgreSQL database dump
--

-- Dumped from database version 16.1
-- Dumped by pg_dump version 16.1

-- Started on 2024-06-24 08:16:36

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
-- TOC entry 4992 (class 0 OID 41067)
-- Dependencies: 236
-- Data for Name: ohd_enum_status; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'Active', 'Active', NULL, NULL);
INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (3, 'NotVerified', 'Otp not verified', NULL, NULL);
INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (4, 'Pending', 'Pending', NULL, NULL);
INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (5, 'InActive', 'In active', NULL, NULL);
INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (6, 'Rejected', 'Rejected', NULL, NULL);
INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (7, 'Sent', 'Sent', NULL, NULL);
INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (8, 'Read', 'Read', NULL, NULL);
INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (9, 'InProcess', 'In process', NULL, NULL);
INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (10, 'DeActive', 'De active', NULL, NULL);
INSERT INTO public.ohd_enum_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (2, 'InComplete', 'In Complete', NULL, NULL);


--
-- TOC entry 4984 (class 0 OID 41041)
-- Dependencies: 226
-- Data for Name: ohd_country_master; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_country_master (id, name, status_id) OVERRIDING SYSTEM VALUE VALUES (1, 'South Africa', 1);


--
-- TOC entry 4978 (class 0 OID 41025)
-- Dependencies: 220
-- Data for Name: ohd_company; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_company (id, name, status_id, created_at, modified_at, firstname, lastname, address, country, state, mobile, email, city, zip, description, state_id, country_id, companylogourl, faviconlogourl, smalllogourl) OVERRIDING SYSTEM VALUE VALUES (1, 'Ontec Home', 1, NULL, '2024-06-21 06:27:18.295032', 'Ontec', 'Home', 'South Africa', 'India', 'Maharashtra', '92828228', 'shital.patil47@gmail.com', 'Pune', '121212', 'This is ontec home', 1, 1, 'http://104.251.223.167:7010/assets/images/1.png', NULL, NULL);


--
-- TOC entry 4980 (class 0 OID 41031)
-- Dependencies: 222
-- Data for Name: ohd_configuration; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (1, 'cardtransactiondiscount', '0', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (4, 'cardtransactionfee', '0.05', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (5, 'banktransactionfee', '0.05', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (6, 'banktransactiondiscount', '0.04', 1, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (12, 'userapproval', '0', 1, '2024-04-23 02:42:49.697794', NULL, 1408, NULL, 'User Approval', NULL, NULL, 'boolean', false, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (20, 'eftRandomDigitNumberLength', '8', 1, NULL, '2024-06-22 05:42:29.469056', NULL, 1, 'Random Digit Length', NULL, NULL, 'integer', true, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (18, 'backgroundImage', 'http://104.251.223.167:7010/assets/images/18.png', 1, NULL, '2024-06-22 05:42:29.472153', NULL, 1, 'Sign up Background Image', NULL, NULL, 'File', true, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (16, 'maxtopupamount', '100000', 1, NULL, '2024-06-22 05:42:29.473641', NULL, 1, 'Maximum Top Up Amount', 'maximum top up amount', NULL, 'integer', true, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (15, 'mintopupamount', '50', 1, NULL, '2024-06-22 05:42:29.485457', NULL, 1, 'Minimum Top Up Amount', 'minimum topup amount', NULL, 'integer', true, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (14, 'maxdailytarget', '10000', 1, NULL, '2024-06-22 05:42:29.487057', NULL, 1, 'Maximum Daily Target Consumption', 'maximum daily target', NULL, 'integer', true, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (13, 'mindailytarget', '20', 1, NULL, '2024-06-22 05:42:29.488682', NULL, 1, 'Minimum Daily Target Consumption', 'minimum daily target', NULL, 'integer', true, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (9, 'meterapproval', '1', 1, NULL, '2024-06-22 05:42:29.490851', NULL, 1, 'Enable Meter Approval Through Admin?', 'Admin needs to approve each meter request', NULL, 'boolean', true, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (8, 'consumerapproval', '1', 1, NULL, '2024-06-22 05:42:29.492522', NULL, 1, 'Enable Consumer Approval Through Admin?', NULL, NULL, 'boolean', true, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (7, 'ContractDocumentSizeInMB', '5', 1, NULL, '2024-06-22 05:42:29.494046', NULL, 1, 'Contract Document Size', NULL, NULL, 'integer', true, NULL, 1);
INSERT INTO public.ohd_configuration (id, name, value, status_id, created_at, modified_at, created_by, modified_by, display_name, description, note, configuration_type, is_editable, unit, company_id) OVERRIDING SYSTEM VALUE VALUES (19, 'eftprecharacters', 'OH', 1, '2024-05-23 04:04:20.059673', '2024-06-22 05:42:29.470616', NULL, 1, 'EFT Prefix', 'EFT Number Prefix', NULL, 'Text', true, NULL, 1);


--
-- TOC entry 4982 (class 0 OID 41037)
-- Dependencies: 224
-- Data for Name: ohd_country; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_country (id, name, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'South Africa', 1, NULL, NULL);


--
-- TOC entry 4986 (class 0 OID 41053)
-- Dependencies: 230
-- Data for Name: ohd_document_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_document_type (id, name, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'Contract proof', 1, NULL, NULL);
INSERT INTO public.ohd_document_type (id, name, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (3, 'Driving License', 1, NULL, NULL);
INSERT INTO public.ohd_document_type (id, name, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (5, 'South Africa ID', 1, NULL, NULL);
INSERT INTO public.ohd_document_type (id, name, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (4, 'Passport', 1, NULL, NULL);


--
-- TOC entry 4988 (class 0 OID 41057)
-- Dependencies: 232
-- Data for Name: ohd_enum_communications; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_communications (id, name, display_name, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'Mobile', 'On Mobile', 1, NULL, NULL);
INSERT INTO public.ohd_enum_communications (id, name, display_name, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (2, 'Email', 'Email', 1, NULL, NULL);


--
-- TOC entry 4990 (class 0 OID 41063)
-- Dependencies: 234
-- Data for Name: ohd_enum_meter_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_meter_type (id, name, display_value, status_id, created_at, modified_at, unitofmeasure, meterreadingtype, priority, meterreadingtypeid, mindailytarget, maxdailytarget) OVERRIDING SYSTEM VALUE VALUES (1, 'Gas', 'Gas', 1, NULL, NULL, 'm³', 'GAS_VOLUME', 1, 'GAS_VOLUME', 10, 1000);
INSERT INTO public.ohd_enum_meter_type (id, name, display_value, status_id, created_at, modified_at, unitofmeasure, meterreadingtype, priority, meterreadingtypeid, mindailytarget, maxdailytarget) OVERRIDING SYSTEM VALUE VALUES (2, 'Water', 'Water', 1, NULL, NULL, 'kL', 'POTABLE_WATER_VOLUME', 1, 'POTABLE_WATER_VOLUME', 10, 1000);
INSERT INTO public.ohd_enum_meter_type (id, name, display_value, status_id, created_at, modified_at, unitofmeasure, meterreadingtype, priority, meterreadingtypeid, mindailytarget, maxdailytarget) OVERRIDING SYSTEM VALUE VALUES (3, 'Electricity', 'Electricity', 1, NULL, NULL, 'KWh', 'REAL_ENERGY_FWD', 1, 'REAL_ENERGY_FWD', 10, 1000);
INSERT INTO public.ohd_enum_meter_type (id, name, display_value, status_id, created_at, modified_at, unitofmeasure, meterreadingtype, priority, meterreadingtypeid, mindailytarget, maxdailytarget) OVERRIDING SYSTEM VALUE VALUES (4, 'Hot water', 'Hot water', 1, NULL, NULL, 'KL', 'Potable', 1, 'Potable', 10, 1000);
INSERT INTO public.ohd_enum_meter_type (id, name, display_value, status_id, created_at, modified_at, unitofmeasure, meterreadingtype, priority, meterreadingtypeid, mindailytarget, maxdailytarget) OVERRIDING SYSTEM VALUE VALUES (5, 'Cooling', 'Cooling', 1, NULL, NULL, 'KL', 'REAL_ENERGY_FWD', 1, 'REAL_ENERGY_FWD', 10, 1000);
INSERT INTO public.ohd_enum_meter_type (id, name, display_value, status_id, created_at, modified_at, unitofmeasure, meterreadingtype, priority, meterreadingtypeid, mindailytarget, maxdailytarget) OVERRIDING SYSTEM VALUE VALUES (6, 'Solar', 'Solar', 1, NULL, NULL, 'Kw/m²', 'REAL_ENERGY_FWD', 1, 'REAL_ENERGY_FWD', 10, 1000);


--
-- TOC entry 4994 (class 0 OID 41071)
-- Dependencies: 238
-- Data for Name: ohd_enum_title; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_title (id, name, display_value, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (2, 'Mr', 'Mr', 1, NULL, NULL);
INSERT INTO public.ohd_enum_title (id, name, display_value, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (3, 'Mrs', 'Mrs', 1, NULL, NULL);
INSERT INTO public.ohd_enum_title (id, name, display_value, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (4, 'Ms', 'Ms', 1, NULL, NULL);


--
-- TOC entry 4996 (class 0 OID 41075)
-- Dependencies: 240
-- Data for Name: ohd_enum_user_relation; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_enum_user_relation (id, name, display_name, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'Tenant', 'Tenant', 1, NULL, NULL);
INSERT INTO public.ohd_enum_user_relation (id, name, display_name, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (2, 'Associate', 'Associate', 1, NULL, NULL);


--
-- TOC entry 4998 (class 0 OID 41089)
-- Dependencies: 246
-- Data for Name: ohd_mobile_country_code; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_mobile_country_code (id, code, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, '27', 1, NULL, NULL);
INSERT INTO public.ohd_mobile_country_code (id, code, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (2, '91', 1, NULL, NULL);


--
-- TOC entry 5000 (class 0 OID 41093)
-- Dependencies: 248
-- Data for Name: ohd_notification_type_master; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_notification_type_master (id, notification_type, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'Logout', 1, NULL, NULL);


--
-- TOC entry 5002 (class 0 OID 41103)
-- Dependencies: 252
-- Data for Name: ohd_payment_methods; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_payment_methods (id, name, display_name, slug, percentage, discount, status_id, create_at, modified_at, modifed_by, ipaymethod) OVERRIDING SYSTEM VALUE VALUES (3, 'You can pay with master card or visa card', 'Credit Card', 'cc', 7, 1, 1, '2024-04-18 00:42:46.322479', '2024-06-22 02:26:20.982143', 1, NULL);
INSERT INTO public.ohd_payment_methods (id, name, display_name, slug, percentage, discount, status_id, create_at, modified_at, modifed_by, ipaymethod) OVERRIDING SYSTEM VALUE VALUES (5, 'bank-transfer', 'Debitec Deposits', 'bank-transfer', 10, 2, 5, NULL, '2024-06-22 02:26:20.984311', 1, NULL);
INSERT INTO public.ohd_payment_methods (id, name, display_name, slug, percentage, discount, status_id, create_at, modified_at, modifed_by, ipaymethod) OVERRIDING SYSTEM VALUE VALUES (1, 'Electronic Fund Transfer', 'EFT', 'eft', 10, 2, 1, '2024-04-18 00:41:57.849295', '2024-06-22 02:26:20.986281', 1, NULL);
INSERT INTO public.ohd_payment_methods (id, name, display_name, slug, percentage, discount, status_id, create_at, modified_at, modifed_by, ipaymethod) OVERRIDING SYSTEM VALUE VALUES (2, 'You can pay with debit card', 'Debit Card', 'dc', 5, 2, 1, '2024-04-18 00:42:21.997249', '2024-06-22 02:26:20.988113', 1, NULL);


--
-- TOC entry 5004 (class 0 OID 41109)
-- Dependencies: 254
-- Data for Name: ohd_payment_status; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'Complete', 'Completed', NULL, NULL);
INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (2, 'Refunded', 'Refunded', NULL, NULL);
INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (3, 'Revoked', 'Revoked', NULL, NULL);
INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (4, 'Pending', 'Pending', NULL, NULL);
INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (5, 'Failed', 'Failed', NULL, NULL);
INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (6, 'OnHold', 'OnHold', NULL, NULL);
INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (7, 'Abandoned', 'Abandoned', NULL, NULL);
INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (8, 'Preapproved', 'Preapproved', NULL, NULL);
INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (9, 'Cancelled', 'Cancelled', NULL, NULL);
INSERT INTO public.ohd_payment_status (id, name, display_value, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (10, 'InProcess', 'InProcess', NULL, NULL);


--
-- TOC entry 5006 (class 0 OID 41123)
-- Dependencies: 260
-- Data for Name: ohd_request_reject_reason; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (2, 'Invalid Document', 1, '07:45:35.072331', NULL, 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (4, 'Invalid Reason', 1, '07:40:08.549523', NULL, 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (5, 'Invalid meter type', 1, '07:45:06.672699', NULL, 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (9, 'ABC', 5, '04:15:27.003647', '05:25:27.311387', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (8, 'other', 5, '03:48:39.674906', '05:25:43.006066', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (7, 'uhi', 5, '01:00:22.373102', '05:25:46.977374', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (6, 'test', 5, '02:14:15.963514', '05:25:50.040124', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (3, 'Invalid Proof', 5, '07:33:10.663544', '05:25:56.356073', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (10, 'new  reason', 5, '05:26:11.07654', '05:58:33.157329', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (11, 'new', 5, '05:58:50.131325', '05:59:01.418138', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (12, 'Test abc', 5, '01:34:16.268255', '01:35:24.358142', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (14, 'test application', 1, '02:34:14.392591', NULL, 1);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (1, 'Reject Application', 1, '06:38:30.330134', '02:42:58.888376', 1);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (15, 'Reject Application 1', 1, '02:43:34.822554', NULL, 1);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (16, 'test qeqe', 5, '02:44:20.033594', '02:44:28.237923', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (17, 'RejectApplication', 1, '02:45:50.533097', NULL, 1);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (18, 'RejectApplication', 1, '02:46:04.904539', NULL, 0);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (13, 'reject', 5, '01:35:39.603135', '04:16:16.819035', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (19, 'testing', 5, '09:12:10.032357', '10:25:27.789234', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (20, 'others', 5, '04:45:35.861893', '04:45:46.913119', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (21, 'test a', 5, '23:07:31.551645', '23:07:41.157011', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (22, 'Test', 5, '01:43:03.84918', '01:43:09.180514', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (23, 'testyghj', 5, '10:54:21.741668', '10:54:33.194753', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (24, 'sefsdfsdefsedfsdgsddsfseef wgsdggsv dsgfvx', 5, '06:25:30.295607', '06:30:46.059489', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (25, 'abcdefghigklmnopqrstuvwxyzabcdefghijklmnopqresr', 5, '06:30:14.405722', '06:30:50.077367', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (26, 'abcdefghigklmnopqrwxyzabcdefghijklmnopqresr', 5, '06:31:00.967438', '06:38:17.296038', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (28, 'asdsafdsadfdsfdsfsdfsfsdfsdfaaaaaaaaaaaaaa', 5, '06:38:46.484664', '06:39:30.813611', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (31, 'aaaaaaaaaaaaaaaaaaaaaaaaabbbbbaaaaa', 5, '06:40:07.873938', '06:40:26.739201', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (32, 'aaaaaaaaaaaaaaaaaaaaaaaaabbbbbaaa', 5, '06:40:21.117841', '06:40:58.618323', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (33, 'aaaaaaaaaaaaaaaaaaaaaaaaabbb', 5, '06:41:11.403702', '06:41:22.607299', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (30, 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa', 5, '06:39:58.241615', '06:41:26.713599', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (29, 'asdsafdsadfdaaaaaaaaaaaa', 5, '06:39:02.922549', '06:41:30.950193', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (27, 'asdsafdsadfdsfdsfsdfsfsdfsdf', 5, '06:38:09.092368', '06:41:34.974206', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (34, 'adasdfsfsdfsf', 5, '08:22:34.456866', '08:26:13.574626', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (36, 'asADADA', 5, '00:39:55.442028', '00:40:07.853715', 2);
INSERT INTO public.ohd_request_reject_reason (id, reason, status_id, created_at, modified_at, rejectionreasonfor) OVERRIDING SYSTEM VALUE VALUES (35, 'fhgh', 5, '08:26:26.102468', '00:40:11.420165', 2);


--
-- TOC entry 5008 (class 0 OID 41129)
-- Dependencies: 262
-- Data for Name: ohd_setting_type; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_setting_type (id, settingtype, createdat, display_name, priority) VALUES (9, 'Configuration', NULL, 'Configuration', 7);
INSERT INTO public.ohd_setting_type (id, settingtype, createdat, display_name, priority) VALUES (7, 'Operators', NULL, 'Operators', 6);
INSERT INTO public.ohd_setting_type (id, settingtype, createdat, display_name, priority) VALUES (1, 'TopUpHistory', '00:39:01.307088', 'Top up history', 5);
INSERT INTO public.ohd_setting_type (id, settingtype, createdat, display_name, priority) VALUES (2, 'RegistrationRequest', '00:39:17.437623', 'Registration request', 1);
INSERT INTO public.ohd_setting_type (id, settingtype, createdat, display_name, priority) VALUES (4, 'MeterRequest', '00:39:35.488176', 'Meter request', 2);
INSERT INTO public.ohd_setting_type (id, settingtype, createdat, display_name, priority) VALUES (5, 'ConsumerRequest', NULL, 'Consumers', 3);
INSERT INTO public.ohd_setting_type (id, settingtype, createdat, display_name, priority) VALUES (6, 'CompanyMaster', NULL, 'Company master', 4);


--
-- TOC entry 5010 (class 0 OID 41133)
-- Dependencies: 264
-- Data for Name: ohd_state; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_state (id, name, country_id, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'Eastern Cape', 1, 1, NULL, NULL);
INSERT INTO public.ohd_state (id, name, country_id, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (2, 'Free State', 1, 1, NULL, NULL);
INSERT INTO public.ohd_state (id, name, country_id, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (3, 'Gauteng', 1, 1, NULL, NULL);


--
-- TOC entry 5012 (class 0 OID 41137)
-- Dependencies: 266
-- Data for Name: ohd_state_master; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- TOC entry 5014 (class 0 OID 41149)
-- Dependencies: 270
-- Data for Name: ohd_transaction_type_master; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_transaction_type_master (id, transaction_type, status_id, created_at, last_modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'Credit', 1, NULL, NULL);
INSERT INTO public.ohd_transaction_type_master (id, transaction_type, status_id, created_at, last_modified_at) OVERRIDING SYSTEM VALUE VALUES (2, 'Debit', 1, NULL, NULL);


--
-- TOC entry 5016 (class 0 OID 41188)
-- Dependencies: 282
-- Data for Name: ohd_user_role_master; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.ohd_user_role_master (id, name, description, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (1, 'Customer', 'Customer', 1, '2023-12-29 17:32:05.52404', '2023-12-29 17:32:05.52404');
INSERT INTO public.ohd_user_role_master (id, name, description, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (2, 'Admin', 'Admin', 1, '2024-02-25 03:16:21.592284', '2024-02-25 03:16:21.592284');
INSERT INTO public.ohd_user_role_master (id, name, description, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (3, 'SuperAdmin', 'SuperAdmin', 1, '2024-02-25 03:16:50.856147', '2024-02-25 03:16:50.856147');
INSERT INTO public.ohd_user_role_master (id, name, description, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (4, 'Temporary', 'Temporary User as associate or tenant', 1, '2024-02-25 03:17:17.142705', '2024-02-25 03:17:17.142705');
INSERT INTO public.ohd_user_role_master (id, name, description, status_id, created_at, modified_at) OVERRIDING SYSTEM VALUE VALUES (5, 'Operator', 'Operator', 1, '2024-03-09 04:30:40.738666', '2024-03-09 04:30:40.738666');


--
-- TOC entry 5023 (class 0 OID 0)
-- Dependencies: 221
-- Name: ohd_company_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_company_id_seq', 1, true);


--
-- TOC entry 5024 (class 0 OID 0)
-- Dependencies: 223
-- Name: ohd_configuration_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_configuration_id_seq', 20, true);


--
-- TOC entry 5025 (class 0 OID 0)
-- Dependencies: 225
-- Name: ohd_country_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_country_id_seq', 1, true);


--
-- TOC entry 5026 (class 0 OID 0)
-- Dependencies: 227
-- Name: ohd_country_master_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_country_master_id_seq', 1, false);


--
-- TOC entry 5027 (class 0 OID 0)
-- Dependencies: 231
-- Name: ohd_document_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_document_type_id_seq', 1, false);


--
-- TOC entry 5028 (class 0 OID 0)
-- Dependencies: 233
-- Name: ohd_enum_communications_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_enum_communications_id_seq', 2, true);


--
-- TOC entry 5029 (class 0 OID 0)
-- Dependencies: 235
-- Name: ohd_enum_meter_type_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_enum_meter_type_id_seq', 6, true);


--
-- TOC entry 5030 (class 0 OID 0)
-- Dependencies: 237
-- Name: ohd_enum_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_enum_status_id_seq', 10, true);


--
-- TOC entry 5031 (class 0 OID 0)
-- Dependencies: 239
-- Name: ohd_enum_title_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_enum_title_id_seq', 1, false);


--
-- TOC entry 5032 (class 0 OID 0)
-- Dependencies: 241
-- Name: ohd_enum_user_relation_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_enum_user_relation_id_seq', 2, true);


--
-- TOC entry 5033 (class 0 OID 0)
-- Dependencies: 247
-- Name: ohd_mobile_country_code_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_mobile_country_code_id_seq', 2, true);


--
-- TOC entry 5034 (class 0 OID 0)
-- Dependencies: 249
-- Name: ohd_notification_type_master_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_notification_type_master_id_seq', 1, true);


--
-- TOC entry 5035 (class 0 OID 0)
-- Dependencies: 253
-- Name: ohd_payment_methods_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_payment_methods_id_seq', 5, true);


--
-- TOC entry 5036 (class 0 OID 0)
-- Dependencies: 255
-- Name: ohd_payment_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_payment_status_id_seq', 10, true);


--
-- TOC entry 5037 (class 0 OID 0)
-- Dependencies: 261
-- Name: ohd_request_reject_reason_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_request_reject_reason_id_seq', 36, true);


--
-- TOC entry 5038 (class 0 OID 0)
-- Dependencies: 263
-- Name: ohd_setting_type_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ohd_setting_type_Id_seq"', 9, true);


--
-- TOC entry 5039 (class 0 OID 0)
-- Dependencies: 265
-- Name: ohd_state_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_state_id_seq', 3, true);


--
-- TOC entry 5040 (class 0 OID 0)
-- Dependencies: 267
-- Name: ohd_state_master_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ohd_state_master_Id_seq"', 1, false);


--
-- TOC entry 5041 (class 0 OID 0)
-- Dependencies: 271
-- Name: ohd_transaction_type_master_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_transaction_type_master_id_seq', 2, true);


--
-- TOC entry 5042 (class 0 OID 0)
-- Dependencies: 283
-- Name: ohd_user_role_master_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.ohd_user_role_master_id_seq', 5, true);


-- Completed on 2024-06-24 08:16:37

--
-- PostgreSQL database dump complete
--

