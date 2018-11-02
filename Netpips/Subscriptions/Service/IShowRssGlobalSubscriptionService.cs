 namespace Netpips.Subscriptions.Service
{
    public interface IShowRssGlobalSubscriptionService
    {
        ShowRssGlobalSubscriptionService.SubscriptionResult SubscribeToShow(ShowRssAuthenticationContext context, int showId);

        ShowRssGlobalSubscriptionService.UnsubscriptionResult UnsubscribeToShow(ShowRssAuthenticationContext context, int showId);

        ShowRssAuthenticationContext Authenticate(out SubscriptionsSummary subscriptions);
    }
}