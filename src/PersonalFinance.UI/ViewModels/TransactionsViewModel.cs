using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;
using PersonalFinance.UI.Util;
using PersonalFinance.UI.Views;

namespace PersonalFinance.UI.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    
    #region Constructors

    public TransactionsViewModel(IStatementService statementService)
    {
        _statementService = statementService;
        LoadTransactionsCommand = new AsyncRelayCommand(LoadTransactionsAsync);
        OpenTransactionCommand = new AsyncRelayCommand<Transaction?>(OpenTransactionAsync);
        InitializePayPeriods();
        _ = InitializeDateFilterAsync();
        _ = LoadTransactionsAsync();
    }

    #endregion

    #region Properties

    public bool IsPayPeriodEnabled => !UseDateRange;
    public bool IsDateRangeEnabled => UseDateRange;
    public IAsyncRelayCommand LoadTransactionsCommand { get; }
    public IAsyncRelayCommand<Transaction?> OpenTransactionCommand { get; }

    [ObservableProperty]
    private ObservableCollection<string> payPeriods = new();
    
    [ObservableProperty]
    private string? selectedPayPeriod;
    
    [ObservableProperty]
    private ObservableCollection<Transaction> _transactions = new();
    
    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _stopDate;
    
    [ObservableProperty]
    private bool useDateRange;

    #endregion

    #region Fields

    private readonly IStatementService _statementService;

    #endregion

    #region Tasks

    private async Task LoadTransactionsAsync()
    {
        try
        {
            Transaction[] transactions;

            if (UseDateRange)
            {
                var options = new UiOptions
                {
                    StartDate = StartDate,
                    StopDate = StopDate
                };
                
                transactions = await _statementService.GetAsync(options);
            }
            else
            {
                var payPeriod = SelectedPayPeriod ?? DateTime.Today.ToString("MMMMyyyy", CultureInfo.InvariantCulture);
                transactions = await new PayPeriodTransactionsProvider(_statementService).GetTransactionsAsync(payPeriod);
                if (transactions.Length > 0)
                {
                    StartDate = transactions.Min(t => t.Date);
                    StopDate  = transactions.Max(t => t.Date);
                }
            }

            Transactions.Clear();
            foreach (var t in transactions.OrderByDescending(t => t.Date))
            {
                Transactions.Add(t);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
    
    private async Task OpenTransactionAsync(Transaction? transaction)
    {
        if (transaction is null)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(TransactionDetailsPage), new Dictionary<string, object>
        {
            ["Transaction"] = transaction
        });
    }

    #endregion

    #region Helper Methods

    private async Task InitializeDateFilterAsync()
    {
        string payPeriod = $"{DateTime.Today:MMMM}{DateTime.Today.Year}";
        var dateRange = await new PayPeriodTransactionsProvider(_statementService).GetDateRangeAsync(payPeriod);
        StartDate = dateRange.StartDate ?? DateTime.MinValue;
        StopDate = dateRange.StopDate ?? DateTime.MaxValue;
    }

    private void InitializePayPeriods()
    {
        PayPeriods.Clear();

        var today = DateTime.Today;
        for (int i = 0; i < 12; i++)
        {
            var d = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
            PayPeriods.Add(d.ToString("MMMMyyyy", CultureInfo.InvariantCulture));
        }

        SelectedPayPeriod = PayPeriods.FirstOrDefault();
    }
    
    partial void OnStartDateChanged(DateTime value)
    {
        _ = LoadTransactionsAsync();
    }

    partial void OnStopDateChanged(DateTime value)
    {
        _ = LoadTransactionsAsync();
    }

    partial void OnSelectedPayPeriodChanged(string? value)
    {
        _ = LoadTransactionsAsync();
    }

    partial void OnUseDateRangeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsPayPeriodEnabled));
        OnPropertyChanged(nameof(IsDateRangeEnabled));
        _ = LoadTransactionsAsync();
    }
    
    #endregion
    
}
