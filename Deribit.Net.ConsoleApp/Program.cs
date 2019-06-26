using QuickFix;
using QuickFix.Transport;
using System;

namespace Deribit.Net.ConsoleApp
{
    /*
     # default settings for sessions
 [DEFAULT]
 =store
 =log
 =30
 =
 =30
 =
 =
 =
 =
 =

 [SESSION]
 =FIX.4.4
 =deribitique424242
 =initiator*/
    class Program
    {       
        static void Main(string[] args)
        {
            SessionSettings settings = new SessionSettings("config.txt");
            DerbitFixClient application = new DerbitFixClient();
            
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
     
            QuickFix.Transport.SocketInitiator initiator = new QuickFix.Transport.SocketInitiator(application, storeFactory, settings, logFactory);

            // this is a developer-test kludge.  do not emulate.
            application.MyInitiator = initiator;
            
            initiator.Start();
            application.QuerySecurityListRequest();
            application.Run();
            initiator.Stop();

        }
    }
}



