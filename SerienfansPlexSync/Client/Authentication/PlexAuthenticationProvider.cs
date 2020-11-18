using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace SerienfansPlexSync.Client.Authentication
{
    public class PlexAuthenticationProvider: AuthenticationStateProvider
    {
        public const string PLEX_TOKEN = "Plex:AuthToken";
        
        private readonly ILocalStorageService _localStorageService;

        public PlexAuthenticationProvider(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }
        
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorageService.GetItemAsync<string>(PLEX_TOKEN);

            if (string.IsNullOrEmpty(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            
            var identity = new ClaimsIdentity(new []
            {
                new Claim(PLEX_TOKEN, token)
            }, "bearer");

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
    }
}