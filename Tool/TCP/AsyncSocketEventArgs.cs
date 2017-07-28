using System;  
using System.Collections.Generic;  
using System.Linq;  
using System.Text;  
  
namespace Tool.TCP
{  
    /// <summary>  
    /// �첽Socket TCP�¼�������  
    /// </summary>  
    public class AsyncSocketEventArgs:EventArgs  
    {  
        /// <summary>  
        /// ��ʾ��Ϣ  
        /// </summary>  
        public string _msg;  
  
        /// <summary>  
        /// �ͻ���״̬��װ��  
        /// </summary>  
        public AsyncSocketState _state;  
  
        /// <summary>  
        /// �Ƿ��Ѿ��������  
        /// </summary>  
        public bool IsHandled { get; set; }  
  
        public AsyncSocketEventArgs(string msg)  
        {  
            this._msg = msg;  
            IsHandled = false;  
        }  
        public AsyncSocketEventArgs(AsyncSocketState state)  
        {  
            this._state = state;  
            IsHandled = false;  
        }  
        public AsyncSocketEventArgs(string msg, AsyncSocketState state)  
        {  
            this._msg = msg;  
            this._state = state;  
            IsHandled = false;  
        }  
    }  
}  