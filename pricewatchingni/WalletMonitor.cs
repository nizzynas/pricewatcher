using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pricewatchingni
{
    public class WalletMonitor
    {
        private readonly SolanaRpcClient _solanaClient;
        private readonly Dictionary<string, Dictionary<string, decimal>> _lastKnownBalances;

        public WalletMonitor()
        {
            _solanaClient = new SolanaRpcClient();
            _lastKnownBalances = new Dictionary<string, Dictionary<string, decimal>>();
        }

        public async Task StartMonitoring(string walletAddress, decimal changeThreshold)
        {
            Console.WriteLine($"Starting to monitor wallet: {walletAddress}");
            Console.WriteLine($"Alert threshold set to: {changeThreshold}%");

            while (true)
            {
                try
                {
                    var currentBalances = await _solanaClient.GetTokenAccounts(walletAddress);
                    Console.WriteLine("\nCurrent Token Balances:");
                    Console.WriteLine("----------------------");

                    foreach (var token in currentBalances)
                    {
                        Console.WriteLine($"Token: {token.Name} ({token.Symbol})");
                        Console.WriteLine($"Address: {token.Mint}");
                        Console.WriteLine($"Balance: {token.Amount}");
                        Console.WriteLine("----------------------");

                        // Check for significant changes
                        if (!_lastKnownBalances.ContainsKey(walletAddress))
                        {
                            _lastKnownBalances[walletAddress] = new Dictionary<string, decimal>();
                        }

                        if (_lastKnownBalances[walletAddress].TryGetValue(token.Mint, out var lastBalance))
                        {
                            if (lastBalance != 0)
                            {
                                var percentageChange = ((token.Amount - lastBalance) / lastBalance) * 100;

                                if (Math.Abs(percentageChange) >= changeThreshold)
                                {
                                    Console.WriteLine($"=== ALERT ===");
                                    Console.WriteLine($"Token {token.Name} ({token.Symbol}) changed by {percentageChange:F2}%");
                                    Console.WriteLine($"Old balance: {lastBalance}");
                                    Console.WriteLine($"New balance: {token.Amount}");
                                    Console.WriteLine($"============");
                                }
                            }
                        }

                        _lastKnownBalances[walletAddress][token.Mint] = token.Amount;
                    }

                    Console.WriteLine("\nWaiting 60 seconds before next check...");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error monitoring wallet: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }
    }
}
