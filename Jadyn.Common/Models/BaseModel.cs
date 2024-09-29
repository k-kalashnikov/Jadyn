using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Jadyn.Common.Models
{
    public class BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        //public BaseModelStatus Status { get; set; } = BaseModelStatus.Active;
    }

    public enum BaseModelStatus
    {
        Active = 0,
        Blocked = 1,
        Deleted = 2,
    }
}
