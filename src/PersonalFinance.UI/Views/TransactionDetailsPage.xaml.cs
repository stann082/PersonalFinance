using PersonalFinance.UI.ViewModels;

namespace PersonalFinance.UI.Views;

public partial class TransactionDetailsPage
{
    public TransactionDetailsPage(TransactionDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
