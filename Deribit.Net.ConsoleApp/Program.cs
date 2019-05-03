using QuickFix;
using QuickFix.Transport;

namespace Deribit.Net.ConsoleApp
{
   
    class Program
    {
        public class Myinit : SocketInitiator
        {
            public Myinit(IApplication application, IMessageStoreFactory storeFactory, SessionSettings settings, ILogFactory logFactory, IMessageFactory messageFactory) : base(application, storeFactory, settings, logFactory, messageFactory)
            {
            }

            protected override void DoConnect(SessionID sessionID, Dictionary settings)
            {
                base.DoConnect(sessionID, settings);
            }

            protected override void OnConfigure(SessionSettings settings)
            {
                
                base.OnConfigure(settings);
            }

          

            protected override bool OnPoll(double timeout)
            {
                return base.OnPoll(timeout);
            }

            protected override void OnRemove(SessionID sessionID)
            {
                base.OnRemove(sessionID);
            }

            protected override void OnStart()
            {
                base.OnStart();
            }

            protected override void OnStop()
            {
                base.OnStop();
            }
        }
        static void Main(string[] args)
        {
            SessionSettings settings = new SessionSettings("config.txt");

            TradeClientApp application = new TradeClientApp();
            
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            //ThreadedSocketAcceptor acceptor = new ThreadedSocketAcceptor(
            //    application,
            //    storeFactory,
            //    settings,
            //    logFactory);
            QuickFix.Transport.SocketInitiator initiator = new QuickFix.Transport.SocketInitiator(application, storeFactory, settings, logFactory);

            // this is a developer-test kludge.  do not emulate.
            application.MyInitiator = initiator;
            
            initiator.Start();
            application.Run();
            initiator.Stop();

        }
    }
}



