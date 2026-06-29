using PersonalFinance.UI.ViewModels;

namespace PersonalFinance.UI.Views;

public partial class TransactionsPage
{
    public TransactionsPage(TransactionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
