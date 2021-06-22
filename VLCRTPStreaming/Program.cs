using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VLCRTPStreaming
{
    class Program
    {
        static void Main(string[] args)
        {
            //================ socket connection =================

            //TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 84);
            //server.Start();

            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 84;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    if (stream.CanRead)
                    {
                        int i;
                        i = stream.Read(bytes, 0, bytes.Length);
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }
                    else
                    {
                        Console.WriteLine("You cannot read data from this stream.");
                        client.Close();

                        // Closing the tcpClient instance does not close the network stream.
                        stream.Close();
                        return;
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }





            //================= vlc emitir audio ================

            Core.Initialize();                // instantiate the main libvlc object

            var VIDEO_URL = @"C:\videostream\Loki1x01.mp4";

            var _libvlc = new LibVLC(/*enableDebugLogs: true*/);
            var reproductor = new MediaPlayer(_libvlc);
            var rtsp1 = new Media(_libvlc, VIDEO_URL);       //Create a media object and then set its options to duplicate streams - 1 on display 2 as RTSP
            rtsp1.AddOption(":sout=#transcode{vcodec=h264,acodec=mpga,ab=128,channels=2,samplerate=44100,scodec=none}:rtp{sdp=rtsp://192.168.0.44:8554/capitulo} -sout-keep");  //The address has to be your local network adapters addres not localhost
            reproductor.Play(rtsp1);

            Console.WriteLine("Streaming on rtsp://192.168.0.44:8554/capitulo");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

        }
    }
}
