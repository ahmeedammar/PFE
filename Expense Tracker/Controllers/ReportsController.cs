﻿
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Models;
using System.Globalization;
using Expense_Tracker.Filters;

namespace Expense_Tracker.Controllers
{
    [AuthorizeAdmin]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            //Last 7 Days
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions1 = await _context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= StartDate && y.Date <= EndDate)
                .ToListAsync();

            //Total Income
            int TotalIncome = SelectedTransactions1
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            CultureInfo culture = CultureInfo.CreateSpecificCulture("ar-TN");
            ViewBag.TotalIncome = TotalIncome.ToString("C", culture);

            //Total Expense
            int TotalExpense = SelectedTransactions1
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C", culture);

            //Balance
            int Balance = TotalIncome - TotalExpense;
            //CultureInfo culture = CultureInfo.CreateSpecificCulture("ar-TN");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = Balance.ToString("C", culture);

            //Doughnut Chart - Expense By Category
            ViewBag.DoughnutChartData = SelectedTransactions1
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C", culture),
                    ////give it a try to show "DT"
                    ////CultureInfo culture = new CultureInfo("fr-TN");
                    ////string formattedAmount = string.Format(culture, "{0:C}", amount);

                })
                .OrderByDescending(l => l.amount)
                .ToList();

            //Spline Chart - Income vs Expense

            //Income
            List<SplineChartData> IncomeSummary = SelectedTransactions1
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            //Expense
            List<SplineChartData> ExpenseSummary = SelectedTransactions1
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineChartData()
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

            //Combine Income & Expense
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();

            ViewBag.SplineChartData = from day in Last7Days
                                      join income in IncomeSummary on day equals income.day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in ExpenseSummary on day equals expense.day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          day = day,
                                          income = income == null ? 0 : income.income,
                                          expense = expense == null ? 0 : expense.expense,
                                      };
            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();


            return View();
        }
    }

    public class SplineChartData1
    {
        public string day;
        public int income;
        public int expense;

    }
    
    
}

