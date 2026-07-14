
namespace Puyaq.CrossCutting.Settings
{
    public sealed class SocialAuthenticationSettings
    {
        public string GoogleClientId { get; set; } = string.Empty;

        public string FacebookAppId { get; set; } = string.Empty;

        public string FacebookAppSecret { get; set; } = string.Empty;

        public string AppleClientId { get; set; } = string.Empty;
    }
}
