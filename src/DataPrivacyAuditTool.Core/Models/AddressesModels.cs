namespace DataPrivacyAuditTool.Core.Models
{
    public class AddressData
    {
        public List<AutofillEntry> Autofill { get; set; } = new List<AutofillEntry>();
        public List<AutofillProfile> AutofillProfile { get; set; } = new List<AutofillProfile>();
        public List<ContactInfo> ContactInfo { get; set; } = new List<ContactInfo>();
    }

    public class AutofillEntry
    {
        public List<long> UsageTimestamp { get; set; } = new List<long>();
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class AutofillProfile
    {
        public string Guid { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string CountryCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int UseCount { get; set; }
        public long UseDate { get; set; }
        public long DateModified { get; set; }
    }

    public class ContactInfo
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public long CreationDate { get; set; }
        public long LastUsedDate { get; set; }
    }

    public class PhoneNumber
    {
        public string Number { get; set; }
        public string Type { get; set; }
    }
}
