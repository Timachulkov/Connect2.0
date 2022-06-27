using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Connect2._0
{
    internal class Clientnet
    {
        string ip;
        int port;
        bool exit = false;
        int errorCount;
        string newstring = "\n";
        Thread threadtcp = null;
        String responseData = String.Empty;
        bool connectionState = false;
        bool active = false;
        int arrayIter = 0;
        int[] sendNumbers;
        int[] saveNumbers;
        internal bool arrayEmpty = true;
        byte[] dataget = new byte[256];
        List<byte> datarecive = new List<byte>();
        TcpClient tcpClient;
        NetworkStream netStream;

        internal Clientnet(string newip,int newport)
        {
            ip = newip;
            port = newport;

        }
        internal void Fillnum(int[] toFill)
        {
            sendNumbers = toFill;
            
            saveNumbers = new int[sendNumbers.Length];
            arrayEmpty = false;
        }
        internal int[] Takearray()
        {
            return saveNumbers;
        }
        internal void Start()
        {
            Connect();
            threadtcp = new Thread(tcp);
            threadtcp.Start();
            active = true;
        }
        internal void Stop()
        {
            Disconnect();
            active = false;
        }
        void Connect()
        {
            try
            {
                tcpClient = new TcpClient(ip, port);
                netStream = tcpClient.GetStream();
                connectionState = true;
            }
            catch
            {
                connectionState = false;
            }
        }
        void Disconnect()
        {
            netStream.Close();
            connectionState = false;
        }
        void tcp()
        {
            while (active)
            {
                if (connectionState)
                {
                    if (sendNumbers.Length > 0)
                    {
                        try
                        {
                            errorCount = 0;
                                netStream.ReadTimeout = 20000;
                                byte[] data = System.Text.Encoding.ASCII.GetBytes(Convert.ToString(sendNumbers[0]) + newstring);
                                netStream.Write(data, 0, data.Length);
                                Console.WriteLine("Sent: {0}", Convert.ToString(sendNumbers[0]));
                                
                                do
                                {

                                    int tmpByte;
                                    exit = false;
                                    while(exit == false)
                                    {
                                        tmpByte = netStream.ReadByte();
                                        if (tmpByte != -1)
                                        {

                                            datarecive.Add(Convert.ToByte(tmpByte));
                                        }
                                        else
                                        {
                                            Thread.Sleep(1000);
                                            errorCount++;
                                        }


                                        if (datarecive.Count() > 5)
                                        {
                                            if (datarecive[datarecive.Count()-1] == 10 && datarecive[datarecive.Count() - 2] == 13 && datarecive[datarecive.Count() - 3] == 10 && datarecive[datarecive.Count() - 4] == 13)
                                            {
                                                exit = true;
                                            }
                                        }

                                        if (errorCount > 150)
                                        {
                                            throw new InvalidOperationException("Infinit loop");
                                        }
                                        Thread.Sleep(10);
                                    }
                                    while (!exit);

                                    for (int i = 0; i < datarecive.Count(); i++)
                                    {
                                        dataget[i] = datarecive[i];
                                    }
                                    Encoding encoding = Encoding.GetEncoding(20866);
                                    responseData = encoding.GetString(dataget, 0, dataget.Length);
                                    responseData = Regex.Match(responseData, @"\d+").Value;
                                    Thread.Sleep(100);

                                }
                                while (responseData.Length < 7);
                                Console.WriteLine("Received: {0}", responseData);
                                saveNumbers[arrayIter] = Convert.ToInt32(responseData);
                                arrayIter++;
                                sendNumbers = sendNumbers.Skip(1).ToArray();

                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine(ex);
                            Connect();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Connect();

                        }
                        finally
                        {
                            datarecive.Clear();
                        }
                        arrayEmpty = false;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                        arrayEmpty = true;
                    }
                }
                else
                {
                    Connect();
                }
                Thread.Sleep(1);
            }
            Disconnect();
        }
    }
}
