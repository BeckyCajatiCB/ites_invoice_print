using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoicePrint.Models
{
    public class Invoice
    {
        public string BillToName { get; set; }

        public string BilltoAddress { get; set; }

        public DateTime InvoiceDue { get; set; }

        public decimal InvoiceAmount { get; set; }
    }
}
