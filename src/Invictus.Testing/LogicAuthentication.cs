﻿using System;
using System.Threading.Tasks;
using GuardNet;
using Microsoft.Azure.Management.Logic;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;

namespace Invictus.Testing 
{
    /// <summary>
    /// Authentication representation to authenticate with logic apps running on Azure.
    /// </summary>
    public class LogicAuthentication
    {
        private readonly Func<Task<LogicManagementClient>> _authenticateAsync;

        private LogicAuthentication(Func<Task<LogicManagementClient>> authenticateAsync)
        {
            Guard.NotNull(authenticateAsync, nameof(authenticateAsync));

            _authenticateAsync = authenticateAsync;
        }

        /// <summary>
        /// Uses the service principal to authenticate with Azure.
        /// </summary>
        /// <param name="tenantId">The ID where the resources are located on Azure.</param>
        /// <param name="subscriptionId">The ID that identifies the subscription on Azure.</param>
        /// <param name="clientId">The ID of the client or application that has access to the logic apps running on Azure.</param>
        /// <param name="clientSecret">The secret of the client or application that has access to the logic apps running on Azure.</param>
        public static LogicAuthentication UsingServicePrincipal(string tenantId, string subscriptionId, string clientId, string clientSecret)
        {
            Guard.NotNullOrWhitespace(tenantId, nameof(tenantId));
            Guard.NotNullOrWhitespace(subscriptionId, nameof(subscriptionId));
            Guard.NotNullOrWhitespace(clientId, nameof(clientId));
            Guard.NotNullOrWhitespace(clientSecret, nameof(clientSecret));

            return new LogicAuthentication(
                () => AuthenticateLogicAppsManagementAsync(subscriptionId, tenantId, clientId, clientSecret));
        }

        /// <summary>
        /// Authenticate with Azure with the previously chosen authentication mechanism.
        /// </summary>
        /// <returns>
        ///     The management client to interact with logic app resources running on Azure.
        /// </returns>
        public async Task<LogicManagementClient> AuthenticateAsync()
        {
            return await _authenticateAsync();
        }

        private static async Task<LogicManagementClient> AuthenticateLogicAppsManagementAsync(string subscriptionId, string tenantId, string clientId, string clientSecret)
        {
            string authority = $"https://login.windows.net/{tenantId}";
            
            var authContext = new AuthenticationContext(authority);
            var credential = new ClientCredential(clientId, clientSecret);

            AuthenticationResult token = await authContext.AcquireTokenAsync("https://management.azure.com/", credential);

            return new LogicManagementClient(new TokenCredentials(token.AccessToken))
            {
                SubscriptionId = subscriptionId
            };
        }
    }
}