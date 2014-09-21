using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;

namespace MinecraftIntegration
{
    public class PipeServer
    {
        public void Run()
        {
            // Create pipe instance
            using (var pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, 4))
            {
                Console.WriteLine("[SJPS] Thread created");
                while (true)
                {
                    // wait for connection
                    Console.WriteLine("[SJPS] Waiting for connection");
                    pipeServer.WaitForConnection();
                    Console.WriteLine("[SJPS] Client has connected");

                    // Stream for the request.
                    var sr = new StreamReader(pipeServer);
                    // Stream for the response.
                    var sw = new StreamWriter(pipeServer) {AutoFlush = true};
                    var hasAuth = false;
                    int tryCount = 1, maxTry = 3;

                    while (true)
                    {
                        try
                        {
                            // Read request from the stream.
                            var echo = sr.ReadLine();
                            Console.WriteLine("[SJPS] Recieved request: " + echo);

                            if (echo == "CLOSE")
                                break;

                            if (!hasAuth)
                            {
                                // UUID format: XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
                                Guid user;
                                if (echo != null && (echo.Length >= 41 && echo.Substring(0, 4) == "AUTH" && Guid.TryParseExact(echo.Substring(5, 36), "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX", out user)))
                                {
                                    sw.WriteLine("AUTH OK");
                                    hasAuth = true;
                                }
                                else
                                {
                                    if (tryCount > maxTry)
                                    {
                                        sw.WriteLine("AUTH FAIL");
                                        break;
                                    }
                                    sw.WriteLine("AUTH REQUEST " + tryCount + " OF " + maxTry);
                                    ++tryCount;
                                }
                                continue;
                            }

                            try
                            {
                                ParseRequest(echo);
                            }
                            catch (Exception)
                            {
                                sw.WriteLine("INVALID REQ");
                            }

                            // Write response to the stream.
                            sw.WriteLine("ACK");
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine("[SJPS] ERROR: {0}", e.Message);
                        }
                    }

                    pipeServer.Disconnect();
                }
                pipeServer.Close();
            }
        }

        private void ParseRequest(string req)
        {
            var pars = req.Split(' ');
            switch (pars[0])
            {
                case "INFO":
                    switch (pars[1])
                    {
                        case "LIST":
                            break;
                        case "SONG":
                            ParseSongInfoRequest(pars.Skip(3), int.Parse(pars[2]));
                            break;
                        case "NOW":
                            ParseSongInfoRequest(pars.Skip(2), -1);
                            break;
                        case "VOL":
                            break;
                    }
                    break;
                case "PLAY":
                    break;
                case "PAUSE":
                    break;
                case "STOP":
                    break;
                case "NEXT":
                    break;
                case "PREV":
                    break;
                case "SELECT":
                    break;
                case "VOL":
                    switch (pars[1])
                    {
                        case "ADD":
                            break;
                        case "SET":
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
