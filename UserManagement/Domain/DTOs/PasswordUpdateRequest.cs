using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class PasswordUpdateRequest
    {

        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }

}
