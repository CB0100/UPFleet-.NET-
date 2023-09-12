using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UPFleet.Models
{
    public partial class PeachtreeExportedArchive
    {
        public string Owner { get; set; }

        public string SalesOrder { get; set; }

        public DateTime Date { get; set; }

        public string Closed { get; set; }

        [Column("\"Customer PO\"")]
        public string CustomerPo { get; set; }

        [Column("\"Sales Representative ID\"")]
        public string SalesRepresentativeId { get; set; }

        public string Account { get; set; }

        [Column("\"Sales Tax ID\"")]
        public string SalesTaxId { get; set; }

        public double NumDistributions { get; set; }

        public double Transfer { get; set; }

        public string Qnty { get; set; }

        public string Invitem { get; set; }

        public string Description { get; set; }

        [Column("GLaccount")]
        public string Glaccount { get; set; }

        public double TaxType { get; set; }

        public double Amount { get; set; }

        public string SalesTaxAuth { get; set; }
        [Column("ID")]
        public int Id { get; set; }
    }
}