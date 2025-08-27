using PlayerRoles;
using ToolForExiled;

namespace ToolForExiled.Wave;

public interface IVtCustomWave 
{
    void SpawnPlayer(List<ReferenceHub> players);
    void GenerateUnit();
}
