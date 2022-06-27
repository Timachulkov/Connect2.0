using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Connect2._0
{
    class Program
    {
        static List<int> array = new List<int>();

        static void Main(string[] args)
        {
            int copyLenth = 0;
            int arraySize = 1;
            int workThreads = 1;
            Console.WriteLine("Запишите колличество потоков");
            workThreads =Convert.ToInt32(Console.ReadLine()); 
            List<Clientnet> clientList = new List<Clientnet>();
            double median = 0;
            string host = "88.212.241.115";
            double unixTimestamp = 1380619079;
            int port = UnixTimeStampToYear(unixTimestamp);
            for (int i = 0; i < workThreads; i++)
            {
                clientList.Add(new Clientnet(host, port));
            }
            int[] arr = Enumerable.Range(1, arraySize).ToArray();
            if (arr.Length % workThreads > 0)
            {
                int num = 0;
                int sizeOfnewArray = arr.Length / workThreads;
                int[] buffer;
                for (int i = 0; i < arr.Length; i += sizeOfnewArray)
                {
                    if(num != workThreads-1)
                    {
                        buffer = new int[sizeOfnewArray];
                        Array.Copy(arr, i, buffer, 0, sizeOfnewArray);
                        clientList[num].Fillnum(buffer);
                        num++;
                    }
                    else
                    {
                        buffer = new int[arr.Length - i];
                        Array.Copy(arr, i, buffer, 0, arr.Length - i);
                        clientList[num].Fillnum(buffer);
                        break;
                    }
                }
            }
            else
            {   
                int num = 0;
                int sizeOfnewArray = arr.Length / workThreads;
                int[] buffer;
                for (int i = 0; i < arr.Length; i += sizeOfnewArray)
                {
                    buffer = new int[sizeOfnewArray];
                    Array.Copy(arr, i, buffer, 0, sizeOfnewArray);
                    clientList[num].Fillnum(buffer);
                    num++;
                }
            }
            for (int i = 0; i < workThreads; i++)
            {
                clientList[i].Start();
            };
            int[] anserArr = new int[arr.Length];
            List<bool> check = new List<bool>(new bool[clientList.Count()]);
            do
            {
                for (int i = 0; i < clientList.Count(); i++)
                {
                    check[i] = clientList[i].arrayEmpty;
                }
                Thread.Sleep(1);
            }
            while (!check.All(c => c == true));
            Thread.Sleep(1000);

            for (int i = 0; i < workThreads; i++)
            {
                clientList[i].Takearray().CopyTo(anserArr, copyLenth);
                copyLenth = clientList[i].Takearray().Length + copyLenth;
            }
            median = CalculateMedian(anserArr);
            sendtcp(host, port, Convert.ToString(median));
            Console.ReadKey();

        }
        public static void sendtcp(string ip, int port, string send)
        {
            String responseData = String.Empty;
            List<byte> datarecive = new List<byte>();
            byte[] dataget = new byte[256];
            TcpClient tcpClient = new TcpClient(ip, port);
            string response = "Check " + send + "\n";
            byte[] data = System.Text.Encoding.ASCII.GetBytes(response);
            NetworkStream netStream = tcpClient.GetStream();
            netStream.Write(data, 0, data.Length);
            Console.WriteLine("Sent: {0}", response);
            int[] read = new int[2];
            read[0] = 0;

            do
            {
                    read[1] = read[0];
                    read[0] = netStream.ReadByte();
                    datarecive.Add(Convert.ToByte(read[0]));
            }
            while (read[0] != 10 || read[1] != 13);
            for (int i = 0; i < datarecive.Count(); i++)
            {
                dataget[i] = datarecive[i];
            }
            Encoding encoding = Encoding.GetEncoding(20866);
            responseData = encoding.GetString(dataget, 0, dataget.Length);
            Console.Write("Received: {0}", responseData);
        }

        public static double CalculateMedian(int[] Arr)
        {
            double median;
            Array.Sort(Arr);
            if (Arr.Length % 2 == 0)
            {
                int medianIndex1 = Arr.Length / 2;
                int medianIndex2 = Arr.Length / 2 + 1;
                int number1 = Arr[medianIndex1 - 1];
                int number2 = Arr[medianIndex2 - 1];
                median = (number1 + number2) / 2.00;
            }
            else
            {
                int medianIndex = ((Arr.Length - 1) / 2) + 1;
                median = Arr[medianIndex - 1];
            }
            return median;
        }

        public static int UnixTimeStampToYear(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime.Year;
        }

       
    }
}
