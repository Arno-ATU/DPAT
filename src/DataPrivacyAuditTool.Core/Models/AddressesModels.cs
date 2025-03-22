using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DataPrivacyAuditTool.Core.Models
{
    public class AddressData
    {
        [JsonPropertyName("Autofill")]
        public List<AutofillEntry> Autofill { get; set; } = new List<AutofillEntry>();

        [JsonPropertyName("Autofill Profile")]
        public List<AutofillProfile> AutofillProfile { get; set; } = new List<AutofillProfile>();

        [JsonPropertyName("Contact Info")]
        public List<ContactInfo> ContactInfo { get; set; } = new List<ContactInfo>();
    }

    public class AutofillEntry
    {
        [JsonPropertyName("usage_timestamp")]
        public List<long> UsageTimestamp { get; set; } = new List<long>();

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class AutofillProfile
    {
        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("company_name")]
        public string CompanyName { get; set; }

        [JsonPropertyName("street_address")]
        public string StreetAddress { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("zipcode")]
        public string Zipcode { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("use_count")]
        public int UseCount { get; set; }

        [JsonPropertyName("use_date")]
        public long UseDate { get; set; }

        [JsonPropertyName("date_modified")]
        public long DateModified { get; set; }
    }

    public class ContactInfo
    {
        [JsonPropertyName("guid")]
        public string Guid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("job_title")]
        public string JobTitle { get; set; }

        [JsonPropertyName("phone_numbers")]
        public List<PhoneNumber> PhoneNumbers { get; set; } = new List<PhoneNumber>();

        [JsonPropertyName("street_address")]
        public string StreetAddress { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("creation_date")]
        public long CreationDate { get; set; }

        [JsonPropertyName("last_used_date")]
        public long LastUsedDate { get; set; }
    }

    public class PhoneNumber
    {
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
