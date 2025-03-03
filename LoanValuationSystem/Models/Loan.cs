using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanValuationSystem.Models
{
    [Table("loans")]
    public class Loan
    {
        [Key]
        [Column("loan_id")]
        public long LoanId { get; set; }

        [Column("origination_date")]
        public DateTime OriginationDate { get; set; }

        [Column("maturity_date")]
        public DateTime MaturityDate { get; set; }

        [Column("loan_term")]
        public int LoanTerm { get; set; }

        [Column("original_loan_amount")]
        public decimal OriginalLoanAmount { get; set; }

        [Column("interest_rate")]
        public decimal InterestRate { get; set; }

        [Column("amortization_type")]
        public string? AmortizationType { get; set; }
    }
}