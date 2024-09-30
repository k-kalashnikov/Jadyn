using Jadyn.Common.Attributes;
using System.ComponentModel;

namespace Jadyn.Common.Models
{
    public class Person : BaseModel
    {
        [ExcelPropety(Name = "FirstName", IsLink = false)]
        public string FirstName { get; set; }
        
        [ExcelPropety(Name = "LastName", IsLink = false)]
        public string LastName { get; set; }

        [ExcelPropety(Name = "SurName", IsLink = false)]
        public string SurName { get; set; }

        [ExcelPropety(Name = "PhoneMobile", IsLink = false)]
        public string PhoneNumber { get; set; }

        [ExcelPropety(Name = "CardCode", IsLink = false)]
        public int CardCode { get; set; }

        [ExcelPropety(Name = "Email", IsLink = false    )]
        public string Email { get; set; }

        [ExcelPropety(Name = "GenderId", IsLink = false)]
        public Gender Gender { get; set; }

        [ExcelPropety(Name = "Birthday", IsLink = false)]
        public DateOnly BirthDay { get; set; }

        [ExcelPropety(Name = "Bonus", IsLink = false)]
        public long Bonus { get; set; }

        [ExcelPropety(Name = "Pincode", IsLink = false)]
        public int PinCode { get; set; }

        [ExcelPropety(Name = "Turnover", IsLink = false)]
        public long TurnOver { get; set; }


        public Guid CityId { get; set; }

        [ExcelPropety(Name = "City", IsLink = true)]
        public City City { get; set; }
    }

    public enum Gender
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
    }
}
