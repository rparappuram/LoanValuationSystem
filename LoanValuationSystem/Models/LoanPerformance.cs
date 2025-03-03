using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoanValuationSystem.Models
{
    [Table("loan_performance")]
    public class LoanPerformance
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("Loan")]
        [Column("loan_id")]
        public long LoanId { get; set; }

        [Column("reporting_period")]
        public DateTime ReportingPeriod { get; set; }

        [Column("current_balance")]
        public decimal CurrentBalance { get; set; }

        [Column("delinquency_status")]
        public int? DelinquencyStatus { get; set; }

        public Loan? Loan { get; set; }
    }
}