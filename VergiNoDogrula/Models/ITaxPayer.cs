using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VergiNoDogrula.Models
{
    public interface ITaxPayer
    {
        string Title { get; set; }
        string TaxNumber { get; set; }
    }
}
