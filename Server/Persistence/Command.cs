using static SerializeUtilities;

/* The command class is essentially a serializable, union-like structure. Only a single container
 * -class can be populated per command, with all other containers kept as null to save space when
 * -this script is converted to JSON and sent to the server.
 */
[Serializable]
public class Command
{
    public enum Type { ADD, DELETE, MODIFY }

    public Type type;

    public SVector2 targetChunk;

    public AddContainer addContainer;

    public DeleteContainer deleteContainer;

    public ModifyContainer modifyContainer;
}