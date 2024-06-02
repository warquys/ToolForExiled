using System.Runtime.CompilerServices;
using YamlDotNet.Serialization;

namespace ToolForExiled;

public class Zone : IRestable
{
    public RoomPoint CornerA { get; set; }

    public RoomPoint CornerB { get; set; }

    [YamlIgnore]
    private Bounds? bounds = null;

    [YamlIgnore]
    public Bounds Bounds
    {
        get
        {
            if (!bounds.HasValue)
                RefreshCordonate();
            return bounds.Value;
        }
    }

    void IRestable.Reset()
    {
        RefreshCordonate();
    }


    public bool IsInBound(Player player)
    {
        return Bounds.Contains(player.Position);
    }

    public void RefreshCordonate()
    {
        var bounds = new Bounds();
        var min = CornerA.GetMapPosition();
        var max = CornerB.GetMapPosition();
        MaximizeAndMinimize(ref min, ref max);
        bounds.SetMinMax(min, max);
        this.bounds = bounds;
    }

    private void MaximizeAndMinimize(ref Vector3 min, ref Vector3 max)
    {
        MinMax(ref min.x, ref max.x);
        MinMax(ref min.y, ref max.y);
        MinMax(ref min.z, ref max.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MinMax(ref float min, ref float max)
        {
            if (min > max)
            {
                var temp = min;
                min = max;
                max = temp;
            }
        }
    }
}