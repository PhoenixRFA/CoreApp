namespace MVCApp.Models
{
    public class PushSubscription
    {
        public PushSubscription() { }

        public string Endpoint { get; set; }
        public PushSubscriptionKeys Keys { get; set; }
    }

    public class PushSubscriptionKeys
    {
        public PushSubscriptionKeys () { }

        public byte[] Auth { get; set; }
        public byte[] P256DH { get; set; }
    }
}
