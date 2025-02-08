using System.Net;
using System.Net.Sockets;
using ServerTools;
using System.Threading.Tasks;
using System;

namespace UnityClient
{
    public class UnityClient
    {
        private Socket client;
        private byte[] bytes = new byte[100];

        public async Task OpenConnection()
        {
            var hostName = Dns.GetHostName();
            IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);

            IPAddress localIpAddress = localhost.AddressList[0];

            IPEndPoint ipEnd = new IPEndPoint(localIpAddress, 11_000);
            client = new Socket(
                ipEnd.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            await client.ConnectAsync(ipEnd);
        }

        public async Task CloseConnection()
        {
            bytes[0] = (byte)CommandIdx.CloseConnection;

            ArraySegment<byte> sendArr = new ArraySegment<byte>(bytes);
            _ = await client.SendAsync(sendArr, 0);
        }

        public async Task<User> GetUser(string name, string password)
        {
            User user;

            LoggingInfo userInfo = new LoggingInfo()
            {
                Login = name,
                Password = password,
            };

            bytes[0] = (byte)CommandIdx.GetUser;
            userInfo.CodeAsBytes(1, bytes);

            ArraySegment<byte> arr = new ArraySegment<byte>(bytes);
            await client.SendAsync(arr, 0);

            int received = await client.ReceiveAsync(arr, SocketFlags.None);
            if (received > 0)
            {
                if (arr.Array[0] == (byte)CommandIdx.GetUser)
                {
                    user = User.ConvertToUser(1, arr.Array);
                    return user;
                }
            }
            user = new User();
            return user;
        }

        public async Task<MessegeType> ChangeUserPoints(uint id, float newPoints)
        {
            bytes[0] = (byte)CommandIdx.ChangePointsRequest;

            BitConverter.GetBytes(id).CopyTo(bytes, 1);
            BitConverter.GetBytes(newPoints).CopyTo(bytes, 5);

            ArraySegment<byte> sendArr = new ArraySegment<byte>(bytes);
            _ = await client.SendAsync(sendArr, SocketFlags.None);

            return await ReadMessege();
        }

        public async Task<MessegeType> RegisterNewUser(string name, string password)
        {
            LoggingInfo loggingInfo = new LoggingInfo() { Login = name, Password = password };
            bytes[0] = (byte)CommandIdx.RegisterUser;
            loggingInfo.CodeAsBytes(1, bytes);

            ArraySegment<byte> sendArr = new ArraySegment<byte>(bytes);
            _ = await client.SendAsync(sendArr, SocketFlags.None);

            ArraySegment<byte> recArr = new ArraySegment<byte>(bytes);
            _ = await client.ReceiveAsync(recArr, 0);

            return (MessegeType)recArr.Array[1];
        }

        private async Task<MessegeType> ReadMessege()
        {
            ArraySegment<byte> recArr = new ArraySegment<byte>(bytes);
            await client.ReceiveAsync(recArr, 0);

            if (recArr.Array[0] == (byte)CommandIdx.SendMessege)
                return (MessegeType)recArr.Array[1];
            else 
                return (MessegeType)0;
        }
    }
}
