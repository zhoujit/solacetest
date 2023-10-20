using SolaceSystems.Solclient.Messaging;
using System.Text;

namespace SolacePubTest;

internal class Program
{
    static void Main(string[] args)
    {
        ContextFactoryProperties cfp = new ContextFactoryProperties()
        {
            SolClientLogLevel = SolLogLevel.Warning
        };
        cfp.LogToConsoleError();
        ContextFactory.Instance.Init(cfp);

        SessionProperties sessionProps = new SessionProperties()
        {
            Host = "localhost", // tcps://hostname:55443
            VPNName = "JerryVPN",
            UserName = "test",
            Password = "test",
            ReconnectRetries = 3,
            // SSLTrustStoreDir = "certificateDir",  // If use SSL certificate, make sure the exists certificateDir folder, and exists certificates.
        };

        using (IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), null))
        using (ISession session = context.CreateSession(sessionProps, null, null))
        using (IMessage message = ContextFactory.Instance.CreateMessage())
        {
            session.Connect();
            message.Destination = ContextFactory.Instance.CreateQueue("TestQueue"); //.CreateTopic("tutorial/topic");
            message.BinaryAttachment = Encoding.ASCII.GetBytes("Send message in c#. Now:" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
            // message.XmlContent = Encoding.UTF8.GetBytes("<test></test>");

            ReturnCode returnCode = session.Send(message);
            if (returnCode == ReturnCode.SOLCLIENT_OK)
            {
                Console.WriteLine("Done.");
            }
            else
            {
                Console.WriteLine("Publishing failed, return code: {0}", returnCode);
            }
        }
    }
}