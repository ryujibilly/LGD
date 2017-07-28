using System;  
using System.Collections.Generic;  
using System.Linq;  
using System.Text;  
using System.Net.Sockets;  
  
namespace Tool.TCP
{  
    /// <summary>  
    /// �첽SOCKET TCP �������洢�ͻ���״̬��Ϣ����  
    /// </summary>  
    public class AsyncSocketState  
    {  
        #region �ֶ�  
        /// <summary>  
        /// �������ݻ�����  
        /// </summary>  
        private byte[] _recvBuffer;  
  
        /// <summary>  
        /// �ͻ��˷��͵��������ı���  
        /// ע��:����Щ����±��Ŀ���ֻ�Ǳ��ĵ�Ƭ�϶�������  
        /// </summary>  
        private string _datagram;  
  
        /// <summary>  
        /// �ͻ��˵�Socket  
        /// </summary>  
        private Socket _clientSock;
 
        #endregion  
 
        #region ����  
  
        /// <summary>  
        /// �������ݻ�����   
        /// </summary>  
        public byte[] RecvDataBuffer  
        {  
            get  
            {  
                return _recvBuffer;  
            }  
            set  
            {  
                _recvBuffer = value;  
            }  
        }  
  
        /// <summary>  
        /// ��ȡ�Ự�ı���  
        /// </summary>  
        public string Datagram  
        {  
            get  
            {  
                return _datagram;  
            }  
            set  
            {  
                _datagram = value;  
            }  
        }  
  
        /// <summary>  
        /// �����ͻ��˻Ự������Socket����  
        /// </summary>  
        public Socket ClientSocket  
        {  
            get  
            {  
                return _clientSock;  
  
            }  
        }  
 
 
        #endregion  
  
        /// <summary>  
        /// ���캯��  
        /// </summary>  
        /// <param name="cliSock">�Ựʹ�õ�Socket����</param>  
        public AsyncSocketState(Socket cliSock)  
        {  
            _clientSock = cliSock;  
        }  
  
        /// <summary>  
        /// ��ʼ�����ݻ�����  
        /// </summary>  
        public void InitBuffer()  
        {  
            if (_recvBuffer == null&&_clientSock!=null)  
            {  
                _recvBuffer=new byte[_clientSock.ReceiveBufferSize];  
            }  
        }  
  
        /// <summary>  
        /// �رջỰ  
        /// </summary>  
        public void Close()  
        {  
  
            //�ر����ݵĽ��ܺͷ���  
            _clientSock.Shutdown(SocketShutdown.Both);  
  
            //������Դ  
            _clientSock.Close();  
        }  
    }  
}  