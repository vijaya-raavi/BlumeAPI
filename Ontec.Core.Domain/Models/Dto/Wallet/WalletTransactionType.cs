using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Core.Domain.Models.Dto.Wallet
{
    public enum WalletTransactionType
    {
        AddBalance=1,
        DeductBalance=2,
        RefundBalance=3
    }
}
