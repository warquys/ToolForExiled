// Synapse MIT license
namespace ToolForExiled;

public class RoomPoint
{
    public RoomPoint() { }
    
    public RoomPoint(RoomType type, Vector3 relativePosition, Vector3 relativeRotation)
    {
        RoomType = type;
        position = relativePosition;
        rotation = relativeRotation;
    }

    public RoomPoint(Vector3 mapPosition, Quaternion mapRotation) : this(GetNearestRoom(mapPosition), mapPosition, mapRotation) { }

    public RoomPoint(Room relativeRoom, Vector3 mapPosition, Quaternion mapRotation)
    {
        RoomType = relativeRoom.Type;
        position = relativeRoom.GameObject.transform.InverseTransformPoint(mapPosition);
        rotation = (Quaternion.Inverse(relativeRoom.GameObject.transform.rotation) * mapRotation).eulerAngles;
    }

    private static Room GetNearestRoom(Vector3 position)
    {
        var room = Room.Get(position);
        return room ?? Room.List.OrderBy(x => Vector3.Distance(x.Position, position))?.FirstOrDefault();
    }

    public RoomType RoomType { get; set; } = RoomType.Unknown;

    /// <summary>
    /// Position relative to the room
    /// </summary>
    public Vector3 position { get; set; } = Vector3.zero;

    /// <summary>
    /// Rotation relative to the room
    /// </summary>
    public Vector3 rotation { get; set; } = Vector3.zero;

    /// <summary>
    /// Absolute position of the room
    /// </summary>
    public Vector3 GetMapPosition()
    {
        var room = Room.Get(RoomType);
        if (room != null) return room.GameObject.transform.TransformPoint(position);

        Log.Warn("Couldn't find a Room with Name " + RoomType + " for the Room Point");
        return Vector3.zero;
    }

    /// <summary>
    /// Absolute rotation of the room
    /// </summary>
    public Quaternion GetMapRotation()
    {
        var room = Room.Get(RoomType);

        if (room != null) return room.Rotation * Quaternion.Euler(rotation);

        Log.Warn("Couldn't find a Room with Name " + RoomType + " for the Room Point");
        return Quaternion.identity;
    }
}