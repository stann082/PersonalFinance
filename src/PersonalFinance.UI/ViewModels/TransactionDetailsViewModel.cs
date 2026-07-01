using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.UI.ViewModels;

public partial class TransactionDetailsViewModel : ObservableObject, IQueryAttributable
{

    #region Constructors

    public TransactionDetailsViewModel(IStatementService statementService)
    {
        _statementService = statementService;
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CloseCommand = new AsyncRelayCommand(CloseAsync);
    }

    #endregion

    #region Properties

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand CloseCommand { get; }

    public bool CanEditRecurring => Transaction?.IsRecurring.HasValue == true;

    [ObservableProperty]
    private Transaction? _transaction;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _category;

    [ObservableProperty]
    private bool _isRecurring;

    #endregion

    #region Fields

    private readonly IStatementService _statementService;

    #endregion

    #region Public Methods

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Transaction", out var value) && value is Transaction transaction)
        {
            Transaction = transaction;
            Description = transaction.Description;
            Category = transaction.Category;
            IsRecurring = transaction.IsRecurring ?? false;
            OnPropertyChanged(nameof(CanEditRecurring));
        }
        else
        {
            Debug.WriteLine("TransactionDetailsViewModel: no 'Transaction' in query");
        }
    }

    #endregion

    #region Tasks

    private async Task SaveAsync()
    {
        if (Transaction is null)
        {
            return;
        }

        try
        {
            Transaction.Description = Description;
            Transaction.Category = Category;
            if (Transaction.IsRecurring.HasValue)
            {
                Transaction.IsRecurring = IsRecurring;
            }

            await _statementService.UpdateAsync(Transaction);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            if (Shell.Current?.CurrentPage is { } page)
            {
                await page.DisplayAlertAsync("Save failed", ex.Message, "OK");
            }
        }
    }

    private async Task CloseAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    #endregion

}
