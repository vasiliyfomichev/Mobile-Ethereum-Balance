using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VF.Ethereum.AccountBalance.Models;
using Xamarin.Forms;

namespace VF.Ethereum.AccountBalance
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<string> Transactions { get; }

        private bool _isRefreshing = false;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public MainPage()
        {
            InitializeComponent();
            myLocalImage.Source = ImageSource.FromUri(new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/0/05/Ethereum_logo_2014.svg/256px-Ethereum_logo_2014.svg.png"));

            BindingContext = this;
            RefreshData();
        }


        public ICommand RefreshCommand
        {
            get
            {
                return new Command( () =>
                {
                    IsRefreshing = true;
                    RefreshData();
                    IsRefreshing = false;
                });
            }
        }

        public void RefreshData()
        {
            var ethBalance = GetAccountBalance();
            var usdBalance = GetUsdAmountAsync(ethBalance).Result;
            lblEthBalance.Text = $"{Math.Round(ethBalance, 2)} ETH";
            lblUsdBalance.Text = $"${Math.Round(usdBalance, 2)}";
        }

        private decimal GetAccountBalance()
        {
            var web3 = new Web3("http://192.168.1.81:8545");
            var balance = web3.Eth.GetBalance.SendRequestAsync("0x4582a5aa65cbf54d3b9f268c62a7b698bc55a31d").Result;
            var eth = (new UnitConversion()).FromWei(balance.Value);
            return eth;
        }

        private async Task<decimal> GetUsdAmountAsync(decimal ethAmount)
        {
            try
            {
                var client = new HttpClient();
                var res = client.GetAsync("https://api.coinmarketcap.com/v1/ticker/ethereum/").Result;
                if (res.IsSuccessStatusCode)
                {
                    string result = await res.Content.ReadAsStringAsync();
                    var priceInfo = JsonConvert.DeserializeObject<EthPriceResult[]>(result);

                    var ethPrice = priceInfo.FirstOrDefault().price_usd;
                    return ethPrice * ethAmount;
                    //return 0;
                }
            }catch(Exception ex)
            {

            }
            
            return 0;
        }
    }
}
