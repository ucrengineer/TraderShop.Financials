﻿namespace TraderShop.Financials.TdAmeritrade.Abstractions.Services
{
    public interface ITdAmeritradeAuthService
    {
        Task<string> GetBearerToken();
    }
}
