using SolaceSystems.Solclient.Messaging;
using System.Text;

namespace SolaceSubTest;

internal class Program
{
    private static EventWaitHandle MQEventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

    private static IFlow flow = null;

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
            Host = "localhost",  // tcps://hostname:55443
            VPNName = "JerryVPN",
            UserName = "test",
            Password = "test",
            ReconnectRetries = 3,
            // SSLTrustStoreDir = "certificateDir",  // If use SSL certificate, make sure the exists certificateDir folder, and exists certificates.
        };

        //using (IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), null))
        //using (ISession session = context.CreateSession(sessionProps, HandleMessage, null))
        //{
        //    ReturnCode returnCode = session.Connect();
        //    if (returnCode == ReturnCode.SOLCLIENT_OK)
        //    {
        //        Console.WriteLine("Session successfully connected.");

        //        session.Subscribe(ContextFactory.Instance.CreateTopic("MTM/*"), true);

        //        Console.WriteLine("Waiting for a message to be published...");
        //        MQEventWaitHandle.WaitOne();
        //    }
        //}

        //using (IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), null))
        //using (ISession session = context.CreateSession(sessionProps, HandleMessage, null))
        //{
        //    ReturnCode returnCode = session.Connect();
        //    if (returnCode == ReturnCode.SOLCLIENT_OK)
        //    {
        //        Console.WriteLine("Session successfully connected.");

        //        session.Subscribe(ContextFactory.Instance.CreateTopic("TestTopic"), true);

        //        Console.WriteLine("Waiting for a message to be published...");
        //        MQEventWaitHandle.WaitOne();
        //    }
        //}

        using (IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), null))
        using (ISession session = context.CreateSession(sessionProps, HandleMessage, null))
        {
            session.Connect();
            flow = session.CreateFlow(new FlowProperties()
            {
                AckMode = MessageAckMode.ClientAck,
                // Selector = "ReleaseYear > 2022"
            },
            ContextFactory.Instance.CreateQueue("TestQueue"), null, HandleMessage, FlowHandleMessage);
            flow.Start();

            Thread.Sleep(1000 * 1000);
        }

    }

    private static void HandleMessage(object source, MessageEventArgs args)
    {
        // Received a message
        using (IMessage message = args.Message)
        {
            // Expecting the message content as a binary attachment
            Console.WriteLine("Message content: {0}", Encoding.ASCII.GetString(message.BinaryAttachment ?? message.XmlContent));
            
            flow.Ack(message.ADMessageId);
            MQEventWaitHandle.Set();
        }
    }

    private static void FlowHandleMessage(object source, FlowEventArgs args)
    {
        Console.WriteLine(string.Format("Flow: {0}", args.Info));
    }

}