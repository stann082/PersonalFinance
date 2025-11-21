using System.Diagnostics;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.UI.Views;

public partial class TransactionDetailsPage
{
    #region Constructors

    public TransactionDetailsPage()
    {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public ITransaction? Transaction
    {
        get => BindingContext as ITransaction;
        set => BindingContext = value;
    }

    #endregion

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
