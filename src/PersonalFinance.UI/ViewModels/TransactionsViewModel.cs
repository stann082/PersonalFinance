using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;
using PersonalFinance.Domain.Util;

namespace PersonalFinance.UI.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{

    #region Constructors

    public TransactionsViewModel(IStatementService statementService)
    {
        _statementService = statementService;
        LoadTransactionsCommand = new AsyncRelayCommand(LoadTransactionsAsync);
        _ = LoadTransactionsAsync();
    }

    #endregion
    
    #region Properties
    
    public IAsyncRelayCommand LoadTransactionsCommand { get; }
    
    [ObservableProperty]
    private ObservableCollection<ITransaction> _transactions = new();
    
    #endregion
    
    #region Fields
    
    private readonly IStatementService _statementService;
    
    #endregion
    
    #region Tasks

    private async Task LoadTransactionsAsync()
    {
        try
        {
            ITransaction[] transactions = await GetTransactionsAsync();
            Transactions.Clear();
            foreach (var transaction in transactions.OrderByDescending(t => t.Date))
            {
                Transactions.Add(transaction);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<ITransaction[]> GetTransactionsAsync()
    {
        UiOptions options = new UiOptions();
        DateTime today = DateTime.Today;
        options.StartDate = new DateTime(today.Year, today.Month, 1).AddDays(-3);
        options.StopDate = today;
        return await _statementService.GetAsync(options);
    }

    #endregion
    
}
