namespace FedexShipping.Data
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    public static class Constants
    {
        public const string SETTINGS_NAME = "merchantSettings";
        public const string SETTINGS_BUCKET = "shipping_utilities";
        public const string HEADER_VTEX_CREDENTIAL = "X-Vtex-Credential";
        public const string HEADER_VTEX_ACCOUNT = "X-Vtex-Account";
        public const string HEADER_VTEX_WORKSPACE = "X-Vtex-Workspace";
        public const string APPLICATION_JSON = "application/json";
        public const string AUTHORIZATION_HEADER_NAME = "Authorization";
        public const string RATES_BUCKET = "rates-response";
        public const string VTEX_ID_HEADER_NAME = "VtexIdclientAutCookie";
        public const string CARRIER = "FedEx";
        public const string PACKING_ACCESS_KEY = "AccessKey";

        public static ImmutableDictionary<string, string> POSTAL_CODE_REGEX= (new Dictionary<string, string> {
            {"USA", "\\d{5}$"},
            {"MEX", "\\d{5}$"},
            {"BRA", "([\\d]{5})\\-?([\\d]{3})$"},
            {"GBR", "([A-Za-z][A-Ha-hJ-Yj-y]?[0-9][A-Za-z0-9]? ?[0-9][A-Za-z]{2}|[Gg][Ii][Rr] ?0[Aa]{2})$"},
            {"CAN", "[A-z][0-9][A-z]\\ ?[0-9][A-z][0-9]$"},
            {"FRA", "\\d{5}$"},
            {"ITA", "\\d{5}$"},
            {"DEU", "\\d{5}$"}
        }).ToImmutableDictionary();
    }
}