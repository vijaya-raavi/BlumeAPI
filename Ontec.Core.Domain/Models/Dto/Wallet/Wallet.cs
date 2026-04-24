using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Core.Domain.Models.Dto.Wallet
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double Balance { get; set; }
    }
}
