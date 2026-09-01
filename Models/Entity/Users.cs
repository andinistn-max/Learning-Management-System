using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Learning_Management_System.Models.Entity
{
    [Table("Users")]
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdUser { get; set; }

        [Required]
        public int IdRole { get; set; }

        [Required]
        [StringLength(100)]
        public string NamaLengkap { get; set; }

        [Required]
        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(500)]
        public string PasswordHash { get; set; }

        [StringLength(200)]
        public string GoogleId { get; set; }

        [StringLength(500)]
        public string FotoProfile { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [StringLength(255)]
        public string ResetPasswordToken { get; set; }

        public DateTime? ResetPasswordExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("IdRole")]
        public virtual Roles Role { get; set; }

        public virtual Profiles Profile { get; set; }
    }
}
