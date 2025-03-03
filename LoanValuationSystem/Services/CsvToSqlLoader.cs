using Microsoft.Data.SqlClient;
using System.Globalization;
using LoanValuationSystem.Models;

class CsvToSqlLoader
{
    private static string? connectionString;
    private static string csvFilePath = "Data/Sample.csv";
    public CsvToSqlLoader()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        connectionString = configuration.GetConnectionString("AzureSQL");
    }

    public void LoadData()
    {
        try
        {
            Console.WriteLine("Starting CSV to SQL import...");

            using (StreamReader reader = new StreamReader(csvFilePath))
            {
                List<Loan> loans = new List<Loan>();
                List<LoanPerformance> performanceRecords = new List<LoanPerformance>();

                string? line;
                int lineNumber = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    string[] fields = line.Split('|');

                    try
                    {
                        // Loan details
                        long loanId = long.Parse(fields[1]);
                        DateTime originationDate = ParseDate(fields[13]);
                        DateTime maturityDate = ParseDate(fields[18]);
                        int loanTerm = int.Parse(fields[12]);
                        decimal originalLoanAmount = decimal.Parse(fields[9]);
                        decimal interestRate = decimal.Parse(fields[7]);
                        string amortizationType = fields[34].Trim();

                        // Performance details
                        DateTime reportingPeriod = ParseDate(fields[2]);
                        decimal currentBalance = decimal.Parse(fields[11]) == 0 && !string.IsNullOrWhiteSpace(fields[5]) ? originalLoanAmount : decimal.Parse(fields[11]);
                        int delinquencyStatus = string.IsNullOrWhiteSpace(fields[39]) ? 0 : int.Parse(fields[39]);

                        // Add loan (if not already added)
                        if (!loans.Any(l => l.LoanId == loanId))
                        {
                            loans.Add(new Loan
                            {
                                LoanId = loanId,
                                OriginationDate = originationDate,
                                MaturityDate = maturityDate,
                                LoanTerm = loanTerm,
                                OriginalLoanAmount = originalLoanAmount,
                                InterestRate = interestRate,
                                AmortizationType = amortizationType
                            });
                        }

                        // Add performance record
                        performanceRecords.Add(new LoanPerformance
                        {
                            LoanId = loanId,
                            ReportingPeriod = reportingPeriod,
                            CurrentBalance = currentBalance,
                            DelinquencyStatus = delinquencyStatus,
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing line {lineNumber}: {ex.Message}");
                    }
                }
                // Insert data into database
                InsertLoansIntoDatabase(loans);
                InsertLoanPerformanceIntoDatabase(performanceRecords);
            }

            Console.WriteLine("CSV to SQL import completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
        }
    }

    private static DateTime ParseDate(string mmYYYY)
    {
        return DateTime.ParseExact(mmYYYY, "MMyyyy", CultureInfo.InvariantCulture);
    }

    private static void InsertLoansIntoDatabase(List<Loan> loans)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                foreach (var loan in loans)
                {
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO loans (loan_id, origination_date, maturity_date, loan_term, 
                                           original_loan_amount, interest_rate, amortization_type)
                        VALUES (@LoanId, @OriginationDate, @MaturityDate, @LoanTerm, 
                                @OriginalLoanAmount, @InterestRate, @AmortizationType)", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@LoanId", loan.LoanId);
                        cmd.Parameters.AddWithValue("@OriginationDate", loan.OriginationDate);
                        cmd.Parameters.AddWithValue("@MaturityDate", loan.MaturityDate);
                        cmd.Parameters.AddWithValue("@LoanTerm", loan.LoanTerm);
                        cmd.Parameters.AddWithValue("@OriginalLoanAmount", loan.OriginalLoanAmount);
                        cmd.Parameters.AddWithValue("@InterestRate", loan.InterestRate);
                        cmd.Parameters.AddWithValue("@AmortizationType", loan.AmortizationType);

                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }
    }

    private static void InsertLoanPerformanceIntoDatabase(List<LoanPerformance> performanceRecords)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                foreach (var record in performanceRecords)
                {
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO loan_performance (loan_id, reporting_period, current_balance, 
                                                      delinquency_status)
                        VALUES (@LoanId, @ReportingPeriod, @CurrentBalance, @DelinquencyStatus)", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@LoanId", record.LoanId);
                        cmd.Parameters.AddWithValue("@ReportingPeriod", record.ReportingPeriod);
                        cmd.Parameters.AddWithValue("@CurrentBalance", record.CurrentBalance);
                        cmd.Parameters.AddWithValue("@DelinquencyStatus", record.DelinquencyStatus);

                        cmd.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }
    }
}