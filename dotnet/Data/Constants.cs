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

        public static readonly ImmutableDictionary<string, string> POSTAL_CODE_REGEX= (new Dictionary<string, string> {
            {"ARE", ""},
            {"ARG", "\\d{4}|[A-Za-z]\\d{4}[a-zA-Z]{3}$"},
            {"AUS", "\\d{4}$"},
            {"AUT", "\\d{4}$"},
            {"BEL", "\\d{4}$"},
            {"BGR", "\\d{4}$"},
            {"BOL", "\\d{4}$"},
            {"BRA", "([\\d]{5})\\-?([\\d]{3})$"},
            {"CAN", "[A-z][0-9][A-z]\\ ?[0-9][A-z][0-9]$"},
            {"CHL", "\\d{7}\\s\\(\\d{3}-\\d{4}\\)$"},
            {"COL", "\\d{6}$"},
            {"CRI", "\\d{4,5}$"},
            {"CZE", "\\d{5}\\s\\(\\d{3}\\s\\d{2}\\)$"},
            {"DEU", "\\d{5}$"},
            {"DNK", "\\d{4}$"},
            {"ECU", "\\d{6}$"},
            {"ESP", "\\d{5}$"},
            {"FIN", "\\d{5}$"},
            {"FRA", "\\d{5}$"},
            {"GBR", "([A-Za-z][A-Ha-hJ-Yj-y]?[0-9][A-Za-z0-9]? ?[0-9][A-Za-z]{2}|[Gg][Ii][Rr] ?0[Aa]{2})$"},
            {"GTM", "\\d{5}$"},
            {"HRV", "\\d{5}$"},
            {"IDN", "\\d{5}$"},
            {"IND", "\\d{6}$"},
            {"IRL", ""},
            {"ITA", "\\d{5}$"},
            {"KOR", "\\d{6}\\s\\(\\d{3}-\\d{3}\\)$"},
            {"MEX", "\\d{5}$"},
            {"NIC", "\\d{5}$"},
            {"NLD", "\\d{4}\\s{0,1}[A-Za-z]{2}$"},
            {"NZL", "\\d{4}$"},
            {"PER", "\\d{5}$"},
            {"POL", "\\d{2}[- ]{0,1}\\d{3}$"},
            {"PRT", "\\d{4}[- ]{0,1}\\d{3}$"},
            {"PRY", "\\d{4}$"},
            {"ROU", "\\d{6}$"},
            {"RUS", "\\d{6}$"},
            {"SAU", "\\d{5}(-{1}\\d{4})?$"},
            {"SGP", "\\d{4}$"},
            {"SLV", "1101$"},
            {"SMR", "4789\\d$"},
            {"SRB", "\\d{5}$"},
            {"SVK", "\\d{5}\\s\\(\\d{3}\\s\\d{2}\\)$"},
            {"SWE", "\\d{3}\\s*\\d{2}$"},
            {"UKR", "\\d{5}$"},
            {"URY", "\\d{5}$"},
            {"USA", "\\d{5}$"},
            {"VEN", "\\d{4}(\\s[a-zA-Z]{1})?$"},
            {"ZAF", "\\d{4}$"}
        }).ToImmutableDictionary();
    }
}