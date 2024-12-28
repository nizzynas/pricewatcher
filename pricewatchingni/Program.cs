namespace pricewatchingni
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var monitor = new WalletMonitor();

            // Example wallet address (replace with the one you want to monitor)
            string walletAddress = "6G433PQZ8aJpxk2yZLNCbQovtNhwG2i4fiJDdWCupTov";

            // Alert if balance changes by 5% or more
            decimal changeThreshold = 5.0m;

            try
            {
                await monitor.StartMonitoring(walletAddress, changeThreshold);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

        }
    }
}
