namespace Jadyn.Common.Models
{
    public class Person : BaseModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SurName { get; set; }
        public string PhoneNumber { get; set; }
        public int CardCode { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public DateOnly BirthDay { get; set; }
        public long Bonus { get; set; }
        public int PinCode { get; set; }
        public long TurnOver { get; set; }


        public Guid CityId { get; set; }
        public City City { get; set; }
    }

    public enum Gender
    {
        Unknown = 0,
        Male = 1,
        Female = 2,
    }
}
