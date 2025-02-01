namespace PersonalFinance.Domain.Util;

public static class ConsoleHelper
{
    
    #region Public Methods

    public static bool ConfirmAction(string message)
    {
        Console.Write($"{message} (Y/N): ");
        string? input = Console.ReadLine();
        while (input == null || (!input.Equals("Y", StringComparison.OrdinalIgnoreCase) && !input.Equals("N", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Invalid input. Please enter 'Y' or 'N'.");
            input = Console.ReadLine();
        }
        
        return input.Equals("Y", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
    
}