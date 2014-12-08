using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using SkyJukebox.Api;

namespace SkyJukebox.MinecraftIntegration
{
    internal class PipeServer
    {
        internal volatile bool IsRunning;
        internal IPlaybackManager PlaybackManager { get; set; }
        internal void Run()
        {
            // Create pipe instance
            using (var pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 4))
            {
                Console.WriteLine("[SJPS] Thread created");
                // wait for connection
                Console.WriteLine("[SJPS] Waiting for connection");
                pipeServer.WaitForConnection();
                Console.WriteLine("[SJPS] Client has connected");
                IsRunning = true;
                pipeServer.ReadTimeout = 1000;

                // Stream for the request.
                var sr = new StreamReader(pipeServer);
                // Stream for the response.
                var sw = new StreamWriter(pipeServer) {AutoFlush = true};

                while (IsRunning)
                {
                    try
                    {
                        // Read request from the stream.
                        var echo = sr.ReadLine();
                        Console.WriteLine("[SJPS] Recieved request: " + echo);

                        if (echo == "Close")
                        {
                            IsRunning = false;
                            break;
                        }

                        try
                        {
                            ParseRequest(echo);
                            sw.WriteLine("Ack");
                        }
                        catch (Exception)
                        {
                            sw.WriteLine("Error");
                        }
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("[SJPS] Error: {0}", e.Message);
                        IsRunning = false;
                    }
                }

                pipeServer.Disconnect();
                pipeServer.Close();
            }
        }

        private void ParseRequest(string req)
        {
            var pars = req.Split(' ');
            switch (pars[0])
            {
                case "Info":
                    switch (pars[1])
                    {
                        case "List":
                            break;
                        case "Song":
                            ParseSongInfoRequest(pars.Skip(3), int.Parse(pars[2]));
                            break;
                        case "Now":
                            ParseSongInfoRequest(pars.Skip(2), -1);
                            break;
                        case "Vol":
                            break;
                    }
                    break;
                case "Play":
                    PlaybackManager.PlayPauseResume();
                    break;
                case "Stop":
                    PlaybackManager.Stop();
                    break;
                case "Next":
                    PlaybackManager.Next();
                    break;
                case "Previous":
                    PlaybackManager.Previous();
                    break;
                case "Select":
                    break;
                case "Vol":
                    switch (pars[1])
                    {
                        case "Add":
                            break;
                        case "Set":
                            break;
                    }
                    break;
            }
        }

        private void ParseSongInfoRequest(IEnumerable<string> req, int id)
        {

        }
    }
}
