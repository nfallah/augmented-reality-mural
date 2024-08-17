using MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class JustMonika : MonoBehaviour//skull_emoji
{
    public static JustMonika Instance { get; private set; }

    [SerializeField] Transform player;

    [SerializeField] float chunkSize;

    [SerializeField] int renderDistance;

    private Vector2 lastChunk;

    public Dictionary<Vector2, GameObject> activeChunks = new();

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

        enabled = false;
    }

    public async void ActualStart()
    {
        lastChunk = GetChunk(player.position);
        for (int x = -renderDistance; x <= renderDistance; x++)
        { 
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                await ClientManager.Instance.QueryChunk(new Vector2(lastChunk.x + x, lastChunk.y + z));
            }
        }

        enabled = true; // Start updates as well.
    }

    public void QueryResponse(List<Command> addCommands, Vector2 responseChunk)
    {
        //Debug.Log("ADDED: " + responseChunk);
        GameObject g = activeChunks[responseChunk];

        foreach (Command addCommand in addCommands)
        {
            GameObject obj = DeserializeUtilities.Regenerate(addCommand.addContainer);
            IDContainer metadata = obj.AddComponent<IDContainer>();
            obj.AddComponent<TheMover>();
            metadata.lastPos = obj.transform.position;
            metadata.id = addCommand.addContainer.id;
            SelectEnterEvent e_ = new();
            e_.AddListener(ClientManager.Instance.StartMove);
            SelectExitEvent e = new();
            e.AddListener(ClientManager.Instance.SendMove);
            obj.GetComponent<ObjectManipulator>().firstSelectEntered = e_;
            obj.GetComponent<ObjectManipulator>().lastSelectExited = e;
            obj.transform.SetParent(g.transform); // override draw tool parent objects!
            obj.transform.position = addCommand.addContainer.pos.ToVector3();
            obj.transform.eulerAngles = addCommand.addContainer.rot.ToVector3();
            obj.transform.localScale = addCommand.addContainer.sca.ToVector3();

            // TODO: remove as delete command will work by searching through each gameobject in the specified chunk for that id container.
            //ClientManager.Instance.drawings.Add(addCommand.addContainer.id, obj);
        }
    }

    private void Update()
    {
        Vector2 newChunk = GetChunk(player.position);

        // unrender/render dem chunks
        // recall analogy of accepting old drawings in cached chunks, but make them invis.
        // completely reject drawing commands for old chunks.
        if (newChunk.x != lastChunk.x)
        {
            int renderDirection = (int)Mathf.Sign(newChunk.x - lastChunk.x);
            for(int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2 makeChunk = new Vector2(newChunk.x + renderDistance * renderDirection, newChunk.y + z);
                Vector2 destroyChunk = new Vector2(newChunk.x - (renderDistance + 1) * renderDirection, newChunk.y + z);
                ClientManager.Instance.QueryChunk(makeChunk);
                if (activeChunks.ContainsKey(destroyChunk))
                {
                    GameObject destroyObj = activeChunks[destroyChunk];
                    Destroy(destroyObj); // since all drawings of that chunk are children, destroying the chunk itself will also destroy all of the children!
                    activeChunks.Remove(destroyChunk);
                }
            }
        }

        if (newChunk.y != lastChunk.y)
        {
            int renderDirection = (int)Mathf.Sign(newChunk.y - lastChunk.y);
            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                Vector2 makeChunk = new Vector2(newChunk.x + x, newChunk.y + renderDistance * renderDirection);
                Vector2 destroyChunk = new Vector2(newChunk.x + x, newChunk.y - (renderDistance + 1) * renderDirection);
                ClientManager.Instance.QueryChunk(makeChunk);
                if (activeChunks.ContainsKey(destroyChunk))
                {
                    GameObject destroyObj = activeChunks[destroyChunk];
                    Destroy(destroyObj); // since all drawings of that chunk are children, destroying the chunk itself will also destroy all of the children!
                    activeChunks.Remove(destroyChunk);
                }
            }
        }

        lastChunk = newChunk;
    }

    public Vector2Int GetChunk(Vector3 pos)
    {
        return new Vector2Int((int)(pos.x / chunkSize), (int)(pos.z / chunkSize));
    }
}
