using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SqsClient
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        AmazonSQSClient client;
        string queueUrl = "https://sqs.us-east-1.amazonaws.com/845377795179/Hqueue";

        public MainWindow()
        {
            InitializeComponent();
            var sqsConfig = new AmazonSQSConfig();
            sqsConfig.RegionEndpoint = Amazon.RegionEndpoint.USEast1;
            client = new AmazonSQSClient(sqsConfig);
            Task task = new Task(() => runReceiveMessage());
            task.Start();
        }

        //不断向后台请求接收消息
        public void runReceiveMessage()
        {
            while (true)
            {

                var messages = receiveMessage();
                foreach (var message in messages)
                {
                    try
                    {
                        deleteMessage(message.ReceiptHandle);
                        string time = DateTime.Now.ToLocalTime().ToString();
                        
                        txtBoxOutput.Dispatcher.BeginInvoke(new Action(() => txtBoxOutput.Text += "(" + time + "):" + message.Body + "\n"));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        
                    }
                }
                Thread.Sleep(200);

            }
        }

        void deleteMessage(string receiptHandle)
        {
            var deleteMessageRequest = new DeleteMessageRequest();

            deleteMessageRequest.QueueUrl = queueUrl;
            deleteMessageRequest.ReceiptHandle = receiptHandle;
            client.DeleteMessage(deleteMessageRequest);
        }

        void sendMessage(string message)
        {
            var request = new SendMessageRequest();
            request.QueueUrl = queueUrl;
            request.MessageBody = message;
            client.SendMessage(request);

        }
        List<Message> receiveMessage()
        {
            //string queueUrl = "https://sqs.us-east-1.amazonaws.com/845377795179/Hqueue";
            //var sqsConfig = new AmazonSQSConfig();
            //sqsConfig.RegionEndpoint = Amazon.RegionEndpoint.USEast1;

            var request = new ReceiveMessageRequest();
            request.QueueUrl = queueUrl;
            var response = client.ReceiveMessage(request);
            return response.Messages;

        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = txtBoxInput.Text;
                if (message == string.Empty)
                {
                    return;
                }
                Task task = new Task(() => sendMessage(message));
                //sendMessage(message);
                task.Start();
                clearInput();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnReceive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Task task = new Task(() =>
                {
                    var messages = receiveMessage();
                    foreach (var message in messages)
                    {
                        try
                        {
                            string time = DateTime.Now.ToLocalTime().ToString();

                            txtBoxOutput.Dispatcher.BeginInvoke(new Action(() => txtBoxOutput.Text += "(" + time + "):" + message.Body + "\n"));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }

                });
                task.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void clearInput()
        {
            txtBoxInput.Text = string.Empty;
        }
        private void clearOutput()
        {
            txtBoxOutput.Text = string.Empty;
        }

        private void BtnClearOutput_Click(object sender, RoutedEventArgs e)
        {
            clearOutput();
        }

        private void BtnClearInput_Click(object sender, RoutedEventArgs e)
        {
            clearInput();
        }
    }
}
