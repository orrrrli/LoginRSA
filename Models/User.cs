using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace WebApplication1.Models
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }

        public string? Email { get; set; }

        public string? PasswordEncrypted { get; set; }
        public int d { get; set; }

    }
}
