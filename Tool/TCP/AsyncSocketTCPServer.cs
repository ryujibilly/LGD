using System;  
using System.Collections.Generic;  
using System.Linq;  
using System.Text;  
using System.Net.Sockets;  
using System.Net;  
  
namespace Tool.TCP
{  
    /// <summary>  
    /// Socketʵ�ֵ��첽TCP������  
    /// </summary>  
    public class AsyncSocketTCPServer : IDisposable  
    {  
        #region Fields  
        /// <summary>  
        /// ������������������ͻ���������  
        /// </summary>  
        private int _maxClient;  
  
        /// <summary>  
        /// ��ǰ�����ӵĿͻ�����  
        /// </summary>  
        private int _clientCount;  
  
        /// <summary>  
        /// ������ʹ�õ��첽socket  
        /// </summary>  
        private Socket _serverSock;  
  
        /// <summary>  
        /// �ͻ��˻Ự�б�  
        /// </summary>  
        private List<AsyncSocketState> _clients;  
  
        private bool disposed = false;  
 
        #endregion  
 
        #region Properties  
  
        /// <summary>  
        /// �������Ƿ���������  
        /// </summary>  
        public bool IsRunning { get; private set; }  
        /// <summary>  
        /// ������IP��ַ  
        /// </summary>  
        public IPAddress Address { get; private set; }  
        /// <summary>  
        /// �����Ķ˿�  
        /// </summary>  
        public int Port { get; private set; }  
        /// <summary>  
        /// ͨ��ʹ�õı���  
        /// </summary>  
        public Encoding Encoding { get; set; }  
 
 
        #endregion  
 
        #region ���캯��  
  
        /// <summary>  
        /// �첽Socket TCP������  
        /// </summary>  
        /// <param name="listenPort">�����Ķ˿�</param>  
        public AsyncSocketTCPServer(int listenPort) : this(IPAddress.Any, listenPort,1024)  
        {  
        }  
  
        /// <summary>  
        /// �첽Socket TCP������  
        /// </summary>  
        /// <param name="localEP">�������ս��</param>  
        public AsyncSocketTCPServer(IPEndPoint localEP) : this(localEP.Address, localEP.Port,1024)  
        {  
        }  
  
        /// <summary>  
        /// �첽Socket TCP������  
        /// </summary>  
        /// <param name="localIPAddress">������IP��ַ</param>  
        /// <param name="listenPort">�����Ķ˿�</param>  
        /// <param name="maxClient">���ͻ�������</param>  
        public AsyncSocketTCPServer(IPAddress localIPAddress, int listenPort,int maxClient)  
        {  
            this.Address = localIPAddress;  
            this.Port = listenPort;  
            this.Encoding = Encoding.Default;  
  
            _maxClient = maxClient;  
            _clients = new List<AsyncSocketState>();  
            _serverSock = new Socket(localIPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
        }  
 
        #endregion  
 
        #region Method  
  
        /// <summary>  
        /// ����������  
        /// </summary>  
        public void Start()  
        {  
            if (!IsRunning)  
            {  
                IsRunning = true;  
                _serverSock.Bind(new IPEndPoint(this.Address, this.Port));  
                _serverSock.Listen(1024);  
                _serverSock.BeginAccept(new AsyncCallback(HandleAcceptConnected), _serverSock);  
            }  
        }  
        /// <summary>  
        /// ����������  
        /// </summary>  
        /// <param name="backlog">  
        /// ������������Ĺ����������е���󳤶�  
        /// </param>  
        public void Start(int backlog)  
        {  
            if (!IsRunning)  
            {  
                IsRunning = true;  
                _serverSock.Bind(new IPEndPoint(this.Address, this.Port));  
                _serverSock.Listen(backlog);  
                _serverSock.BeginAccept(new AsyncCallback(HandleAcceptConnected), _serverSock);  
            }  
        }  
        /// <summary>  
        /// ֹͣ������  
        /// </summary>  
        public void Stop()  
        {  
            if (IsRunning)  
            {  
                IsRunning = false;  
                _serverSock.Close();  
                //TODO �رն����пͻ��˵�����  
  
            }  
        }  
  
        /// <summary>  
        /// ����ͻ�������  
        /// </summary>  
        /// <param name="ar"></param>  
        private void HandleAcceptConnected(IAsyncResult ar)  
        {  
            if (IsRunning)  
            {  
                Socket server = (Socket)ar.AsyncState;  
                Socket client = server.EndAccept(ar);  
  
                //����Ƿ�ﵽ��������Ŀͻ�����Ŀ  
                if (_clientCount >= _maxClient)  
                {  
                    //C-TODO �����¼�  
                    RaiseOtherException(null);  
                }  
                else  
                {  
                    AsyncSocketState state = new AsyncSocketState(client);  
                    lock (_clients)  
                    {  
                        _clients.Add(state);  
                        _clientCount++;  
                        RaiseClientConnected(state); //�����ͻ��������¼�  
                    }  
                    state.RecvDataBuffer = new byte[client.ReceiveBufferSize];  
                    //��ʼ�������Ըÿͻ��˵�����  
                    client.BeginReceive(state.RecvDataBuffer, 0, state.RecvDataBuffer.Length, SocketFlags.None,  
                     new AsyncCallback(HandleDataReceived), state);  
                }  
                //������һ������  
                server.BeginAccept(new AsyncCallback(HandleAcceptConnected), ar.AsyncState);  
            }  
        }  
        /// <summary>  
        /// ����ͻ�������  
        /// </summary>  
        /// <param name="ar"></param>  
        private void HandleDataReceived(IAsyncResult ar)  
        {  
            if (IsRunning)  
            {  
                AsyncSocketState state = (AsyncSocketState)ar.AsyncState;  
                Socket client = state.ClientSocket;  
                try  
                {  
                    //������ο�ʼ���첽�Ľ���,���Ե��ͻ����˳���ʱ��  
                    //������ִ��EndReceive  
                    int recv = client.EndReceive(ar);  
                    if (recv == 0)  
                    {  
                        //C- TODO �����¼� (�رտͻ���)  
                        Close(state);  
                        RaiseNetError(state);  
                        return;  
                    }  
                    //TODO �����Ѿ���ȡ������ ps:������state��RecvDataBuffer��  
  
                    //C- TODO �������ݽ����¼�  
                    RaiseDataReceived(state);  
                }  
                catch (SocketException)  
                {  
                    //C- TODO �쳣����  
                    RaiseNetError(state);  
                }  
                finally  
                {  
                    //���������������ͻ��˵�����  
                    client.BeginReceive(state.RecvDataBuffer, 0, state.RecvDataBuffer.Length, SocketFlags.None,  
                     new AsyncCallback(HandleDataReceived), state);  
                }  
            }  
        }  
  
        /// <summary>  
        /// ��������  
        /// </summary>  
        /// <param name="state">�������ݵĿͻ��˻Ự</param>  
        /// <param name="data">���ݱ���</param>  
        public void Send(AsyncSocketState state, byte[] data)  
        {  
            RaisePrepareSend(state);  
            Send(state.ClientSocket, data);  
        }  
  
        /// <summary>  
        /// �첽����������ָ���Ŀͻ���  
        /// </summary>  
        /// <param name="client">�ͻ���</param>  
        /// <param name="data">����</param>  
        public void Send(Socket client, byte[] data)  
        {  
            if (!IsRunning)  
                throw new InvalidProgramException("This TCP Scoket server has not been started.");  
  
            if (client == null)  
                throw new ArgumentNullException("client");  
  
            if (data == null)  
                throw new ArgumentNullException("data");  
            client.BeginSend(data, 0, data.Length, SocketFlags.None,  
             new AsyncCallback(SendDataEnd), client);  
        }  
  
        /// <summary>  
        /// ����������ɴ�����  
        /// </summary>  
        /// <param name="ar">Ŀ��ͻ���Socket</param>  
        private void SendDataEnd(IAsyncResult ar)  
        {  
            ((Socket)ar.AsyncState).EndSend(ar);  
            RaiseCompletedSend(null);  
        }  
        #endregion  
 
        #region �¼�  
  
        /// <summary>  
        /// ��ͻ��˵������ѽ����¼�  
        /// </summary>  
        public event EventHandler<AsyncSocketEventArgs> ClientConnected;  
        /// <summary>  
        /// ��ͻ��˵������ѶϿ��¼�  
        /// </summary>  
        public event EventHandler<AsyncSocketEventArgs> ClientDisconnected;  
  
        /// <summary>  
        /// �����ͻ��������¼�  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseClientConnected(AsyncSocketState state)  
        {  
            if (ClientConnected != null)  
            {  
                ClientConnected(this, new AsyncSocketEventArgs(state));  
            }  
        }  
        /// <summary>  
        /// �����ͻ������ӶϿ��¼�  
        /// </summary>  
        /// <param name="client"></param>  
        private void RaiseClientDisconnected(Socket client)  
        {  
            if (ClientDisconnected != null)  
            {  
                ClientDisconnected(this, new AsyncSocketEventArgs("���ӶϿ�"));  
            }  
        }  
  
        /// <summary>  
        /// ���յ������¼�  
        /// </summary>  
        public event EventHandler<AsyncSocketEventArgs> DataReceived;  
  
        private void RaiseDataReceived(AsyncSocketState state)  
        {  
            if (DataReceived != null)  
            {  
                DataReceived(this, new AsyncSocketEventArgs(state));  
            }  
        }  
  
        /// <summary>  
        /// ��������ǰ���¼�  
        /// </summary>  
        public event EventHandler<AsyncSocketEventArgs> PrepareSend;  
  
        /// <summary>  
        /// ������������ǰ���¼�  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaisePrepareSend(AsyncSocketState state)  
        {  
            if (PrepareSend != null)  
            {  
                PrepareSend(this, new AsyncSocketEventArgs(state));  
            }  
        }  
  
        /// <summary>  
        /// ���ݷ�������¼�  
        /// </summary>  
        public event EventHandler<AsyncSocketEventArgs> CompletedSend;  
          
        /// <summary>  
        /// �������ݷ�����ϵ��¼�  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseCompletedSend(AsyncSocketState state)  
        {  
            if (CompletedSend != null)  
            {  
                CompletedSend(this, new AsyncSocketEventArgs(state));  
            }  
        }  
  
        /// <summary>  
        /// ��������¼�  
        /// </summary>  
        public event EventHandler<AsyncSocketEventArgs> NetError;  
        /// <summary>  
        /// ������������¼�  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseNetError(AsyncSocketState state)  
        {  
            if (NetError != null)  
            {  
                NetError(this, new AsyncSocketEventArgs(state));  
            }  
        }  
  
        /// <summary>  
        /// �쳣�¼�  
        /// </summary>  
        public event EventHandler<AsyncSocketEventArgs> OtherException;  
        /// <summary>  
        /// �����쳣�¼�  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseOtherException(AsyncSocketState state, string descrip)  
        {  
            if (OtherException != null)  
            {  
                OtherException(this, new AsyncSocketEventArgs(descrip, state));  
            }  
        }  
        private void RaiseOtherException(AsyncSocketState state)  
        {  
            RaiseOtherException(state, "");  
        }  
        #endregion  
 
        #region Close  
        /// <summary>  
        /// �ر�һ����ͻ���֮��ĻỰ  
        /// </summary>  
        /// <param name="state">��Ҫ�رյĿͻ��˻Ự����</param>  
        public void Close(AsyncSocketState state)  
        {  
            if (state != null)  
            {  
                state.Datagram = null;  
                state.RecvDataBuffer = null;  
  
                _clients.Remove(state);  
                _clientCount--;  
                //TODO �����ر��¼�  
                state.Close();  
            }  
        }  
        /// <summary>  
        /// �ر����еĿͻ��˻Ự,�����еĿͻ������ӻ�Ͽ�  
        /// </summary>  
        public void CloseAllClient()  
        {  
            foreach (AsyncSocketState client in _clients)  
            {  
                Close(client);  
            }  
            _clientCount = 0;  
            _clients.Clear();  
        }  
        #endregion  
 
        #region �ͷ�  
        /// <summary>  
        /// Performs application-defined tasks associated with freeing,   
        /// releasing, or resetting unmanaged resources.  
        /// </summary>  
        public void Dispose()  
        {  
            Dispose(true);  
            GC.SuppressFinalize(this);  
        }  
  
        /// <summary>  
        /// Releases unmanaged and - optionally - managed resources  
        /// </summary>  
        /// <param name="disposing"><c>true</c> to release   
        /// both managed and unmanaged resources; <c>false</c>   
        /// to release only unmanaged resources.</param>  
        protected virtual void Dispose(bool disposing)  
        {  
            if (!this.disposed)  
            {  
                if (disposing)  
                {  
                    try  
                    {  
                        Stop();  
                        if (_serverSock != null)  
                        {  
                            _serverSock = null;  
                        }  
                    }  
                    catch (SocketException)  
                    {  
                        //TODO  
                        RaiseOtherException(null);  
                    }  
                }  
                disposed = true;  
            }  
        }  
        #endregion  
    }  
}  