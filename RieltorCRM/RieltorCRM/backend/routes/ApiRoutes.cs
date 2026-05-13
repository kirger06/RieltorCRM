namespace RieltorCRM.Routes
{
    public static class ApiRoutes
    {
        public const string Base = "api";

        public static class Auth
        {
            public const string Login = $"{Base}/auth/login";
            public const string Register = $"{Base}/auth/register";
            public const string Profile = $"{Base}/auth/profile";
        }

        public static class Admin
        {
            public const string Users = $"{Base}/admin/users";
            public const string CreateUser = $"{Base}/admin/users";
            public const string ToggleUserStatus = $"{Base}/admin/users/{{userId}}/toggle-status";
            public const string Dashboard = $"{Base}/admin/dashboard";
        }

        public static class Agent
        {
            public const string Properties = $"{Base}/agent/properties";
            public const string Deals = $"{Base}/agent/deals";
            public const string Showings = $"{Base}/agent/showings";
            public const string UpdateDealStatus = $"{Base}/agent/deals/{{dealId}}/status";
            public const string Report = $"{Base}/agent/report";
        }

        public static class Accountant
        {
            public const string Transactions = $"{Base}/accountant/transactions";
            public const string ConfirmTransaction = $"{Base}/accountant/transactions/{{transactionId}}/confirm";
            public const string Report = $"{Base}/accountant/report";
        }

        public static class OfficeManager
        {
            public const string Properties = $"{Base}/office/properties";
            public const string AssignAgent = $"{Base}/office/properties/{{propertyId}}/assign-agent";
            public const string Deals = $"{Base}/office/deals";
        }

        public static class Client
        {
            public const string Properties = $"{Base}/client/properties";
            public const string MyDeals = $"{Base}/client/my-deals";
        }

        public static class Seller
        {
            public const string MyProperties = $"{Base}/seller/my-properties";
            public const string AddProperty = $"{Base}/seller/properties";
            public const string MyDeals = $"{Base}/seller/my-deals";
        }
    }
}