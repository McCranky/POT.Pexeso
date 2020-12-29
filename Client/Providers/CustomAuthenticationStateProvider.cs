using Microsoft.AspNetCore.Components.Authorization;
using POT.Pexeso.Shared;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POT.Pexeso.Client.Providers
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;

        public CustomAuthenticationStateProvider(HttpClient http)
        {
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = await _http.GetFromJsonAsync<UserDisplayInfo>("auth/get");
            if (user != null && !string.IsNullOrWhiteSpace(user.Nickname)) {
                // vytvoriť claim
                var claimName = new Claim(ClaimTypes.Name, user.Nickname);
                // vytvorit claimIdentity
                var claimIdentity = new ClaimsIdentity(new[] { claimName }, "serverAuth");
                // create claimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimIdentity);
                // login user
                return new AuthenticationState(claimsPrincipal);
            }
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void Notify()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
