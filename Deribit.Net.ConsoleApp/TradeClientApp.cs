using QuickFix;
using QuickFix.Fields;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;


namespace Deribit.Net.ConsoleApp
{
    public class DerbitFixClient : MessageCracker, IApplication
    {
        Session _session = null;

        // This variable is a kludge for developer test purposes.  Don't do this on a production application.
        public IInitiator MyInitiator = null;

        #region IApplication interface overrides

        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
        }

        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine("Logon - " + sessionID.ToString());
        }
        public void OnLogout(SessionID sessionID)
        {
            Console.WriteLine("Logout - " + sessionID.ToString());
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            if (message.Header.GetField(Tags.MsgType) == MsgType.LOGON)
            {
                //message.SetField(new QuickFix.Fields.Username("QTaUk5S9uMrt"));
                //var rawdata = $"{(long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds}.{GetBase64(GetRandomBytes())}";
                //message.SetField(new QuickFix.Fields.RawData(rawdata));
                //var pass = GetBase64(GetSha256Hash(rawdata + "YHL42CUPBYLRAGUM54YRX4X3OQJI3BKT"));
                message.SetField(new QuickFix.Fields.Password("Sandbox1234"));
                message.SetField(new QuickFix.Fields.ResetSeqNumFlag(true));
            }
        }
        /*
         The RawData(96) tag contains a timestamp and a nonce, separated by an ASCII period (.).
         The timestamp needs to be a strictly increasing integer. We recommend using a timestamp in milliseconds. 
         The nonce is composed base64-encoded randomly chosen bytes. For safety reasons, it is important that the nonce is sufficiently long and sourced from a cryptographically secure random number generator.
         We recommend at least 32 bytes, but the nonce can be up to 512 bytes.
         The Password(553) tag should contain a base64 encoded SHA256 hash of the concatenation of the RawData(96) 
         content and the access secret: base64(sha256(RawData ++ access_secret)).*/
        public void ToAdmin(Message message, SessionID sessionID)
        {
            //var t = message.
            if (message.Header.GetField(Tags.MsgType) == MsgType.LOGON)
            {
                //message.SetField(new QuickFix.Fields.Username("QTaUk5S9uMrt"));
                //var rawdata = $"{(long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds}.{GetBase64(GetRandomBytes())}";
                //message.SetField(new QuickFix.Fields.RawData(rawdata));
                //var pass = GetBase64(GetSha256Hash(rawdata + "YHL42CUPBYLRAGUM54YRX4X3OQJI3BKT"));
                message.SetField(new QuickFix.Fields.Password("Sandbox1234"));
                message.SetField(new QuickFix.Fields.ResetSeqNumFlag(true));

            }
        }

        private byte[] GetSha256Hash(string input)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
            SHA256Managed sha256 = new SHA256Managed();
            byte[] hash = sha256.ComputeHash(bytes);
            return hash;
        }

        private string GetBase64(byte[] input)
        {
           // var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
           return System.Convert.ToBase64String(input);
        }
        private byte[] GetRandomBytes()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                // Buffer storage.
                byte[] data = new byte[256];
                rng.GetBytes(data);
                return data;
            }

        }
        public void FromApp(Message message, SessionID sessionID)
        {
            Console.WriteLine("IN:  " + message.ToString());
            try
            {
                Crack(message, sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(QuickFix.Fields.Tags.PossDupFlag))
                {
                    possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(
                        message.Header.GetField(QuickFix.Fields.Tags.PossDupFlag)); /// FIXME
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }

            Console.WriteLine();
            Console.WriteLine("OUT: " + message.ToString());
        }
        #endregion


        #region MessageCracker handlers
        public void OnMessage(QuickFix.FIX44.ExecutionReport m, SessionID s)
        {
            Console.WriteLine("Received execution report");
        }
        public void OnMessage(QuickFix.FIX44.SecurityListRequest m, SessionID s)
        {            
            Console.WriteLine("Received securities list report");
        }
        public void OnMessage(QuickFix.FIX44.OrderCancelReject m, SessionID s)
        {
            Console.WriteLine("Received order cancel reject");
        }
        #endregion

        public void QuerySecurityListRequest()
        {
            Console.WriteLine("\nSecurityListRequest");

            QuickFix.FIX44.SecurityListRequest m = new QuickFix.FIX44.SecurityListRequest() { SecurityReqID = new SecurityReqID("1"), SecurityListRequestType = new SecurityListRequestType(4) };

            if (m != null)
                SendMessage(m);
        }
        public void Run()
        {
            //QuerySecurityListRequest();
            while (true)
            {
                try
                {
                    char action = QueryAction();
                    if (action == '1')
                        QueryEnterOrder();
                    else if (action == '2')
                        QueryCancelOrder();
                    else if (action == '3')
                        QueryReplaceOrder();
                    else if (action == '4')
                        QueryMarketDataRequest();
                    else if (action == 'g')
                    {
                        if (this.MyInitiator.IsStopped)
                        {
                            Console.WriteLine("Restarting initiator...");
                            this.MyInitiator.Start();
                        }
                        else
                            Console.WriteLine("Already started.");
                    }
                    else if (action == 'x')
                    {
                        if (this.MyInitiator.IsStopped)
                            Console.WriteLine("Already stopped.");
                        else
                        {
                            Console.WriteLine("Stopping initiator...");
                            this.MyInitiator.Stop();
                        }
                    }
                    else if (action == 'q' || action == 'Q')
                        break;
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Message Not Sent: " + e.Message);
                    Console.WriteLine("StackTrace: " + e.StackTrace);
                }
            }
            Console.WriteLine("Program shutdown.");
        }

        private void SendMessage(Message m)
        {
            if (_session != null)
            {
                Console.WriteLine($"Sending message # "+ m.GetField(Tags.MsgSeqNum));
                _session.Send(m);

            }
            else
            {
                // This probably won't ever happen.
                Console.WriteLine("Can't send message: session not created.");
            }
        }

        private char QueryAction()
        {
            // Commands 'g' and 'x' are intentionally hidden.
            Console.Write("\n"
                + "1) Enter Order\n"
                + "2) Cancel Order\n"
                + "3) Replace Order\n"
                + "4) Market data test\n"
                + "Q) Quit\n"
                + "Action: "
            );

            HashSet<string> validActions = new HashSet<string>("1,2,3,4,q,Q,g,x".Split(','));

            string cmd = Console.ReadLine().Trim();
            if (cmd.Length != 1 || validActions.Contains(cmd) == false)
                throw new System.Exception("Invalid action");

            return cmd.ToCharArray()[0];
        }

        private void QueryEnterOrder()
        {
            QuerySecurityListRequest();
            Console.WriteLine("\nNewOrderSingle");

            QuickFix.FIX44.NewOrderSingle m = QueryNewOrderSingle44();

            if (m != null && QueryConfirm("Send order"))
            {
                m.Header.GetField(Tags.BeginString);

                SendMessage(m);
            }
        }

        private void QueryCancelOrder()
        {
            Console.WriteLine("\nOrderCancelRequest");

            QuickFix.FIX44.OrderCancelRequest m = QueryOrderCancelRequest44();

            if (m != null && QueryConfirm("Cancel order"))
                SendMessage(m);
        }

        private void QueryReplaceOrder()
        {
            Console.WriteLine("\nCancelReplaceRequest");

            QuickFix.FIX44.OrderCancelReplaceRequest m = QueryCancelReplaceRequest44();

            if (m != null && QueryConfirm("Send replace"))
                SendMessage(m);
        }

        private void QueryMarketDataRequest()
        {
            Console.WriteLine("\nMarketDataRequest");

            QuickFix.FIX44.MarketDataRequest m = QueryMarketDataRequest44();

            if (m != null && QueryConfirm("Send market data request"))
                SendMessage(m);
        }
     
        private bool QueryConfirm(string query)
        {
            Console.WriteLine();
            Console.WriteLine(query + "?: ");
            string line = Console.ReadLine().Trim();
            return (line[0].Equals('y') || line[0].Equals('Y'));
        }

        #region Message creation functions
        private QuickFix.FIX44.NewOrderSingle QueryNewOrderSingle44()
        {
            QuickFix.Fields.OrdType ordType = null;

            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
                QueryClOrdID(),
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now),
                ordType = QueryOrdType());

            newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(QueryOrderQty());
            newOrderSingle.Set(QueryTimeInForce());
            if (ordType.getValue() == OrdType.LIMIT || ordType.getValue() == OrdType.STOP_LIMIT)
                newOrderSingle.Set(QueryPrice());
            if (ordType.getValue() == OrdType.STOP || ordType.getValue() == OrdType.STOP_LIMIT)
                newOrderSingle.Set(QueryStopPx());

            return newOrderSingle;
        }

        private QuickFix.FIX44.OrderCancelRequest QueryOrderCancelRequest44()
        {
            QuickFix.FIX44.OrderCancelRequest orderCancelRequest = new QuickFix.FIX44.OrderCancelRequest(
                QueryOrigClOrdID(),
                QueryClOrdID(),
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now));

            orderCancelRequest.Set(QueryOrderQty());
            return orderCancelRequest;
        }

        private QuickFix.FIX44.OrderCancelReplaceRequest QueryCancelReplaceRequest44()
        {
            QuickFix.FIX44.OrderCancelReplaceRequest ocrr = new QuickFix.FIX44.OrderCancelReplaceRequest(
                QueryOrigClOrdID(),
                QueryClOrdID(),
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now),
                QueryOrdType());

            ocrr.Set(new HandlInst('1'));
            if (QueryConfirm("New price"))
                ocrr.Set(QueryPrice());
            if (QueryConfirm("New quantity"))
                ocrr.Set(QueryOrderQty());

            return ocrr;
        }

        private QuickFix.FIX44.MarketDataRequest QueryMarketDataRequest44()
        {

            MDReqID mdReqID = new MDReqID();
            SubscriptionRequestType subType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT);
            MarketDepth marketDepth = new MarketDepth(0);

            QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup marketDataEntryGroup = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
            marketDataEntryGroup.Set(new MDEntryType(MDEntryType.BID));

            QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup symbolGroup = new QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup();
            symbolGroup.Set(new Symbol("BTC-PERPETUAL"));

            QuickFix.FIX44.MarketDataRequest message = new QuickFix.FIX44.MarketDataRequest(mdReqID, subType, marketDepth);
            message.AddGroup(marketDataEntryGroup);
            message.AddGroup(symbolGroup);

            return message;
        }
      
        #endregion

        #region field query private methods
        private ClOrdID QueryClOrdID()
        {
            Console.WriteLine();
            Console.Write("ClOrdID? ");
            return new ClOrdID(Console.ReadLine().Trim());
        }

        private OrigClOrdID QueryOrigClOrdID()
        {
            Console.WriteLine();
            Console.Write("OrigClOrdID? ");
            return new OrigClOrdID(Console.ReadLine().Trim());
        }

        private Symbol QuerySymbol()
        {
            Console.WriteLine();
            Console.Write("Symbol? ");
            return new Symbol(Console.ReadLine().Trim());
        }

        private Side QuerySide()
        {
            Console.WriteLine();
            Console.WriteLine("1) Buy");
            Console.WriteLine("2) Sell");
            Console.WriteLine("3) Sell Short");
            Console.WriteLine("4) Sell Short Exempt");
            Console.WriteLine("5) Cross");
            Console.WriteLine("6) Cross Short");
            Console.WriteLine("7) Cross Short Exempt");
            Console.Write("Side? ");
            string s = Console.ReadLine().Trim();

            char c = ' ';
            switch (s)
            {
                case "1": c = Side.BUY; break;
                case "2": c = Side.SELL; break;
                case "3": c = Side.SELL_SHORT; break;
                case "4": c = Side.SELL_SHORT_EXEMPT; break;
                case "5": c = Side.CROSS; break;
                case "6": c = Side.CROSS_SHORT; break;
                case "7": c = 'A'; break;
                default: throw new Exception("unsupported input");
            }
            return new Side(c);
        }

        private OrdType QueryOrdType()
        {
            Console.WriteLine();
            Console.WriteLine("1) Market");
            Console.WriteLine("2) Limit");
            Console.WriteLine("3) Stop");
            Console.WriteLine("4) Stop Limit");
            Console.Write("OrdType? ");
            string s = Console.ReadLine().Trim();

            char c = ' ';
            switch (s)
            {
                case "1": c = OrdType.MARKET; break;
                case "2": c = OrdType.LIMIT; break;
                case "3": c = OrdType.STOP; break;
                case "4": c = OrdType.STOP_LIMIT; break;
                default: throw new Exception("unsupported input");
            }
            return new OrdType(c);
        }

        private OrderQty QueryOrderQty()
        {
            Console.WriteLine();
            Console.Write("OrderQty? ");
            return new OrderQty(Convert.ToDecimal(Console.ReadLine().Trim()));
        }

        private TimeInForce QueryTimeInForce()
        {
            Console.WriteLine();
            Console.WriteLine("1) Day");
            Console.WriteLine("2) IOC");
            Console.WriteLine("3) OPG");
            Console.WriteLine("4) GTC");
            Console.WriteLine("5) GTX");
            Console.Write("TimeInForce? ");
            string s = Console.ReadLine().Trim();

            char c = ' ';
            switch (s)
            {
                case "1": c = TimeInForce.DAY; break;
                case "2": c = TimeInForce.IMMEDIATE_OR_CANCEL; break;
                case "3": c = TimeInForce.AT_THE_OPENING; break;
                case "4": c = TimeInForce.GOOD_TILL_CANCEL; break;
                case "5": c = TimeInForce.GOOD_TILL_CROSSING; break;
                default: throw new Exception("unsupported input");
            }
            return new TimeInForce(c);
        }

        private Price QueryPrice()
        {
            Console.WriteLine();
            Console.Write("Price? ");
            return new Price(Convert.ToDecimal(Console.ReadLine().Trim()));
        }

        private StopPx QueryStopPx()
        {
            Console.WriteLine();
            Console.Write("StopPx? ");
            return new StopPx(Convert.ToDecimal(Console.ReadLine().Trim()));
        }

        #endregion
    }
}
