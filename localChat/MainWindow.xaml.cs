using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace localChat
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socketClient = null;   //通讯socket
        public MainWindow()
        {
            InitializeComponent();
        }

        private void send_Btn_Click(object sender, RoutedEventArgs e)
        {
            sendBtn();
        }

        private void sendBtn()
        {
            try
            {
                string msg = sendTxt.Text.Trim();
                if (socketClient != null && !string.IsNullOrEmpty(msg))
                {
                    socketClient.Send(Encoding.UTF8.GetBytes("(" + DateTime.Now.ToLongTimeString().ToString() + ")" + usrNameTxt.Text + ":\n" + msg));  //发送数据
                    ShowMsg("(" + DateTime.Now.ToLongTimeString().ToString() + ")" + "我:\n" + msg);
                }
                messageText.ScrollToEnd();
                sendTxt.Text = "";
            }
            catch (Exception er)
            {
                ShowMsg("发送数据异常"/* + er.ToString()*/);
            }
        }

        private void sendMsg(string msg)
        {
            try
            {
                if (socketClient != null && !string.IsNullOrEmpty(msg))
                {
                    socketClient.Send(Encoding.UTF8.GetBytes(msg));  //发送数据
                }
            }
            catch (Exception er)
            {
                ShowMsg("发送数据异常"/* + er.ToString()*/);
            }
        }

        private async void Recive()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        byte[] bytes = new byte[1024 * 1024 * 2];
                        int length = socketClient.Receive(bytes);
                        string msg = Encoding.UTF8.GetString(bytes, 0, length);
                        ShowMsg(msg);
                        //ShowMsg("接收到数据：" + msg);
                    }
                    catch (Exception er)
                    {
                        ShowMsg("接收数据异常"+ er.ToString());
                        break;
                    }
                }
            });
        }


        private void ShowMsg(string msg)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                messageText.Text += "\n"+ msg + "\n";
            }));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Thread connectth = new Thread(connect);
            connectth.Start();
        }

        private void connect()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (socketClient == null)
                    {
                        socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socketClient.Connect(IPAddress.Parse(ipText.Text.Trim()), int.Parse(portText.Text.Trim()));
                        ShowMsg(string.Format("连接服务器（{0}:{1}）成功！", ipText.Text.Trim(), portText.Text.Trim()));
                        Recive();
                        sendMsg("[提示]" + usrNameTxt.Text + "加入会话");
                    }
                }
                catch (Exception er)
                {
                    ShowMsg("连接服务器异常：" /*+ er.ToString()*/);
                    ShowMsg("连接服务器异常");
                    socketClient = null;
                }
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                sendMsg("[提示]" + usrNameTxt.Text + "退出会话");
            }
            catch { }
        }

        private void messageText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            messageText.ScrollToEnd();
        }

        private void sendTxt_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    sendBtn();
                    break;
            }
        }
    }
}
