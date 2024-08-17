using System;
using System.Net.Sockets;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using System.Threading;
using System.Collections.Generic;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UIElements;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance { get; private set; }

    [SerializeField]
    private string serverName;

    [SerializeField]
    private int serverPort;

    [SerializeField]
    private float moveTimer;

    private IMqttClient mqttClient = null;

    public bool connected = false;

    private SynchronizationContext _unitySynchronizationContext;

    private string clientID;

    private void Awake()
    {
        // Enforce a singleton state pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        _unitySynchronizationContext = SynchronizationContext.Current;
    }

    private async void Start()
    {
        await Connect();
    }

    private async void OnApplicationQuit()
    {
        await DisconnectFromClient();
    }

    private async Task Connect()
    {
        if (connected)
        {
            return;
        }

        try
        {
            MqttFactory mqttFactory = new();
            mqttClient = mqttFactory.CreateMqttClient();
            clientID = Guid.NewGuid().ToString();
            MqttClientOptions mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(serverName).WithClientId(clientID).Build();
            MqttClientConnectResult response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            mqttClient.ApplicationMessageReceivedAsync += ReceiveCommand;
            mqttClient.DisconnectedAsync += DisconnectFromServer;
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("cmd").WithExactlyOnceQoS().Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("response/" + clientID).WithExactlyOnceQoS().Build());
            await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("response_single/" + clientID).WithExactlyOnceQoS().Build());
            connected = true;
            JustMonika.Instance.ActualStart(); // tell chunk manager that it can finally start querying the server.
            Debug.Log($"Connect() ==> success");
        }
        catch (Exception e)
        {
            Debug.Log($"Connect() ==> failure: {e.Message}");
        }
    }

    private async Task DisconnectFromClient()
    {
        if (!connected)
        {
            return;
        }

        await mqttClient.DisconnectAsync();
        //mqttClient.Dispose(); // giving null reference errors; fix this.
        mqttClient = null;
        connected = false;
        Debug.Log("DisconnectFromClient(): success");
    }

    public async Task DisconnectFromServer(MqttClientDisconnectedEventArgs args)
    {
        if (!connected)
        {
            return;
        }
        // mqttClient.Dispose(); is this needed?
        mqttClient = null;
        connected = false;
        Debug.Log("DisconnectFromServer(): success");
        await Task.CompletedTask; // is this right? 'return' does not work.
    }

    public async Task SendCommand(Command command)
    {
        if (!connected)
        {
            return;
        }

        string commandString = JsonConvert.SerializeObject(command, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                                         .WithTopic("cmd")
                                         .WithPayload(commandString)
                                         .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                                         .Build();

        try
        {
            await mqttClient.PublishAsync(message);
        }
        catch (Exception e)
        {
            Debug.Log($"SendCommand() ==> error: {e.Message}");
        }
    }

    private async Task ReceiveCommand(MqttApplicationMessageReceivedEventArgs args)
    {
        //Debug.Log("received a command!");

        if (args.ApplicationMessage.Topic != "cmd" && args.ApplicationMessage.Topic != ("response/" + clientID) &&
            args.ApplicationMessage.Topic != ("response_single/" + clientID))
        {
            Debug.Log(args.ApplicationMessage.Topic);
            return;
        }

        //Debug.Log(Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment));
        // we queried the server for all of the drawings in a chunk, and must now read in the data and send it to the chunk manager.
        //design-wise goes a bit against MQTT since qry_response is only sent to the single client that queried the server.
        if (args.ApplicationMessage.Topic == ("response/" + clientID))
        {
            List<Command> addCommands = new List<Command>();

            string str = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
            // The chunk we queried for is empty.
            // Serverside this means the key doesnt have an existing value or it does and is simply the EMPTY list.
            // We still *want* to create a gameobject and add this to the list of rendered gameobjects.
            if (str.Contains("NULL"))
            {
                // server returns "NULL<svector2>" so go past NULL and just read the <svector2>
                string vector2Str = str.Substring(4);
                // we return the empty list because theres nothing to render.
                await Task.Run(() =>
                {
                    _unitySynchronizationContext.Post(_ =>
                    {
                        JustMonika.Instance.QueryResponse(new List<Command>(), JsonConvert.DeserializeObject<SerializeUtilities.SVector2>(vector2Str).ToVector2());
                    }, null);
                });
                return;
            }

            List<string> commandsStr = JsonConvert.DeserializeObject<List<string>>(str);
            foreach (string s in commandsStr)
            {
                Command addCommand = JsonConvert.DeserializeObject<Command>(s);
                addCommands.Add(addCommand);
            }
            await Task.Run(() =>
            {
                _unitySynchronizationContext.Post(_ =>
                {
                    JustMonika.Instance.QueryResponse(addCommands, addCommands[0].targetChunk.ToVector2());
                }, null);
            });
            return;
        }

        //Otherwise if not qry response, we assume it's a cmd (aka ADD, MODIFY, DELETE)
        //Here we will "ONLY" care about the command if the "target chunk" of the command is for a chunk currently active or cached.
        //-Otherwise we will ignore as we do not care.
        string commandStr = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
        Command command = JsonConvert.DeserializeObject<Command>(commandStr);

        if (command == null)
        {
            return;
        }

        // If we have a delete or add command but we are not currently rendering the specified chunk for that command,
        // -then we have no reason to care and will simply just discard it.
        if ((command.type == Command.Type.DELETE || command.type == Command.Type.ADD) &&
             !JustMonika.Instance.activeChunks.ContainsKey(command.targetChunk.ToVector2()))
        {
            return;
        }

        // Offload Unity-specific operations to the main thread
        // TODO: rework this wrt to chunk operations
        await Task.Run(() =>
        {
            _unitySynchronizationContext.Post(_ =>
            {
                switch (command.type)
                {
                    case Command.Type.ADD:
                        //Debug.Log("ADD object has unique ID: " + command.addContainer.id);
                        GameObject obj = DeserializeUtilities.Regenerate(command.addContainer);
                        IDContainer metadata = obj.AddComponent<IDContainer>();
                        obj.AddComponent<TheMover>();
                        metadata.id = command.addContainer.id;
                        metadata.lastPos = obj.transform.position;
                        SelectEnterEvent e_ = new();
                        e_.AddListener(StartMove);
                        SelectExitEvent e = new();
                        e.AddListener(SendMove);
                        obj.GetComponent<ObjectManipulator>().firstSelectEntered = e_;
                        obj.GetComponent<ObjectManipulator>().lastSelectExited = e;
                        //drawings.Add(command.addContainer.id, obj);
                        obj.transform.position = command.addContainer.pos.ToVector3();
                        obj.transform.eulerAngles = command.addContainer.rot.ToVector3();
                        obj.transform.localScale = command.addContainer.sca.ToVector3();
                        obj.transform.SetParent(JustMonika.Instance.activeChunks[command.targetChunk.ToVector2()].transform);
                        break;

                    case Command.Type.DELETE:
                        uint id = command.deleteContainer.id;
                        GameObject obj2 = null;
                        foreach (Transform child in JustMonika.Instance.activeChunks[command.targetChunk.ToVector2()].transform)
                        {
                            if (child.GetComponent<IDContainer>().id == id)
                            {
                                obj2 = child.gameObject;
                                break;
                            }
                        }
                        if (obj2 != null)
                        {
                            //TODO: also if something in multiselect is affected, must call function that disables multiselect.
                            if (StateManager.Instance.selectedObj != null && StateManager.Instance.selectedObj == obj2)
                            {
                                ButtonManager.Instance.disableLineSelectButtons();
                            }
                            else if (SelectTool.Instance.selectedObjects != null && SelectTool.Instance.selectedObjects.Contains(obj2))
                            {
                                ButtonManager.Instance.disableMultipleSelect(false);
                            }

                            Destroy(obj2);
                        }
                        break;

                    case Command.Type.MODIFY:
                        // Can stay in same chunk (rendered or unrendered chunk)
                        // 1) Rendered-->Rendered: FOLLOW WITH THE TRANSFORMATION
                        // 2) Unrendered-->unrendered: DISCARD COMMAND!
                        // Can move between chunks (rendered-->unrendered, rendered-->rendered, unrendered-->unrendered, unrendered-->rendered
                        // 1) Rendered-->Unrendered: DESTROY THE GAMEOBJECT!
                        // 2) Rendered-->Rendered: FOLLOW WITH THE TRANSFORMATION, CHANGE PARENT TO NEW CHUNK!
                        // 3) Unrendered-->unrendered: DISCARD COMMAND!
                        // 4) Unrendered-->rendered: SEND A QRY_SINGLE TOPIC WITH ID and "targetChunk" from serializeutilities.singleqryobj
                        ModifyContainer c = command.modifyContainer;
                        
                        // SAME CHUNK
                        if (command.targetChunk.Equals(c.lastChunk))
                        {
                            // SAME CHUNK, RENDERED
                            //Debug.Log("SAME CHUNK!");
                            if (JustMonika.Instance.activeChunks.ContainsKey(command.targetChunk.ToVector2()))
                            {
                                GameObject obj3 = null;

                                foreach (Transform child in JustMonika.Instance.activeChunks[command.targetChunk.ToVector2()].transform)
                                {
                                    if (child.GetComponent<IDContainer>().id == c.id)
                                    {
                                        obj3 = child.gameObject;
                                        break;

                                    }
                                }

                                if (obj3 == null) return; // shouldnt really happen if everything
                                //-is working fine and there are no race conditions.

                                obj3.transform.position = c.newPosition.ToVector3();
                                obj3.transform.localScale = c.newSize.ToVector3();
                                obj3.transform.eulerAngles = c.newRotation.ToVector3();
                            }
                            // SAME CHUNK, UNRENDERED
                            else
                            {
                                return;
                            }
                        }
                        // DIFFERENT CHUNK
                        else
                        {
                            //Debug.Log("OLD CHUNK: " + c.lastChunk.ToVector2());
                            //Debug.Log("NEW CHUNK: " + command.targetChunk.ToVector2());
                            bool renderOldChunk = JustMonika.Instance.activeChunks.ContainsKey(c.lastChunk.ToVector2());
                            bool renderNewChunk = JustMonika.Instance.activeChunks.ContainsKey(command.targetChunk.ToVector2());
                            //Debug.Log("OLD CHUNK RENDERED: " + renderOldChunk);
                            //Debug.Log("NEW CHUNK RENDERED: " + renderNewChunk);

                            // DIFFERENT CHUNK, UNRENDERED --> UNRENDERED
                            if (!renderOldChunk && !renderNewChunk)
                            {
                                return;
                            }

                            // DIFFERENT CHUNK, RENDERED --> RENDERED
                            if (renderOldChunk && renderNewChunk)
                            {
                                GameObject oldChunk = JustMonika.Instance.activeChunks[c.lastChunk.ToVector2()];
                                GameObject newChunk = JustMonika.Instance.activeChunks[command.targetChunk.ToVector2()];
                                GameObject obj4 = null;
                                
                                // Find object from the old chunk
                                foreach (Transform child in oldChunk.transform)
                                {
                                    if (child.GetComponent<IDContainer>().id == c.id)
                                    {
                                        obj4 = child.gameObject;
                                        break;

                                    }
                                }

                                if (obj4 == null) return;

                                // Swap parents and transform
                                obj4.transform.SetParent(newChunk.transform);
                                obj4.transform.position = c.newPosition.ToVector3();
                                obj4.transform.localScale = c.newSize.ToVector3();
                                obj4.transform.eulerAngles = c.newRotation.ToVector3();

                            }
                        
                            // DIFFERENT CHUNK, UNRENDERED --> RENDERED
                            else if (!renderOldChunk && renderNewChunk)
                            {
                                // Simply ask server to add it since we have no reference to the old chunk.
                                SerializeUtilities.SingleQueryObj o;
                                o.targetChunk = command.targetChunk;
                                o.targetID = c.id;
                                string payload = JsonConvert.SerializeObject(o);
                                MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                                                                 .WithTopic("qry_single")
                                                                 .WithPayload(payload)
                                                                 .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                                                                 .Build();
                                mqttClient.PublishAsync(message);
                                Debug.Log("Sent query single!");
                            }
                        
                            // DIFFERENT CHUNK, RENDERED --> UNRENDERED
                            else if (renderOldChunk && !renderNewChunk)
                            {
                                //Debug.Log("RENDERED TO UNRENDERED!!");
                                GameObject obj5 = null;
                                foreach (Transform child in JustMonika.Instance.activeChunks[c.lastChunk.ToVector2()].transform)
                                {
                                    if (child.GetComponent<IDContainer>().id == c.id)
                                    {
                                        obj5 = child.gameObject;
                                        break;
                                    }
                                }
                                if (obj5 != null)
                                {
                                    //Debug.Log("NOT NULL!");
                                    //TODO: also if something in multiselect is affected, must call function that disables multiselect.
                                    if (StateManager.Instance.selectedObj != null && StateManager.Instance.selectedObj == obj5)
                                    {
                                        ButtonManager.Instance.disableLineSelectButtons();
                                    }
                                    else if (SelectTool.Instance.selectedObjects != null && SelectTool.Instance.selectedObjects.Contains(obj5))
                                    {
                                        ButtonManager.Instance.disableMultipleSelect(false);
                                    }

                                    Transform t = obj5.transform;
                                    uint id2 = t.GetComponent<IDContainer>().id;
                                    ModifyContainer c2 = new ModifyContainer(id2, t.position, t.localScale, t.eulerAngles,
                                        JustMonika.Instance.GetChunk(t.GetComponent<IDContainer>().lastPos));
                                    Command command2 = new Command(c2, JustMonika.Instance.GetChunk(t.position));
                                    t.gameObject.GetComponent<TheMover>().isEnabled = false;
                                    Destroy(obj5);
                                    SendCommand(command2);
                                }
                                else
                                {
                                    //Debug.Log("NULL!");
                                }
                            }
                        }
                        
                        break;

                    default:
                        Debug.LogWarning("ReceiveCommand: enum not implemented.");
                        break;
                }
            }, null);   
        });
    }

    public void StartMove(SelectEnterEventArgs args)
    {
        //Debug.Log("Started move!");
        Transform t = args.interactableObject.transform;
        t.GetComponent<TheMover>().isEnabled = true;
        StartCoroutine(t.GetComponent<TheMover>().Move(moveTimer));
    }

    public async void SendMove(SelectExitEventArgs args)
    {
        //Debug.Log("Stopped move");
        Transform t = args.interactableObject.transform;
        uint id = t.GetComponent<IDContainer>().id;
        ModifyContainer c = new ModifyContainer(id, t.position, t.localScale, t.eulerAngles,
            JustMonika.Instance.GetChunk(t.GetComponent<IDContainer>().lastPos));
        t.GetComponent<IDContainer>().lastPos = t.position; // Update last pos.
        Command command = new Command(c, JustMonika.Instance.GetChunk(t.position));
        t.gameObject.GetComponent<TheMover>().isEnabled = false;
        await SendCommand(command);
    }

    //TODO: send command
    public async Task QueryChunk(Vector2 position)
    {
        GameObject g = new GameObject($"CHUNK({position.x},{position.y})");
        g.transform.position = Vector3.zero;
        JustMonika.Instance.activeChunks.Add(position, g);
        //Debug.Log("SENT QUERY FOR: " + position);
        string payload = JsonConvert.SerializeObject(new SerializeUtilities.SVector2(position));
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
                                         .WithTopic("qry")
                                         .WithPayload(payload)
                                         .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                                         .Build();
        await mqttClient.PublishAsync(message);
    }
}