using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using L2Base;
using L2Flag;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LM2RandomiserMod
{
    public class ItemTracker : MonoBehaviour
    {
        public static ItemTracker instance;

        private const int port = 56789;

        private TcpListener listener;
        private List<ConnectedClient> clients = new List<ConnectedClient>();
        private Queue<L2FlagBoxEnd> flags = new Queue<L2FlagBoxEnd>();

        private L2System sys;
        private bool onTitle;
        private Font currentFont;

        public void OnGUI()
        {
            if (onTitle)
            {
                if (currentFont == null)
                    currentFont = Font.CreateDynamicFontFromOSFont("Consolas", 14);

                GUIStyle guistyle = new GUIStyle(GUI.skin.label);
                guistyle.normal.textColor = Color.white;
                guistyle.font = currentFont;
                guistyle.fontStyle = FontStyle.Bold;
                guistyle.fontSize = 10;

                GUIContent content = new GUIContent($"Item Tracker\nClients connected: {clients.Count}");
                Vector2 size = guistyle.CalcSize(content);
                GUI.Label(new Rect(Screen.width - size.x, 0, size.x, size.y), content, guistyle);
            }
        }

        public void Start()
        {
            sys = GameObject.Find("GameSystem").GetComponent<L2System>();
            instance = this;
            StartListener();
        }

        public void Update()
        {
            if(flags.Count > 0)
            {
                byte[] data = new byte[3 * flags.Count];
                int index = 0;
                while (flags.Count > 0)
                {
                    L2FlagBoxEnd l2Flag = flags.Dequeue();
                    data[index + 0] = (byte)l2Flag.seet_no1;
                    data[index + 1] = (byte)l2Flag.flag_no1;
                    data[index + 2] = (byte)l2Flag.data;
                    index += 3;
                }
                Send(data);
            }
        }

        public void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            onTitle = scene.name.Equals("title");
        }

        public void Add(int sheet, int flag)
        {
            if(sheet == 0)
            {
                short data = 0;
                if (sys.getFlag(sheet, flag, ref data))
                {
                    if (data > 0)
                    {
                        if (flag == 32)
                        {
                            flags.Enqueue(new L2FlagBoxEnd()
                            {
                                seet_no1 = 2,
                                flag_no1 = 8,
                                data = data
                            });
                        }
                        else if (flag == 3)
                        {
                            flags.Enqueue(new L2FlagBoxEnd()
                            {
                                seet_no1 = 2,
                                flag_no1 = 76,
                                data = data
                            });
                        }
                    }
                    else
                    {
                        if (flag == 3)
                        {
                            flags.Enqueue(new L2FlagBoxEnd()
                            {
                                seet_no1 = 2,
                                flag_no1 = 76,
                                data = data
                            });
                        }
                    }
                }
            }
            else if (sheet == 2)
            {
                if (flag == 8)
                    return;

                short data = 0;
                if (sys.getFlag(sheet, flag, ref data))
                {
                    if ((data > 0 && flag != 15) || (data > 1 && flag == 15))
                    {
                        flags.Enqueue(new L2FlagBoxEnd()
                        {
                            seet_no1 = sheet,
                            flag_no1 = flag,
                            data = data
                        });
                    }
                }
            }
            else if (sheet == 3 && (flag >= 10 || flag <= 18))
            {
                short data = 0;
                if (sys.getFlag(sheet, flag, ref data))
                {
                    if (data >= 4)
                    {
                        flags.Enqueue(new L2FlagBoxEnd()
                        {
                            seet_no1 = sheet,
                            flag_no1 = flag,
                            data = data
                        });
                    }
                }
            }
            else if(sheet > 99)
            {
                flags.Enqueue(new L2FlagBoxEnd()
                {
                    seet_no1 = sheet,
                    flag_no1 = 0,
                    data = 0
                });
            }
        }

        public void RemoveClient(ConnectedClient client)
        {
            lock (clients)
            {
                clients.Remove(client);
            }
        }

        private void StartListener()
        {
            listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }

        private void OnClientConnected(IAsyncResult ar)
        {
            TcpClient client = listener.EndAcceptTcpClient(ar);
            ConnectedClient cClient = new ConnectedClient(client);
            clients.Add(cClient);
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }

        private void Send(byte[] data)
        {
            foreach (var client in clients)
            {
                if (client.IsConnected)
                    client.Send(data);
            }
        }

        private void Send(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (var client in clients)
            {
                if (client.IsConnected)
                    client.Send(data);
            }
        }
    }

    public class ConnectedClient
    {
        private const int BufferSize = 1024;
        private byte[] Buffer = new byte[BufferSize];
        private TcpClient Client;

        public bool IsConnected {
            get => Client.Connected;
        }

        public ConnectedClient(TcpClient client)
        {
            Client = client;
            Client.GetStream().BeginRead(Buffer, 0, Buffer.Length, OnRead, null);
        }

        public void Send(byte[] data)
        {
            Client.GetStream().Write(data, 0, data.Length);
        }

        private void OnRead(IAsyncResult ar)
        {
            try
            {
                int bytesRead = Client.GetStream().EndRead(ar);
                if (bytesRead <= 0)
                {
                    ItemTracker.instance.RemoveClient(this);
                    Client.Close();
                    return;
                }
                Client.GetStream().BeginRead(Buffer, 0, Buffer.Length, OnRead, null);
            }
            catch (Exception)
            {
                ItemTracker.instance.RemoveClient(this);
            }
        }
    }
}
