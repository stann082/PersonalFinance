using PersonalFinance.UI.Views;

namespace PersonalFinance.UI;

public partial class AppShell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(TransactionDetailsPage), typeof(TransactionDetailsPage));
    }
}
