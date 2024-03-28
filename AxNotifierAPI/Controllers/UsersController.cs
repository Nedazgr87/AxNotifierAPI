using System.Data.SqlClient;
using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using CryptoExchange.Net.Authentication;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AxNotifierAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public BinanceClient Client;

        [HttpPost("Login")]
        public ApiResult Login(LoginModel model)
        {
            try
            {
                using var db = new SqlConnection(Statics.ConnectionString);
                var query = "select * from AxUsers where UserName = @username and Active = 1";
                var user = db.QueryFirstOrDefault<User>(query, new { username = model.UserName });
                return user != null && model.Pwd == "123@qwE" ? new ApiResult("Ok") : new ApiResult("Invalid UserName or Password", false);
            }
            catch (Exception e)
            {
                return new ApiResult(e.ToString(), false);
            }
        }

        [HttpPost("SetToken")]
        public ApiResult<bool> SetToken(TokenModel tokenModel)
        {
            try
            {
                using var db = new SqlConnection(Statics.ConnectionString);
                var query = "delete from UsersToken where device = @device";
                db.Execute(query, new { device = tokenModel.Device });
                query = "insert into UsersToken (device, token) VALUES (@device,@token)";
                db.Execute(query, new { device = tokenModel.Device, token = tokenModel.Token });
                return new ApiResult<bool>("Ok", true);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(e.ToString(), false);
            }
        }

        [HttpGet("UsersList")]
        public Task<ApiResult<List<User>>> UsersList()
        {
            using var db = new SqlConnection(Statics.ConnectionString);
            var query = "select username from AxUsers where Active = 1";
            var users = new List<User>();
            users.Add(new User { Active = true, UserName = "Select a User" });
            users.AddRange(db.Query<string>(query).Select(x => new User { Active = true, UserName = x }).ToList());

            return Task.FromResult(new ApiResult<List<User>>("Ok", users));
        }

        [HttpGet("{username}")]
        public async Task<ApiResult<PositionModel>> Get(string username)
        {
            await using var db = new SqlConnection(Statics.ConnectionString);
            var query = "select * from AxUsers where UserName = @username";
            var u = db.QueryFirst<User>(query, new { username });

            if (u == null)
                return new ApiResult<PositionModel>("Invalid User", null!, false, TypeEnum.Error);

            Client = new BinanceClient();
            Client.SetApiCredentials(new ApiCredentials(u.Api, u.Secret));

            var result = new PositionModel();
            var res = await Client.UsdFuturesApi.Account.GetAccountInfoAsync();
            if (!res.Success && res.Error != null)
                return new ApiResult<PositionModel>(res.Error.Message, null!, false, TypeEnum.Error);

            var prices0 = await Client.UsdFuturesApi.ExchangeData.GetPricesAsync();
            var prices = prices0.Data.ToList();
            var list = res.Data.Positions.Where(x => x.EntryPrice != 0).ToList();

            foreach (var item in list)
            {
                item.MaintMargin = prices.FirstOrDefault(x => x.Symbol == item.Symbol)!.Price;
            }
            result.List = list;
            result.TotalMargin = res.Data.TotalMarginBalance;
            result.TotalWallet = res.Data.TotalWalletBalance;
            result.TotalUnPnl = res.Data.TotalUnrealizedProfit;
            result.AllInTrade = res.Data.TotalPositionInitialMargin;

            var apiResult = new ApiResult<PositionModel>("Ok", result);
            return apiResult;
        }

        [HttpPost("Market")]
        public async Task<ApiResult<bool>> Market(Order order)
        {
            try
            {
                await using var db = new SqlConnection(Statics.ConnectionString);
                var query = "select * from AxUsers where UserName = @username";
                var u = db.QueryFirst<User>(query, new { username = order.UserName });

                if (u == null)
                    return new ApiResult<bool>("Invalid User", false, false, TypeEnum.Error);

                Client = new BinanceClient();
                Client.SetApiCredentials(new ApiCredentials(u.Api, u.Secret));

                var position = await Client.UsdFuturesApi.Account.GetPositionInformationAsync(order.SymbolTitle);

                var quantity = position.Data.FirstOrDefault()?.Quantity;
                quantity = quantity < 0 ? quantity * -1 : quantity;
                var endOrder = await Client.UsdFuturesApi.Trading.PlaceOrderAsync(order.SymbolTitle, order.OrderSide, FuturesOrderType.Market, quantity, null, null, null, null, null, null, null, null, null, true);
                if (!endOrder.Success)
                    return new ApiResult<bool>(endOrder.Error?.Message, false);

                return new ApiResult<bool>("Ok", true);
            }
            catch (Exception e)
            {
                return new ApiResult<bool>(e.ToString(), false);
            }
        }

    }

    public class LoginModel
    {
        public string UserName { get; set; }
        public string Pwd { get; set; }
    }

    [Table("AxUsers")]
    public class User
    {
        public string UserName { get; set; }
        public string Api { get; set; }
        public string Secret { get; set; }
        public bool Active { get; set; }
    }

    public class Order
    {
        public string SymbolTitle { get; set; }
        public OrderSide OrderSide { get; set; }
        public decimal Quantity { get; set; }
        public string UserName { get; set; }
        public decimal? Price { get; set; }
    }

    public class PositionModel
    {
        public List<BinancePositionInfoUsdt> List { get; set; } = new();
        public decimal TotalWallet { get; set; }
        public decimal TotalUnPnl { get; set; }
        public decimal TotalMargin { get; set; }
        public decimal AllInTrade { get; set; }
    }

    public class TokenModel
    {
        public string Token { get; set; }
        public string Device { get; set; }
    }

}
