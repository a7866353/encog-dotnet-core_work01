using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using SocketTestClient.RequestObject;

namespace SocketTestClient.Sender
{
    enum DeamonState
    {
        Disconnected,
        Connected,
        Receiving,
        Sending,
        Disconnecting,
    }
    class SocketDeamonSender : ISender
    {
        private IPAddress _ipAddress = IPAddress.Parse("127.0.0.1");
        private int _listenPort = 9000;
        private Socket _socket;
        private Socket _clientSocket;
        private Thread _listenThread;

        private Semaphore _sendFinishEvent;
        private Semaphore _sendRequestEvent;

        private IRequest _sendRquest;
        private List<IRequest> _receiveRequestList;

        private DeamonState _deamonState;
        public DeamonState State
        {
            get { return _deamonState; }
        }

        private byte[] _rcvBuffer = new byte[1024*1024];

        public SocketDeamonSender()
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(_ipAddress, _listenPort));
            _socket.Listen(1);

            _sendFinishEvent = new Semaphore(0, 1);
            _sendRequestEvent = new Semaphore(0, 1);
            _deamonState = DeamonState.Disconnected;

            _receiveRequestList = new List<IRequest>();

            _listenThread = new Thread(new ThreadStart(ListenClientTask));
            _listenThread.Start();

        }
        public int Send(IRequest req)
        {
            switch(_deamonState)
            {
                case DeamonState.Connected:
                case DeamonState.Receiving:
                case DeamonState.Sending:
                    break;
                default:
                    return -1;
            }

            // send request
            printf("[Srv]Send:" + req.GetType());
            _sendRquest = req;
            _sendRequestEvent.Release();
            _sendFinishEvent.WaitOne();

            // return ok.
            return 0;
        }

        public int ReceiveRequestCount
        {
            get { return _receiveRequestList.Count; }
        }

        public IRequest Get()
        {
            if( _receiveRequestList.Count == 0)
            {
                return null;
            }
            IRequest res = _receiveRequestList[0];
            _receiveRequestList.RemoveAt(0);

            printf("[Srv]Rcv:" + res.GetType());

            return res;
        }

        private void ListenClientTask()
        {
            printf("[Srv] Start listen!");
            DeamonState state = DeamonState.Disconnected;
            while(true)
            {
                switch(state)
                {
                    case DeamonState.Disconnected:
                        state = StateDisconnected();
                        break;
                    case DeamonState.Connected:
                        state = StateConnected();
                        break;
                    case DeamonState.Receiving:
                        state = StateReceiving();
                        break;
                    case DeamonState.Sending:
                        state = StateSending();
                        break;
                    case DeamonState.Disconnecting:
                        state = StateDisconnecting();
                        break;
                    default:
                        throw(new Exception("Error state!"));
                        break;
                }
            }

            
        }

        private DeamonState StateDisconnected()
        {
            _clientSocket = _socket.Accept();
            _deamonState = DeamonState.Connected;
            printf("[Srv] Client attatched!");

            return _deamonState;
        }
        private DeamonState StateConnected()
        {
            while (true)
            {
                if (_clientSocket.Connected == false)
                {
                    _deamonState = DeamonState.Disconnected;
                    break;
                }
                if (_clientSocket.Available > 0)
                {
                    _deamonState = DeamonState.Receiving;
                    break;
                }
                if (_sendRequestEvent.WaitOne(100) == true)
                {
                    _deamonState = DeamonState.Sending;
                    break;
                }

            }
            return _deamonState;
        }

        private DeamonState StateSending()
        {
            int rcvCount;

            // Send data
            try
            {
                rcvCount = _clientSocket.Send(_sendRquest.GetBytes());
            }
            catch (Exception e)
            {
                printf("[Srv]" + e.Message);
            }

            // Get ACK
            try
            {
                rcvCount = _clientSocket.Receive(_rcvBuffer);
            }
            catch (Exception e)
            {
                // timeout
            }

            _sendFinishEvent.Release();

            _deamonState = DeamonState.Connected;
            return _deamonState;
        }

        private DeamonState StateReceiving()
        {
            int rcvCount = 0;
            // _clientSocket.ReceiveTimeout = 500;
            try
            {
                rcvCount = _clientSocket.Receive(_rcvBuffer);
            }
            catch (Exception e)
            {
                // timeout
            }

            // data received
            IRequest rcvReq = Request.FromBytes(_rcvBuffer, rcvCount);
            if (rcvReq != null)
                _receiveRequestList.Add(rcvReq);

            byte[] ackBuffer = new byte[] { 0x55 };

            try
            {
                rcvCount = _clientSocket.Send(ackBuffer);
            }
            catch (Exception e)
            {
                printf("[Srv]" + e.Message);
            }

            _deamonState = DeamonState.Connected;
            return _deamonState;

        }

        private DeamonState StateDisconnecting()
        {
            printf("[Srv] Client disconnected!");
            _deamonState = DeamonState.Disconnected;
            return _deamonState;
        }
        private void printf(string str)
        {
            System.Console.WriteLine(str);
        }


    }
}
