using System.Net;
using System.Text;
using MQTTnet;
using MQTTnet.Server;
using Newtonsoft.Json;

public class Server
{
    private static MqttServer? mqttServer = null;

    private static bool connected = false;

    private static readonly Dictionary<IPEndPoint, string> mqttClients = [];

    private static Identifier? idManager;

    private static LevelDBWrapper? database;

    private static async Task Main(string[] args)
    {   
        // Load the LevelDB database
        database = new LevelDBWrapper("./server_data");

        // Create and load the drawing allocation object
        idManager = new();
        idManager.Load("./allocation_data");

        // Start the server
        await ServerStart();

        // Stop the server after we press exit.
        Console.WriteLine("Press ENTER to exit.");
        Console.ReadLine();
        await ServerStop();

        // Save the drawing allocation data before exiting
        Console.WriteLine("Writing allocation data.....");
        idManager.Save("./allocation_data");
    }

    private static async Task ServerStart()
    {
        if (connected)
        {
            return;
        }

        MqttFactory mqttFactory = new();
        MqttServerOptions mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();
        mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
        mqttServer.ClientConnectedAsync += OnClientConnect;
        mqttServer.ClientDisconnectedAsync += OnClientDisconnect;
        mqttServer.InterceptingPublishAsync += OnMessageReceived;
        await mqttServer.StartAsync();
        connected = true;
        Console.WriteLine("ServerStart() ==> success");
    }

    public static async Task OnClientConnect(ClientConnectedEventArgs args)
    {
        IPEndPoint clientEndPoint = ParseIPEndPoint(args.Endpoint);

        if (mqttClients.ContainsKey(clientEndPoint))
        {
            Console.WriteLine("OnClientConnect() ==> WARNING: duplicate connection");
            return;
        }

        mqttClients.Add(clientEndPoint, args.ClientId);
        Console.WriteLine("OnClientConnect() ==> " + args.Endpoint + " connected");
        await Task.CompletedTask;
    }

    public static async Task OnClientDisconnect(ClientDisconnectedEventArgs args)
    {
        IPEndPoint clientEndPoint = ParseIPEndPoint(args.Endpoint);
        
        if (!mqttClients.ContainsKey(clientEndPoint))
        {
            Console.WriteLine("OnClientConnect() ==> warning: non-existing connection");
            return;
        }

        mqttClients.Remove(clientEndPoint);
        Console.WriteLine("OnClientDisconnect() ==> " + args.Endpoint + " disconnected");
        await Task.CompletedTask;
    }

    public static async Task OnMessageReceived(InterceptingPublishEventArgs args)
    {
        if (!connected)
        {
            return;
        }

        if (args.ApplicationMessage.Topic != "cmd" && args.ApplicationMessage.Topic != "qry" &&
            args.ApplicationMessage.Topic != "qry_single")
        {
            args.ProcessPublish = false;
            return;
        }

        // Given a request of (target chunk, target id) try to find it and return the command metadata for its add container.
        if (args.ApplicationMessage.Topic == "qry_single")
        {
            string s = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
            SerializeUtilities.SingleQueryObj qryObj = JsonConvert.DeserializeObject<SerializeUtilities.SingleQueryObj>(s);
            string qry_key = qryObj.targetChunk.x + "," + qryObj.targetChunk.y;
            string qry_val = database.Get(qry_key);
            if (qry_val==null){
                args.ProcessPublish=false;
                return;
            }
            List<string> qry_list = JsonConvert.DeserializeObject<List<string>>(qry_val);
            string m = "NULL";
            foreach (string commandstr in qry_list){
                Command c = JsonConvert.DeserializeObject<Command>(commandstr);
                if (c.addContainer.id == qryObj.targetID){
                    m=commandstr;
                    break;
                }
            }

            // on success, we have sent a command string corresponding to what we found.
            //otherwise send failure (NULL str) if (chunk, id) object was not found.
            args.ApplicationMessage.PayloadSegment = Encoding.UTF8.GetBytes(m);
            args.ApplicationMessage.Topic = "response_single/" + args.ClientId;
            args.ProcessPublish = true;
            return;
        }

        // DO STUFF HERE AND THEN RETURN FOR QRY
        if (args.ApplicationMessage.Topic == "qry")
        {
            SerializeUtilities.SVector2 v = JsonConvert.DeserializeObject<SerializeUtilities.SVector2>
                                            (Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment));
            string key = $"{v.x},{v.y}";
            string value = database.Get(key);

            string topic = "response/" + args.ClientId;
            string payload;
            if (value == null || JsonConvert.DeserializeObject<List<string>>(value).Count == 0)
            {
                payload = "NULL" + JsonConvert.SerializeObject(v);
            }
            else
            {
                payload = value;
            }

            // Send payload to client by overriding previous values
            args.ApplicationMessage.Topic = topic;
            args.ApplicationMessage.PayloadSegment = Encoding.UTF8.GetBytes(payload);
            args.ProcessPublish = true;
            return;
        }
        
        string commandStr = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
        Command? command = JsonConvert.DeserializeObject<Command>(commandStr);
        
        if (command == null)
        {
            args.ProcessPublish = false;
            return;
        }

        if (command.type == Command.Type.ADD)
        {
            command.addContainer.id = idManager.Allocate();
            commandStr = JsonConvert.SerializeObject(command, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            // Store in our database
            List<string> values;
            string key = command.targetChunk.ToString();
            string value = database.Get(key);
            // new list
            if (value==null){
                values = new List<string>();
            }else{
                values = JsonConvert.DeserializeObject<List<string>>(value);
            }
            values.Add(commandStr);
            value = JsonConvert.SerializeObject(values, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            database.Put(key, value);
            args.ApplicationMessage.PayloadSegment = Encoding.UTF8.GetBytes(commandStr);
        }
        else if (command.type == Command.Type.DELETE)
        {
            // Race conditions -- someone has already submitted this.
            if (!idManager.GetBitmap(command.deleteContainer.id))
            {
                args.ProcessPublish = false;
                return;
            }

            idManager.Deallocate(command.deleteContainer.id);

            // Locate the object in the target chunk, delete it, and write back to list.
            string key = command.targetChunk.x + "," + command.targetChunk.y;
            string list = database.Get(key);
            List<string> commands = JsonConvert.DeserializeObject<List<string>>(list);
            
            for (int i = 0; i < commands.Count; i++)
            {
                string s = commands[i];
                Command c = JsonConvert.DeserializeObject<Command>(s);
                
                if (c.addContainer.id == command.deleteContainer.id)
                {
                    commands.RemoveAt(i);
                    //Console.WriteLine("SUCCESSFULLY DELETED!");
                    break;
                }
            }
            string newList = JsonConvert.SerializeObject(commands);
            database.Put(key, newList); // Put it back with the removed object.   
        }
        else if (command.type == Command.Type.MODIFY)
        {
            try{
            // LASTCHUNK != NEWCHUNK
            if (!command.targetChunk.Equals(command.modifyContainer.lastChunk))
            {
                string oldS = database.Get(command.modifyContainer.lastChunk.ToString());
                string newS = database.Get(command.targetChunk.ToString());
                List<string> cstrsOld = JsonConvert.DeserializeObject<List<string>>(oldS);
                List<string> cstrsNew;
                if (newS != null)
                {
                    cstrsNew = JsonConvert.DeserializeObject<List<string>>(newS);
                }
                cstrsNew = new List<string>();

                Command c = null;
                // Find and remove command from old db
                for (int i = 0; i < cstrsOld.Count; i++)
                {
                    string cstr = cstrsOld[i];
                    c = JsonConvert.DeserializeObject<Command>(cstr);

                    if (c.addContainer.id == command.modifyContainer.id)
                    {
                        cstrsOld.RemoveAt(i);
                        break;
                    }
                }

                if (c == null)
                {
                    Console.WriteLine("failure");
                    args.ProcessPublish = false;
                    return;
                }

                //update old database
                database.Put(command.modifyContainer.lastChunk.ToString(), JsonConvert.SerializeObject(cstrsOld));
                c.targetChunk = command.targetChunk; // reflect new chunk
                c.addContainer.pos = command.modifyContainer.newPosition;
                c.addContainer.sca = command.modifyContainer.newSize;
                c.addContainer.rot = command.modifyContainer.newRotation;
                cstrsNew.Add(JsonConvert.SerializeObject(c));
                string final = JsonConvert.SerializeObject(cstrsNew);
                database.Put(command.targetChunk.ToString(), final);
            }
            // LASTCHUNK == NEWCHUNK
            else
            {
                string s = database.Get(command.targetChunk.ToString());
                List<string> cstrs = JsonConvert.DeserializeObject<List<string>>(s);

                for (int i = 0; i < cstrs.Count; i++)
                {
                    string cstr = cstrs[i];
                    Command c = JsonConvert.DeserializeObject<Command>(cstr);

                    if (c.addContainer.id == command.modifyContainer.id)
                    {
                        c.addContainer.pos = command.modifyContainer.newPosition;
                        c.addContainer.sca = command.modifyContainer.newSize;
                        c.addContainer.rot = command.modifyContainer.newRotation;
                        cstrs[i] = JsonConvert.SerializeObject(c);
                        database.Put(command.targetChunk.ToString(), JsonConvert.SerializeObject(cstrs));
                        return;
                    }
                }
            }}catch(Exception e){Console.WriteLine(e.Message);
                Console.WriteLine(e.Message);
                //var st = new StackTrace(e, true);
                // Get the top stack frame
                //var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                //var line = frame.GetFileLineNumber();
                //Console.WriteLine(line);
            };

        }

        //args.ProcessPublish = true;
        await Task.CompletedTask;
    }

    // If server stops, does it send the event to all clients before doing so? Or do we need to do this manually?
    public static async Task ServerStop()
    {
        if (!connected)
        {
            return;
        }

        await mqttServer.StopAsync();
        mqttServer = null;
        connected = false;  
        Console.WriteLine("ServerStop() ==> success");
    }

    private static IPEndPoint ParseIPEndPoint(string ipEndPoint)
    {
        string[] ipComponents = ipEndPoint.Split(":");

        if (ipComponents.Length != 2)
        {
            // Is this the right exception?
            throw new InvalidDataException("ParseIPEndPoint() ==> invalid string representation.");
        }

        return new IPEndPoint(IPAddress.Parse(ipComponents[0]),
                              int.Parse(ipComponents[1]));
    }
}