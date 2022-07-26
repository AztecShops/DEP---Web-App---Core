using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models
{
    public class TransactionSearchViewModel
    {
        [Display(Name = "Transaction Number")]
        [Required]
        public string TransactionNumber { get; set; }
    }
}