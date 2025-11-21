using System.Diagnostics;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.UI.Views;

public partial class TransactionDetailsPage : IQueryAttributable
{
    #region Constructors

    public TransactionDetailsPage()
    {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public Transaction? Transaction
    {
        get => BindingContext as Transaction;
        set => BindingContext = value;
    }

    #endregion

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Transaction", out var value))
        {
            BindingContext = value;
            Debug.WriteLine($"TransactionDetailsPage: got transaction = {value}");
        }
        else
        {
            Debug.WriteLine("TransactionDetailsPage: no 'Transaction' in query");
        }
    }

    
    #region Tasks

    private async void OnCloseClicked(object sender, EventArgs e)
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
