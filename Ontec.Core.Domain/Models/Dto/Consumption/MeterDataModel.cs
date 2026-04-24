namespace Ontec.Core.Domain.Models.Dto.Consumption
{
    using System;
    using System.Collections.Generic;

    public class Meter
    {
        public string Id { get; set; }
        public bool IdExternal { get; set; }
        public string SerialNum { get; set; }
        public string MeterNum { get; set; }
        public string RecordStatus { get; set; }
        public string CustomVarchar1 { get; set; }
        public string CustomVarchar2 { get; set; }
        public MeterType Type { get; set; }
        public Model Model { get; set; }
        public double PowerLimit { get; set; }
    }
    public class Model
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ServiceResource { get; set; }
    }
    public class MeterType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RecordStatus { get; set; }
    }

    public class UsagePoint
    {
        public string Id { get; set; }
        public bool IdExternal { get; set; }
        public string Name { get; set; }
        public DateTime InstallationDate { get; set; }
        public DateTime ActivationDate { get; set; }
        public string RecordStatus { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; }
        public bool IdExternal { get; set; }
        public string firstnames { get; set; }
        public string Surname { get; set; }
        public string email1 { get; set; }
        public string phone1 { get; set; }
        public string customNumeric1 { get; set; }
        public string RecordStatus { get; set; }
        public string CustomVarchar1 { get; set; }
        public string CustomerReference { get; set; }
        public Location Location { get; set; }
        public string AgreementRef { get; set; }


    }
    public class Location
    {
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string addressLine3 { get; set; }
    }
    public class CustomerAccount
    {
        public string Id { get; set; }
        public bool IdExternal { get; set; }
        public string CustomerId { get; set; }
        public string AccountName { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal CreditLimit { get; set; }
        public string RecordStatus { get; set; }
        public string NotificationEmail { get; set; }
        public string NotificationPhone { get; set; }
        public bool NotifyLowBalance { get; set; }
    }

    public class CustomerAgreement
    {
        public string Id { get; set; }
        public bool IdExternal { get; set; }
        public string AgreementRef { get; set; }
        public string RecordStatus { get; set; }
        public bool Billable { get; set; }
    }

    public class UnitsAccount
    {
        public string Id { get; set; }
        public bool IdExternal { get; set; }
        public string AccountName { get; set; }
        public decimal AccountBalance { get; set; }
        public string RecordStatus { get; set; }
        public bool NotifyLowBalance { get; set; }
    }

    public class MeterDataModel
    {
        public List<Data> Data { get; set; }
    }

    public class Data
    {
        public Meter Meter { get; set; }
        public UsagePoint UsagePoint { get; set; }
        public Customer Customer { get; set; }
        public CustomerAccount CustomerAccount { get; set; }
        public CustomerAgreement CustomerAgreement { get; set; }
        public UnitsAccount UnitsAccount { get; set; }


    }


}
