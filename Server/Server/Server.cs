using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using ServerTools;

namespace Server
{
    public static class Server
    {
        private static MySqlConnection conn;
        private static MySqlCommand cmd;
        private static bool connected;
        private static byte[] bytes;

        private static bool OpenConnection()
        {
            conn = new MySqlConnection(
                "server=serverIP;user=root;database=databaseName;" +
                "port=3306;password=*********;");
            cmd = new();

            try
            {
                conn.Open();
                cmd.Connection = conn;
                Console.WriteLine("Success");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataBaseConnectionError");
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static async Task<int> Main()
        {
            OpenConnection();
            
            bytes = new byte[100];
            var hostName = Dns.GetHostName();
            IPHostEntry localHost = await Dns.GetHostEntryAsync(hostName);
            IPAddress ip = localHost.AddressList[0];

            IPEndPoint ipEnd = new(ip, 11_000);

            using Socket listener = new(
                ipEnd.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
                );

            listener.Bind(ipEnd);
            listener.Listen(100);

            while (true)
            {
                Console.WriteLine("rounding");

                Socket handler = await listener.AcceptAsync();
                connected = true;

                while (connected)
                {
                    var received = await handler.ReceiveAsync(bytes, SocketFlags.None);

                    await ProcessResponse(bytes, handler, received);
                }
            }
        }

        private static async Task ProcessResponse(byte[] request, Socket handler, int recevied)
        {
            if (recevied > 0)
            {
                Console.WriteLine((CommandIdx)request[0]);
                switch ((CommandIdx)request[0])
                {
                    case CommandIdx.GetUser:
                        await ReturnUser(request, handler); break;

                    case CommandIdx.RegisterUser:
                        await RegisterNewUser(request, handler); break;

                    case CommandIdx.CloseConnection:
                        CloseConnection(handler); break;

                    case CommandIdx.ChangePointsRequest:
                        await ChangeUserPoints(request, handler); break;
                }
            }
        }

        private static async Task ReturnUser(byte[] request, Socket handler)
        {
            LoggingInfo userInfo = LoggingInfo.GetFromBytes(1, request);

            if (GetUserInDataBase(userInfo, out User user))
            {
                bytes[0] = (byte)CommandIdx.GetUser;
                user.CodeUserAsBytes(1, bytes);

                await handler.SendAsync(bytes, 0);
            }
            else
            {
                await SendMessege(handler, MessegeType.UserNotFound);
            }
        }

        private static async Task ChangeUserPoints(byte[] request, Socket handler)
        {
            uint userId = BitConverter.ToUInt32(request, 1);
            float newPoints = BitConverter.ToSingle(request, 5);

            if (ChangePointsInDataBase(userId, newPoints))
            {
                await SendMessege(handler, MessegeType.Success);
            }
            else
            {
                await SendMessege(handler, MessegeType.Error);
            }
        }

        private static async Task RegisterNewUser(byte[] request, Socket handler)
        {
            LoggingInfo userInfo = LoggingInfo.GetFromBytes(1, request);
            bool res = await RegisterNewUserInDataBase(handler, userInfo, DateTime.UtcNow);

            if (res)
                await SendMessege(handler, MessegeType.Success);
        }

        private static async Task SendMessege(Socket handler, MessegeType messege)
        {
            byte[] echoBytes = [(byte)CommandIdx.SendMessege, (byte)messege];
            await handler.SendAsync(echoBytes, 0);
        }

        private static bool CloseConnection(Socket handler)
        {
            if (!connected)
                return false;

            try
            {
                connected = false;
                handler.Shutdown(SocketShutdown.Both);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private static bool ChangePointsInDataBase(uint userID, float newPoints)
        {
            try
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "UPDATE users SET points = @points WHERE id = @userID";
                cmd.CommandType = System.Data.CommandType.Text;

                cmd.Parameters.AddWithValue("@userID", userID);
                cmd.Parameters.AddWithValue("@points", newPoints);

                cmd.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static async Task<bool> RegisterNewUserInDataBase(Socket handler, LoggingInfo userInfo, DateTime regDate)
        {
            try
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT * FROM Users WHERE name = @userName";

                cmd.Parameters.AddWithValue("@userName", userInfo.Login);

                MySqlDataReader rdr = cmd.ExecuteReader();
                if(rdr.HasRows) 
                {
                    await SendMessege(handler, MessegeType.UserAlreadyExist);
                    return false;
                }
                await rdr.CloseAsync();

                cmd.Parameters.Clear();
                cmd.CommandText = "INSERT INTO users (points, rdate, name, password) " +
                    "VALUES(@Points, @Rdate, @Name, @Password)";

                cmd.Parameters.AddWithValue("@Points", 0);
                cmd.Parameters.AddWithValue("@Rdate", regDate);
                cmd.Parameters.AddWithValue("@Name", userInfo.Login);
                cmd.Parameters.AddWithValue("@Password", userInfo.Password);

                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await SendMessege(handler, MessegeType.RegisterError);
                return false;
            }
        }

        public static bool GetUserInDataBase(LoggingInfo userInfo, out User user)
        {
            try
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "SELECT * FROM Users WHERE name = @userName AND password = @password";

                cmd.Parameters.AddWithValue("@userName", userInfo.Login);
                cmd.Parameters.AddWithValue("@password", userInfo.Password);

                MySqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.HasRows)
                {
                    if (rdr.Read())
                    {
                        uint id = (uint)rdr[0];
                        float points = (float)rdr[1];
                        DateTime date = (DateTime)rdr[2];
                        string name = (string)rdr[3];

                        user = new User()
                        {
                            Id = id,
                            Points = points,
                            RegistrationDate = new RegistrationDate() { Day = (byte)date.Day, Month = (byte)date.Month, Year = (ushort)date.Year },
                            Name = name
                        };

                        rdr.Close();
                        return true;
                    }
                }

                user = new();
                rdr.Close();
                return false;
            }
            catch (Exception ex)
            {
                user = new();
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}