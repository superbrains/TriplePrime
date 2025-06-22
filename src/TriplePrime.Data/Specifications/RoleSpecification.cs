using Microsoft.AspNetCore.Identity;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Specifications
{
    public class RoleSpecification : BaseSpecification<IdentityRole>
    {
        public RoleSpecification(string roleName)
            : base(r => r.Name == roleName)
        {
        }
    }
} 