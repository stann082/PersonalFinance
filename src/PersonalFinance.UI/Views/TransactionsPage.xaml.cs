using PersonalFinance.UI.ViewModels;

namespace PersonalFinance.UI.Views;

public partial class TransactionsPage
{
    private readonly TransactionsViewModel _viewModel;
    private bool _navigatedAway;

    public TransactionsPage(TransactionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _navigatedAway = true;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // reload only when returning from another page (e.g. after an edit),
        // not on the initial navigation
        if (_navigatedAway)
        {
            _navigatedAway = false;
            _viewModel.LoadTransactionsCommand.Execute(null);
        }
    }
}
