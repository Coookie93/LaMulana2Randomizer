using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Collections.Generic;

namespace LM2RandomiserMod
{
    public class ItemTracker : MonoBehaviour
    {
        public static ItemTracker instance;

        private const int port = 56789;

        private TcpListener listener;
        private List<ConnectedClient> clients = new List<ConnectedClient>();

        private Font currentFont = null;
        public void OnGUI()
        {
            if (currentFont == null)
                currentFont = Font.CreateDynamicFontFromOSFont("Consolas", 14);

            GUIStyle guistyle = new GUIStyle(GUI.skin.label);
            guistyle.normal.textColor = Color.white;
            guistyle.font = currentFont;
            guistyle.fontStyle = FontStyle.Bold;
            guistyle.fontSize = 14;

            GUIContent content = new GUIContent($"Total clients connected {clients.Count}");
            Vector2 size = guistyle.CalcSize(content);
            GUI.Label(new Rect(Screen.width - size.x, 0, size.x, size.y), content, guistyle);
        }

        public void Start()
        {
            instance = this;
            StartListener();
            On.L2Base.L2System.setItem += OnSetItem;
        }

        public void OnSetItem(On.L2Base.L2System.orig_setItem orig, L2Base.L2System sys, string item_name, int num, bool direct, bool loadcall, bool sub_add)
        {
            orig(sys, item_name, num, direct, loadcall, sub_add);
            Send(item_name);
        }

        public void Send(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (var client in clients)
                client.Send(data);
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
            ConnectedClient newClient = new ConnectedClient(client);
            clients.Add(newClient);
            listener.BeginAcceptTcpClient(OnClientConnected, null);
        }
    }

    public class ConnectedClient
    {
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public TcpClient Client;

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
