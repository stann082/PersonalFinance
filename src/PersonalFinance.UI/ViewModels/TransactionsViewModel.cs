using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
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
        InitializeDateFilter();
        _ = LoadTransactionsAsync();
    }

    #endregion

    #region Properties

    public IAsyncRelayCommand LoadTransactionsCommand { get; }
    public IAsyncRelayCommand<Transaction?> OpenTransactionCommand { get; }

    [ObservableProperty]
    private ObservableCollection<Transaction> _transactions = new();
    
    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _stopDate;

    #endregion

    #region Fields

    private readonly IStatementService _statementService;

    #endregion

    #region Tasks

    private async Task LoadTransactionsAsync()
    {
        try
        {
            Transaction[] transactions = await GetTransactionsAsync();
            Transactions.Clear();
            foreach (var transaction in transactions.OrderByDescending(t => t.Date))
            {
                Transactions.Add(transaction);
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

    private void InitializeDateFilter()
    {
        DateTime today = DateTime.Today;
        StartDate = new DateTime(today.Year, today.Month, 1).AddDays(-2);
        StopDate = today;
    }

    private async Task<Transaction[]> GetTransactionsAsync()
    {
        UiOptions options = new UiOptions
        {
            StartDate = StartDate,
            StopDate = StopDate
        };
        return await _statementService.GetAsync(options);
    }

    partial void OnStartDateChanged(DateTime value)
    {
        StartDate = value;
        _ = LoadTransactionsAsync();
    }

    partial void OnStopDateChanged(DateTime value)
    {
        StopDate = value;
        _ = LoadTransactionsAsync();
    }
    
    #endregion
    
}
