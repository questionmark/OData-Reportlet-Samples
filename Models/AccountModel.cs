using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QM.Reporting.ODataDashboard.Web.Models
{
    [Serializable]
    public class AccountModel
    {
        [DisplayName("Area")]
        [Required]
        public string TenantId { get; set; }

        [DisplayName("Username")]
        [Required]
        public string Username { get; set; }

        [DisplayName("Password")]
        [Required]
        public string Password { get; set; }

        [DisplayName("Remember Me")]
        public bool RememberMe { get; set; }
    }
}