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
        static Dictionary defaults = new Dictionary("DEFAULT", new System.Collections.Generic.Dictionary<string, string>()
        {
            {"FileStorePath","store" },
            {"FileLogPath","log" },
            {"ReconnectInterval","30" },
            {"TargetCompID","DERIBITSERVER" },
            {"HeartBtInt","30" },
            {"SocketConnectPort","9881" },
            {"SocketConnectHost","test.deribit.com" },
            {"DataDictionary","FIX44.xml" },
            {"StartTime","00:00:00" },
            {"EndTime","23:59:59" }    ,
                      {"ConnectionType","initiator" }

        }) ;
        static Dictionary session = new Dictionary("SESSION", new System.Collections.Generic.Dictionary<string, string>()
        {
            {"BeginString","FIX.4.4" },
          //  {"SenderCompID",Guid.NewGuid().ToString() },
  
        });
        static void Main(string[] args)
        {
            //SessionSettings settings = new SessionSettings("config.txt");
            SessionSettings settings = new SessionSettings();
            settings.Set(defaults);
            settings.Set(new SessionID("FIX.4.4",Guid.NewGuid().ToString(),"DERIBITSERVER"), new Dictionary("SESSION", new System.Collections.Generic.Dictionary<string, string>()
        {
            {"BeginString","FIX.4.4" },
          //  {"SenderCompID",Guid.NewGuid().ToString() },
          //  {"ConnectionType","initiator" }
        }));
           // settings.Set(session);
           
            DerbitFixClient application = new DerbitFixClient();
            
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
     
            QuickFix.Transport.SocketInitiator initiator = new QuickFix.Transport.SocketInitiator(application, storeFactory, settings, logFactory);

            // this is a developer-test kludge.  do not emulate.
            application.MyInitiator = initiator;
            
            initiator.Start();
            //application.QuerySecurityListRequest();
            application.Run();
            initiator.Stop();

        }
    }
}



