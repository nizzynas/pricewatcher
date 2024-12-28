using RestSharp;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;


namespace pricewatchingni
{
    public class SolanaRpcClient
    {
        private readonly RestClient _client;
        private const string RPC_ENDPOINT = "https://api.mainnet-beta.solana.com";

        public SolanaRpcClient()
        {
            _client = new RestClient(RPC_ENDPOINT);
        }

        public async Task<TokenMetadata> GetTokenMetadata(string mintAddress)
        {
            var request = new RestRequest("", Method.Post);

            var requestBody = new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "getAccountInfo",
                @params = new object[]
                {
                mintAddress,
                new { encoding = "jsonParsed" }
                }
            };

            request.AddJsonBody(requestBody);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful && response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<TokenMetadataResponse>(response.Content);
                return new TokenMetadata
                {
                    Mint = mintAddress,
                    Decimals = result?.Result?.Value?.Data?.Parsed?.Info?.Decimals ?? 0,
                    Symbol = await GetTokenSymbol(mintAddress),
                    Name = await GetTokenName(mintAddress)
                };
            }

            return new TokenMetadata { Mint = mintAddress };
        }

        private async Task<string> GetTokenSymbol(string mintAddress)
        {
            // You might want to implement caching here
            try
            {
                // Using Jupiter API to get token info
                var jupiterClient = new RestClient("https://token.jup.ag/all");
                var response = await jupiterClient.ExecuteGetAsync(new RestRequest());

                if (response.IsSuccessful && response.Content != null)
                {
                    var tokens = JsonConvert.DeserializeObject<List<JupiterToken>>(response.Content);
                    return tokens?.FirstOrDefault(t => t.Address == mintAddress)?.Symbol ?? "Unknown";
                }
            }
            catch
            {
                // Fallback to mint address if token info cannot be retrieved
            }
            return mintAddress;
        }

        private async Task<string> GetTokenName(string mintAddress)
        {
            try
            {
                var jupiterClient = new RestClient("https://token.jup.ag/all");
                var response = await jupiterClient.ExecuteGetAsync(new RestRequest());

                if (response.IsSuccessful && response.Content != null)
                {
                    var tokens = JsonConvert.DeserializeObject<List<JupiterToken>>(response.Content);
                    return tokens?.FirstOrDefault(t => t.Address == mintAddress)?.Name ?? "Unknown Token";
                }
            }
            catch
            {
                // Fallback to mint address if token info cannot be retrieved
            }
            return "Unknown Token";
        }

        public async Task<List<TokenBalance>> GetTokenAccounts(string walletAddress)
        {
            // Previous GetTokenAccounts implementation remains the same
            var request = new RestRequest("", Method.Post);

            var requestBody = new
            {
                jsonrpc = "2.0",
                id = 1,
                method = "getTokenAccountsByOwner",
                @params = new object[]
                {
                walletAddress,
                new { programId = "TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA" },
                new { encoding = "jsonParsed" }
                }
            };

            request.AddJsonBody(requestBody);

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful && response.Content != null)
            {
                var result = JsonConvert.DeserializeObject<TokenAccountsResponse>(response.Content);
                var tokens = await ParseTokenAccounts(result?.Result?.Value);
                return tokens;
            }

            throw new Exception($"Failed to get token accounts: {response.Content}");
        }

        private async Task<List<TokenBalance>> ParseTokenAccounts(List<TokenAccount> accounts)
        {
            var tokens = new List<TokenBalance>();

            if (accounts == null) return tokens;

            foreach (var account in accounts)
            {
                if (account?.Account?.Data?.Parsed?.Info != null)
                {
                    var info = account.Account.Data.Parsed.Info;
                    var metadata = await GetTokenMetadata(info.Mint);

                    tokens.Add(new TokenBalance
                    {
                        Mint = info.Mint,
                        Symbol = metadata.Symbol,
                        Name = metadata.Name,
                        Amount = decimal.Parse(info.TokenAmount.Amount) / (decimal)Math.Pow(10, info.TokenAmount.Decimals),
                        Decimals = info.TokenAmount.Decimals
                    });
                }
            }

            return tokens;
        }
    }
}
