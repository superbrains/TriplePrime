using Microsoft.AspNetCore.Identity;
using System;

namespace TriplePrime.Data.Entities
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 